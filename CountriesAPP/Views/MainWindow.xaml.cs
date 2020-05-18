namespace CountriesAPP
{
    using Services;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using ViewModels;
    using Views;
    using CountriesAPP.Models;
    using CountriesAPP.Models.API_Models;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        private NetworkService networkService;
        private APIConnection apiConnection;
        private List<Country> Countries;
        private CountryMainViewModel countriesTab;
        private SQLService dataService;
        private bool network;
        private bool firstRun;
        #endregion

        public MainWindow()
        {
            #region Init Attributes
            countriesTab = new CountryMainViewModel();
            Countries = new List<Country>();
            dataService = new SQLService();
            network = true;
            firstRun = true;
            #endregion

            InitializeComponent();

            StartClock();

            DataContext = countriesTab;

            HelpPageLoad();

            LoadContent();
        }

        /// <summary>
        /// Loads the content needed for the APP
        /// </summary>
        /// <remarks>
        /// Can get the data from an API 
        /// or the local DataBase, if exists!
        /// </remarks> 
        private async void LoadContent()
        {
            try
            {
                LoadingBar.Value = 0;

                //check if there is internet connection
                networkService = new NetworkService();
                //TODO true for testing
                if (networkService.CheckNetConnection())
                {
                    LoadFromDB();
                    LblLoadFrom.Text = $"Data loaded from local DataBase on {DateTime.Now}";
                    network = false;
                }
                else
                {
                    await LoadFromAPI();
                    LblLoadFrom.Text = $"Data loaded from API on {DateTime.Now}";
                }

                //jumps from current block because there is no data loaded into Countries
                //or some save error happened
                if (Countries.Count == 0 || Countries.Count < 250)
                {
                    LblLoadFrom.Text = $"Couldn't connect to the API or the Database.";
                    LblLoadSave.Text = "Error loading...";
                    ProgressText.Text = "N/A";
                    return;
                }

                //adds the items from the list to a the dropdown list, displays the attribute Name from the list
                CbCountry.ItemsSource = Countries;
                CbCountry.DisplayMemberPath = "Name";

                LoadingBar.Value = 100;

                //when connected to the internet saves/updates data in DB
                if (network)
                {
                    await CheckLastUpdate();
                }

                //delaying the Data Saved lbl
                await Task.Delay(5000);

                LblLoadSave.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// limits the data update of the DB(once every 24h)
        /// </summary>
        /// <returns></returns>
        private async Task CheckLastUpdate()
        {
            string path = $"Data/config.sys";
            //1st run
            if (!File.Exists(path))
            {
                await SaveDataToSql(path);

                return;
            }

            DateTime lastUpdate = ReadConfig(path);

            if (DateTime.Now.Subtract(lastUpdate).TotalHours > 24)
            {
                await SaveDataToSql(path);
            }
        }

        /// <summary>
        /// opens and reads the config.sys file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>last DateTime DB updated</returns>
        private DateTime ReadConfig(string path)
        {
            StreamReader reader = new StreamReader(path);

            DateTime lastUpdate = DateTime.Parse(reader.ReadLine());

            firstRun = false;

            reader.Close();

            return lastUpdate;
        }

        /// <summary>
        /// saves API data to a local SQLite DB
        /// deletes all tables 1st to avoid data duplication
        /// </summary>
        /// <param name="countries"></param>
        /// <returns></returns>
        private async Task SaveDataToSql(string path)
        {
            try
            {
                await Task.Delay(3000);
                //deletes data on the DB 
                dataService.DeleteData();

                Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
                progress.ProgressChanged += UpdateProgress;

                LblLoadSave.Visibility = Visibility.Visible;
                LblLoadSave.Text = "Saving to DataBase";

                //saves new data on to DB               
                await dataService.SaveData(Countries, progress, firstRun);

                File.WriteAllText(path, DateTime.Now.ToString("dd-MM-yyyy HH:mm"));

                LblLoadSave.Text = "Database updated";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// kicks the event that progresses the bar!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateProgress(object sender, ProgressReportModel e)
        {
            LoadingBar.Value = e.CompletedPercent;
            LblLoadSave.Text = $"{e.ItemName}";
        }

        /// <summary>
        /// loads the data from the local DataBase
        /// </summary>
        private void LoadFromDB()
        {
            Countries = dataService.GetData();
        }

        /// <summary>
        /// Downloads the API data to a List 
        /// </summary>
        /// <returns></returns>
        private async Task LoadFromAPI()
        {
            try
            {
                apiConnection = new APIConnection();

                //gets data from the API
                var response = await apiConnection.GetApiData("http://restcountries.eu", "/rest/v2/all");

                //if data is unsuccessfull for some reason(internet connection problems/api down)
                if (!response.Success)
                {
                    throw new Exception(response.Answer);
                }
                //loads the result on to the Countries list 
                Countries = (List<Country>)response.Result;

                response = await apiConnection.GetRates();

                CurrencyConverter.Rates = (List<Rate>)response.Result;

                CurrencyConverter.AddEuro();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// creates a new tab when a item from the CBox is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country selected;
            try
            {
                selected = (Country)CbCountry.SelectedItem;

                if (!network)
                {
                    selected = dataService.QueryCountry(selected.Alpha3Code);
                }

                //creates the UserControl
                var newTab = new ShowCountryDetails(selected);

                //adds the selected item name(country) and the usercontrol page to the viewModel to be presented in the tab
                CountryTab tab = new CountryTab(selected.Name, newTab);

                CountryMainViewModel.AddTab(tab);

                CountryTabs.SelectedItem = tab;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Works like a swiss clock
        /// </summary>
        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TickEvent;
            timer.Start();
        }

        private void TickEvent(object sender, EventArgs e)
        {
            LblClock.Text = DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpPageLoad();
        }

        /// <summary>
        /// creates an help page and displays it
        /// </summary>
        private void HelpPageLoad()
        {
            string name = "Help";

            HelpPage page = new HelpPage();

            HelpTab tab = new HelpTab(name, page);

            CountryMainViewModel.AddTab(tab);

            CountryTabs.SelectedItem = tab;
        }

        /// <summary>
        /// Exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            string name = "About";

            About page = new About();

            AboutTab tab = new AboutTab(name, page);

            CountryMainViewModel.AddTab(tab);

            CountryTabs.SelectedItem = tab;
        }
    }
}
