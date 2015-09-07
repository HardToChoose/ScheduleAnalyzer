using GraphLogic.Entities;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphVisual.Controls
{
    public abstract class ValueEditorBase : ContentControl
    {
        static ValueEditorBase()
        {
            AcceptCommandProperty = DependencyProperty.Register("AcceptCommand", typeof(ICommand), typeof(ValueEditorBase), new PropertyMetadata(new DelegateCommand(delegate {} )));
            CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(ValueEditorBase), new PropertyMetadata(new DelegateCommand(delegate {} )));
        }

        public static readonly DependencyProperty AcceptCommandProperty;
        public static readonly DependencyProperty CancelCommandProperty;

        public ICommand AcceptCommand
        {
            get { return (ICommand)GetValue(AcceptCommandProperty); }
            set { SetValue(AcceptCommandProperty, value); }
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public abstract EditableValueBase GetDefaultValue(object arg);
    }
}
