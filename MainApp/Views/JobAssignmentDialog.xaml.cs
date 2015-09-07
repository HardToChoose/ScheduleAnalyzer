using GraphLogic.Entities;
using TaskPlanning.JobAssignment;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;

namespace MainApp.Views
{
    public partial class JobAssignmentDialog : Window
    {
        private class Pair : BindableBase
        {
            private int links;

            public Processor Proc { get; private set; }
            public int Links
            {
                get { return links; }
                set
                {
                    links = value;
                    OnPropertyChanged("Links");
                }
            }
            
            public Pair(KeyValuePair<Processor, int> pair)
            {
                Proc = pair.Key;
                Links = pair.Value;
            }
        }

        private List<Pair> wrapper;
        private int defaultLinkCount = 1;

        public int DefaultLinkCount
        {
            get { return defaultLinkCount; }
            set
            {
                foreach (var pair in wrapper)
                {
                    if (pair.Links == defaultLinkCount)
                        pair.Links = value;
                }
                defaultLinkCount = value;
            }
        }

        public JobAssignmentDialog()
        {
            InitializeComponent();

            DataContextChanged += delegate
            {
                var options = DataContext as PcsOptions;
                if (options != null)
                {
                    LinksTable.ItemsSource = wrapper = options.PhysicalLinks.Select(pair => new Pair(pair)).ToList();
                }
            };
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            var options = DataContext as PcsOptions;
            foreach (var pair in wrapper)
            {
                options.SetPhysicalLinks(pair.Proc, pair.Links);
            }
            DialogResult = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
