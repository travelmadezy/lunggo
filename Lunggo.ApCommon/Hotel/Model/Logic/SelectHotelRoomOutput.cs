﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Lunggo.ApCommon.Hotel.Model.Logic
{
    public class SelectHotelRoomOutput :ResultBase
    {
        public string Token { get; set; }
        public DateTime? Timelimit { get; set; }
    }
}
