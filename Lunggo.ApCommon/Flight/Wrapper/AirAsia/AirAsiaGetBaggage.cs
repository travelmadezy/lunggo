﻿using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using CsQuery;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;
using Lunggo.Framework.Web;
using RestSharp;
using Lunggo.ApCommon.Constant;

namespace Lunggo.ApCommon.Flight.Wrapper.AirAsia
{
    internal partial class AirAsiaWrapper
    {
        private partial class AirAsiaClientHandler
        {
            public string GetBaggage(string origin, string destination)
            {
                string baggage = "0";
                var path = "http://www.airasia.com/api/feeandchargesapi/get?siteid=my/en&dep=" + origin + "&arr=" + destination;
                var client = new RestClient(path);
                var request = new RestRequest("", Method.GET);

                IRestResponse respons = client.Execute(request);
                var content = respons.Content;

                //Change to JSON Object
                var objects = JArray.Parse(content);
                var data = objects[6]["details"];//(string)objects[6]["details"][0]["feedesc"];
                string price = "";
                string dummyBaggage = "";
                int totalData = data.Count();
                for (var i = 0; i < totalData; i++)
                {
                    price = (string)data[i]["value"];
                    if (string.IsNullOrEmpty(price))
                    {
                        dummyBaggage = (string)data[i]["feedesc"];
                        Debug.Print("Dummy Bagasi : " + dummyBaggage);
                    }
                }

                //Debug.Print(data);
                if (!string.IsNullOrEmpty(dummyBaggage))
                {
                    var temp = dummyBaggage.Split(new string[] { "to" }, StringSplitOptions.None);
                    if (temp.Length > 1)
                    {
                        baggage = temp[1].Trim().Replace("kg)", "").Trim();
                    }

                }
                if (baggage != null || baggage != "")
                {
                    Debug.Print("Bagasi : "+baggage);
                    return baggage;
                }
                else
                {
                    baggage = "0";
                    return baggage;
                }                
            }
        }

    }

}
