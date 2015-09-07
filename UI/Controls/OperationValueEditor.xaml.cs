using GraphLogic.Entities;
using GraphVisual.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace UI.Controls
{
    public partial class OperationValueEditor : ValueEditorBase
    {
        public OperationValueEditor()
        {
            InitializeComponent();

            Loaded += OperationValueEditor_Loaded;
            RadioList.SelectionChanged += RadioList_SelectionChanged;
            
            ComplexityTextBox.KeyDown += ComplexityTextBox_KeyDown;
            ComplexityTextBox.PreviewKeyDown += ComplexityTextBox_PreviewKeyDown;
            ComplexityTextBox.GotKeyboardFocus += ComplexityTextBox_GotKeyboardFocus;
        }

        public override EditableValueBase GetDefaultValue(object arg)
        {
            return new Operation((int)arg);
        }

        #region Event handlers

        private void OperationValueEditor_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RadioList.SelectedValue = (DataContext as Operation).Type;
            ComplexityTextBox.Focus();
        }

        private void RadioList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is Operation && RadioList.SelectedIndex != -1)
            {
                (DataContext as Operation).Type = (OperationType)RadioList.SelectedValue;
            }
        }

        private void ComplexityTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ComplexityTextBox.SelectAll();
        }

        private void ComplexityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && RadioList.SelectedIndex != -1)
            {
                AcceptCommand.Execute(null);
            }
        }

        private void ComplexityTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}
