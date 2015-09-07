using System.Windows;

namespace MainApp.DialogServices
{
    interface IDialogService
    {
        bool? ShowDialog(Window parent, object datacontext);
    }
}
