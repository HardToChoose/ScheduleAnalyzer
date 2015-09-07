using MainApp.ViewModels;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

using System.Linq;
using System.Windows;
using System.Windows.Interactivity;

namespace MainApp.Views
{
    public class ShowFloatingWindowAction : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty DialogOwnerProperty =
            DependencyProperty.Register("DialogOwner", typeof(Window), typeof(ShowDialogAction), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogProperty =
            DependencyProperty.Register("Dialog", typeof(Window), typeof(ShowDialogAction), new PropertyMetadata(null));

        public Window DialogOwner
        {
            get { return (Window)GetValue(DialogOwnerProperty); }
            set { SetValue(DialogOwnerProperty, value); }
        }

        public Window Dialog
        {
            get { return (Window)GetValue(DialogProperty); }
            set { SetValue(DialogProperty, value); }
        }

        protected override void Invoke(object arg)
        {
            var request = arg as InteractionRequestedEventArgs;

            if (request != null && request.Context != null && Dialog != null)
            {
                var confirmation = request.Context as Confirmation;

                Dialog.Closing += (s, e) =>
                {
                    confirmation.Confirmed = (Dialog.DialogResult == true);
                    Dialog.Visibility = Visibility.Hidden;
                    e.Cancel = true;
                };

                Dialog.Owner = DialogOwner;
                Dialog.DataContext = request.Context.Content;

                Dialog.Visibility = Visibility.Visible;
                Dialog.ShowDialog();
            }
        }
    }
}
