﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunggo.ApCommon.Hotel.Model.Logic
{
    public class SelectHotelInput
    {
        public string SearchId { get; set; }
        public string HotelCode { get; set; }
        public int Page { get; set; }
        public List<KeyValuePair<string, string>> SortingParam { get; set; }
        public List<KeyValuePair<string, string>> FilterParam { get; set; }
    }
}
