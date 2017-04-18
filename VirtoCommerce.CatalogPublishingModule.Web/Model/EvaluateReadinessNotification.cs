﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CatalogPublishingModule.Web.Model
{
    public class EvaluateReadinessNotification : PushNotification
    {
        public EvaluateReadinessNotification(string creator, string notifyType) : base(creator)
        {
            NotifyType = notifyType;
        }

        public ReadinessEntry[] Readiness { get; set; }

        [JsonProperty("finished")]
        public DateTime? Finished { get; set; }

        [JsonProperty("errorCount")]
        public long ErrorCount
        {
            get
            {
                return Errors != null ? Errors.Count() : 0;
            }
        }

        [JsonProperty("errors")]
        public ICollection<string> Errors { get; set; }
    }
}