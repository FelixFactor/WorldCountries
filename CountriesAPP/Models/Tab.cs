namespace CountriesAPP.Models
{
    using Interfaces;
    using Microsoft.Expression.Interactivity.Core;
    using System;
    using System.Windows.Input;

    public abstract class Tab : ITab
    {
        public Tab()
        {
            CloseCommand = new ActionCommand(p => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        public string TabName { get; set; }

        public ICommand CloseCommand { get; }

        public event EventHandler CloseRequest;
    }
}
