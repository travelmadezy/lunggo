﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lunggo.ApCommon.Hotel.Constant;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Content.Model;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Sdk;

namespace Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Content
{
    public partial class HotelBedsService
    {
        public static List<FacilityApi> hotelFacilityList = new List<FacilityApi>();
        public void GetFacility(int from, int to)
        {
            try
            {
                //var client = new HotelApiClient("zvwtnf83dj86bf58sejb6e3f", "HBbpT4u3xE",
                //    "https://api.test.hotelbeds.com/hotel-content-api");
                var client = new HotelApiClient(HotelApiType.ContentApi);
                var total = GetTotalFacility(client);
                Console.WriteLine("Total Facility : " + total);
                var isValid = true;
                do
                {
                    Debug.Print("From : " + from);

                    Console.WriteLine("From : " + from);

                    if (to >= total)
                    {
                        to = total;
                        isValid = false;
                    }
                    Debug.Print("To : " + to);
                    DoGetFacility(client, from, to);

                    from = from + 1000;
                    to = to + 1000;
                    Thread.Sleep(1000);

                } while (isValid);
                Console.WriteLine("Done");
            }
            catch
            {
                Console.WriteLine(from);
                Console.WriteLine(to);
                GetFacility(from, to);
            }
        }

        public void DoGetFacility(HotelApiClient client, int from, int to)
        {
            var languageCd = new List<string> { "ENG", "IND" };
            var facilityTemp = new List<FacilityApi>();
            var counter = 0;

            foreach (var t in languageCd)
            {
                List<Tuple<string, string>> param;

                param = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("${fields}", "all"),
                new Tuple<string, string>("${language}", t),
                new Tuple<string, string>("${from}", from.ToString()),
                new Tuple<string, string>("${to}", to.ToString()),
                new Tuple<string, string>("${useSecondaryLanguage}", "false"),
            };
                var facilityRs = client.GetFacility(param);
                if (facilityRs != null && facilityRs.facilities != null && facilityRs.facilities.Count != 0)
                {
                    foreach (var facility in facilityRs.facilities)
                    {
                        if (t.Equals("ENG"))
                        {
                            var singlefacility = new FacilityApi
                            {
                                code = (facility.facilityGroupCode*1000) + facility.code,
                                DescriptionEng = facility.description == null ? null : facility.description.content
                            };
                            facilityTemp.Add(singlefacility);
                        }
                        else
                        {
                            facilityTemp[counter].DescriptionInd = facility.description == null
                                ? null
                                : facility.description.content;
                            counter++;
                        }
                    }
                }
            }
            hotelFacilityList.AddRange(facilityTemp);
        }

        public int GetTotalFacility(HotelApiClient client)
        {
            List<Tuple<string, string>> param;

            param = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("${fields}", "all"),
                new Tuple<string, string>("${language}", "ENG"),
                new Tuple<string, string>("${from}", "1"),
                new Tuple<string, string>("${to}", "10"),
                new Tuple<string, string>("${useSecondaryLanguage}", "false"),
            };
            var FacilityRs = client.GetFacility(param);
            var total = FacilityRs.total;
            return total;
        }
    }
}
