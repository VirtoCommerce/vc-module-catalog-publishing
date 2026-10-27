<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="channels-form tw-space-y-4">
        <VcInput
          v-model="item.name"
          :label="$t('CHANNELS.PAGES.DETAILS.FIELDS.NAME')"
          :hint="$t('CHANNELS.PAGES.DETAILS.FIELDS.NAME_HINT')"
          :error="!nameValid"
          :error-message="nameValid ? undefined : $t('CHANNELS.PAGES.DETAILS.FIELDS.NAME_ERROR')"
          required
        />
        <VcSelect
          v-model="item.languages"
          :label="$t('CHANNELS.PAGES.DETAILS.FIELDS.LANGUAGES')"
          :hint="$t('CHANNELS.PAGES.DETAILS.FIELDS.LANGUAGES_HINT')"
          :options="languages"
          multiple
          searchable
          required
        />
        <VcSelect
          v-model="item.currencies"
          :label="$t('CHANNELS.PAGES.DETAILS.FIELDS.CURRENCIES')"
          :hint="$t('CHANNELS.PAGES.DETAILS.FIELDS.CURRENCIES_HINT')"
          :options="currencies"
          multiple
          searchable
          required
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="item.catalogId"
              :label="$t('CHANNELS.PAGES.DETAILS.FIELDS.CATALOG')"
              :options="catalogs"
              option-value="id"
              option-label="name"
              searchable
              required
              :clearable="false"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.evaluatorType"
              :label="$t('CHANNELS.PAGES.DETAILS.FIELDS.EVALUATOR')"
              :hint="$t('CHANNELS.PAGES.DETAILS.FIELDS.EVALUATOR_HINT')"
              :options="evaluators"
              required
              :clearable="false"
            />
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useApiClient, useAsync, useBlade, useBladeForm, useBladeWidgets, useLoading, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { CompletenessClient, type CompletenessChannel } from "../../api_client/virtocommerce.catalogpublishing";
import { errorText, getCurrencies, getLanguages, searchCatalogs, type Catalog } from "../utils";

defineBlade({
  url: "/channel",
  name: "ChannelsDetails",
  permissions: ["channel:read"],
});

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, closeSelf, exposeToChildren } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();
const { getApiClient } = useApiClient(CompletenessClient);

const item = ref<CompletenessChannel>({ languages: [], currencies: [] });
const languages = ref<{ id: string; title: string }[]>([]);
const currencies = ref<{ id: string; title: string }[]>([]);
const catalogs = ref<Catalog[]>([]);
const evaluators = ref<{ id: string; title: string }[]>([]);

const isNew = computed(() => !item.value.id);
const title = computed(() => (isNew.value ? t("CHANNELS.PAGES.DETAILS.NEW") : (item.value.name ?? t("CHANNELS.PAGES.DETAILS.TITLE"))));

// the name becomes the product index field completeness_<name>
const nameValid = computed(() => /^[a-zA-Z0-9_]*$/.test(item.value.name ?? ""));
const valid = computed(
  () =>
    !!item.value.name &&
    nameValid.value &&
    !!item.value.languages?.length &&
    !!item.value.currencies?.length &&
    !!item.value.catalogId &&
    !!item.value.evaluatorType,
);
const form = useBladeForm({
  data: item,
  canSaveOverride: valid,
  closeConfirmMessage: computed(() => t("CHANNELS.ALERTS.CLOSE_CONFIRMATION")),
});

async function loadLookups() {
  const client = await getApiClient();
  const [langs, curs, cats, evs] = await Promise.all([getLanguages(), getCurrencies(), searchCatalogs(), client.getEvaluators()]);
  languages.value = langs.map((x) => ({ id: x, title: x }));
  currencies.value = curs.map((x) => ({ id: x.code, title: x.code }));
  catalogs.value = cats.results;
  evaluators.value = (evs ?? []).map((x) => ({ id: x, title: x }));
  if (isNew.value && evaluators.value.length === 1) item.value.evaluatorType = evaluators.value[0].id;
}

const { loading: itemLoading, action: loadItem } = useAsync<string>(async (id) => {
  const client = await getApiClient();
  item.value = await client.getChannel(id!);
});

// notify: false: Save reports the failure itself with the server's message (errorText), so no second toast
const { loading: saveLoading, action: save } = useAsync(async () => {
  const client = await getApiClient();
  const body = { ...item.value, catalogName: catalogs.value.find((x) => x.id === item.value.catalogId)?.name };
  if (isNew.value) {
    item.value = await client.createChannel(body);
  } else {
    await client.updateChannel(body);
    item.value = await client.getChannel(item.value.id!);
  }
}, { notify: false });

const loading = useLoading(itemLoading, saveLoading);

async function evaluate() {
  try {
    const client = await getApiClient();
    const notification = await client.evaluateChannelCompleteness(item.value.id!);
    await openBlade({ name: "ChannelsEvaluation", options: { notification } });
  } catch (e) {
    showError(errorText(e));
  }
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("CHANNELS.PAGES.DETAILS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    // POST needs channel:create, PUT channel:update (an existing channel, or a new one once created)
    isVisible: computed(() => hasAccess(param.value || item.value.id ? "channel:update" : "channel:create")),
    disabled: computed(() => !form.canSave.value),
    async clickHandler() {
      try {
        await save();
        form.setBaseline();
        await callParent("reload");
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
  {
    id: "reset",
    title: t("CHANNELS.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "evaluate",
    title: t("CHANNELS.PAGES.DETAILS.TOOLBAR.EVALUATE"),
    icon: "lucide-calculator",
    permissions: "channel:update",
    disabled: computed(() => isNew.value || form.isModified.value),
    clickHandler: () => evaluate(),
  },
  {
    id: "delete",
    title: t("CHANNELS.PAGES.DETAILS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "channel:delete",
    disabled: isNew,
    async clickHandler() {
      if (!(await showConfirmation(t("CHANNELS.ALERTS.DELETE_CHANNELS", { count: 1 })))) return;
      try {
        const client = await getApiClient();
        await client.deleteChannels([item.value.id!]);
        form.setBaseline();
        await callParent("reload");
        await closeSelf();
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
]);

useBladeWidgets([
  {
    id: "ChannelProducts",
    icon: "lucide-package-search",
    title: "CHANNELS.PAGES.DETAILS.WIDGETS.PRODUCTS",
    disabled: isNew,
    onClick: () => openBlade({ name: "ChannelsProducts", param: item.value.id, options: { channel: item.value } }),
  },
]);

onMounted(async () => {
  await Promise.all([loadLookups(), param.value ? loadItem(param.value) : Promise.resolve()]);
  form.setBaseline();
});

// the evaluation updates the channel's completeness
exposeToChildren({
  async reload() {
    if (item.value.id) await loadItem(item.value.id);
    form.setBaseline();
    await callParent("reload");
  },
});
</script>

<style lang="scss">
.channels-form {
  padding: 20px 22px 36px;
}
</style>
