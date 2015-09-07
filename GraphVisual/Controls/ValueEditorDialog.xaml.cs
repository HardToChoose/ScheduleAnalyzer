using Entities;
using GraphLogic.Entities;
using Microsoft.Practices.Prism.Commands;

using System.Windows;
using System.Windows.Input;

namespace GraphVisual.Controls
{
    partial class ValueEditorDialog : Window
    {
        public ValueEditorDialog(ValueEditorBase editorControl, IDeepClonable value)
        {
            InitializeComponent();

            KeyDown += ValueEditorDialog_KeyDown;

            editorControl.AcceptCommand = new DelegateCommand(() => OK_ButtonClick(null, null));
            editorControl.CancelCommand = new DelegateCommand(() => Cancel_ButtonClick(null, null));

            EditorControlContainer.Content = editorControl;
            DataContext = value;
        }

        private void ValueEditorDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Cancel_ButtonClick(null, null);
        }

        private void OK_ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
