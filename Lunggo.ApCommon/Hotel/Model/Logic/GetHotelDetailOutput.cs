﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunggo.ApCommon.Hotel.Model.Logic
{
    public class GetHotelDetailOutput : ResultBase
    {
        public HotelDetailForDisplay HotelDetail { get; set; }
    }
}
