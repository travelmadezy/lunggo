﻿using System;
using StackExchange.Redis;

namespace Lunggo.Generator.HotelDesc
{
    public class HotelDescGeneratorTest
    {
        public static void Test(IDatabase redisDb)
        {
            var hotelDetail = HotelDescGenerator.RetrieveFromCache(4006592, redisDb);
                foreach (var desc in hotelDetail.DescriptionList)
                {
                    Console.WriteLine("{0} {1}",desc.Line,desc.Description.Lang);
                    Console.WriteLine("{0}", desc.Description.Value);
                    Console.WriteLine("-------------------------------------------------------------------------------------------------");
                }
            
            /*var hotelDetail = RetrieveFromCache(4005214, redisDb);
            foreach (var facility in hotelDetail.FacilityList)
            {
                Console.WriteLine("{0}",facility.FacilityId);
            }*/
        }
    }
}
