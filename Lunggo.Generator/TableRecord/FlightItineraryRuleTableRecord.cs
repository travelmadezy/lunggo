using System;
using System.Collections.Generic;
using System.Linq;
using Lunggo.Framework.Database;

namespace Lunggo.Repository.TableRecord
{
    public class FlightItineraryRuleTableRecord : Lunggo.Framework.Database.TableRecord
    {
		private static List<ColumnMetadata> _recordMetadata;
        private static List<ColumnMetadata> _primaryKeys;
        private static String _tableName;

		public long? Id
		{
		    get { return _Id; }
		    set
		    {
		        _Id = value;
		        IncrementLog("Id");
		    }
		}
		public int? ConstraintCount
		{
		    get { return _ConstraintCount; }
		    set
		    {
		        _ConstraintCount = value;
		        IncrementLog("ConstraintCount");
		    }
		}
		public int? Priority
		{
		    get { return _Priority; }
		    set
		    {
		        _Priority = value;
		        IncrementLog("Priority");
		    }
		}
		public String BookingDateSpans
		{
		    get { return _BookingDateSpans; }
		    set
		    {
		        _BookingDateSpans = value;
		        IncrementLog("BookingDateSpans");
		    }
		}
		public String BookingDays
		{
		    get { return _BookingDays; }
		    set
		    {
		        _BookingDays = value;
		        IncrementLog("BookingDays");
		    }
		}
		public String BookingDates
		{
		    get { return _BookingDates; }
		    set
		    {
		        _BookingDates = value;
		        IncrementLog("BookingDates");
		    }
		}
		public String FareTypes
		{
		    get { return _FareTypes; }
		    set
		    {
		        _FareTypes = value;
		        IncrementLog("FareTypes");
		    }
		}
		public String AirlineTypes
		{
		    get { return _AirlineTypes; }
		    set
		    {
		        _AirlineTypes = value;
		        IncrementLog("AirlineTypes");
		    }
		}
		public String CabinClasses
		{
		    get { return _CabinClasses; }
		    set
		    {
		        _CabinClasses = value;
		        IncrementLog("CabinClasses");
		    }
		}
		public String RequestedTripTypes
		{
		    get { return _RequestedTripTypes; }
		    set
		    {
		        _RequestedTripTypes = value;
		        IncrementLog("RequestedTripTypes");
		    }
		}
		public String DepartureDateSpans
		{
		    get { return _DepartureDateSpans; }
		    set
		    {
		        _DepartureDateSpans = value;
		        IncrementLog("DepartureDateSpans");
		    }
		}
		public String DepartureDays
		{
		    get { return _DepartureDays; }
		    set
		    {
		        _DepartureDays = value;
		        IncrementLog("DepartureDays");
		    }
		}
		public String DepartureDates
		{
		    get { return _DepartureDates; }
		    set
		    {
		        _DepartureDates = value;
		        IncrementLog("DepartureDates");
		    }
		}
		public String DepartureTimeSpans
		{
		    get { return _DepartureTimeSpans; }
		    set
		    {
		        _DepartureTimeSpans = value;
		        IncrementLog("DepartureTimeSpans");
		    }
		}
		public String ReturnDateSpans
		{
		    get { return _ReturnDateSpans; }
		    set
		    {
		        _ReturnDateSpans = value;
		        IncrementLog("ReturnDateSpans");
		    }
		}
		public String ReturnDays
		{
		    get { return _ReturnDays; }
		    set
		    {
		        _ReturnDays = value;
		        IncrementLog("ReturnDays");
		    }
		}
		public String ReturnDates
		{
		    get { return _ReturnDates; }
		    set
		    {
		        _ReturnDates = value;
		        IncrementLog("ReturnDates");
		    }
		}
		public String ReturnTimeSpans
		{
		    get { return _ReturnTimeSpans; }
		    set
		    {
		        _ReturnTimeSpans = value;
		        IncrementLog("ReturnTimeSpans");
		    }
		}
		public int? MaxPassengers
		{
		    get { return _MaxPassengers; }
		    set
		    {
		        _MaxPassengers = value;
		        IncrementLog("MaxPassengers");
		    }
		}
		public int? MinPassengers
		{
		    get { return _MinPassengers; }
		    set
		    {
		        _MinPassengers = value;
		        IncrementLog("MinPassengers");
		    }
		}
		public String Airlines
		{
		    get { return _Airlines; }
		    set
		    {
		        _Airlines = value;
		        IncrementLog("Airlines");
		    }
		}
		public Boolean? AirlinesIsExclusion
		{
		    get { return _AirlinesIsExclusion; }
		    set
		    {
		        _AirlinesIsExclusion = value;
		        IncrementLog("AirlinesIsExclusion");
		    }
		}
		public String AirportPairs
		{
		    get { return _AirportPairs; }
		    set
		    {
		        _AirportPairs = value;
		        IncrementLog("AirportPairs");
		    }
		}
		public Boolean? AirportPairsIsExclusion
		{
		    get { return _AirportPairsIsExclusion; }
		    set
		    {
		        _AirportPairsIsExclusion = value;
		        IncrementLog("AirportPairsIsExclusion");
		    }
		}
		public String CityPairs
		{
		    get { return _CityPairs; }
		    set
		    {
		        _CityPairs = value;
		        IncrementLog("CityPairs");
		    }
		}
		public Boolean? CityPairsIsExclusion
		{
		    get { return _CityPairsIsExclusion; }
		    set
		    {
		        _CityPairsIsExclusion = value;
		        IncrementLog("CityPairsIsExclusion");
		    }
		}
		public String CountryPairs
		{
		    get { return _CountryPairs; }
		    set
		    {
		        _CountryPairs = value;
		        IncrementLog("CountryPairs");
		    }
		}
		public Boolean? CountryPairsIsExclusion
		{
		    get { return _CountryPairsIsExclusion; }
		    set
		    {
		        _CountryPairsIsExclusion = value;
		        IncrementLog("CountryPairsIsExclusion");
		    }
		}
		public String InsertBy
		{
		    get { return _InsertBy; }
		    set
		    {
		        _InsertBy = value;
		        IncrementLog("InsertBy");
		    }
		}
		public DateTime? InsertDate
		{
		    get { return _InsertDate; }
		    set
		    {
		        _InsertDate = value;
		        IncrementLog("InsertDate");
		    }
		}
		public String InsertPgId
		{
		    get { return _InsertPgId; }
		    set
		    {
		        _InsertPgId = value;
		        IncrementLog("InsertPgId");
		    }
		}
		public String UpdateBy
		{
		    get { return _UpdateBy; }
		    set
		    {
		        _UpdateBy = value;
		        IncrementLog("UpdateBy");
		    }
		}
		public DateTime? UpdateDate
		{
		    get { return _UpdateDate; }
		    set
		    {
		        _UpdateDate = value;
		        IncrementLog("UpdateDate");
		    }
		}
		public String UpdatePgId
		{
		    get { return _UpdatePgId; }
		    set
		    {
		        _UpdatePgId = value;
		        IncrementLog("UpdatePgId");
		    }
		}

		
		private long? _Id;
		private int? _ConstraintCount;
		private int? _Priority;
		private String _BookingDateSpans;
		private String _BookingDays;
		private String _BookingDates;
		private String _FareTypes;
		private String _AirlineTypes;
		private String _CabinClasses;
		private String _RequestedTripTypes;
		private String _DepartureDateSpans;
		private String _DepartureDays;
		private String _DepartureDates;
		private String _DepartureTimeSpans;
		private String _ReturnDateSpans;
		private String _ReturnDays;
		private String _ReturnDates;
		private String _ReturnTimeSpans;
		private int? _MaxPassengers;
		private int? _MinPassengers;
		private String _Airlines;
		private Boolean? _AirlinesIsExclusion;
		private String _AirportPairs;
		private Boolean? _AirportPairsIsExclusion;
		private String _CityPairs;
		private Boolean? _CityPairsIsExclusion;
		private String _CountryPairs;
		private Boolean? _CountryPairsIsExclusion;
		private String _InsertBy;
		private DateTime? _InsertDate;
		private String _InsertPgId;
		private String _UpdateBy;
		private DateTime? _UpdateDate;
		private String _UpdatePgId;


