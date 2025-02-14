﻿using Lunggo.ApCommon.Payment.Constant;
using Lunggo.ApCommon.Payment.Model;
using Newtonsoft.Json;

namespace Lunggo.WebAPI.ApiSrc.Payment.Model
{
    public class PayApiRequest : PaymentData
    {
        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentMethod Method { get; set; }
        [JsonProperty("submethod", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentSubmethod? Submethod { get; set; }
        [JsonProperty("discCd", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscountCode { get; set; }
        [JsonProperty("rsvNo", NullValueHandling = NullValueHandling.Ignore)]
        public string RsvNo { get; set; }
        [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
        public int Test { get; set; }
    }
}