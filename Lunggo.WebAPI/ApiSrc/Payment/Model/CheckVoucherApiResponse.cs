﻿using Lunggo.WebAPI.ApiSrc.Common.Model;
using Newtonsoft.Json;

namespace Lunggo.WebAPI.ApiSrc.Payment.Model
{
    public class CheckVoucherApiResponse : ApiResponseBase
    {
        [JsonProperty("discount", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Discount { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
    }
}