//using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapApp.Models
{
    public class BusStationTime
    {
        //[PrimaryKey]
        public string busID { get; set; }
        //[PrimaryKey]
        public int stationID { get; set; }

        public string stationName { get; set; }
        public GPS sLocation { get; set; }
        public List<DateTime> sTime { get; set; }
    }
}
