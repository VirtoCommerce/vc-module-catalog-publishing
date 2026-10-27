import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import CompletenessWidget from "./widgets/CompletenessWidget.vue";

// AngularJS registered catalogPublishingWidgetController on the product blade ('itemDetail')
registerExternalWidget({
  id: "ChannelsProductCompleteness",
  title: "Completeness",
  component: markRaw(CompletenessWidget),
  targetBlades: ["CatalogProductDetails"],
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
