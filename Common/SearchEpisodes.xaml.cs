using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Centapp.CartoonCommon.ViewModels;

namespace Centapp.CartoonCommon
{
    public partial class SearchEpisodes : PhoneApplicationPage
    {
        public SearchEpisodes()
        {
            InitializeComponent();
            DataContext = App.ViewModel;

            //http://windowsphonegeek.com/articles/autocompletebox-for-wp7-in-depth
            autoCompleteBox.Populating += new PopulatingEventHandler(autoCompleteBox_Populating);
            autoCompleteBox.Populated += new PopulatedEventHandler(autoCompleteBox_Populated);
            autoCompleteBox.ItemsSource = App.ViewModel.Items;
            autoCompleteBox.ItemFilter += SearchTitle;
            autoCompleteBox.SelectionChanged += new SelectionChangedEventHandler(autoCompleteBox_SelectionChanged);
            Loaded += SearchEpisodes_Loaded;
            ButtonShowVideo.Visibility = App.ViewModel.SelectedEpisode != null ? Visibility.Visible : Visibility.Collapsed;
        }

        void SearchEpisodes_Loaded(object sender, RoutedEventArgs e)
        {
            autoCompleteBox.Focus();
        }

        void autoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.ViewModel.SelectedEpisode = (((Microsoft.Phone.Controls.AutoCompleteBox)(sender)).SelectedItem as ItemViewModel);
            ButtonShowVideo.Visibility = App.ViewModel.SelectedEpisode != null ? Visibility.Visible : Visibility.Collapsed;

            //hides keyboard
            Dispatcher.BeginInvoke(() => Focus());
        }

        bool SearchTitle(string search, object value)
        {
            if (value != null)
            {
                if ((value as ItemViewModel).Title.ToUpper().ToString().Contains(search.ToUpper()))
                    return true;
            }
            return false;
        }

        void autoCompleteBox_Populated(object sender, PopulatedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
            });
        }

        void autoCompleteBox_Populating(object sender, PopulatingEventArgs e)
        {
        }

        private void BorderClearButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                autoCompleteBox.Text = "";
            });
            
        }

        private void ButtonShowVideo_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GotoMainPage();
        }

        private void GotoMainPage()
        {
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                               Dispatcher,
                                                               string.Format("/MainPage.xaml?autoplay=true"));
        }
    }
}