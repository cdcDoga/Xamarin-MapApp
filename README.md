# Xamarin-MapApp
Bus station and remaining time map application in Eskişehir with Xamarin.Forms

“Catch a Bus” App

It’s basically an information application with one page. It’s using the current location of the user. The goals are:
o	Showing all the stations of the selected bus in the map
o	Telling to the user the nearest station according to the current location
o	The remaining time for the nearest station


Why Do We Need It?

Eskişehir is a student city as everybody knows and it has three big universities.  One of them is Eskişehir Technical University and this university is so far from the city center. So students must use a vehicle to come to the university.  The buses are useful for going to school but there are some problems with buses. Because there are so many students who want to come to school especially in the mornings and also there are few buses which are small. So students want to know which bus when will come to the station but normally this is an uncertain situation. Because of that, we did this application.
Similar Current Application
In this section, we try and observe three of the current most used apps in Google Play:
Otobüs Saatleri => is an app for 13 cities in Turkey. As same as "http://www.eskisehir.bel.tr/", only the first stations' departure times are shown. So for middle stations, the exact departure times are not known.
Otobüs Nerede => is an app for 16 cities in Turkey. It is compulsory to select the city at first and can't change it after registration. For the selected city it shows the busses that passed from a specific station and remaining times for those busses arrive. But all this information is shown as list views and tables. So there is no map visualization.
Moovit => is an app for 7 countries. It's the most successful of these three. When the destination point is entered, it shows the possible ways to reach that destination point including walking, bus, subways etc. If you click the start button it's also coming all the way with you. But it doesn't show all the stations of the bus.


Future Expectations

In the current xamarin project, we use a model and ObservableCollection<> structures for adding data because we have a little data and limited time. In the future version of this project, we are planning to use an SQLite database for Storin data, and also an admin page for adding/updating the routes. 
In the current project, we are using the departure times of each station of each bus. We are using these data for telling the user the remaining time of the bus in a specific station. Instead of time data, using GPS data would be more efficient.
