﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CsQuery.ExtensionMethods.Internal;
using Lunggo.ApCommon.Hotel.Constant;
using Lunggo.ApCommon.Hotel.Model;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Sdk.auto.model;
using Lunggo.Framework.BlobStorage;

namespace Lunggo.ApCommon.Hotel.Service
{
    public partial class HotelService
    {
        public class Country
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string IsoCode { get; set; }
            public List<Destination> Destinations { get; set; }
        }

        public class Destination
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public List<Zone> Zones { get; set; }
            public string CountryCode { get; set; }
        }

        public class Zone
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public Hotels Hotel { get; set; }
            public List<Area> Areas { get; set; }
            public string DestinationCode { get; set; }
        }

        public class Area
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public Hotels Hotel { get; set; }
            public string ZoneCode { get; set; }
            public string DestinationCode { get; set; }
            public string CountryCode { get; set; }
        }

        public class Hotels
        {
            public List<int> HotelCodes { get; set; }
            public string ZoneCode { get; set; }
        }
        public class FacilityFilter
        {
            public string Code { get; set; }
            public List<string> FacilityCode { get; set; }
        }

        public class FacilityGroup
        {
            public int Code { get; set; }
            public string NameId { get; set; }
            public string NameEn { get; set; }
            public List<Facility> Facilities { get; set; }

        }

        public class Facility
        {
            public int Code { get; set; }
            public string NameId { get; set; }
            public string NameEn { get; set; }

        }

        public class Chain
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class Accommodation
        {
            public string Code { get; set; }
            public string MultiDescription { get; set; }
            public string TypeNameId { get; set; }
            public string TypeNameEn { get; set; }
        }

        public class Board
        {
            public string Code { get; set; }
            public string NameId { get; set; }
            public string NameEn { get; set; }
            public string MultilingualCode { get; set; }
        }

        public class SegmentDict
        {
            public string Code { get; set; }
            public string NameId { get; set; }
            public string NameEn { get; set; }
        }

        public class Category
        {
            public string Code { get; set; }
            public int SimpleCode { get; set; }
            public string AccomodationType { get; set; }
            public string NameEn { get; set; }
            public string NameId { get; set; }
        }

        public class HotelRoomType
        {
            public string Type { get; set; }
            public string DescId { get; set; }
            public string DescEn { get; set; }
        }

        public class Room
        {
            public RoomCharacteristic RoomCharacteristic { get; set; }
            public HotelRoomType RoomType { get; set; }
            public string RoomCd { get; set; }
            public string RoomDescId { get; set; }
            public string RoomDescEn { get; set; }
            public int MinPax { get; set; }
            public int MaxPax { get; set; }
            public int MinAdult { get; set; }
            public int MaxAdult { get; set; }
            public int MinChild { get; set; }
            public int MaxChild { get; set; }

        }

        public class RateClass
        {
            public string Code { get; set; }
            public string DescId { get; set; }
            public string DescEn { get; set; }
        }

        public class RateType
        {
            public string Type { get; set; }
            public string DescId { get; set; }
            public string DescEn { get; set; }
        }

        public class PaymentType
        {
            public string Type { get; set; }
            public string DescId { get; set; }
            public string DescEn { get; set; }
        }

        public class CountryDict
        {
            public string CountryCode;
            public string IsoCode;
            public string Name;
        }
        public class RoomCharacteristic
        {
            public string CharacteristicCd { get; set; }
            public string CharacteristicDescId { get; set; }
            public string CharacteristicDescEn { get; set; }
        }

        public class Autocomplete
        {
            public String Name;
            public AutocompleteType Type;
            public string Id;
            public string Code;
        }

        //FOR AUTOCOMPLETE
        public static Dictionary<string, Autocomplete> _Autocompletes;
        public static List<HotelAutoComplete> AutoCompletes = new List<HotelAutoComplete>();
        public static List<HotelAutoComplete> AutocompleteList = new List<HotelAutoComplete>();

        public static string[] GeoCodeApiKeyList;

        public static Dictionary<string, string> HotelSegmentDictId;
        public static Dictionary<string, string> HotelSegmentDictEng;

        public static Dictionary<int, string> HotelFacilityGroupDictId;
        public static Dictionary<int, string> HotelFacilityGroupDictEng;
        public static Dictionary<int, Facility> HotelRoomFacility;

        public static Dictionary<string, Chain> HotelChains;
        public static Dictionary<string, Accommodation> HotelAccomodations;
        public static Dictionary<string, Board> HotelBoards;
        public static Dictionary<string, Category> HotelCategories;

        public static Dictionary<string, Room> HotelRoomDict;
        public static Dictionary<string, HotelRoomType> HotelRoomTypeDict;
        public static Dictionary<string, RoomCharacteristic> HotelRoomCharacteristicDict;

        public static Dictionary<string, RateClass> HotelRoomRateClassDict;
        public static Dictionary<string, RateType> HotelRoomRateTypeDict;
        public static Dictionary<string, PaymentType> HotelRoomPaymentTypeDict;
        public static Dictionary<string, CountryDict> HotelCountry;
        public static Dictionary<string, Destination> HotelDestinationDict;
        public static Dictionary<string, Country> HotelDestinationCountryDict;
        public static Dictionary<string, Zone> HotelDestinationZoneDict;
        public static Dictionary<string, Area> HotelDestinationAreaDict;
        public static Dictionary<int, string> HotelCodeAndZoneDict;
        public static Dictionary<string, FacilityFilter> HotelFacilityFilters;

        public static List<Country> Countries;
        public static List<FacilityGroup> FacilityGroups;
        public static List<Room> Rooms;
        public static List<FacilityFilter> FacilityFilters;

        
        private const string HotelRoomRateClassFileName = @"HotelRoomRateClass.csv";
        private const string HotelRoomRateTypeFileName = @"HotelRoomRateType.csv";
        private const string HotelRoomPaymentTypeFileName = @"HotelRoomPaymentType.csv";
        private const string HotelFacilityFilterGroupFileName = @"HotelFacilitiesFilterGroup.csv";
        private const string HotelGeoCodeApiFileName = @"GeoCodingAPI.txt";
        private static string _hotelGeoCodeApiKeyPath;


        private static string _hotelRoomRateClassFilePath;
        private static string _hotelRoomRateTypeFilePath;
        private static string _hotelRoomPaymentTypeFilePath;
        private static string _hotelFacilitiesFilter;

        private static string _configPath;

        public void InitDictionary(string folderName)
        {
            _configPath = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath(@"~/" + folderName + @"/")
                : string.IsNullOrEmpty(folderName)
                    ? ""
                    : folderName + @"\";

            _hotelRoomRateClassFilePath = Path.Combine(_configPath, HotelRoomRateClassFileName);
            _hotelRoomRateTypeFilePath = Path.Combine(_configPath, HotelRoomRateTypeFileName);
            _hotelRoomPaymentTypeFilePath = Path.Combine(_configPath, HotelRoomPaymentTypeFileName);

            _hotelFacilitiesFilter = Path.Combine(_configPath, HotelFacilityFilterGroupFileName);
            AutoCompletes = GetAutocompleteFromBlob();
            PopulateHotelSegmentDict();
            _hotelGeoCodeApiKeyPath = Path.Combine(_configPath, HotelGeoCodeApiFileName);

            PopulateGeoCodeApiList(_hotelGeoCodeApiKeyPath);

            PopulateHotelAccomodationDict();
            PopulateHotelBoardDict();
            PopulateHotelChainDict();
            PopulateHotelCategoryDict();

            PopulateHotelFacilityGroupDict();
            PopulateHotelFacilityGroupList();
            PopulateHotelRoomFacilityDict(FacilityGroups);
            PopulateHotelFacilityFilter(_hotelFacilitiesFilter);

            PopulateHotelRoomList();
            PopulateHotelRoomDict(Rooms);
            PopulateHotelRoomTypeDict(Rooms);
            PopulateHotelRoomCharacteristicDict(Rooms);

            PopulateHotelRoomRateClassDict(_hotelRoomRateClassFilePath);
            PopulateHotelRoomRateTypeDict(_hotelRoomRateTypeFilePath);
            PopulateHotelRoomPaymentTypeDict(_hotelRoomPaymentTypeFilePath);

            PopulateHotelCountriesDict();

            PopulateHotelCountryFromBlob();
            PopulateHotelDestinationCountryDict(Countries);
            PopulateHotelDestinationDict(Countries);
            PopulateHotelZoneDict(Countries);
            PopulateHotelAreaDict(Countries);

            //PopulateHotelCodeAndZoneDict(Countries);
            //PopulateAutocomplete();
            //PopulateHotel();
        }

        private static void PopulateGeoCodeApiList(String _hotelGeoCodeApiKeyPath)
        {
            GeoCodeApiKeyList = File.ReadAllLines(_hotelGeoCodeApiKeyPath, Encoding.UTF8);
            Console.WriteLine("Coba Cek Hasil aja");
        }

        private static void PopulateHotel()
        {
            for (var i = 1; i < 1000; i++)
            {
                var hotel = new HotelDetailsBase();
                try
                {
                    hotel = GetInstance().GetHotelDetailFromTableStorage(i);
                    var index = ((long) AutocompleteType.Hotel).ToString() + hotel.HotelCode*4294967295;
                    var input = new Autocomplete
                    {
                        Id = index,
                        Code = hotel.HotelCode.ToString(),
                        Type = AutocompleteType.Hotel,
                        Name = hotel.HotelName + ", " + GetInstance().
                            GetZoneNameFromDict(hotel.ZoneCode) + ", "
                            + GetInstance().GetDestinationNameFromDict(hotel.DestinationCode).Name + ", "
                            + GetInstance().GetCountryNameFromDict(hotel.CountryCode).Name
                    };

                    _Autocompletes.Add(index, input);
                }
                catch
                {

                }

            }
        }

        //POPULATE METHODS REGARDING HOTEL SEGMENT
        private static void PopulateHotelSegmentDict()
        {
            HotelSegmentDictEng = new Dictionary<string, string>();
            HotelSegmentDictId = new Dictionary<string, string>();

            var segments = GetInstance().GetHotelSegmentFromStorage();
            foreach (var segment in segments)
            {
                if (!HotelSegmentDictEng.ContainsKey(segment.Code))
                HotelSegmentDictEng.Add(segment.Code, segment.NameEn);
                if (!HotelSegmentDictId.ContainsKey(segment.Code))
                HotelSegmentDictId.Add(segment.Code, segment.NameId);
            }

        }

        //POPULATE METHODS REGARDING FACILITY
        private static void PopulateHotelFacilityGroupList()
        {
            FacilityGroups = new List<FacilityGroup>();
            var facilities = GetInstance().GetHotelFacilityFromStorage();
            foreach (var facility in facilities)
            {
                var foundFacilityGroup = FacilityGroups.Where(g => g.Code == Convert.ToInt32(facility.Code) / 1000).ToList();
                if (foundFacilityGroup.Count == 0)
                {
                    var newFacilityGroup = new FacilityGroup
                    {
                        Code = Convert.ToInt32(facility.Code) / 1000,
                        NameEn =
                            GetInstance()
                                .GetHotelFacilityGroupEng(Convert.ToInt32(facility.Code) / 1000),
                        NameId =
                            GetInstance()
                                .GetHotelFacilityGroupId(Convert.ToInt32(facility.Code) / 1000),
                        Facilities = new List<Facility>
                            {
                                new Facility
                                {
                                    Code = Convert.ToInt32(facility.Code) % 1000,
                                    NameEn = facility.NameEn,
                                    NameId = facility.NameId,
                                }
                            }
                    };

                    FacilityGroups.Add(newFacilityGroup);
                }
                else
                {
                    var foundFacility =
                        foundFacilityGroup[0].Facilities.Where(f => f.Code == Convert.ToInt32(facility.Code) / 1000)
                            .ToList();
                    if (foundFacility.Count == 0)
                    {
                        var newFacility = new Facility
                        {
                            Code = Convert.ToInt32(facility.Code) % 1000,
                            NameEn = facility.NameEn,
                            NameId = facility.NameId,
                        };
                        FacilityGroups.Where(g => g.Code == Convert.ToInt32(facility.Code) / 1000).
                            ToList()[0].Facilities.Add(newFacility);
                    }
                }
            }

        }

        private static void PopulateHotelFacilityGroupDict()
        {
            HotelFacilityGroupDictEng = new Dictionary<int, string>();
            HotelFacilityGroupDictId = new Dictionary<int, string>();
            var facilitiesGrp = GetInstance().GetHotelFacilityGroupFromStorage();
            foreach (var group in facilitiesGrp)
            {
                if (!HotelFacilityGroupDictEng.ContainsKey(group.Code))
                HotelFacilityGroupDictEng.Add(group.Code, group.NameEn);
                if (!HotelFacilityGroupDictId.ContainsKey(group.Code))
                HotelFacilityGroupDictId.Add(group.Code, group.NameId);
            }
        }

        private static void PopulateHotelFacilityFilter(String hotelFacilityFilter)
        {
            HotelFacilityFilters = new Dictionary<string, FacilityFilter>();
            FacilityFilters = new List<FacilityFilter>();

            using (var file = new StreamReader(hotelFacilityFilter))
            {
                var line = file.ReadLine();
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    var splittedLine = line.Split('|');
                    if (HotelFacilityFilters.ContainsKey(splittedLine[1]))
                    {
                        HotelFacilityFilters[splittedLine[1]].FacilityCode.Add(splittedLine[0]);
                    }
                    else
                    {
                        if (!HotelFacilityFilters.ContainsKey(splittedLine[1]))
                        HotelFacilityFilters.Add(splittedLine[1], new FacilityFilter
                        {
                            Code = splittedLine[1],
                            FacilityCode = new List<string> { splittedLine[0] }
                        });
                    }
                }
            }
        }

        private static void PopulateHotelRoomFacilityDict(List<FacilityGroup> facilityGroups)
        {
            HotelRoomFacility = new Dictionary<int, Facility>();

            for (var index = 0; index < facilityGroups.Where(f => f.Code == 60).ToList().Count; index++)
            {
                var facilityGroup = facilityGroups.Where(f => f.Code == 60).ToList()[index];
                foreach (var fac in facilityGroup.Facilities)
                {
                    if (!HotelRoomFacility.ContainsKey(fac.Code))
                    HotelRoomFacility.Add(fac.Code, fac);
                }
            }
        }

        //POPULATE METHODS REGARDING HOTEL ROOM

        private static void PopulateHotelRoomList()
        {
            Rooms = new List<Room>();
            Rooms = GetInstance().GetHotelRoomFromStorage();
        }

        private static void PopulateHotelRoomDict(IEnumerable<Room> rooms)
        {
            HotelRoomDict = new Dictionary<string, Room>();

            foreach (var room in rooms)
            {
                if (!HotelRoomDict.ContainsKey(room.RoomCd))
                HotelRoomDict.Add(room.RoomCd, room);
            }

        }

        private static void PopulateHotelRoomTypeDict(IEnumerable<Room> rooms)
        {
            HotelRoomTypeDict = new Dictionary<string, HotelRoomType>();

            foreach (var room in rooms)
            {
                HotelRoomType x;
                if (!HotelRoomTypeDict.TryGetValue(room.RoomType.Type, out x))
                {
                    if (!HotelRoomTypeDict.ContainsKey(room.RoomType.Type))
                    HotelRoomTypeDict.Add(room.RoomType.Type, room.RoomType);
                }
            }
        }

        private static void PopulateHotelRoomCharacteristicDict(IEnumerable<Room> rooms)
        {
            HotelRoomCharacteristicDict = new Dictionary<string, RoomCharacteristic>();
            foreach (var room in rooms)
            {
                RoomCharacteristic x;
                if (!HotelRoomCharacteristicDict.TryGetValue(room.RoomCharacteristic.CharacteristicCd, out x))
                {
                    if (!HotelRoomCharacteristicDict.ContainsKey(room.RoomCharacteristic.CharacteristicCd))
                    HotelRoomCharacteristicDict.Add(room.RoomCharacteristic.CharacteristicCd, room.RoomCharacteristic);
                }
            }

        }

        private static void PopulateHotelRoomRateClassDict(String hotelRoomRateClassFilePath)
        {
            HotelRoomRateClassDict = new Dictionary<string, RateClass>(); ;

            using (var file = new StreamReader(hotelRoomRateClassFilePath))
            {
                var line = file.ReadLine();
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    var splittedLine = line.Split('|');

                    if (!HotelRoomRateClassDict.ContainsKey(splittedLine[0]))
                        HotelRoomRateClassDict.Add(splittedLine[0], new RateClass
                    {
                        Code = splittedLine[0],
                        DescEn = splittedLine[1],
                        DescId = splittedLine[2]
                    });
                }
            }
        }

        private static void PopulateHotelRoomRateTypeDict(String hotelRoomRateTypeFilePath)
        {
            HotelRoomRateTypeDict = new Dictionary<string, RateType>();

            using (var file = new StreamReader(hotelRoomRateTypeFilePath))
            {
                var line = file.ReadLine();
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    var splittedLine = line.Split('|');

                    if (!HotelRoomRateTypeDict.ContainsKey(splittedLine[0]))
                        HotelRoomRateTypeDict.Add(splittedLine[0], new RateType
                    {
                        Type = splittedLine[0],
                        DescEn = splittedLine[1],
                        DescId = splittedLine[2]
                    });
                }
            }
        }

        private static void PopulateHotelRoomPaymentTypeDict(String hotelRoomPaymentTypeFilePath)
        {
            HotelRoomPaymentTypeDict = new Dictionary<string, PaymentType>();

            using (var file = new StreamReader(hotelRoomPaymentTypeFilePath))
            {
                var line = file.ReadLine();
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    var splittedLine = line.Split('|');

                    if (!HotelRoomPaymentTypeDict.ContainsKey(splittedLine[0]))
                        HotelRoomPaymentTypeDict.Add(splittedLine[0], new PaymentType
                    {
                        Type = splittedLine[0],
                        DescEn = splittedLine[1],
                        DescId = splittedLine[2]
                    });
                }
            }
        }


        private static void PopulateHotelAccomodationDict()
        {
            HotelAccomodations = new Dictionary<string, Accommodation>();
            var accomodations = GetInstance().GetHotelAccomodationFromStorage();
            foreach (var acc in accomodations)
            {
                if (!HotelAccomodations.ContainsKey(acc.Code))
                HotelAccomodations.Add(acc.Code, acc);
            }
        }


        private static void PopulateHotelBoardDict()
        {
            HotelBoards = new Dictionary<string, Board>();
            var boards = GetInstance().GetHotelBoardFromStorage();
            foreach (var board in boards)
            {
                if (!HotelBoards.ContainsKey(board.Code))
                HotelBoards.Add(board.Code, board);
            }
            
        }

        private static void PopulateHotelChainDict()
        {
            HotelChains = new Dictionary<string, Chain>();
            var chains = GetInstance().GetHotelChainFromStorage();
            foreach (var chain in chains)
            {
                if (!HotelChains.ContainsKey(chain.Code))
                HotelChains.Add(chain.Code, chain);
            }
        }

        private static void PopulateHotelCategoryDict()
        {
            HotelCategories = new Dictionary<string, Category>();
            var categories = GetInstance().GetHotelCategoryFromStorage();
            foreach (var category in categories)
            {
                if (!HotelCategories.ContainsKey(category.Code))
                HotelCategories.Add(category.Code, category);
            }
        }

        private static void PopulateHotelCountriesDict()
        {
            HotelCountry = new Dictionary<string, CountryDict>();
            var countries = GetInstance().GetHotelCountryFromStorage();
            foreach (var country in countries)
            {
                if (!HotelCountry.ContainsKey(country.CountryCode))
                    HotelCountry.Add(country.CountryCode, country);
            }
        }

        //POPULATE METHODS REGARDING DESTINATION AND ZONE

        /*Type 1 : Populate from Blob with data as csv format*/
        //private static void PopulateHotelDestinationList()
        //{
        //    Countries = new List<Country>();
        //    var blobService = BlobStorageService.GetInstance();
        //    var byteFile = blobService.GetByteArrayByFileInContainer("HotelDestination", "hotelcsvcontent");
        //    MemoryStream fileStream = new MemoryStream(byteFile);
        //    string[] result;
        //    using (StreamReader reader = new StreamReader(fileStream))
        //    {
        //        result = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    }

        //    foreach (var line in result)
        //    {
        //        var splittedLine = line.Split('|');
        //        var foundCountry = Countries.Where(c => c.Code == splittedLine[2]).ToList();
        //        if (foundCountry.Count == 0)
        //        {
        //            var newCountry = new Country
        //            {
        //                Code = splittedLine[2],
        //                Name = GetInstance().GetHotelCountryName(splittedLine[2]),
        //                IsoCode = GetInstance().GetHotelCountryIsoCode(splittedLine[2]),
        //                Destinations = new List<Destination>
        //                    {
        //                        new Destination
        //                        {
        //                            Code = splittedLine[0],
        //                            Name = splittedLine[1],
        //                            Zones = new List<Zone>
        //                            {
        //                                new Zone
        //                                {
        //                                    Code = splittedLine[4],
        //                                    Name = splittedLine[5],
        //                                    DestinationCode = splittedLine[0],
        //                                    Areas = new List<Area>
        //                                    {
        //                                        new Area
        //                                        {
        //                                            Code = splittedLine[6],
        //                                            Name = splittedLine[7],
        //                                            ZoneCode = splittedLine[4],
        //                                            DestinationCode = splittedLine[0],
        //                                            CountryCode = splittedLine[2]
        //                                        }
        //                                    }
        //                                    //Hotel = new Hotels
        //                                    //    {
        //                                    //        HotelCodes = GetInstance().GetHotelListByLocationFromStorage(splittedLine[0] + "-" + splittedLine[4]),
        //                                    //        ZoneCode = splittedLine[0] + "-" + splittedLine[4],
        //                                    //    }
        //                                }
        //                            },
        //                            CountryCode = splittedLine[2]
        //                            }
        //                        }
        //            };
        //            Countries.Add(newCountry);

        //        }
        //        else
        //        {
        //            var foundDestination = foundCountry[0].Destinations.Where(d => d.Code == splittedLine[0]).ToList();
        //            if (foundDestination.Count == 0)
        //            {
        //                var newDestination = new Destination
        //                {
        //                    Code = splittedLine[0],
        //                    Name = splittedLine[1],
        //                    Zones = new List<Zone>
        //                        {
        //                            new Zone
        //                            {
        //                                Code = splittedLine[4],
        //                                Name = splittedLine[5],
        //                                DestinationCode = splittedLine[0],
        //                                Areas = new List<Area>
        //                                    {
        //                                        new Area
        //                                        {
        //                                            Code = splittedLine[6],
        //                                            Name = splittedLine[7],
        //                                            ZoneCode = splittedLine[4],
        //                                            DestinationCode = splittedLine[0],
        //                                            CountryCode = splittedLine[2]
        //                                        }
        //                                    }
        //                                //Hotel = new Hotels
        //                                //    {
        //                                //        HotelCodes = GetInstance().GetHotelListByLocationFromStorage(splittedLine[0] + "-" + splittedLine[4]),
        //                                //        ZoneCode = splittedLine[0] + "-" + splittedLine[4],
        //                                //    }
        //                            }
        //                        },
        //                    CountryCode = splittedLine[2]
        //                };
        //                Countries.Where(c => c.Code == splittedLine[2]).ToList()[0].Destinations.Add(newDestination);
        //            }
        //            else
        //            {
        //                var foundZone = foundDestination.Where(d => d.Code == splittedLine[0]).ToList()[0].Zones.Where(e => e.Code == splittedLine[4]).ToList();
        //                if (foundZone.Count == 0)
        //                {
        //                    var newZone = new Zone
        //                    {
        //                        Code = splittedLine[4],
        //                        Name = splittedLine[5],
        //                        DestinationCode = splittedLine[0],
        //                        Areas = new List<Area>
        //                        {
        //                            new Area
        //                            {
        //                                Code = splittedLine[6],
        //                                Name = splittedLine[7],
        //                                ZoneCode = splittedLine[4],
        //                                DestinationCode = splittedLine[0],
        //                                CountryCode = splittedLine[2]
        //                            }
        //                        }
        //                        //Hotel = new Hotels
        //                        //{
        //                        //    HotelCodes = GetInstance().GetHotelListByLocationFromStorage(splittedLine[0] + "-" + splittedLine[4]),
        //                        //    ZoneCode = splittedLine[0] + "-" + splittedLine[4],
        //                        //}
        //                    };
        //                    Countries.Where(c => c.Code == splittedLine[2]).ToList()[0].Destinations.Where(
        //                        d => d.Code == splittedLine[0]).ToList()[0].Zones.Add(newZone);
        //                }
        //                else
        //                {
        //                    var foundArea = foundZone.Where(d => d.Code == splittedLine[6]).ToList();
        //                    if (foundArea.Count == 0)
        //                    {
        //                        var newArea = new Area
        //                        {
        //                            Code = splittedLine[6],
        //                            Name = splittedLine[7],
        //                            DestinationCode = splittedLine[0],
        //                            CountryCode = splittedLine[2],
        //                            ZoneCode = splittedLine[4]
        //                            //Hotel = new Hotels
        //                            //{
        //                            //    HotelCodes = GetInstance().GetHotelListByLocationFromStorage(splittedLine[0] + "-" + splittedLine[4]),
        //                            //    ZoneCode = splittedLine[0] + "-" + splittedLine[4],
        //                            //}
        //                        };
        //                        Countries.Where(c => c.Code == splittedLine[2]).ToList()[0].Destinations.Where(
        //                            d => d.Code == splittedLine[0]).ToList()[0].Zones.Where(
        //                                e => e.Code == splittedLine[4]).ToList()[0].Areas.Add(newArea);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //}

        /*Type 2 : Populate from Blob with data as Destination Object*/

        private static void PopulateHotelCountryFromBlob()
        {
            Countries = new List<Country>();
            var destinations = GetInstance().GetHotelDestinationFromStorage();
            foreach (var destination in destinations)
            {
                var foundCountry = Countries.Where(c => c.Code == destination.CountryCode).ToList();
                if (foundCountry.Count == 0)
                {
                    var country = new Country
                    {
                        Code = destination.CountryCode,
                        Name = GetInstance().GetHotelCountryName(destination.CountryCode),
                        IsoCode = GetInstance().GetHotelCountryIsoCode(destination.CountryCode),
                        Destinations = new List<Destination> { destination }
                    };
                    Countries.Add(country);
                }
                else
                {
                    Countries.First(x => x.Code == destination.CountryCode).Destinations.Add(destination);
                }
            }
        }

        private static void PopulateHotelDestinationCountryDict(IEnumerable<Country> countries)
        {
            HotelDestinationCountryDict = new Dictionary<string, Country>();
            foreach (var country in countries)
            {
                if (!HotelDestinationCountryDict.ContainsKey(country.Code))
                    HotelDestinationCountryDict.Add(country.Code, country);
            }
        }

        private static void PopulateHotelDestinationDict(IEnumerable<Country> countries)
        {
            HotelDestinationDict = new Dictionary<string, Destination>();
            foreach (var destination in countries.SelectMany(country => country.Destinations))
            {
                if (!HotelDestinationDict.ContainsKey(destination.Code))
                    HotelDestinationDict.Add(destination.Code, destination);
            }
        }

        private static void PopulateHotelZoneDict(IEnumerable<Country> countries)
        {
            HotelDestinationZoneDict = new Dictionary<string, Zone>();
            var zones =
                countries.SelectMany(country => country.Destinations).SelectMany(destination => destination.Zones).Where(z => z != null).ToList();
            zones = zones.Distinct().ToList();
            foreach (var zone in zones)
            {
                zone.Code = zone.Code.Split('-').Length == 2 ? zone.Code : zone.DestinationCode + "-" + zone.Code;
                if (!HotelDestinationZoneDict.ContainsKey(zone.Code))
                    HotelDestinationZoneDict.Add(zone.Code, zone);
            }
        }

        private static void PopulateHotelAreaDict(IEnumerable<Country> countries)
        {
            HotelDestinationAreaDict = new Dictionary<string, Area>();
            var areas =
                countries.SelectMany(country => country.Destinations)
                    .SelectMany(destination => destination.Zones)
                    .SelectMany(zone => zone.Areas).Where(z => z != null).ToList();
            areas = areas.Distinct().ToList();
            foreach (var area in areas)
            {
                if (!string.IsNullOrEmpty(area.Code) && !HotelDestinationAreaDict.ContainsKey(area.Code))
                {
                    HotelDestinationAreaDict.Add(area.Code, area);
                }
            }
        }

        //private void PopulateHotelCodeAndZoneDict(IEnumerable<Country> countries)
        //{
        //    GetInstance().HotelCodeAndZoneDict = new Dictionary<int, string>();
        //    foreach (var country in countries)
        //    {
        //        foreach (var destination in country.Destinations)
        //        {
        //            foreach (var zone in destination.Zones)
        //            {
        //                foreach (var hotel in zone.Hotel.HotelCodes)
        //                {
        //                    HotelCodeAndZoneDict.Add(hotel, zone.Code);
        //                }
        //            }
        //        }
        //    }
        //}
        //GET METHOD REGARDING AUTOCOMPLETE
        public HotelAutoComplete GetLocationById(string id)
        {
            var value = AutoCompletes.First(c => c.Id == id);
            if (value != null)
            {
                return value;
            }

            return new HotelAutoComplete();
        }

        //GET METHODS REGARDING SEGMENT
        public string GetHotelSegmentId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = "";
            HotelSegmentDictId.TryGetValue(code, out value);
            return value;
        }
        public string GetHotelSegmentEng(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = "";
            HotelSegmentDictEng.TryGetValue(code, out value);
            return value;
        }

        //GET METHODS REGARDING FACILITY
        public Facility GetHotelFacility(int code)
        {
            //var found = false;
            foreach (var facility in FacilityGroups.Where(@group => @group.Code == code / 1000).SelectMany(@group => @group.Facilities.Where(facility => facility.Code == code % 1000)))
            {
                return facility;
            }

            return new Facility();
        }

        public string GetHotelFacilityDescId(int code)
        {
            foreach (var facility in FacilityGroups.Where(@group => @group.Code == code / 1000).SelectMany(@group => @group.Facilities.Where(facility => facility.Code == code % 1000)))
            {
                return facility.NameId;
            }

            return "";
        }

        public string GetHotelFacilityDescEn(int code)
        {
            foreach (var facility in FacilityGroups.Where(@group => @group.Code == code / 1000).SelectMany(@group => @group.Facilities.Where(facility => facility.Code == code % 1000)))
            {
                return facility.NameEn;
            }

            return "";
        }

        public string GetHotelFacilityGroupId(int code)
        {
            var value = "";
            HotelFacilityGroupDictId.TryGetValue(code, out value);
            return value;
        }
        public string GetHotelFacilityGroupEng(int code)
        {
            var value = "";
            HotelFacilityGroupDictEng.TryGetValue(code, out value);
            return value;
        }
        public Facility GetHotelRoomFacility(int code)
        {
            var value = new Facility();
            HotelRoomFacility.TryGetValue(code, out value);
            return value;

        }
        public string GetHotelRoomFacilityDescId(int roomFacilityCd)
        {
            foreach (var facility in FacilityGroups.Where(@group => @group.Code == 60).SelectMany(@group => @group.Facilities.Where(facility => facility.Code == roomFacilityCd)))
            {
                return facility.NameId;
            }

            return "";
        }

        public string GetHotelRoomFacilityDescEn(int roomFacilityCd)
        {
            foreach (var facility in FacilityGroups.Where(@group => @group.Code == 60).SelectMany(@group => @group.Facilities.Where(facility => facility.Code == roomFacilityCd)))
            {
                return facility.NameEn;
            }

            return "";
        }

        public List<Facility> GetAllFacilitiesInAGroup(int cd)
        {
            foreach (var @group in FacilityGroups.Where(@group => @group.Code == cd))
            {
                return @group.Facilities;
            }

            return new List<Facility>();
        }

        public string GetNameOfFacilityGroup(int facilityCd, string lang)
        {
            if (lang == "EN")
            {
                foreach (var @group in FacilityGroups.Where(@group => @group.Code == facilityCd / 1000))
                {
                    return @group.NameEn;
                }
            }
            else
            {
                foreach (var @group in FacilityGroups.Where(@group => @group.Code == facilityCd / 1000))
                {
                    return @group.NameId;
                }
            }

            return "";

        }

        //GET METHODS REGARDING HOTEL ROOM
        public Room GetHotelRoom(string code)
        {
            if (code == null)
            {
                return new Room();
            }

            foreach (var room in Rooms.Where(room => room.RoomCd == code))
            {
                return room;
            }

            return new Room();
        }

        public string GetHotelRoomDescEn(String cd)
        {
            if (cd == null)
            {
                return "";
            }
            foreach (var room in Rooms.Where(room => room.RoomCd == cd))
            {
                return room.RoomDescEn;
            }

            return "";
        }

        public string GetHotelRoomDescId(String cd)
        {
            if (cd == null)
            {
                return "";
            }
            foreach (var room in Rooms.Where(room => room.RoomCd == cd))
            {
                return room.RoomDescId;
            }

            return "";
        }

        public int GetMaxAdult(string code)
        {
            return code == null ? 0 : Rooms.Where(room => room.RoomCd == code).Select(room => room.MaxAdult).FirstOrDefault();
        }

        public int GetMaxChild(string code)
        {
            if (code == null)
            {
                return 0;
            }
            return Rooms.Where(room => room.RoomCd == code).Select(room => room.MaxChild).FirstOrDefault();
        }

        public int GetPaxCapacity(string code)
        {
            if (code == null)
            {
                return 0;
            }
            return Rooms.Where(room => room.RoomCd == code).Select(room => room.MaxPax).FirstOrDefault();
        }
        public HotelRoomType GetHotelRoomType(string code)
        {
            if (code == null)
            {
                return new HotelRoomType();
            }
            var value = new HotelRoomType();
            HotelRoomTypeDict.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelRoomTypeDescEn(String cd)
        {
            if (cd == null)
            {
                return "";
            }
            var value = new HotelRoomType();
            var found = HotelRoomTypeDict.TryGetValue(cd, out value);
            return found ? value.DescEn : "";
        }

        public string GetHotelRoomTypeDescId(String cd)
        {
            if (cd == null)
            {
                return "";
            }
            var value = new HotelRoomType();
            var found = HotelRoomTypeDict.TryGetValue(cd, out value);
            return found ? value.DescId : "";
        }

        public RoomCharacteristic GetHotelRoomCharacteristic(string code)
        {
            if (code == null)
            {
                return new RoomCharacteristic();
            }
            var value = new RoomCharacteristic();
            HotelRoomCharacteristicDict.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelRoomCharacteristicDescEn(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RoomCharacteristic();
            var found = HotelRoomCharacteristicDict.TryGetValue(code, out value);
            return found ? value.CharacteristicDescEn : "";
        }

        public string GetHotelRoomCharacteristicDescId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RoomCharacteristic();
            var found = HotelRoomCharacteristicDict.TryGetValue(code, out value);
            return found ? value.CharacteristicDescId : "";
        }

        public RateClass GetHotelRoomRateClass(string code)
        {
            if (code == null)
            {
                return new RateClass();
            }
            var value = new RateClass();
            HotelRoomRateClassDict.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelRoomRateClassId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RateClass();
            var found = HotelRoomRateClassDict.TryGetValue(code, out value);
            return found ? value.DescId : "";
        }

        public string GetHotelRoomRateClassEng(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RateClass();
            var found = HotelRoomRateClassDict.TryGetValue(code, out value);
            return found ? value.DescEn : "";
        }

        public RateType GetHotelRoomRateType(string code)
        {
            if (code == null)
            {
                return new RateType();
            }
            var value = new RateType();
            HotelRoomRateTypeDict.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelRoomRateTypeId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RateType();
            var found = HotelRoomRateTypeDict.TryGetValue(code, out value);
            return found ? value.DescId : "";
        }

        public string GetHotelRoomRateTypeEng(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new RateType();
            var found = HotelRoomRateTypeDict.TryGetValue(code, out value);
            return found ? value.DescEn : "";
        }

        public PaymentType GetHotelRoomPaymentType(string code)
        {
            if (code == null)
            {
                return new PaymentType();
            }
            var value = new PaymentType();
            HotelRoomPaymentTypeDict.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelRoomPaymentTypeId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new PaymentType();
            var found = HotelRoomPaymentTypeDict.TryGetValue(code, out value);
            return found ? value.DescId : "";
        }

        public string GetHotelRoomPaymentTypeEng(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new PaymentType();
            var found = HotelRoomPaymentTypeDict.TryGetValue(code, out value);
            return found ? value.DescEn : "";
        }

        //
        public string GetHotelCountryName(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new CountryDict();
            var found = HotelCountry.TryGetValue(code, out value);
            return found ? value.Name : "";
        }

        public string GetHotelCountryIsoCode(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new CountryDict();
            var found = HotelCountry.TryGetValue(code, out value);
            return found ? value.IsoCode : "";
        }

        public CountryDict GetHotelCountry(string code)
        {
            if (code == null)
            {
                return new CountryDict();
            }
            var value = new CountryDict();
            HotelCountry.TryGetValue(code, out value);
            return value;
        }

        public string GetCountryFromDestination(string cd)
        {
            if (cd == null)
            {
                return "";
            }
            foreach (var country in Countries.Where(country => country.Destinations.Any(d => d.Code == cd)))
            {
                return country.Code;
            }

            return "";
        }

        //GET METHODS REGARDING DESTINATION AND ZONE
        public Country GetHotelCountryFromMasterList(string countryCode)
        {
            if (countryCode == null)
            {
                return new Country();
            }
            foreach (var country in Countries.Where(country => country.Code == countryCode))
            {
                return country;
            }
            return new Country();
        }

        public Country GetCountryNameFromDict(string countryCode)
        {
            if (countryCode == null)
            {
                return new Country();
            }
            var value = new Country();
            HotelDestinationCountryDict.TryGetValue(countryCode, out value);
            return value;
        }

        public List<string> GetAllLocations()
        {
            var locations = new List<string>();

            foreach (var country in Countries)
            {
                foreach (var dest in country.Destinations)
                {
                    locations.Add(dest.Code);
                    locations.AddRange(dest.Zones.Select(zone => zone.Code));
                    if (dest.Zones != null)
                    {
                        foreach (var zone in dest.Zones)
                        {
                            if (zone.Areas != null)
                            {
                                locations.AddRange(zone.Areas.Where(x => x.Code != "").Select(x => x.Code));
                            }
                        }
                    }

                }
            }

            return locations;
        }

        public Destination GetDestinationNameFromDict(string destinationCode)
        {
            if (destinationCode == null)
            {
                return new Destination();
            }
            var value = new Destination();
            HotelDestinationDict.TryGetValue(destinationCode, out value);
            return value;
        }

        public List<Destination> GetDestinationByCountryList(string countryCd)
        {
            if (countryCd == null)
            {
                return new List<Destination>();
            }
            var result = (from destination in HotelDestinationDict
                          where destination.Value.CountryCode.Equals(countryCd)
                          select new Destination
                          {
                              Code = destination.Key,
                              Name = destination.Value.Name,
                              Zones = destination.Value.Zones,
                              CountryCode = destination.Value.CountryCode
                          }).ToList();

            return result;
        }

        public Zone GetHotelZoneFromDict(string zoneCode)
        {
            if (zoneCode == null)
            {
                return new Zone();
            }
            var value = new Zone();
            HotelDestinationZoneDict.TryGetValue(zoneCode, out value);
            return value;
        }

        public string GetZoneNameFromDict(string zoneCode)
        {
            if (zoneCode == null)
            {
                return "";
            }
            var value = new Zone();
            var found = HotelDestinationZoneDict.TryGetValue(zoneCode, out value);
            return found ? value.Name : "";
        }

        public Zone GetZoneFromHotel(int hotelCd)
        {
            var value = "";
            var found = HotelCodeAndZoneDict.TryGetValue(hotelCd, out value);
            if (!found) return new Zone();
            var zone = new Zone();
            var foundZone = HotelDestinationZoneDict.TryGetValue(value, out zone);
            return foundZone ? zone : new Zone();
        }

        public Area GetHotelAreaFromDict(string areaCode)
        {
            if (areaCode == null)
            {
                return new Area();
            }
            var value = new Area();
            HotelDestinationAreaDict.TryGetValue(areaCode, out value);
            return value;
        }

        public Area GetAreaFromHotel(int hotelCd)
        {
            var value = "";
            var found = HotelCodeAndZoneDict.TryGetValue(hotelCd, out value);
            if (!found) return new Area();
            var area = new Area();
            var foundZone = HotelDestinationAreaDict.TryGetValue(value, out area);
            return foundZone ? area : new Area();
        }

        public string GetAreaNameFromDict(string areaCode)
        {
            if (areaCode == null)
            {
                return "";
            }
            var value = new Area();
            var found = HotelDestinationAreaDict.TryGetValue(areaCode, out value);
            return found ? value.Name : "";
        }

        public Destination GetDestinationFromZone(string zoneCd)
        {
            if (zoneCd == null)
            {
                return new Destination();
            }
            var value = new Zone();
            var found = HotelDestinationZoneDict.TryGetValue(zoneCd, out value);
            if (!found) return new Destination();
            var destCd = value.DestinationCode;
            var dest = new Destination();
            var foundDest = HotelDestinationDict.TryGetValue(destCd, out dest);
            return foundDest ? dest : new Destination();
        }

        //GETTER FOR HOTEL CHAIN
        public Chain GetHotelChain(string code)
        {
            if (code == null)
            {
                return new Chain();
            }
            var value = new Chain();
            HotelChains.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelChainDesc(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Chain();
            var found = HotelChains.TryGetValue(code, out value);
            return found ? value.Description : "";
        }

        //GETTER FOR HOTEL ACCOMODATION
        public Accommodation GetHotelAccomodation(string code)
        {
            if (code == null)
            {
                return new Accommodation();
            }
            var value = new Accommodation();
            HotelAccomodations.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelAccomodationMultiDesc(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Accommodation();
            var found = HotelAccomodations.TryGetValue(code, out value);
            return found ? value.MultiDescription : "";
        }

        public string GetHotelAccomodationDescEng(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Accommodation();
            var found = HotelAccomodations.TryGetValue(code, out value);
            return found ? value.TypeNameEn : "";
        }

        public string GetHotelAccomodationDescId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Accommodation();
            var found = HotelAccomodations.TryGetValue(code, out value);
            return found ? value.TypeNameId : "";
        }

        //GETTER FOR HOTEL BOARD
        public Board GetHotelBoard(string code)
        {
            if (code == null)
            {
                return new Board();
            }
            var value = new Board();
            HotelBoards.TryGetValue(code, out value);
            return value;
        }

        public string GetHotelBoardDescEn(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Board();
            var found = HotelBoards.TryGetValue(code, out value);
            return found ? value.NameEn : "";
        }

        public string GetHotelBoardDescId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Board();
            var found = HotelBoards.TryGetValue(code, out value);
            return found ? value.NameId : "";
        }

        //GETTER FOR HOTEL CATEGORY
        public Category GetHotelCategory(string code)
        {
            if (code == null)
            {
                return new Category();
            }
            var value = new Category();
            HotelCategories.TryGetValue(code, out value);
            return value;
        }

        public int GetSimpleCodeByCategoryCode(string code)
        {
            if (code == null)
            {
                return 0;
            }
            var value = new Category();
            var found = HotelCategories.TryGetValue(code, out value);
            return found ? value.SimpleCode : 0;
        }

        public string GetHotelCategoryDescEn(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Category();
            var found = HotelCategories.TryGetValue(code, out value);
            return found ? value.NameEn : "";
        }

        public string GetHotelCategoryDescId(string code)
        {
            if (code == null)
            {
                return "";
            }
            var value = new Category();
            var found = HotelCategories.TryGetValue(code, out value);
            return found ? value.NameId : "";
        }
    }
}