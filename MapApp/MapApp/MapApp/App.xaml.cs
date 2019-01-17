using MapApp.Data;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MapApp
{
    public partial class App : Application
    {
        //static BusStationTimeDB database;

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
        /*
        public static BusStationTimeDB Database
        {
            get
            {
                if (database == null)
                {
                    database = new BusStationTimeDB(DependencyService.Get<ISQLite>().GetLocalFilePath("BusStationTimeDb.db3"));


                }
                return database;
            }
        }*/

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
