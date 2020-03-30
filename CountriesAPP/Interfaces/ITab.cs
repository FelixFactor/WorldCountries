namespace CountriesAPP.Interfaces
{
    using System;
    using System.Windows.Input;

    public interface ITab
    {
        string TabName { get; set; }
        ICommand CloseCommand { get; }
        event EventHandler CloseRequest;
    }
}
