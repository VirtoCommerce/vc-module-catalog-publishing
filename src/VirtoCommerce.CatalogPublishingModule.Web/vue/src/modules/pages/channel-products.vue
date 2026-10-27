<template>
  <VcBlade
    :title="$t('CHANNELS.PAGES.PRODUCTS.TITLE')"
    :subtitle="channel?.name"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      :items="rows"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="CHANNEL_PRODUCTS"
      :empty-state="{ icon: 'lucide-package', title: $t('CHANNELS.PAGES.PRODUCTS.EMPTY') }"
      @pagination-click="pagination.goToPage"
    >
      <!-- the image cell falls back to the original when the "_sm" thumbnail doesn't exist -->
      <VcColumn
        id="imageUrl"
        :title="$t('CHANNELS.PAGES.PRODUCTS.COLUMNS.IMAGE')"
        type="image"
        width="80px"
      />
      <VcColumn
        id="name"
        :title="$t('CHANNELS.PAGES.PRODUCTS.COLUMNS.NAME')"
        body-class="channels-list__name"
        always-visible
      />
      <VcColumn
        id="code"
        :title="$t('CHANNELS.PAGES.PRODUCTS.COLUMNS.CODE')"
      />
      <VcColumn
        id="missing"
        :title="$t('CHANNELS.PAGES.PRODUCTS.COLUMNS.MISSING')"
      />
      <VcColumn
        id="completeness"
        :title="$t('CHANNELS.PAGES.PRODUCTS.COLUMNS.COMPLETENESS')"
        align="end"
        always-visible
      >
        <template #body="{ data: row }">
          <div class="dash-inline">
            <VcStatus :variant="row.completeness == null ? 'info' : row.completeness >= 100 ? 'success' : row.completeness >= 50 ? 'warning' : 'danger'">
              {{ row.completeness == null ? "…" : percent(row.completeness) }}
            </VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// Evaluation results per product: as the AngularJS screen, the channel's catalog filtered to the incomplete
// products by the indexed completeness field (index search), each page evaluated on the fly (read-only).
import { useApiClient, useAsync, useBlade, useDataTablePagination, useFunctions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { CompletenessClient, type CompletenessChannel } from "../../api_client/virtocommerce.catalogpublishing";
import { percent, searchProducts, type ListEntry } from "../utils";

// not routable: the channel arrives only through options
defineBlade({
  url: "/channel-products",
  name: "ChannelsProducts",
  permissions: ["channel:read"],
  routable: false,
});

type Row = ListEntry & { completeness?: number; missing?: string };

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ channel: CompletenessChannel }>();
const { debounce } = useFunctions();
const { getApiClient } = useApiClient(CompletenessClient);

const PAGE_SIZE = 20;
const channel = computed(() => options.value?.channel);
const rows = ref<Row[]>([]);
const totalCount = ref(0);
const searchValue = ref<string>();
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  if (!channel.value?.catalogId) return;
  // legacy keyword: completeness_<channel name>:[0 TO 99], the user's search text added to it
  const incomplete = `completeness_${(channel.value.name ?? "").toLowerCase()}:[0 TO 99]`;
  const keyword = [incomplete, searchValue.value].filter(Boolean).join(" ");
  const page = await searchProducts(channel.value.catalogId, keyword, pagination.skip, PAGE_SIZE);
  rows.value = (page.listEntries ?? []).map((x) => ({ ...x }));
  totalCount.value = page.totalCount ?? 0;
  if (!rows.value.length) return;
  const client = await getApiClient();
  const entries = await client.evaluateProductsCompleteness(channel.value.id!, rows.value.map((x) => x.id));
  rows.value = rows.value.map((row) => {
    const entry = entries.find((e) => e.productId === row.id);
    return {
      ...row,
      completeness: entry?.completenessPercent ?? 0,
      missing: (entry?.details ?? [])
        .filter((d) => (d.completenessPercent ?? 0) < 100)
        .map((d) => `${d.name} ${percent(d.completenessPercent)}`)
        .join(", "),
    };
  });
});

watch(searchValue, () => pagination.setPage(1));
watch([searchValue, () => pagination.skip], debounce(load, 300));
load();

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("CHANNELS.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);
</script>
