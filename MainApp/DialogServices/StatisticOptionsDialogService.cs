using MainApp.Views;
using System.Windows;

namespace MainApp.DialogServices
{
    class StatisticOptionsDialogService : IDialogService
    {
        public bool? ShowDialog(Window parent, object datacontext)
        {
            return new TestOptionsDialog
            {
                Owner = parent,
                DataContext = datacontext,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }
            .ShowDialog();
        }
    }
}
