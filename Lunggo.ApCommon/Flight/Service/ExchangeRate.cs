﻿using System.Linq;
using Lunggo.ApCommon.Flight.Constant;

namespace Lunggo.ApCommon.Flight.Service
{
    public partial class FlightService
    {
        
        public void CurrencyGetterInternal(string currency, Supplier supplierName)
        {
            var supplier = Suppliers.Where(entry => entry.Value.SupplierName == supplierName).Select(entry => entry.Value).Single();
            var result = supplier.CurrencyGetter(currency);
                
            //return result;
        }
    }
}
