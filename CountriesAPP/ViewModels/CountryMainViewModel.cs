namespace CountriesAPP.ViewModels
{
    using MyToolkit.Mvvm;
    using System.Collections.ObjectModel;

    class CountryMainViewModel : ViewModelBase
    {
        public ObservableCollection<CountryTabViewModel> Countries { get; } = new ObservableCollection<CountryTabViewModel>();
    }
}
