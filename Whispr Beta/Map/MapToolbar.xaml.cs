using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WhisprBeta
{
    public partial class MapToolbar
    {
        public delegate void FeedButtonClickedEventHandler();
        public delegate void MyLocationButtonClickedEventHandler();

        public event FeedButtonClickedEventHandler FeedButtonClicked;
        public event MyLocationButtonClickedEventHandler MyLocationButtonClicked;

        public MapToolbar()
        {
            InitializeComponent();
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (FeedButtonClicked != null)
            {
                FeedButtonClicked();
            }
        }

        private void buttonMyLocation_Click(object sender, RoutedEventArgs e)
        {
            if (FeedButtonClicked != null)
            {
                MyLocationButtonClicked();
            }
        }


    }
}
