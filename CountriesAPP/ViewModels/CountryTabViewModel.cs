namespace CountriesAPP.ViewModels
{
    using MyToolkit.Mvvm;
    using System.Windows.Controls;

    public class CountryTabViewModel: ViewModelBase
    {
        //private string name;

        public CountryTabViewModel(string tabName, UserControl newTab )
        {
            TabName = tabName;
            Window = (ShowCountryDetails)newTab;
        }

        public ShowCountryDetails Window { get; set; }

        public string TabName
        {
            get;
            private set;
        }

        //public string Name
        //{
        //    get
        //    {
        //        return name;
        //    }

        //    set
        //    {
        //        if (name != value)
        //        {
        //            name = value;
        //            RaisePropertyChanged("Name");
        //        }
        //    }
        //}
    }
}
