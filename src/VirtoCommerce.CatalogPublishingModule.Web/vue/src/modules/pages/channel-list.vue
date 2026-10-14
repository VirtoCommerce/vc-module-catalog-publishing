<template>
  <VcBlade
    :title="$t('CHANNELS.PAGES.LIST.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      state-key="CHANNELS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :row-actions="rowActions"
      :empty-state="{ icon: 'lucide-list-checks', title: $t('CHANNELS.PAGES.LIST.EMPTY') }"
      @row-click="(e: { data: CompletenessChannel }) => openChannel(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('CHANNELS.PAGES.LIST.COLUMNS.NAME')"
        body-class="channels-list__name"
        sortable
        always-visible
      />
      <VcColumn
        id="catalogName"
        :title="$t('CHANNELS.PAGES.LIST.COLUMNS.CATALOG')"
        sortable
      />
      <VcColumn
        id="languages"
        :title="$t('CHANNELS.PAGES.LIST.COLUMNS.LANGUAGES')"
      >
        <template #body="{ data: channel }">{{ (channel.languages ?? []).join(", ") }}</template>
      </VcColumn>
      <VcColumn
        id="currencies"
        :title="$t('CHANNELS.PAGES.LIST.COLUMNS.CURRENCIES')"
      >
        <template #body="{ data: channel }">{{ (channel.currencies ?? []).join(", ") }}</template>
      </VcColumn>
      <VcColumn
        id="completenessPercent"
        :title="$t('CHANNELS.PAGES.LIST.COLUMNS.COMPLETENESS')"
        align="end"
        sortable
        always-visible
      >
        <template #body="{ data: channel }">
          <div class="dash-inline">
            <VcStatus :variant="statusVariant(channel.completenessPercent)">{{ percent(channel.completenessPercent) }}</VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useApiClient,
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  usePermissions,
  usePopup,
  type IBladeToolbar,
  type TableAction,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { CompletenessClient, type CompletenessChannel } from "../../api_client/virtocommerce.catalogpublishing";
import { errorText, percent } from "../utils";

defineBlade({
  url: "/channels",
  name: "ChannelsList",
  isWorkspace: true,
  permissions: ["channel:access"],
  menuItem: {
    title: "CHANNELS.MENU.TITLE",
    icon: "lucide-list-checks",
    priority: 70,
  },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();
const { getApiClient } = useApiClient(CompletenessClient);

const PAGE_SIZE = 20;
const items = ref<CompletenessChannel[]>([]);
const totalCount = ref(0);
const selection = ref<CompletenessChannel[]>([]);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "CHANNELS", initialField: "name", initialDirection: "ASC" });
const pagination = useDataTablePagination({ stateKey: "CHANNELS", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const client = await getApiClient();
  const result = await client.searchChannel({ sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

const statusVariant = (value?: number) => ((value ?? 0) >= 100 ? "success" : (value ?? 0) >= 50 ? "warning" : "danger");

function openChannel(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "ChannelsDetails",
    param: id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

async function evaluate(channel: CompletenessChannel) {
  try {
    const client = await getApiClient();
    const notification = await client.evaluateChannelCompleteness(channel.id!);
    activeItemId.value = channel.id;
    await openBlade({ name: "ChannelsEvaluation", options: { notification } });
  } catch (e) {
    showError(errorText(e));
  }
}

async function remove(channels: CompletenessChannel[]) {
  if (!(await showConfirmation(t("CHANNELS.ALERTS.DELETE_CHANNELS", { count: channels.length })))) return;
  await closeChildren();
  try {
    const client = await getApiClient();
    await client.deleteChannels(channels.map((x) => x.id!));
    selection.value = [];
  } catch (e) {
    showError(errorText(e));
  }
  await load();
}

const rowActions = (channel: CompletenessChannel): TableAction[] => [
  {
    id: "evaluate",
    icon: "lucide-calculator",
    title: t("CHANNELS.PAGES.LIST.ACTIONS.EVALUATE"),
    type: "info",
    hidden: !hasAccess("channel:update"),
    clickHandler: () => evaluate(channel),
  },
  {
    id: "products",
    icon: "lucide-package-search",
    title: t("CHANNELS.PAGES.LIST.ACTIONS.PRODUCTS"),
    type: "info",
    clickHandler: () => openBlade({ name: "ChannelsProducts", param: channel.id, options: { channel } }),
  },
  {
    id: "delete",
    icon: "lucide-trash-2",
    title: t("CHANNELS.PAGES.LIST.ACTIONS.DELETE"),
    type: "danger",
    variant: "danger",
    hidden: !hasAccess("channel:delete"),
    clickHandler: () => remove([channel]),
  },
];

load();
watch([sortExpression, () => pagination.skip], () => load());

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "add", title: t("CHANNELS.PAGES.LIST.TOOLBAR.ADD"), icon: "lucide-plus", permissions: "channel:create", clickHandler: () => openChannel() },
  { id: "refresh", title: t("CHANNELS.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "delete",
    title: t("CHANNELS.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "channel:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => remove(selection.value),
  },
]);

exposeToChildren({ reload: () => load() });
</script>

<style lang="scss">
.channels-list__name {
  color: var(--neutrals-900);
}
</style>
