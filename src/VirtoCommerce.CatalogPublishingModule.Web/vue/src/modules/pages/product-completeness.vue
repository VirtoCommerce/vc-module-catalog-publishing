<template>
  <VcBlade
    :title="$t('CHANNELS.PAGES.PRODUCT.TITLE')"
    :subtitle="options?.productName"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="channels-form tw-space-y-4">
        <div
          v-if="!channels.length && !loading"
          class="channels-empty"
        >
          {{ $t("CHANNELS.PAGES.PRODUCT.NO_CHANNELS") }}
        </div>
        <template v-else>
          <VcSelect
            v-model="channelId"
            :label="$t('CHANNELS.PAGES.PRODUCT.CHANNEL')"
            :options="channels"
            option-value="id"
            option-label="name"
            :clearable="false"
          />
          <VcField
            :label="$t('CHANNELS.PAGES.PRODUCT.TOTAL')"
            :model-value="entry ? percent(entry.completenessPercent) : '…'"
          />
          <VcCard :header="$t('CHANNELS.PAGES.PRODUCT.DETAILS')">
            <dl class="channels-aspects">
              <template
                v-for="detail in entry?.details ?? []"
                :key="detail.name"
              >
                <dt>{{ detail.name }}</dt>
                <dd>
                  <VcStatus :variant="(detail.completenessPercent ?? 0) >= 100 ? 'success' : 'danger'">{{ percent(detail.completenessPercent) }}</VcStatus>
                </dd>
              </template>
            </dl>
          </VcCard>
        </template>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// Opened from the product blade's Completeness widget (AngularJS catalogPublishingWidgetController)
import { notification, useApiClient, useBlade, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCard, VcContainer, VcField, VcForm, VcSelect, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { CompletenessClient, type CompletenessChannel, type CompletenessEntry } from "../../api_client/virtocommerce.catalogpublishing";
import { percent } from "../utils";

// not routable: the product arrives only through options
defineBlade({
  url: "/product-completeness",
  name: "ChannelsProductCompleteness",
  permissions: ["channel:read"],
  routable: false,
});

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ productId: string; productName?: string; catalogIds: string[]; channelId?: string }>();
const { hasAccess } = usePermissions();
const { getApiClient } = useApiClient(CompletenessClient);

const channels = ref<CompletenessChannel[]>([]);
const channelId = ref<string>();
const entry = ref<CompletenessEntry>();
const loading = ref(false);

async function evaluate(save = false) {
  if (!channelId.value || !options.value?.productId) return;
  loading.value = true;
  try {
    const client = await getApiClient();
    entry.value = (await client.evaluateProductsCompleteness(channelId.value, [options.value.productId]))[0];
    if (save && entry.value && hasAccess("channel:update")) {
      await client.saveEntries([entry.value]);
      notification.success(t("CHANNELS.ALERTS.SAVED"), { timeout: 3000 });
    }
  } finally {
    loading.value = false;
  }
}

async function load() {
  loading.value = true;
  try {
    const client = await getApiClient();
    channels.value = (await client.searchChannel({ catalogIds: options.value?.catalogIds, skip: 0, take: 1000 })).results ?? [];
    channelId.value = channels.value.find((x) => x.id === options.value?.channelId)?.id ?? channels.value[0]?.id;
  } finally {
    loading.value = false;
  }
}

watch(channelId, () => evaluate());
load();

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "evaluate",
    title: t("CHANNELS.PAGES.PRODUCT.TOOLBAR.EVALUATE"),
    icon: "lucide-refresh-cw",
    permissions: "channel:update",
    disabled: computed(() => !channelId.value),
    clickHandler: () => evaluate(true),
  },
]);
</script>

<style lang="scss">
.channels-empty {
  font-size: 14px;
  color: var(--neutrals-500);
}
.channels-aspects {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px 16px;
  margin: 0;
  padding: 16px;
  font-size: 14px;
  dd {
    margin: 0;
  }
}
</style>
