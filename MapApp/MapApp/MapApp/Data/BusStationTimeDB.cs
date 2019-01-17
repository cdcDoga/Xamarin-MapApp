using MapApp.Models;
//using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MapApp.Data
{
    public class BusStationTimeDB
    {/*
        readonly SQLiteAsyncConnection database;

        public BusStationTimeDB(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<BusStationTime>().Wait();
        }

        public Task<List<BusStationTime>> GetEmployeesAsync()
        {
            return database.Table<BusStationTime>().ToListAsync();
        }


        public Task<BusStationTime> GetEmployeeAsync(string bus, int station)
        {
            return database.Table<BusStationTime>().Where(i => i.busID == bus && i.stationID == station).FirstOrDefaultAsync();
        }

        public Task<int> SaveEmployeeAsync(BusStationTime item)
        {
            if (item.stationID != 0 && item.busID != null)
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> DeleteEmployeeAsync(BusStationTime item)
        {
            return database.DeleteAsync(item);
        }*/
    }
}