		public static FlightItineraryRuleTableRecord CreateNewInstance()
        {
            var record = new FlightItineraryRuleTableRecord();
            var iRecord = record.AsInterface();
            iRecord.ManuallyCreated = true;
            return record;
        }

		public FlightItineraryRuleTableRecord()
        {
            ;
        }

        static FlightItineraryRuleTableRecord()
        {
            InitTableName();
            InitRecordMetadata();
            InitPrimaryKeysMetadata();
        }

        private static void InitTableName()
        {
            _tableName = "FlightItineraryRule";
        }

        private static void InitRecordMetadata()
        {
            _recordMetadata = new List<ColumnMetadata>
            {
				new ColumnMetadata("Id", true),
				new ColumnMetadata("ConstraintCount", false),
				new ColumnMetadata("Priority", false),
				new ColumnMetadata("BookingDateSpans", false),
				new ColumnMetadata("BookingDays", false),
				new ColumnMetadata("BookingDates", false),
				new ColumnMetadata("FareTypes", false),
				new ColumnMetadata("AirlineTypes", false),
				new ColumnMetadata("CabinClasses", false),
				new ColumnMetadata("RequestedTripTypes", false),
				new ColumnMetadata("DepartureDateSpans", false),
				new ColumnMetadata("DepartureDays", false),
				new ColumnMetadata("DepartureDates", false),
				new ColumnMetadata("DepartureTimeSpans", false),
				new ColumnMetadata("ReturnDateSpans", false),
				new ColumnMetadata("ReturnDays", false),
				new ColumnMetadata("ReturnDates", false),
				new ColumnMetadata("ReturnTimeSpans", false),
				new ColumnMetadata("MaxPassengers", false),
				new ColumnMetadata("MinPassengers", false),
				new ColumnMetadata("Airlines", false),
				new ColumnMetadata("AirlinesIsExclusion", false),
				new ColumnMetadata("AirportPairs", false),
				new ColumnMetadata("AirportPairsIsExclusion", false),
				new ColumnMetadata("CityPairs", false),
				new ColumnMetadata("CityPairsIsExclusion", false),
				new ColumnMetadata("CountryPairs", false),
				new ColumnMetadata("CountryPairsIsExclusion", false),
				new ColumnMetadata("InsertBy", false),
				new ColumnMetadata("InsertDate", false),
				new ColumnMetadata("InsertPgId", false),
				new ColumnMetadata("UpdateBy", false),
				new ColumnMetadata("UpdateDate", false),
				new ColumnMetadata("UpdatePgId", false),

            };
        }

        private static void InitPrimaryKeysMetadata()
        {
            _primaryKeys = _recordMetadata.Where(p => p.IsPrimaryKey).ToList();
        }

		public override List<ColumnMetadata> GetMetadata()
        {
            return _recordMetadata;
        }

        public override string GetTableName()
        {
            return _tableName;
        }

        public override List<ColumnMetadata> GetPrimaryKeys()
        {
            return _primaryKeys;
        }



    }
}
