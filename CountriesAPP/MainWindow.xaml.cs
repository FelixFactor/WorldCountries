namespace CountriesAPP
{
    using API_Models;
    using Models;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;
    using Views;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        private NetworkService networkService;
        private APIConnection apiConnection;
        private List<Country> Countries;
        CountryMainViewModel countriesTab;
        SQLService dataService;
        //SQLiteService dbService;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            countriesTab = new CountryMainViewModel();
            Countries = new List<Country>();
            dataService = new SQLService();
            //dbService = new SQLiteService();
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
                //TODO progress bar
                //check if there is internet connection
                networkService = new NetworkService();

                if (!networkService.CheckNetConnection())
                {
                    LoadFromDB();
                    LblLoadFrom.Text = $"Data loaded from local DataBase on {DateTime.UtcNow}";
                }
                else
                {
                    await LoadFromAPI();
                    LblLoadFrom.Text = $"Data loaded from API on {DateTime.UtcNow}";
                }

                //jumps from current block because there is no data loaded into Countries
                if (Countries.Count == 0)
                {
                    LblLoadFrom.Text = $"Error Loading Data. Try restarting..";
                    return;
                }

                //no errors found block goes on
                //adds the items from the list to a the dropdown list, displays the attribute Name from the list
                CbCountry.ItemsSource = Countries;
                CbCountry.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// gets the data from the local DataBase
        /// </summary>
        private async void LoadFromDB()
        {
            throw new NotImplementedException("In Development");
        }

        /// <summary>
        /// Downloads the API data 
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
                //deletes data on the DB 
                dataService.DeleteData();
                //saves new data on to DB
                dataService.SaveData(Countries);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// creates a new tab when a item from the CBox is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newTab = new ShowCountryDetails();
            try
            {
                //gets the selected item from the comboBox
                Country selected = (Country)CbCountry.SelectedItem;

                newTab.CreateControl(selected);

                //adds the selected item name(country) and the usercontrol page to the viewModel to be presented in the tab
                CountryTab tab = new CountryTab(selected.Name, newTab);
                countriesTab.AddTab(tab);

                //defines the dataContext of this page ?????? maybe put this in XAML
                DataContext = countriesTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
