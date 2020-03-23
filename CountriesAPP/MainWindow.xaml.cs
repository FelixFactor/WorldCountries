namespace CountriesAPP
{
    using API_Models;
    using Models;
    using Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;
    using System;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        private NetworkService networkService;
        private APIConnection apiConnection;
        private List<Country> Countries;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //testing phase
            //var countriestabs = new CountryMainViewModel();
            //countriestabs.Countries.Add(new CountryTabViewModel("Portugal"));

            //this.DataContext = countriestabs;

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
            //check if there is internet connection
            networkService = new NetworkService();

            if (!networkService.CheckNetConnection())
            {
                LoadFromDB();
            }
            else
            {
                await LoadFromAPI();
            }

            //jumps from current block because there is no data loaded into
            if (Countries.Count == 0)
            {
                return;
            }

            //no errors found block goes on
            CbCountry.ItemsSource = Countries;
            CbCountry.DisplayMemberPath = "Name";
        }

        /// <summary>
        /// gets the data from the local DataBase
        /// </summary>
        private async void LoadFromDB()
        {
            //SQL code goes here
        }

        /// <summary>
        /// Downloads the API data 
        /// </summary>
        /// <returns></returns>
        private async Task LoadFromAPI()
        {
            apiConnection = new APIConnection();

            //gets data from the API
            var response = await apiConnection.GetApiData("http://restcountries.eu", "/rest/v2/all");
            
            Countries = (List<Country>)response.Result;

            //deletes existing data from the DB
            //something here.....
            //saves the new data in the DB
            //something here ......
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
            var countriesTab = new CountryMainViewModel();
            var newTab = new ShowCountryDetails();
            try
            {
                //gets the selected item from the comboBox
                Country selected = (Country)CbCountry.SelectedItem;
                //adds the selected item name(country) to the viewModel to be presented in tabName
                countriesTab.Countries.Add(new CountryTabViewModel(selected.Name, newTab));
                //defines the dataContext of this page ??????
                DataContext = countriesTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
