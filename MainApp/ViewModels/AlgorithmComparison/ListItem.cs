using Microsoft.Practices.Prism.Mvvm;
using System;

namespace MainApp.ViewModels.AlgorithmComparison
{
    internal class ListItem<T> : BindableBase
    {
        private bool isSelected;
        private Action<T, bool> selectionChanged;

        public T Item { get; private set; }
        public string Text { get; private set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                selectionChanged(Item, value);
                OnPropertyChanged("IsSelected");
            }
        }

        public ListItem(T item, string text, Action<T, bool> selectionChangedHandler)
        {
            this.selectionChanged = selectionChangedHandler;

            Item = item;
            Text = text;
            IsSelected = false;
        }

        public void SelectUIOnly(bool selected)
        {
            isSelected = selected;
            OnPropertyChanged("IsSelected");
        }
    }
}
