﻿using System.Collections.Generic;

namespace Lunggo.Flight.Model
{
    public class FlightTicket
    {
        public FlightTicket()
        {
            ListDepartDetail = new List<DepartDetail>();
            AdultTicket = new TicketType();
            ChildTicket = new TicketType();
            InfantTicket = new TicketType();
        }
        public bool returning { get; set; }
        public int AirlineCode { get; set; }
        public string AirlineName { get; set; }
        public List<DepartDetail> ListDepartDetail { get; set; }
        public TicketType AdultTicket { get; set; }
        public TicketType ChildTicket { get; set; }
        public TicketType InfantTicket { get; set; }
        public string TimeDiff { get; set; }
        public string StringDepartTime { get; set; }
        public string StringArrivedTime { get; set; }
    }
    public class TicketType
    {
        public int? PromoPrice { get; set; }
        public int? EconomicPrice { get; set; }
        public int? BusinessPrice { get; set; }
    }
}
