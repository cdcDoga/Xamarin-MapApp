using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;
using MapApp.ViewModals;
using Plugin.Geolocator.Abstractions;
using MapApp.Models;
using System.Collections.ObjectModel;

namespace MapApp
{
    public partial class MainPage : ContentPage
    {
        PickerViewModal vm;
        public ObservableCollection<BusStationTime> busInfo { get; set; }

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = vm = new PickerViewModal();

            RetreiveLocationOnLoad();

            //------------------------------ Fill Data --------------------
            busInfo = new ObservableCollection<BusStationTime>();
            fillData(busInfo);
            //-------------------------------------------------------------

            btnRefresh.Clicked += BtnRefresh_Clicked;
            pckrBus.SelectedIndexChanged += PckrBus_SelectedIndexChanged;

            if (pckrBus.SelectedIndex == -1)
            {
                btnRefresh.IsEnabled = false;
            }
        }
        

        private async void PckrBus_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRefresh.IsEnabled = true;
            await RefreshLocationAndTime();
        }


        private async void BtnRefresh_Clicked(object sender, EventArgs e)
        {
            await RefreshLocationAndTime();
        }

        
        private async Task RetreiveLocationOnLoad()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 20;

                if (!locator.IsGeolocationAvailable)
                    throw new NotSupportedException("Geolocation not available");
                if (!locator.IsGeolocationEnabled)
                    throw new GeolocationException(GeolocationError.PositionUnavailable);

                var position = await locator.GetPositionAsync();
                var myPosition = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);

                lblLatLong.Text = "Konumunuz > emlem: " + position.Latitude.ToString() + "\n\t\t\t\t\t\t\t\t\tboylam: " + position.Longitude.ToString();

                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(myPosition, Distance.FromMiles(2)));
            }
            catch (Exception ex)
            {
                lblTimeLeft.Text = ex.ToString();
            }
        }


        private async Task RefreshLocationAndTime()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 20;

                if (!locator.IsGeolocationAvailable)
                    throw new NotSupportedException("Geolocation not available");
                if (!locator.IsGeolocationEnabled)
                    throw new GeolocationException(GeolocationError.PositionUnavailable);

                var position = await locator.GetPositionAsync();
                var myPosition = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);

                lblLatLong.Text = "Konumunuz > enlem: " + Math.Round(position.Latitude, 7).ToString() + "\n\t\t\t\t\t\t\t\t\tboylam: " + Math.Round(position.Longitude, 7).ToString();

                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(myPosition, Distance.FromMiles(2)));

                // ------------------------------------------------- Pins ----------------------------------------------------

                double minDistance = 10;
                string nearestStation = "Bulunamadı!";
                double myLatitude = position.Latitude;
                double myLongitude = position.Longitude;
                List<DateTime> timeScheduleOfNearest = new List<DateTime>();
                bool isAvailableToday = false;

                MyMap.Pins.Clear();
                foreach (BusStationTime item in busInfo)
                {
                    if (item.busID == pckrBus.SelectedItem.ToString())
                    {
                        var pin = new Pin
                        {
                            Type = PinType.Place,
                            Position = new Xamarin.Forms.Maps.Position(item.sLocation.latitude, item.sLocation.longitude),
                            Label = item.stationName
                        };
                        MyMap.Pins.Add(pin);

                        // -------------- Nearest Station --------------
                        double difLat = myLatitude - item.sLocation.latitude;
                        double difLong = myLongitude - item.sLocation.longitude;
                        if (minDistance > euclidean_distance(difLat, difLong))
                        {
                            minDistance = euclidean_distance(difLat, difLong);
                            nearestStation = item.stationName;
                            // ------------- Remaining Time ---------------
                            timeScheduleOfNearest = item.sTime;
                        }
                    }
                }

                // ------------------------- Remaining Time ---------------------------
                DateTime currentTime = DateTime.Now;
                DateTime endTime = toTime(23, 59);
                TimeSpan timeLeft_min = endTime - currentTime;
                TimeSpan noTime = currentTime - currentTime;

                foreach (DateTime time in timeScheduleOfNearest)
                {
                    if (time - currentTime >= noTime && time - currentTime < timeLeft_min)
                    {
                        timeLeft_min = time - currentTime;
                        isAvailableToday = true;
                    }
                }

                lblNearestStation.BorderColor = Color.FromHex("#FFE047"); //gMapYellow
                lblNearestStation.Text = "Size en yakın istasyon: " + nearestStation;

                lblTimeLeft.BorderColor = Color.FromHex("#DD4B3E"); //gMapRed
                if (isAvailableToday)
                {
                    if (timeLeft_min.Hours == 0 && timeLeft_min.Minutes == 0)
                    {
                        lblTimeLeft.Text = "OTOBÜSÜNÜZ GELDİ!";
                    }
                    else if (timeLeft_min.Hours == 0)
                    {
                        lblTimeLeft.Text = "Tahmini bekleme süreniz: " + timeLeft_min.Minutes + "dk";
                    }
                    else if (timeLeft_min.Minutes == 0)
                    {
                        lblTimeLeft.Text = "Tahmini bekleme süreniz: " + timeLeft_min.Hours + "sa";
                    }
                    else
                    {
                        lblTimeLeft.Text = "Tahmini bekleme süreniz: " + timeLeft_min.Hours + "sa : " + timeLeft_min.Minutes + "dk";
                    }
                }
                else
                {
                    lblTimeLeft.Text = "Bugün başka sefer bulunmamaktadır!";
                }
                
                // -----------------------------------------------------------------------------------------------------------

            }
            catch (Exception ex)
            {
                lblTimeLeft.Text = ex.ToString();
            }

        }


        private double euclidean_distance(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }


        private static DateTime toTime(int h, int m)
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, h, m, 0);
        }


        private ObservableCollection<BusStationTime> fillData(ObservableCollection<BusStationTime> busInfo){
            
            //------------------------------ MAVİ 4 -------------------------------
            busInfo.Add(new BusStationTime
            {
                busID = "4-Mavi",
                stationID = 1,
                stationName = "Yunusemre Kamp. Bankamatik",
                sLocation = new GPS { latitude = 39.790873, longitude = 30.501820 },
                sTime = new List<DateTime> { toTime(08, 00), toTime(08, 10), toTime(08, 30), toTime(08, 45),
                    toTime(09, 00), toTime(09, 20), toTime(09, 35), toTime(09, 55), toTime(10, 15), toTime(10, 35),
                    toTime(10, 40), toTime(10, 55), toTime(11, 15), toTime(11, 35), toTime(11, 55), toTime(12, 15),
                    toTime(12, 35), toTime(12, 55), toTime(13, 15), toTime(13, 35), toTime(13, 55), toTime(14, 15),
                    toTime(14, 35), toTime(14, 55), toTime(15, 15), toTime(15, 35), toTime(15, 55), toTime(16, 15),
                    toTime(16, 35), toTime(17, 00), toTime(17, 15), toTime(17, 40)}
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Mavi",
                stationID = 2,
                stationName = "Yunusemre Kamp. Eczacılık",
                sLocation = new GPS { latitude = 39.790263, longitude = 30.494241 },
                sTime = new List<DateTime> { toTime(08, 05), toTime(08, 15), toTime(08, 35), toTime(08, 50),
                    toTime(09, 05), toTime(09, 25), toTime(09, 40), toTime(10, 00), toTime(10, 20), toTime(10, 40),
                    toTime(10, 45), toTime(11, 00), toTime(11, 20), toTime(11, 40), toTime(12, 00), toTime(12, 20),
                    toTime(12, 40), toTime(13, 00), toTime(13, 20), toTime(13, 40), toTime(14, 00), toTime(14, 20),
                    toTime(14, 40), toTime(15, 00), toTime(15, 20), toTime(15, 40), toTime(16, 00), toTime(16, 20),
                    toTime(16, 40), toTime(17, 05), toTime(17, 20), toTime(17, 45)}
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Mavi",
                stationID = 3,
                stationName = "İlahiyat Camii",
                sLocation = new GPS { latitude = 39.796075, longitude = 30.533653 },
                sTime = new List<DateTime> { toTime(08, 10), toTime(08, 20), toTime(08, 40), toTime(08, 55),
                    toTime(09, 10), toTime(09, 30), toTime(09, 45), toTime(10, 05), toTime(10, 25), toTime(10, 45),
                    toTime(10, 50), toTime(11, 05), toTime(11, 25), toTime(11, 45), toTime(12, 05), toTime(12, 25),
                    toTime(12, 45), toTime(13, 05), toTime(13, 25), toTime(13, 45), toTime(14, 05), toTime(14, 25),
                    toTime(14, 45), toTime(15, 05), toTime(15, 25), toTime(15, 45), toTime(16, 05), toTime(16, 25),
                    toTime(16, 45), toTime(17, 10), toTime(17, 25), toTime(17, 50)}
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Mavi",
                stationID = 4,
                stationName = "İki Eylül Kamp.",
                sLocation = new GPS { latitude = 39.816877, longitude = 30.527889 },
                sTime = new List<DateTime> { toTime(08, 05), toTime(08, 15), toTime(08, 35), toTime(08, 50),
                    toTime(09, 15), toTime(09, 35), toTime(09, 50), toTime(10, 10), toTime(10, 30), toTime(10, 50),
                    toTime(10, 55), toTime(11, 10), toTime(11, 30), toTime(11, 50), toTime(12, 10), toTime(12, 30),
                    toTime(12, 50), toTime(13, 10), toTime(13, 30), toTime(13, 50), toTime(14, 10), toTime(14, 30),
                    toTime(14, 50), toTime(15, 10), toTime(15, 30), toTime(15, 50), toTime(16, 10), toTime(16, 30),
                    toTime(16, 50), toTime(17, 15), toTime(17, 30), toTime(17, 55)}
            });
            //------------------------------ KIRMIZI 4 -------------------------------
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 1,
                stationName = "Yıldız",
                sLocation = new GPS { latitude = 39.775535, longitude = 30.520868 },
                sTime = new List<DateTime> { toTime(07, 20), toTime(07, 30), toTime(07, 45), toTime(08, 00),
                    toTime(08, 05), toTime(08, 10), toTime(08, 15), toTime(08, 20), toTime(08, 25), toTime(08, 30),
                    toTime(08, 35), toTime(08, 40), toTime(08, 45), toTime(08, 50), toTime(08, 55), toTime(09, 00),
                    toTime(09, 05), toTime(09, 10), toTime(09, 15), toTime(09, 20), toTime(09, 25), toTime(09, 30),
                    toTime(09, 45), toTime(10, 00), toTime(10, 15), toTime(10, 30), toTime(10, 40), toTime(10, 45),
                    toTime(11, 05), toTime(11, 20), toTime(11, 35), toTime(11, 50), toTime(12, 05), toTime(12, 20),
                    toTime(12, 30), toTime(12, 35), toTime(12, 40), toTime(12, 45), toTime(12, 50), toTime(12, 55),
                    toTime(13, 00), toTime(13, 05), toTime(13, 10), toTime(13, 15), toTime(13, 20), toTime(13, 25),
                    toTime(13, 30), toTime(13, 40), toTime(13, 45), toTime(13, 50), toTime(13, 55), toTime(14, 05),
                    toTime(14, 20), toTime(14, 30), toTime(14, 35), toTime(14, 40), toTime(14, 45), toTime(14, 50),
                    toTime(15, 10), toTime(15, 25), toTime(15, 45), toTime(16, 05), toTime(16, 20), toTime(16, 25),
                    toTime(16, 30), toTime(16, 35), toTime(16, 40), toTime(16, 45), toTime(16, 50), toTime(16, 55),
                    toTime(17, 00), toTime(17, 05), toTime(17, 10), toTime(17, 15), toTime(17, 20), toTime(17, 25),
                    toTime(17, 30), toTime(17, 35), toTime(17, 40), toTime(17, 45), toTime(18, 00), toTime(18, 20),
                    toTime(18, 40), toTime(19, 00), toTime(19, 40), toTime(20, 20) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 2,
                stationName = "Cengiz Topel Cd.",
                sLocation = new GPS { latitude = 39.779259, longitude = 30.516205 },
                sTime = new List<DateTime> { toTime(07, 25), toTime(07, 35), toTime(07, 50), toTime(08, 05),
                    toTime(08, 10), toTime(08, 15), toTime(08, 20), toTime(08, 25), toTime(08, 30), toTime(08, 35),
                    toTime(08, 40), toTime(08, 45), toTime(08, 50), toTime(08, 55), toTime(09, 00), toTime(09, 05),
                    toTime(09, 10), toTime(09, 15), toTime(09, 20), toTime(09, 25), toTime(09, 30), toTime(09, 35),
                    toTime(09, 50), toTime(10, 05), toTime(10, 20), toTime(10, 35), toTime(10, 45), toTime(10, 50),
                    toTime(11, 10), toTime(11, 25), toTime(11, 40), toTime(11, 55), toTime(12, 10), toTime(12, 25),
                    toTime(12, 35), toTime(12, 40), toTime(12, 45), toTime(12, 50), toTime(12, 55), toTime(13, 00),
                    toTime(13, 05), toTime(13, 10), toTime(13, 10), toTime(13, 20), toTime(13, 25), toTime(13, 30),
                    toTime(13, 35), toTime(13, 45), toTime(13, 50), toTime(13, 55), toTime(14, 00), toTime(14, 10),
                    toTime(14, 25), toTime(14, 35), toTime(14, 40), toTime(14, 45), toTime(14, 50), toTime(14, 55),
                    toTime(15, 15), toTime(15, 30), toTime(15, 50), toTime(16, 10), toTime(16, 25), toTime(16, 30),
                    toTime(16, 35), toTime(16, 40), toTime(16, 45), toTime(16, 50), toTime(16, 55), toTime(17, 00),
                    toTime(17, 05), toTime(17, 10), toTime(17, 15), toTime(17, 20), toTime(17, 25), toTime(17, 30),
                    toTime(17, 35), toTime(17, 40), toTime(17, 45), toTime(17, 50), toTime(18, 05), toTime(18, 25),
                    toTime(18, 45), toTime(19, 05), toTime(19, 45), toTime(20, 25) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 3,
                stationName = "Üniversite Blv.",
                sLocation = new GPS { latitude = 39.785459, longitude = 30.509942 },
                sTime = new List<DateTime> { toTime(07, 27), toTime(07, 37), toTime(07, 52), toTime(08, 07),
                    toTime(08, 12), toTime(08, 17), toTime(08, 22), toTime(08, 27), toTime(08, 32), toTime(08, 37),
                    toTime(08, 42), toTime(08, 47), toTime(08, 52), toTime(08, 57), toTime(09, 02), toTime(09, 07),
                    toTime(09, 12), toTime(09, 17), toTime(09, 22), toTime(09, 27), toTime(09, 32), toTime(09, 37),
                    toTime(09, 52), toTime(10, 07), toTime(10, 22), toTime(10, 37), toTime(10, 47), toTime(10, 52),
                    toTime(11, 12), toTime(11, 27), toTime(11, 42), toTime(11, 57), toTime(12, 12), toTime(12, 27),
                    toTime(12, 37), toTime(12, 42), toTime(12, 47), toTime(12, 52), toTime(12, 57), toTime(13, 02),
                    toTime(13, 07), toTime(13, 12), toTime(13, 12), toTime(13, 22), toTime(13, 27), toTime(13, 32),
                    toTime(13, 37), toTime(13, 47), toTime(13, 52), toTime(13, 57), toTime(14, 02), toTime(14, 12),
                    toTime(14, 27), toTime(14, 37), toTime(14, 42), toTime(14, 47), toTime(14, 52), toTime(14, 57),
                    toTime(15, 17), toTime(15, 32), toTime(15, 52), toTime(16, 12), toTime(16, 27), toTime(16, 32),
                    toTime(16, 37), toTime(16, 42), toTime(16, 47), toTime(16, 52), toTime(16, 57), toTime(17, 02),
                    toTime(17, 07), toTime(17, 12), toTime(17, 17), toTime(17, 22), toTime(17, 27), toTime(17, 32),
                    toTime(17, 37), toTime(17, 42), toTime(17, 47), toTime(17, 52), toTime(18, 07), toTime(18, 27),
                    toTime(18, 47), toTime(19, 07), toTime(19, 47), toTime(20, 27) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 4,
                stationName = "Seylap Cd.",
                sLocation = new GPS { latitude = 39.787154, longitude = 30.510533 },
                sTime = new List<DateTime> { toTime(07, 30), toTime(07, 40), toTime(07, 55), toTime(08, 10),
                    toTime(08, 15), toTime(08, 20), toTime(08, 25), toTime(08, 30), toTime(08, 35), toTime(08, 40),
                    toTime(08, 45), toTime(08, 50), toTime(08, 55), toTime(09, 00), toTime(09, 05), toTime(09, 10),
                    toTime(09, 15), toTime(09, 20), toTime(09, 25), toTime(09, 30), toTime(09, 35), toTime(09, 40),
                    toTime(09, 55), toTime(10, 10), toTime(10, 25), toTime(10, 40), toTime(10, 50), toTime(10, 55),
                    toTime(11, 15), toTime(11, 30), toTime(11, 45), toTime(12, 00), toTime(12, 15), toTime(12, 30),
                    toTime(12, 40), toTime(12, 45), toTime(12, 50), toTime(12, 55), toTime(13, 00), toTime(13, 05),
                    toTime(13, 10), toTime(13, 15), toTime(13, 20), toTime(13, 25), toTime(13, 30), toTime(13, 35),
                    toTime(13, 40), toTime(13, 50), toTime(13, 55), toTime(14, 00), toTime(14, 05), toTime(14, 15),
                    toTime(14, 30), toTime(14, 40), toTime(14, 45), toTime(14, 50), toTime(14, 55), toTime(15, 00),
                    toTime(15, 20), toTime(15, 35), toTime(15, 55), toTime(16, 15), toTime(16, 30), toTime(16, 35),
                    toTime(16, 40), toTime(16, 45), toTime(16, 50), toTime(16, 55), toTime(17, 00), toTime(17, 05),
                    toTime(17, 10), toTime(17, 15), toTime(17, 20), toTime(17, 25), toTime(17, 30), toTime(17, 35),
                    toTime(17, 40), toTime(17, 45), toTime(17, 50), toTime(17, 55), toTime(18, 10), toTime(18, 30),
                    toTime(18, 50), toTime(19, 10), toTime(19, 50), toTime(20, 30) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 5,
                stationName = "Gaffar Okkan Cd.",
                sLocation = new GPS { latitude = 39.788160, longitude = 30.526281 },
                sTime = new List<DateTime> { toTime(07, 35), toTime(07, 45), toTime(08, 00), toTime(08, 15),
                    toTime(08, 00), toTime(08, 25), toTime(08, 30), toTime(08, 35), toTime(08, 40), toTime(08, 45),
                    toTime(08, 50), toTime(08, 55), toTime(09, 00), toTime(09, 05), toTime(09, 10), toTime(09, 15),
                    toTime(09, 20), toTime(09, 25), toTime(09, 30), toTime(09, 35), toTime(09, 40), toTime(09, 45),
                    toTime(10, 00), toTime(10, 15), toTime(10, 30), toTime(10, 45), toTime(10, 55), toTime(11, 00),
                    toTime(11, 20), toTime(11, 35), toTime(11, 50), toTime(12, 05), toTime(12, 20), toTime(12, 35),
                    toTime(12, 45), toTime(12, 50), toTime(12, 55), toTime(13, 00), toTime(13, 05), toTime(13, 10),
                    toTime(13, 15), toTime(13, 20), toTime(13, 25), toTime(13, 30), toTime(13, 35), toTime(13, 40),
                    toTime(13, 45), toTime(13, 55), toTime(14, 00), toTime(14, 05), toTime(14, 10), toTime(14, 20),
                    toTime(14, 35), toTime(14, 45), toTime(14, 50), toTime(14, 55), toTime(15, 00), toTime(15, 05),
                    toTime(15, 25), toTime(15, 40), toTime(16, 00), toTime(16, 20), toTime(16, 35), toTime(16, 40),
                    toTime(16, 45), toTime(16, 50), toTime(16, 55), toTime(17, 00), toTime(17, 05), toTime(17, 10),
                    toTime(17, 15), toTime(17, 20), toTime(17, 25), toTime(17, 30), toTime(17, 35), toTime(17, 40),
                    toTime(17, 45), toTime(17, 50), toTime(17, 55), toTime(18, 00), toTime(18, 15), toTime(18, 35),
                    toTime(18, 55), toTime(19, 15), toTime(19, 55), toTime(20, 35) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 6,
                stationName = "Gazi Yakup Satar Cd.",
                sLocation = new GPS { latitude = 39.789652, longitude = 30.531225 },
                sTime = new List<DateTime> { toTime(07, 37), toTime(07, 47), toTime(08, 02), toTime(08, 17),
                    toTime(08, 02), toTime(08, 27), toTime(08, 32), toTime(08, 37), toTime(08, 42), toTime(08, 47),
                    toTime(08, 52), toTime(08, 57), toTime(09, 02), toTime(09, 07), toTime(09, 12), toTime(09, 17),
                    toTime(09, 22), toTime(09, 27), toTime(09, 32), toTime(09, 37), toTime(09, 42), toTime(09, 47),
                    toTime(10, 02), toTime(10, 17), toTime(10, 32), toTime(10, 47), toTime(10, 57), toTime(11, 02),
                    toTime(11, 22), toTime(11, 37), toTime(11, 52), toTime(12, 07), toTime(12, 22), toTime(12, 37),
                    toTime(12, 47), toTime(12, 52), toTime(12, 57), toTime(13, 02), toTime(13, 07), toTime(13, 12),
                    toTime(13, 17), toTime(13, 22), toTime(13, 27), toTime(13, 32), toTime(13, 37), toTime(13, 42),
                    toTime(13, 47), toTime(13, 57), toTime(14, 02), toTime(14, 07), toTime(14, 12), toTime(14, 22),
                    toTime(14, 37), toTime(14, 47), toTime(14, 52), toTime(14, 57), toTime(15, 02), toTime(15, 07),
                    toTime(15, 27), toTime(15, 42), toTime(16, 02), toTime(16, 22), toTime(16, 37), toTime(16, 42),
                    toTime(16, 47), toTime(16, 52), toTime(16, 57), toTime(17, 02), toTime(17, 07), toTime(17, 12),
                    toTime(17, 17), toTime(17, 22), toTime(17, 27), toTime(17, 32), toTime(17, 37), toTime(17, 42),
                    toTime(17, 47), toTime(17, 52), toTime(17, 57), toTime(18, 02), toTime(18, 17), toTime(18, 37),
                    toTime(18, 57), toTime(19, 17), toTime(19, 57), toTime(20, 37) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 7,
                stationName = "İlahiyat Camii",
                sLocation = new GPS { latitude = 39.796075, longitude = 30.533653 },
                sTime = new List<DateTime> { toTime(07, 38), toTime(07, 48), toTime(08, 03), toTime(08, 18),
                    toTime(08, 03), toTime(08, 28), toTime(08, 33), toTime(08, 38), toTime(08, 43), toTime(08, 48),
                    toTime(08, 53), toTime(08, 58), toTime(09, 03), toTime(09, 08), toTime(09, 13), toTime(09, 18),
                    toTime(09, 23), toTime(09, 28), toTime(09, 33), toTime(09, 38), toTime(09, 43), toTime(09, 48),
                    toTime(10, 03), toTime(10, 18), toTime(10, 33), toTime(10, 48), toTime(10, 58), toTime(11, 03),
                    toTime(11, 23), toTime(11, 38), toTime(11, 53), toTime(12, 08), toTime(12, 23), toTime(12, 38),
                    toTime(12, 48), toTime(12, 53), toTime(12, 58), toTime(13, 03), toTime(13, 08), toTime(13, 13),
                    toTime(13, 18), toTime(13, 23), toTime(13, 28), toTime(13, 33), toTime(13, 38), toTime(13, 43),
                    toTime(13, 48), toTime(13, 58), toTime(14, 03), toTime(14, 08), toTime(14, 13), toTime(14, 23),
                    toTime(14, 38), toTime(14, 48), toTime(14, 53), toTime(14, 58), toTime(15, 03), toTime(15, 08),
                    toTime(15, 28), toTime(15, 43), toTime(16, 03), toTime(16, 23), toTime(16, 38), toTime(16, 43),
                    toTime(16, 48), toTime(16, 53), toTime(16, 58), toTime(17, 03), toTime(17, 08), toTime(17, 13),
                    toTime(17, 18), toTime(17, 23), toTime(17, 28), toTime(17, 33), toTime(17, 38), toTime(17, 43),
                    toTime(17, 48), toTime(17, 53), toTime(17, 58), toTime(18, 03), toTime(18, 18), toTime(18, 38),
                    toTime(18, 58), toTime(19, 18), toTime(19, 58), toTime(20, 38) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "4-Kırmızı",
                stationID = 8,
                stationName = "İki Eylül Kamp.",
                sLocation = new GPS { latitude = 39.816877, longitude = 30.527889 },
                sTime = new List<DateTime> { toTime(07, 40), toTime(07, 50), toTime(08, 05), toTime(08, 20),
                    toTime(08, 25), toTime(08, 30), toTime(08, 35), toTime(08, 40), toTime(08, 45), toTime(08, 50),
                    toTime(08, 55), toTime(09, 00), toTime(09, 05), toTime(09, 10), toTime(09, 15), toTime(09, 20),
                    toTime(09, 25), toTime(09, 30), toTime(09, 35), toTime(09, 40), toTime(09, 45), toTime(09, 50),
                    toTime(10, 05), toTime(10, 20), toTime(10, 35), toTime(10, 50), toTime(11, 00), toTime(11, 05),
                    toTime(11, 25), toTime(11, 40), toTime(11, 55), toTime(12, 10), toTime(12, 25), toTime(12, 40),
                    toTime(12, 50), toTime(12, 55), toTime(13, 00), toTime(13, 05), toTime(13, 10), toTime(13, 15),
                    toTime(13, 20), toTime(13, 25), toTime(13, 30), toTime(13, 35), toTime(13, 40), toTime(13, 45),
                    toTime(13, 50), toTime(14, 00), toTime(14, 05), toTime(14, 20), toTime(14, 15), toTime(14, 25),
                    toTime(14, 40), toTime(14, 50), toTime(14, 55), toTime(15, 00), toTime(15, 05), toTime(15, 10),
                    toTime(15, 30), toTime(15, 45), toTime(16, 05), toTime(16, 25), toTime(16, 40), toTime(16, 45),
                    toTime(16, 50), toTime(16, 55), toTime(17, 00), toTime(17, 05), toTime(17, 10), toTime(17, 15),
                    toTime(17, 20), toTime(17, 25), toTime(17, 30), toTime(17, 35), toTime(17, 40), toTime(17, 45),
                    toTime(17, 50), toTime(17, 55), toTime(18, 00), toTime(18, 05), toTime(18, 20), toTime(18, 40),
                    toTime(19, 00), toTime(19, 20), toTime(20, 00), toTime(20, 40) }
            });
            //------------------------------ SİYAH 55 -------------------------------
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 1,
                stationName = "Üniversite Evleri",
                sLocation = new GPS { latitude = 39.742806, longitude = 30.517210 },
                sTime = new List<DateTime> { toTime(07, 00), toTime(08, 00), toTime(09, 00), toTime(10, 00),
                    toTime(11, 00), toTime(12, 00), toTime(13, 00), toTime(14, 00), toTime(15, 00), toTime(16, 00),
                    toTime(17, 00), toTime(18, 00) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 2,
                stationName = "J.Er Mehmet Alp Cd.",
                sLocation = new GPS { latitude = 39.744159, longitude = 30.514481 },
                sTime = new List<DateTime> { toTime(07, 02), toTime(08, 02), toTime(09, 02), toTime(10, 02),
                    toTime(11, 02), toTime(12, 02), toTime(13, 02), toTime(14, 02), toTime(15, 02), toTime(16, 02),
                    toTime(17, 02), toTime(18, 02) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 3,
                stationName = "Ballı Sk.",
                sLocation = new GPS { latitude = 39.744572, longitude = 30.510710 },
                sTime = new List<DateTime> { toTime(07, 05), toTime(08, 05), toTime(09, 05), toTime(10, 05),
                    toTime(11, 05), toTime(12, 05), toTime(13, 05), toTime(14, 05), toTime(15, 05), toTime(16, 05),
                    toTime(17, 05), toTime(18, 05) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 4,
                stationName = "Asalet Sk.",
                sLocation = new GPS { latitude = 39.744943, longitude = 30.509594 },
                sTime = new List<DateTime> { toTime(07, 08), toTime(08, 08), toTime(09, 08), toTime(10, 08),
                    toTime(11, 08), toTime(12, 08), toTime(13, 08), toTime(14, 08), toTime(15, 08), toTime(16, 08),
                    toTime(17, 08), toTime(18, 08) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 5,
                stationName = "Gençlik Blv.",
                sLocation = new GPS { latitude = 39.748140, longitude = 30.497433 },
                sTime = new List<DateTime> { toTime(07, 10), toTime(08, 10), toTime(09, 10), toTime(10, 10),
                    toTime(11, 10), toTime(12, 10), toTime(13, 10), toTime(14, 10), toTime(15, 10), toTime(16, 10),
                    toTime(17, 10), toTime(18, 10) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 6,
                stationName = "Tıp Fak.",
                sLocation = new GPS { latitude = 39.753068, longitude = 30.493838 },
                sTime = new List<DateTime> { toTime(07, 18), toTime(08, 18), toTime(09, 18), toTime(10, 18),
                    toTime(11, 18), toTime(12, 18), toTime(13, 18), toTime(14, 18), toTime(15, 18), toTime(16, 18),
                    toTime(17, 18), toTime(18, 18) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 7,
                stationName = "Atatürk Blv.",
                sLocation = new GPS { latitude = 39.756033, longitude = 30.491756 },
                sTime = new List<DateTime> { toTime(07, 20), toTime(08, 20), toTime(09, 20), toTime(10, 20),
                    toTime(11, 20), toTime(12, 20), toTime(13, 20), toTime(14, 20), toTime(15, 20), toTime(16, 20),
                    toTime(17, 20), toTime(18, 20) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 8,
                stationName = "Kütahya Yolu",
                sLocation = new GPS { latitude = 39.763540, longitude = 30.478697 },
                sTime = new List<DateTime> { toTime(07, 23), toTime(08, 23), toTime(09, 23), toTime(10, 23),
                    toTime(11, 23), toTime(12, 23), toTime(13, 23), toTime(14, 23), toTime(15, 23), toTime(16, 23),
                    toTime(17, 23), toTime(18, 23) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 9,
                stationName = "Çevre Yolu",
                sLocation = new GPS { latitude = 39.784534, longitude = 30.492375 },
                sTime = new List<DateTime> { toTime(07, 28), toTime(08, 28), toTime(09, 28), toTime(10, 28),
                    toTime(11, 28), toTime(12, 28), toTime(13, 28), toTime(14, 28), toTime(15, 28), toTime(16, 28),
                    toTime(17, 28), toTime(18, 28) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 10,
                stationName = "İsmet İnönü Cd.",
                sLocation = new GPS { latitude = 39.785094, longitude = 30.500806 },
                sTime = new List<DateTime> { toTime(07, 30), toTime(08, 30), toTime(09, 30), toTime(10, 30),
                    toTime(11, 30), toTime(12, 30), toTime(13, 30), toTime(14, 30), toTime(15, 30), toTime(16, 30),
                    toTime(17, 30), toTime(18, 30) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 11,
                stationName = "Eti Cd.",
                sLocation = new GPS { latitude = 39.785930, longitude = 30.506444 },
                sTime = new List<DateTime> { toTime(07, 32), toTime(08, 32), toTime(09, 32), toTime(10, 32),
                    toTime(11, 32), toTime(12, 32), toTime(13, 32), toTime(14, 32), toTime(15, 32), toTime(16, 32),
                    toTime(17, 32), toTime(18, 32) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 12,
                stationName = "Seylap Cd.",
                sLocation = new GPS { latitude = 39.787134, longitude = 30.510331 },
                sTime = new List<DateTime> { toTime(07, 35), toTime(08, 35), toTime(09, 35), toTime(10, 35),
                    toTime(11, 35), toTime(12, 35), toTime(13, 35), toTime(14, 35), toTime(15, 35), toTime(16, 35),
                    toTime(17, 35), toTime(18, 35) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 13,
                stationName = "Seylap Cd. 2",
                sLocation = new GPS { latitude = 39.787612, longitude = 30.514680 },
                sTime = new List<DateTime> { toTime(07, 38), toTime(08, 38), toTime(09, 38), toTime(10, 38),
                    toTime(11, 38), toTime(12, 38), toTime(13, 38), toTime(14, 38), toTime(15, 38), toTime(16, 38),
                    toTime(17, 38), toTime(18, 38) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 14,
                stationName = "Gaffar Okan Cd.",
                sLocation = new GPS { latitude = 39.788094, longitude = 30.518633 },
                sTime = new List<DateTime> { toTime(07, 42), toTime(08, 42), toTime(09, 42), toTime(10, 42),
                    toTime(11, 42), toTime(12, 42), toTime(13, 42), toTime(14, 42), toTime(15, 42), toTime(16, 42),
                    toTime(17, 42), toTime(18, 42) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 15,
                stationName = "Gaffar Okan Cd. 2",
                sLocation = new GPS { latitude = 39.788119, longitude = 30.525240 },
                sTime = new List<DateTime> { toTime(07, 45), toTime(08, 45), toTime(09, 45), toTime(10, 45),
                    toTime(11, 45), toTime(12, 45), toTime(13, 45), toTime(14, 45), toTime(15, 45), toTime(16, 45),
                    toTime(17, 45), toTime(18, 45) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 16,
                stationName = "Gazi Yakup Satar Cd.",
                sLocation = new GPS { latitude = 39.791689, longitude = 30.532261 },
                sTime = new List<DateTime> { toTime(07, 50), toTime(08, 50), toTime(09, 50), toTime(10, 50),
                    toTime(11, 50), toTime(12, 50), toTime(13, 50), toTime(14, 50), toTime(15, 50), toTime(16, 50),
                    toTime(17, 50), toTime(18, 50) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 17,
                stationName = "İlahiyat Camii",
                sLocation = new GPS { latitude = 39.796075, longitude = 30.533653 },
                sTime = new List<DateTime> { toTime(07, 55), toTime(08, 55), toTime(09, 55), toTime(10, 55),
                    toTime(11, 55), toTime(12, 55), toTime(13, 55), toTime(14, 55), toTime(15, 55), toTime(16, 55),
                    toTime(17, 55), toTime(18, 55) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "55-Siyah",
                stationID = 18,
                stationName = "İki Eylül Kamp.",
                sLocation = new GPS { latitude = 39.816877, longitude = 30.527889 },
                sTime = new List<DateTime> { toTime(08, 00), toTime(09, 00), toTime(10, 00), toTime(11, 00),
                    toTime(12, 00), toTime(13, 00), toTime(14, 00), toTime(15, 00), toTime(16, 00), toTime(17, 00),
                    toTime(18, 00), toTime(19, 00) }
            });
            //------------------------------ SİYAH 56 -------------------------------
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 1,
                stationName = "Yenikent-Ayişanlar Sk.",
                sLocation = new GPS { latitude = 39.755143, longitude = 30.517846 },
                sTime = new List<DateTime> { toTime(08, 00), toTime(16, 50) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 2,
                stationName = "Sinan Alaağaç Cd.",
                sLocation = new GPS { latitude = 39.752652, longitude = 30.521246 },
                sTime = new List<DateTime> { toTime(08, 03), toTime(16, 53) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 3,
                stationName = "Piri Reis Cd.",
                sLocation = new GPS { latitude = 39.749645, longitude = 30.520853 },
                sTime = new List<DateTime> { toTime(08, 05), toTime(16, 55) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 4,
                stationName = "Piri Reis Cd. 2",
                sLocation = new GPS { latitude = 39.748350, longitude = 30.519522 },
                sTime = new List<DateTime> { toTime(08, 07), toTime(16, 57) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 5,
                stationName = "Mustafa Özel Blv.",
                sLocation = new GPS { latitude = 39.747220, longitude = 30.517666 },
                sTime = new List<DateTime> { toTime(08, 10), toTime(17, 00) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 6,
                stationName = "Gençlik Blv.",
                sLocation = new GPS { latitude = 39.745558, longitude = 30.500726 },
                sTime = new List<DateTime> { toTime(08, 15), toTime(17, 05) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 7,
                stationName = "Atatürk Blv.",
                sLocation = new GPS { latitude = 39.758129, longitude = 30.479955 },
                sTime = new List<DateTime> { toTime(08, 20), toTime(17, 10) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 8,
                stationName = "Kütahya Yolu",
                sLocation = new GPS { latitude = 39.776190, longitude = 30.484322 },
                sTime = new List<DateTime> { toTime(08, 25), toTime(17, 15) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 9,
                stationName = "Çevre Yolu(Yunus Emre Kamp.)",
                sLocation = new GPS { latitude = 39.789208, longitude = 30.503736 },
                sTime = new List<DateTime> { toTime(08, 30), toTime(17, 20) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 10,
                stationName = "İlahiyat Camii",
                sLocation = new GPS { latitude = 39.796075, longitude = 30.533653 },
                sTime = new List<DateTime> { toTime(08, 35), toTime(17, 25) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "56-Siyah",
                stationID = 11,
                stationName = "İki Eylül Kamp.",
                sLocation = new GPS { latitude = 39.816877, longitude = 30.527889 },
                sTime = new List<DateTime> { toTime(08, 40), toTime(17, 30) }
            });
            //------------------------------ KIRMIZI 63 -------------------------------
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 1,
                stationName = "Vadişehir Mh.",
                sLocation = new GPS { latitude = 39.740955, longitude = 30.542338 },
                sTime = new List<DateTime> { toTime(07, 20), toTime(07, 40), toTime(08, 00), toTime(08, 25),
                    toTime(08, 50), toTime(09, 20), toTime(09, 40), toTime(10, 05), toTime(10, 30), toTime(10, 55),
                    toTime(11, 20), toTime(11, 50), toTime(12, 10), toTime(12, 35), toTime(13, 00), toTime(13, 25),
                    toTime(13, 50), toTime(14, 20), toTime(14, 40), toTime(15, 05), toTime(15, 30), toTime(15, 55),
                    toTime(16, 20), toTime(16, 50), toTime(17, 10), toTime(17, 35), toTime(18, 00), toTime(18, 25),
                    toTime(18, 50), toTime(19, 20), toTime(19, 40) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 2,
                stationName = "Bamsi Sk.",
                sLocation = new GPS { latitude = 39.740662, longitude = 30.542787 },
                sTime = new List<DateTime> { toTime(07, 22), toTime(07, 42), toTime(08, 02), toTime(08, 27),
                    toTime(08, 52), toTime(09, 22), toTime(09, 42), toTime(10, 07), toTime(10, 32), toTime(10, 57),
                    toTime(11, 22), toTime(11, 52), toTime(12, 12), toTime(12, 37), toTime(13, 02), toTime(13, 27),
                    toTime(13, 52), toTime(14, 22), toTime(14, 42), toTime(15, 07), toTime(15, 32), toTime(15, 57),
                    toTime(16, 22), toTime(16, 52), toTime(17, 12), toTime(17, 37), toTime(18, 02), toTime(18, 27),
                    toTime(18, 52), toTime(19, 22), toTime(19, 42) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 3,
                stationName = "Ender Sk.",
                sLocation = new GPS { latitude = 39.737850, longitude = 30.543048 },
                sTime = new List<DateTime> { toTime(07, 24), toTime(07, 44), toTime(08, 04), toTime(08, 29),
                    toTime(08, 54), toTime(09, 24), toTime(09, 44), toTime(10, 09), toTime(10, 34), toTime(10, 59),
                    toTime(11, 24), toTime(11, 54), toTime(12, 14), toTime(12, 39), toTime(13, 04), toTime(13, 29),
                    toTime(13, 54), toTime(14, 24), toTime(14, 44), toTime(15, 09), toTime(15, 34), toTime(15, 59),
                    toTime(16, 24), toTime(16, 54), toTime(17, 14), toTime(17, 39), toTime(18, 04), toTime(18, 29),
                    toTime(18, 54), toTime(19, 24), toTime(19, 44) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 4,
                stationName = "Adalet Cd.",
                sLocation = new GPS { latitude = 39.738288, longitude = 30.541487 },
                sTime = new List<DateTime> { toTime(07, 26), toTime(07, 46), toTime(08, 06), toTime(08, 31),
                    toTime(08, 56), toTime(09, 26), toTime(09, 46), toTime(10, 11), toTime(10, 36), toTime(11, 01),
                    toTime(11, 26), toTime(11, 56), toTime(12, 16), toTime(12, 41), toTime(13, 06), toTime(13, 31),
                    toTime(13, 56), toTime(14, 26), toTime(14, 46), toTime(15, 11), toTime(15, 36), toTime(16, 01),
                    toTime(16, 26), toTime(16, 56), toTime(17, 16), toTime(17, 41), toTime(18, 06), toTime(18, 31),
                    toTime(18, 56), toTime(19, 26), toTime(19, 46) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 5,
                stationName = "Kartopu Cd.",
                sLocation = new GPS { latitude = 39.745699, longitude = 30.529713 },
                sTime = new List<DateTime> { toTime(07, 28), toTime(07, 48), toTime(08, 08), toTime(08, 33),
                    toTime(08, 58), toTime(09, 28), toTime(09, 48), toTime(10, 13), toTime(10, 38), toTime(11, 03),
                    toTime(11, 28), toTime(11, 58), toTime(12, 18), toTime(12, 43), toTime(13, 08), toTime(13, 33),
                    toTime(13, 58), toTime(14, 28), toTime(14, 48), toTime(15, 13), toTime(15, 38), toTime(16, 03),
                    toTime(16, 28), toTime(16, 58), toTime(17, 18), toTime(17, 43), toTime(18, 08), toTime(18, 33),
                    toTime(18, 58), toTime(19, 28), toTime(19, 48) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 6,
                stationName = "Mustafa Özel Cd.",
                sLocation = new GPS { latitude = 39.746096, longitude = 30.525200 },
                sTime = new List<DateTime> { toTime(07, 30), toTime(07, 50), toTime(08, 10), toTime(08, 35),
                    toTime(09, 00), toTime(09, 30), toTime(09, 50), toTime(10, 15), toTime(10, 40), toTime(11, 05),
                    toTime(11, 30), toTime(12, 00), toTime(12, 20), toTime(12, 45), toTime(13, 10), toTime(13, 35),
                    toTime(14, 00), toTime(14, 30), toTime(14, 50), toTime(15, 15), toTime(15, 40), toTime(16, 05),
                    toTime(16, 30), toTime(17, 00), toTime(17, 20), toTime(17, 45), toTime(18, 10), toTime(18, 35),
                    toTime(19, 00), toTime(19, 30), toTime(19, 50) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 7,
                stationName = "Gençlik Blv. ",
                sLocation = new GPS { latitude = 39.752352, longitude = 30.494160 },
                sTime = new List<DateTime> { toTime(07, 35), toTime(07, 55), toTime(08, 15), toTime(08, 40),
                    toTime(09, 05), toTime(09, 35), toTime(09, 55), toTime(10, 20), toTime(10, 45), toTime(11, 10),
                    toTime(11, 35), toTime(12, 05), toTime(12, 25), toTime(12, 50), toTime(13, 15), toTime(13, 40),
                    toTime(14, 05), toTime(14, 35), toTime(14, 55), toTime(15, 20), toTime(15, 45), toTime(16, 10),
                    toTime(16, 35), toTime(17, 05), toTime(17, 25), toTime(17, 50), toTime(18, 15), toTime(18, 40),
                    toTime(19, 05), toTime(19, 35), toTime(19, 55) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 8,
                stationName = "Atatürk Blv.",
                sLocation = new GPS { latitude = 39.765343, longitude = 30.521209 },
                sTime = new List<DateTime> { toTime(07, 44), toTime(08, 04), toTime(08, 24), toTime(08, 49),
                    toTime(09, 14), toTime(09, 44), toTime(10, 04), toTime(10, 29), toTime(10, 54), toTime(11, 19),
                    toTime(11, 44), toTime(12, 14), toTime(12, 34), toTime(12, 59), toTime(13, 24), toTime(13, 49),
                    toTime(14, 14), toTime(14, 44), toTime(15, 04), toTime(15, 29), toTime(15, 54), toTime(16, 19),
                    toTime(16, 44), toTime(17, 14), toTime(17, 34), toTime(17, 59), toTime(18, 24), toTime(18, 49),
                    toTime(19, 14), toTime(19, 44), toTime(20, 04) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 9,
                stationName = "Yunus Emre Cd.",
                sLocation = new GPS { latitude = 39.772659, longitude = 30.524478 },
                sTime = new List<DateTime> { toTime(07, 48), toTime(08, 08), toTime(08, 28), toTime(08, 53),
                    toTime(09, 18), toTime(09, 48), toTime(10, 08), toTime(10, 33), toTime(10, 58), toTime(11, 23),
                    toTime(11, 48), toTime(12, 18), toTime(12, 38), toTime(13, 03), toTime(13, 28), toTime(13, 53),
                    toTime(14, 18), toTime(14, 48), toTime(15, 08), toTime(15, 33), toTime(15, 58), toTime(16, 23),
                    toTime(16, 48), toTime(17, 18), toTime(17, 38), toTime(18, 03), toTime(18, 28), toTime(18, 53),
                    toTime(19, 18), toTime(19, 48), toTime(20, 08) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 10,
                stationName = "Sivrihisar 1 Cd.",
                sLocation = new GPS { latitude = 39.776353, longitude = 30.526322 },
                sTime = new List<DateTime> { toTime(07, 50), toTime(08, 10), toTime(08, 30), toTime(08, 55),
                    toTime(09, 20), toTime(09, 50), toTime(10, 10), toTime(10, 35), toTime(11, 00), toTime(11, 25),
                    toTime(11, 50), toTime(12, 20), toTime(12, 40), toTime(13, 05), toTime(13, 30), toTime(13, 55),
                    toTime(14, 20), toTime(14, 50), toTime(15, 10), toTime(15, 35), toTime(16, 00), toTime(16, 25),
                    toTime(16, 50), toTime(17, 20), toTime(17, 40), toTime(18, 05), toTime(18, 30), toTime(18, 55),
                    toTime(19, 20), toTime(19, 50), toTime(20, 10) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 11,
                stationName = "Gazi Yakup Satar Cd.",
                sLocation = new GPS { latitude = 39.788530, longitude = 30.530064 },
                sTime = new List<DateTime> { toTime(07, 57), toTime(08, 17), toTime(08, 37), toTime(09, 02),
                    toTime(09, 27), toTime(09, 57), toTime(10, 17), toTime(10, 42), toTime(11, 07), toTime(11, 32),
                    toTime(11, 57), toTime(12, 27), toTime(12, 47), toTime(13, 12), toTime(13, 37), toTime(14, 02),
                    toTime(14, 27), toTime(14, 57), toTime(15, 17), toTime(15, 42), toTime(16, 07), toTime(16, 32),
                    toTime(16, 57), toTime(17, 27), toTime(17, 47), toTime(18, 12), toTime(18, 37), toTime(19, 02),
                    toTime(19, 27), toTime(19, 57), toTime(20, 17) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 12,
                stationName = "İlahiyat Camii",
                sLocation = new GPS { latitude = 39.796075, longitude = 30.533653 },
                sTime = new List<DateTime> { toTime(08, 00), toTime(08, 20), toTime(08, 40), toTime(09, 05),
                    toTime(09, 30), toTime(10, 00), toTime(10, 20), toTime(10, 45), toTime(11, 10), toTime(11, 35),
                    toTime(12, 00), toTime(12, 30), toTime(12, 50), toTime(13, 15), toTime(13, 40), toTime(14, 05),
                    toTime(14, 30), toTime(15, 00), toTime(15, 20), toTime(15, 45), toTime(16, 10), toTime(16, 35),
                    toTime(17, 00), toTime(17, 30), toTime(17, 50), toTime(18, 15), toTime(18, 40), toTime(19, 05),
                    toTime(19, 30), toTime(20, 00), toTime(20, 20) }
            });
            busInfo.Add(new BusStationTime
            {
                busID = "63-Kırmızı",
                stationID = 13,
                stationName = "İki Eylül Kamp.",
                sLocation = new GPS { latitude = 39.816877, longitude = 30.527889 },
                sTime = new List<DateTime> { toTime(08, 05), toTime(08, 25), toTime(08, 45), toTime(09, 10),
                    toTime(09, 35), toTime(10, 05), toTime(10, 25), toTime(10, 50), toTime(11, 15), toTime(11, 40),
                    toTime(12, 05), toTime(12, 35), toTime(12, 55), toTime(13, 20), toTime(13, 45), toTime(14, 10),
                    toTime(14, 35), toTime(15, 05), toTime(15, 25), toTime(15, 50), toTime(16, 15), toTime(16, 40),
                    toTime(17, 05), toTime(17, 35), toTime(17, 55), toTime(18, 20), toTime(18, 45), toTime(19, 10),
                    toTime(19, 35), toTime(20, 05), toTime(20, 25) }
            });

            // -------------------------------------------------------------------------------------------------------

            return busInfo;
        }
    }
}
