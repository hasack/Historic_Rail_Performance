using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using RailAppWPF.Classes;

namespace RailAppWPF
{

    public partial class MainWindow : Window
    {

        public ServiceMetricsRequest original_ask = new ServiceMetricsRequest();
        public BindingList<location_history> my_location_history;
        public static string kick_off = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
        public Dictionary<string, string> myStations = Stations.AddStations();

        public MainWindow()
        {
            InitializeComponent();
            WriteLog("Main Window loaded successfully");
            Setup();
        }

        public void Setup()
        {
            on_date.DisplayDateEnd = DateTime.Now.AddDays(-1);

            time_text.Text = "";
            status_text.Text = "Ready";

            from_location.ItemsSource = myStations.Keys;
            to_location.ItemsSource = myStations.Keys;
            from_time.ItemsSource = Times.genTimes();
            to_time.ItemsSource = Times.genTimes();

            btn_export_csv.Visibility = Visibility.Hidden;

            this.Title = $"Rail App - Session ID: {kick_off}";

        }

        public static void WriteLog(string message)
        {

            mySettings_decrypted temp = mySecurity.Pull_Settings();

            if (string.IsNullOrEmpty(temp.log_path))
            {
                temp.log_path = ($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\");
            }

            if (Directory.Exists(temp.log_path))
            {

                string path = $@"{temp.log_path}log-{kick_off}.log";

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("o")} : {message}");
                };

            }
            else
            {
                Directory.CreateDirectory(temp.log_path);

                string path = $@"{temp.log_path}log-{kick_off}.log";

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("o")} : {message}");
                };

            }

        }

        public async void Run_Search(object sender, RoutedEventArgs e)
        {

            WriteLog("Clicked Run Search");

            // Clear Existing Stopwatch & grid

            time_text.Text = null;
            data_grid.ItemsSource = null;
            btn_export_csv.Visibility = Visibility.Hidden;

            // Start new Stopwatch

            Stopwatch myStopwatch = new Stopwatch();

            myStopwatch.Start();

            // Update status to 'Running

            status_text.Text = "Running...";

            // Setup Request - Object into JSON

            ServiceMetricsRequest metricsRequest = new ServiceMetricsRequest();

            try

            {

                string str_from_location = (string.IsNullOrWhiteSpace(from_location.Text) ? throw new ArgumentException("Please set From Location") : myStations[from_location.Text]);
                metricsRequest.from_loc = str_from_location;

                string str_to_location = (string.IsNullOrWhiteSpace(to_location.Text) ? throw new ArgumentException("Please set To Location") : myStations[to_location.Text]);
                metricsRequest.to_loc = str_to_location;

                DateTime parsedDate = (string.IsNullOrEmpty(on_date.Text)) ? throw new ArgumentException("Please set the On Date") : DateTime.Parse(on_date.Text);
                string str_on_date = parsedDate.ToString("yyyy-MM-dd");
                metricsRequest.from_date = str_on_date;
                metricsRequest.to_date = str_on_date;

                string str_from_time = (string.IsNullOrWhiteSpace(from_time.Text) ? throw new ArgumentException("Please set From Time") : from_time.Text);
                metricsRequest.from_time = str_from_time;

                string str_to_time = (string.IsNullOrWhiteSpace(to_time.Text) ? throw new ArgumentException("Please set To Time") : to_time.Text);
                metricsRequest.to_time = str_to_time;

                string day_of_week = parsedDate.ToString("ddd");

                string[] weekdays = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri" };

                string on_day = string.Empty;

                if (weekdays.Contains(day_of_week))
                {
                    on_day = "WEEKDAY";
                }
                else if (day_of_week == "Sat")
                {
                    on_day = "SATURDAY";

                }
                else if (day_of_week == "Sun")
                {

                    on_day = "SUNDAY";
                }

                metricsRequest.days = on_day;

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                status_text.Text = "Ready";
                return;

            }

            original_ask = metricsRequest;

            WriteLog("Successfully set up request");

            // Start HTTP Request

            HttpClientHandler handler = new HttpClientHandler();
            handler.UseProxy = false;

            HttpClient myClient = new(handler)
            {

                BaseAddress = new Uri("https://hsp-prod.rockshore.net")
            };

            mySettings_decrypted temp = mySecurity.Pull_Settings();

            string for_encoding = $"{temp.Stored_api_username}:{temp.Stored_api_password}";

            byte[] for_encoding_split = Encoding.UTF8.GetBytes(for_encoding);

            string str = Convert.ToBase64String(for_encoding_split);

            temp = null;

            myClient.DefaultRequestHeaders.Add("Authorization", $"Basic {str}");

            string urlServiceMetrics = "/api/v1/serviceMetrics";
            string urlServiceDetails = "/api/v1/serviceDetails";

            try
            {

                WriteLog("Issuing services request");

                HttpResponseMessage metricsResponseMessage = await myClient.PostAsJsonAsync(urlServiceMetrics, metricsRequest);

                string response_message = await metricsResponseMessage.Content.ReadAsStringAsync();

                WriteLog("Got response from server");

                myListOfServices? ServicesObj = new myListOfServices();

                ServicesObj = JsonSerializer.Deserialize<myListOfServices>(response_message);

                var rids = new List<String>();

                foreach (Service service in ServicesObj.Services)
                {
                    foreach (string rid in service.serviceAttributesMetrics.rids)
                    {
                        rids.Add(rid);
                    }
                }
                
                WriteLog($"Returned {rids.Count} RID(s)");

                my_location_history = new BindingList<location_history>();



                List<Task<HttpResponseMessage>> tasklist = new();

                int num = 0;

                foreach (string single_rid in rids)
                {
                    ServiceDetailsRequest newDetailsRequest = new ServiceDetailsRequest { rid = single_rid };

                    Task<HttpResponseMessage> response = myClient.PostAsJsonAsync(urlServiceDetails, newDetailsRequest);

                    tasklist.Add(response);

                    WriteLog($"Sent data request for {single_rid}");

                    num++;

                }

                WriteLog($"Request(s) sucessfully sent for total of {num} RID(s)");

                await Task.WhenAll(tasklist);

                WriteLog($"Data received for {num} RID(s)");

                List<Task<String>> stringlist = new();

                foreach (Task<HttpResponseMessage> task in tasklist)
                {
                    Task<string> stringResponse = task.Result.Content.ReadAsStringAsync();

                    stringlist.Add(stringResponse);

                }

                await Task.WhenAll(stringlist);

                foreach (Task<string> stringResponse in stringlist)
                {
                    if (stringResponse.Result.Contains("503 Service Temporarily Unavailable"))
                    {

                        WriteLog("Received 503 Service Temporarily Unavailable response - try again later");
                        WriteLog(stringResponse.Result);
                        myClient.Dispose();
                        status_text.Text = "Error, try again! See log file for more details";
                    }
                    else
                    {

                    myService? serviceResultsObj = JsonSerializer.Deserialize<myService>(stringResponse.Result);

                    foreach (Location location in serviceResultsObj?.serviceAttributesDetails.locations)
                    {

                        if (location.location == metricsRequest.from_loc || location.location == metricsRequest.to_loc)
                        {
                            my_location_history.Add(new location_history { Loc = location.location, ptd = location.gbtt_ptd, actual_td = location.actual_td, pta = location.gbtt_pta, actual_ta = location.actual_ta });
                        }

                    }

                }

                }

                data_grid.ItemsSource = my_location_history;

                status_text.Text = "Done";

                btn_export_csv.Visibility = Visibility.Visible;
            }
            catch (HttpRequestException ex)
            {

                switch (ex.StatusCode)
                {

                    case HttpStatusCode.BadRequest:

                        MessageBox.Show("Badly formed request, try again", "Request Failed - 400: Bad Request");
                        break;

                    default:
                        MessageBox.Show(ex.Message, "HTTP Call Failed");
                        status_text.Text = "Error, try again!";
                        break;
                }

                WriteLog($"Error {ex.Message}");

                myClient.Dispose();

            }

            myStopwatch.Stop();
            TimeSpan ts = myStopwatch.Elapsed;

            time_text.Text = ("Time taken: " + ts.ToString("hh\\:mm\\:ss"));

            WriteLog($"Total time elapsed is: {ts.ToString("hh\\:mm\\:ss")}");

            myStopwatch.Reset();

        }

        private void Data_grid_autogeneratingcolumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();

            if (headername == "Loc")
            {
                e.Column.Header = "Station";
            }
            else if (headername == "ptd")
            {
                e.Column.Header = "Published Departure";
            }
            else if (headername == "actual_td")
            {
                e.Column.Header = "Actual Departure";
            }
            else if (headername == "pta")
            {
                e.Column.Header = "Published Arrival";
            }
            else if (headername == "actual_ta")
            {
                e.Column.Header = "Actual Arrival";
            }
        }

        private void btn_yesterday_Click(object sender, RoutedEventArgs e)
        {
            on_date.SelectedDate = DateTime.Now.AddDays(-1);
        }

        private void btn_morning_Click(object sender, RoutedEventArgs e)
        {
            from_time.Text = "0600";
            to_time.Text = "0800";
        }

        private void btn_evening_Click(object sender, RoutedEventArgs e)
        {
            from_time.Text = "1700";
            to_time.Text = "2100";
        }

        private void menu_preferences_Click(object sender, RoutedEventArgs e)
        {

            Preferences myPrefWindow = new Preferences();
            myPrefWindow.Show();
        }

        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_export_csv_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                mySettings_decrypted temp = mySecurity.Pull_Settings();

                if (string.IsNullOrEmpty(temp.export_path))
                {
                    temp.export_path = ($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\");
                }

                string outputlocation = $@"{temp.export_path}{original_ask.from_loc}-{original_ask.to_loc}-{original_ask.from_date}-{original_ask.from_time}-{original_ask.to_time}.csv";

                if (Directory.Exists(temp.export_path))
                {

                    StreamWriter outputfile = new StreamWriter(outputlocation);

                    outputfile.WriteLine($"From location: {original_ask.from_loc}");
                    outputfile.WriteLine($"To location: {original_ask.to_loc}");
                    outputfile.WriteLine($"From time: {original_ask.from_time}");
                    outputfile.WriteLine($"To time: {original_ask.to_time}");
                    outputfile.WriteLine($"Date: {original_ask.from_date}");

                    outputfile.WriteLine();

                    outputfile.WriteLine("Station,Published Departure,Actual Departure,Published Arrival,Actual Arrival");

                    foreach (location_history loc in my_location_history)
                    {

                        var row = string.Format("{0},{1},{2},{3},{4}", loc.Loc, loc.ptd, loc.actual_td, loc.pta, loc.actual_ta);
                        outputfile.WriteLine(row);
                    }

                    outputfile.Dispose();

                }
                else
                {

                    Directory.CreateDirectory(temp.export_path);

                    StreamWriter outputfile = new StreamWriter(outputlocation);

                    outputfile.WriteLine($"From location: {original_ask.from_loc}");
                    outputfile.WriteLine($"To location: {original_ask.to_loc}");
                    outputfile.WriteLine($"From time: {original_ask.from_time}");
                    outputfile.WriteLine($"To time: {original_ask.to_time}");
                    outputfile.WriteLine($"Date: {original_ask.from_date}");

                    outputfile.WriteLine();

                    outputfile.WriteLine("Station,Published Departure,Actual Departure,Published Arrival,Actual Arrival");

                    foreach (location_history loc in my_location_history)
                    {

                        var row = string.Format("{0},{1},{2},{3},{4}", loc.Loc, loc.ptd, loc.actual_td, loc.pta, loc.actual_ta);
                        outputfile.WriteLine(row);
                    }

                    outputfile.Dispose();

                    temp = null;

                }

                status_text.Text = $"CSV Export Complete";


            }
            catch (UnauthorizedAccessException)
            {

                MessageBox.Show("Unable to write to file");
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.ToString());
                throw;
            }


        }

        private void CloseAll(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }

}
