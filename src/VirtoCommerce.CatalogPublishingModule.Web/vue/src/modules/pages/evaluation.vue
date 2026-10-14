<template>
  <VcBlade
    :title="job?.title ?? $t('CHANNELS.PAGES.EVALUATION.TITLE')"
    width="50%"
  >
    <VcContainer>
      <JobProgress
        :job="job"
        :finished="finished"
        :percent="percent"
        prefix="CHANNELS"
      />
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcContainer } from "@vc-shell/framework/ui";
import { watch } from "vue";
import type { EvaluateCompletenessNotification } from "../../api_client/virtocommerce.catalogpublishing";
import JobProgress from "../components/JobProgress.vue";
import { useJob, type JobNotification } from "../composables/useJob";

// not routable: the job's notification arrives only through options
defineBlade({
  url: "/channel-evaluation",
  name: "ChannelsEvaluation",
  permissions: ["channel:update"],
  routable: false,
});

// "EvaluateCompleteness" push notifications: live from the host's store, polled as a fallback
const { options, callParent } = useBlade<{ notification: EvaluateCompletenessNotification }>();
const { job, finished, percent, follow } = useJob();

if (options.value?.notification) follow(options.value.notification as JobNotification);

// the channel's completeness changes when the evaluation ends
watch(finished, (done) => done && callParent("reload"));
</script>
