using GraphLogic.Entities;
using GraphVisual.Controls;

using System.Windows.Input;

namespace UI.Controls
{
    public partial class EdgeWeightEditor : ValueEditorBase
    {
        public EdgeWeightEditor()
        {
            InitializeComponent();

            Loaded += delegate
            {
                PerformanceTextBox.Focus();
            };

            PerformanceTextBox.GotKeyboardFocus += delegate
            {
                PerformanceTextBox.SelectAll();
            };
            PerformanceTextBox.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    if (AcceptCommand != null && AcceptCommand.CanExecute(null))
                        AcceptCommand.Execute(null);
                }

                if (e.Key < Key.D0 || e.Key > Key.D9)
                {
                    e.Handled = true;
                }
            };
        }

        public override EditableValueBase GetDefaultValue(object arg)
        {
            return null;
        }
    }
}
