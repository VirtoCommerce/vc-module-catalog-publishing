<template>
  <VcWidget
    v-if="hasAccess('channel:read')"
    icon="lucide-list-checks"
    :title="label"
    :disabled="!product?.id || !channelId"
    :class="{ 'channels-widget--warn': value != null && value < 100 }"
    @click="open"
  />
</template>

<script setup lang="ts">
// Completeness of the product on the Catalog module's product blade, for the first channel publishing its catalog
import { injectBladeContext, useApiClient, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { CompletenessClient } from "../../api_client/virtocommerce.catalogpublishing";
import { percent } from "../utils";

type Product = { id?: string; name?: string; catalogId?: string; outlines?: { items?: { id?: string; seoObjectType?: string }[] }[] };

const { t } = useI18n({ useScope: "global" });
const { hasAccess } = usePermissions();
const { openBlade } = useBlade();
const { getApiClient } = useApiClient(CompletenessClient);
const ctx = injectBladeContext();

const product = computed(() => ctx.value.item as Product | undefined);
// the catalogs the product is in: its own plus the virtual ones it is linked to (outlines)
const catalogIds = computed(() => [
  ...new Set([
    product.value?.catalogId,
    ...(product.value?.outlines ?? []).flatMap((o) => (o.items ?? []).filter((i) => i.seoObjectType === "Catalog").map((i) => i.id)),
  ]),
].filter(Boolean) as string[]);

const channelId = ref<string>();
const value = ref<number>();
const label = computed(() =>
  !channelId.value ? t("CHANNELS.WIDGET.NONE") : `${t("CHANNELS.WIDGET.TITLE")} ${value.value == null ? "…" : percent(value.value)}`,
);

async function refresh() {
  if (!product.value?.id || !catalogIds.value.length || !hasAccess("channel:read")) return;
  const client = await getApiClient();
  const channels = (await client.searchChannel({ catalogIds: catalogIds.value, skip: 0, take: 1 })).results ?? [];
  channelId.value = channels[0]?.id;
  value.value = channelId.value
    ? ((await client.evaluateProductsCompleteness(channelId.value, [product.value.id]))[0]?.completenessPercent ?? 0)
    : undefined;
}

watch(() => [product.value?.id, catalogIds.value.join()], refresh, { immediate: true });

function open() {
  openBlade({
    name: "ChannelsProductCompleteness",
    options: { productId: product.value?.id, productName: product.value?.name, catalogIds: catalogIds.value, channelId: channelId.value },
    onClose: refresh,
  });
}
</script>

<style lang="scss">
.channels-widget--warn .vc-widget__icon {
  color: var(--danger-500);
}
</style>
