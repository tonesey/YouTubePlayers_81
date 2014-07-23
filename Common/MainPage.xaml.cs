﻿using System;
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
using MyToolkit.Multimedia;
using Centapp.CartoonCommon;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Resources;
using System.Globalization;
using System.Threading;
using Centapp.CartoonCommon.Converters;
#if NOINTERNET
#else
using Microsoft.Phone.Net.NetworkInformation;
#endif
using System.Windows.Media.Imaging;
using System.Collections;
using System.Reflection;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Centapp.CartoonCommon.ViewModels;
using Centapp.CartoonCommon.Helpers;
using Centapp.CartoonCommon;
using Microsoft.Phone.Info;
using Windows.Storage;
using System.Threading.Tasks;
using Wp81Shared.Helpers;
using Wp81Shared.Exceptions;
using Telerik.Windows.Controls;
using Centapp.CartoonCommon.Controls;


namespace Centapp.CartoonCommon
{

    public enum AppFunctionToLimit
    {
        EpisodeCount,
        BackupEpisodes
    }

    public enum AdvProvider
    {
        Undefined,
        Sooma,
        PubCenter,
        MyAppPromotion
    }

    public delegate void PopupClosedEventHandler();
    public delegate void PopupCancelPressedActionToExecute();

    public partial class MainPage : PhoneApplicationPage
    {
        string _appVer = string.Empty;
        IsolatedStorageFile _curIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

        #region settings
        IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;
        string _usages_setting = "usages_count";
        string _curDate_setting = "curDate";
        #endregion

        private object _currentContextItem = null;
        private bool _userMessageShown = false;


        // Constructor
        public MainPage()
        {

#if DEBUG
            //MigrateTranslationToWeb();
#endif
            InitializeComponent();

            //var test = Wp81Shared.Helpers.AppInfosHelper.GetId();
            CultureInfo cc, cuic;
            cc = Thread.CurrentThread.CurrentCulture;
            cuic = Thread.CurrentThread.CurrentUICulture;

            if (cc.TwoLetterISOLanguageName == "it") {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("it");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("it");
            }


            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            App.ViewModel.OnError -= ViewModel_OnError;
            App.ViewModel.OnError += ViewModel_OnError;

            App.ViewModel.OnUserMessageRequired -= ViewModel_OnUserMessageRequired;
            App.ViewModel.OnUserMessageRequired += ViewModel_OnUserMessageRequired;

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            firstList.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(itemsList_Tap);
            secondList.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(itemsList_Tap);
            thirdList.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(itemsList_Tap);
            fourthList.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(itemsList_Tap);

            favoritesList.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(itemsList_Tap);

            _appVer = GenericHelper.GetAppversion();

            SetCaptions();
            AddContextMenus();

            PanoramaMainControl.SelectionChanged += new EventHandler<SelectionChangedEventArgs>(PanoramaMainControl_SelectionChanged);

            CheckOtherAppsPanoramaItem();

            App.ViewModel.Logger.Reset();

            if (!string.IsNullOrEmpty(AppInfo.Instance.CustomFirstPivotItemName))
            {
                TextBlockFirstPivotItem.Text = AppInfo.Instance.CustomFirstPivotItemName;
            }

#if DEBUG
            //MessageBox.Show("versione BETA : " + GenericHelper.GetAppversion());
#endif

            App.ViewModel.OnLoadCompleted -= new OnLoadCompletedHandler(ViewModel_OnLoadCompleted);
            App.ViewModel.OnLoadCompleted += new OnLoadCompletedHandler(ViewModel_OnLoadCompleted);
        }

        void ViewModel_OnError(string msg, bool isFatalError)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg);
                if (isFatalError)
                {
                    throw new ForcedExitException("service error: " + msg);
                }
            });
        }

        public void ShareApp()
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = "Check this app!";
            shareLinkTask.LinkUri = new Uri(string.Format("http://www.windowsphone.com/s?appid={0}", AppInfosHelper.GetId()), UriKind.Absolute);
            shareLinkTask.Show();
        }

        void ViewModel_OnUserMessageRequired(string msg, bool mustExitApp)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (!_userMessageShown && !mustExitApp)
                {
                    MessageBox.Show(msg);
                    _userMessageShown = true;
                }
                if (mustExitApp)
                {
                    throw new ForcedExitException("service error: " + msg);
                }
            });
        }

        void PanoramaMainControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SetCaptions()
        {
            //noFavoritestextBlock.Text = "No favorites episodes set.\n\nTap and hold an episode to add it here.\n\nTap and hold an episode in this list (if present) to remove it from favorites";
            noFavoritestextBlock.Text = AppResources.favText;

            favoritesHeader.Text = AppResources.favoritesHeader;
            appHeader.Text = AppResources.appHeader;

            //buyAppButtonText.Text = AppResources.buyAppButton;
            rateAppButtonText.Text = AppResources.rateAppButton;
            writeAnEmailButtonText.Text = AppResources.writeAnEmailButton;
            //otherAppsButtonText.Text = AppResources.otherAppsButton;
            infoButtonText.Text = AppResources.infoButtonText;
            backupEpisodesText.Text = AppResources.backupEpisodesText;
            otherAppsPanoramaHeader.Text = AppResources.otherAppsButton;

            firstListTxtNoInternet.Text = AppResources.noNetworkAvailable;
            secondListTxtNoInternet.Text = AppResources.noNetworkAvailable;
            thirdListTxtNoInternet.Text = AppResources.noNetworkAvailable;
            fourthListTxtNoInternet.Text = AppResources.noNetworkAvailable;
            favoritesListTxtNoInternet.Text = AppResources.noNetworkAvailable;

        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAppInfos();
        }

        private void UpdateAppInfos()
        {
            //string trialOrReg = ((App)Application.Current).IsTrial? "TRIAL" : "FULL";
            string trialOrReg = ((App)Application.Current).IsTrial ? AppResources.trial : AppResources.full;
            appInfo.Text = string.Format("v.{0} - {1}", new string[] { _appVer, trialOrReg });
            appInfo.Foreground = ((App)Application.Current).IsTrial ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);

            if (AppInfo.Instance.AppIsOfflineSettingValue)
            {
                appInfo.Text += " (offline)";
            }
        }

        async void itemsList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if ((sender as ListBox).SelectedItem == null)
            {
                return;
            }

            var selectedItem = (ItemViewModel)((sender as ListBox).SelectedItem);

            //per vedere con webbrowser
            //WebBrowserTask webBrowserTask = new WebBrowserTask();
            //webBrowserTask.URL = "vnd.youtube:" + GenericHelper.GetYoutubeID(selectedItem.Url) + "?vndapp=youtube_mobile";
            //webBrowserTask.Show();

            try
            {
                if (AppInfo.Instance.AppIsOfflineSettingValue)
                {
                    #region OFFLINE
                    try
                    {
                        if (AppInfo.Instance.IsAdvertisingEnabled)
                        {
                            #region advertising ON
                            //KO
                            //C:\Data\Users\DefApps\AppData\{2D034F2D-836B-466C-9CA2-A7BB6B24E3F8}\Local\ep_1.mp4
                            //App.ViewModel.CurrentYoutubeMP4FileName = selectedItem.OfflineFileName;
                            //App.ViewModel.CurrentYoutubeMP4Uri = new Uri("ms-appdata:///local/" + selectedItem.OfflineFileName, UriKind.RelativeOrAbsolute);
                            //App.ViewModel.CurrentYoutubeMP4Uri = new Uri("isostore:/" + selectedItem.OfflineFileName, UriKind.RelativeOrAbsolute);
                            //App.ViewModel.CurrentYoutubeMP4Uri = new Uri(selectedItem.OfflineFileName, UriKind.RelativeOrAbsolute);
                            //App.ViewModel.CurrentYoutubeMP4Uri = new Uri("isostore:/Local/" + selectedItem.OfflineFileName, UriKind.RelativeOrAbsolute);
                            //App.ViewModel.CurrentYoutubeMP4Uri = new Uri("isostore:/local/" + selectedItem.OfflineFileName, UriKind.Absolute);
                            App.ViewModel.IsDataLoading = true;
                            //OK
                            //Uri test1 = new Uri(@"C:\Data\Users\DefApps\AppData\{2D034F2D-836B-466C-9CA2-A7BB6B24E3F8}\Local\ep_1.mp4", UriKind.Absolute);
                            Uri episodeUri = new Uri(@"C:\Data\Users\DefApps\AppData\{" + Wp81Shared.Helpers.AppInfosHelper.GetId() + @"}\Local\" + selectedItem.OfflineFileName,
                                                    UriKind.Absolute);
                            App.ViewModel.CurrentYoutubeMP4Uri = episodeUri;
                            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService, Dispatcher, "/PlayerPage.xaml");
                            #endregion
                        }
                        else
                        {
                            #region advertising OFF
                            Uri episodeUri = null;

                            if (AppInfo.Instance.OfflineSupportTypeSettingValue == BackupSupportType.SDCard)
                            {
                                //episodeUri = new Uri(episodeFile.Path, UriKind.Absolute);
                                //episodeUri = new Uri(@"D:\PeppaPigBackup\ep_1.mp4", UriKind.Absolute);
                                //await episodeFile.CopyAndReplaceAsync()

                                const string currendEpisodeFileName = "currendEpisode.mp4";

                                //copy file from SDCard to IsoStore
                                var sdStoredEpisodeFile = await AppInfo.Instance.SDBackupFolder.GetFileAsync(selectedItem.OfflineFileName);
                                var sdFileStream = await sdStoredEpisodeFile.OpenStreamForReadAsync();

                                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (isoStore.FileExists(currendEpisodeFileName))
                                    {
                                        isoStore.DeleteFile(currendEpisodeFileName);
                                    }

                                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(currendEpisodeFileName, System.IO.FileMode.Create, isoStore))
                                    {
                                        byte[] buffer = new byte[1024];
                                        int bytesRead;
                                        while ((bytesRead = sdFileStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            stream.Write(buffer, 0, bytesRead);
                                        }
                                        stream.Flush();
                                    }
                                }
                                episodeUri = new Uri(currendEpisodeFileName, UriKind.RelativeOrAbsolute);
                            }
                            else
                            {
                                episodeUri = new Uri(selectedItem.OfflineFileName, UriKind.RelativeOrAbsolute);
                            }

                            var launcher = new MediaPlayerLauncher
                                      {
                                          Controls = MediaPlaybackControls.All,
                                          Location = MediaLocationType.Data,
                                          Media = episodeUri
                                      };
                            launcher.Show();
                            #endregion
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(AppResources.possibleWrongBackup);
                    }
                    #endregion
                }
                else
                {
                    #region ONLINE

                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        MessageBox.Show(AppResources.noNetworkAvailable);
                        return;
                    }

                    if (((App)Application.Current).IsTrial)
                    {
                        if (!selectedItem.IsAvailableInTrial)
                        {
                            GotoBuyPage(AppFunctionToLimit.EpisodeCount);
                            return;
                        }
                    }

                    if (AppInfo.Instance.DownloadIsAllowed)
                    {
                        GenericHelper.Instance.IncrementOnlineUsagesCount();
                        if (AppInfo.Instance.OnlineUsagesSettingValue % 4 == 0)
                        {
                            switch (MessageBox.Show(AppResources.DownloadEpisodesQuestion, "", MessageBoxButton.OKCancel))
                            {
                                case MessageBoxResult.OK:

                                    await ExecBackup();
                                    return;
                            }
                        }
                    }

                    string id = GenericHelper.GetYoutubeID(selectedItem.Url);
                    App.ViewModel.IsDataLoading = true;

                    if (AppInfo.Instance.IsAdvertisingEnabled)
                    {
                        //TODO 81
                        //#region advertising ON
                        //YouTube.GetVideoUriAsync(id,  MyToolkit.Multimedia.YouTubeQuality.Quality480P,
                        //                    (uri, ex) =>
                        //                    {
                        //                        //SystemTray.ProgressIndicator.IsVisible = true;
                        //                        if (ex == null && uri != null)
                        //                        {
                        //                            App.ViewModel.CurrentYoutubeMP4Uri = uri.Uri;
                        //                            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService, Dispatcher, "/PlayerPage.xaml");
                        //                        }
                        //                        else
                        //                        {
                        //                            switch (MessageBox.Show(AppResources.brokenLinkQuestion,
                        //                                                    AppResources.ExceptionMessageTitle,
                        //                                                    MessageBoxButton.OKCancel))
                        //                            {

                        //                                case MessageBoxResult.OK:
                        //                                    ReportBrokenLink(ex.Message);
                        //                                    break;
                        //                            }
                        //                        }
                        //                    });
                        //#endregion
                    }
                    else
                    {
                        #region advertising OFF
                        YouTube.Play(id, true, YouTubeQuality.Quality480P, x =>
                        {
                            if (x != null)
                            {
                                switch (MessageBox.Show(AppResources.brokenLinkQuestion,
                                                        AppResources.ExceptionMessageTitle,
                                                        MessageBoxButton.OKCancel))
                                {

                                    case MessageBoxResult.OK:
                                        ReportBrokenLink(x.Message);
                                        break;
                                }
                            }
                            App.ViewModel.IsDataLoading = false;
                        });
                        #endregion
                    }
                    #endregion
                }
            }
            catch (InvalidOperationException ignored)
            {

            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (YouTube.CancelPlay())
            {
                //StopUsageTimer();
                e.Cancel = true;
            }
            else
            {
                // your code here
            }
            base.OnBackKeyPress(e);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ToggleNoInternetWarningTxt(false);

            if (!NetworkInterface.GetIsNetworkAvailable() && !AppInfo.Instance.AppIsOfflineSettingValue)
            {
                ToggleNoInternetWarningTxt(true);
                return;
            }

            try
            {
                YouTube.CancelPlay(); // used to reenable page
            }
            catch (Exception)
            {
            }
            // your code here
            // StopUsageTimer();
            base.OnNavigatedTo(e);
        }

        private void CheckOtherAppsPanoramaItem()
        {
            string currentCulture = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            otherAppsPanoramaItem.Visibility = System.Windows.Visibility.Collapsed;
            if (!AppInfo.Instance.ShowOtherApps)
            {
                otherAppsPanoramaItem.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                otherAppsPanoramaItem.Visibility = System.Windows.Visibility.Visible;
                otherApps.SetCurrentAppGuid(Wp81Shared.Helpers.AppInfosHelper.GetId());
                otherApps.SetRequiredGenre(Wp81Shared.UserControls.Genre.KidsAndFamily);
            }

            if (currentCulture.Equals("it") || currentCulture.Equals("en"))
            {
                otherAppsPanoramaItem.Visibility = System.Windows.Visibility.Visible;
                otherApps.SetCurrentAppGuid(Wp81Shared.Helpers.AppInfosHelper.GetId());
                otherApps.SetRequiredGenre(Wp81Shared.UserControls.Genre.KidsAndFamily);
            }
            else
            {
                otherAppsPanoramaItem.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ToggleNoInternetWarningTxt(bool visible)
        {
            if (visible)
            {
                firstListTxtNoInternet.Visibility = System.Windows.Visibility.Visible;
                secondListTxtNoInternet.Visibility = System.Windows.Visibility.Visible;
                thirdListTxtNoInternet.Visibility = System.Windows.Visibility.Visible;
                fourthListTxtNoInternet.Visibility = System.Windows.Visibility.Visible;
                favoritesListTxtNoInternet.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                firstListTxtNoInternet.Visibility = System.Windows.Visibility.Collapsed;
                secondListTxtNoInternet.Visibility = System.Windows.Visibility.Collapsed;
                thirdListTxtNoInternet.Visibility = System.Windows.Visibility.Collapsed;
                fourthListTxtNoInternet.Visibility = System.Windows.Visibility.Collapsed;
                favoritesListTxtNoInternet.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            _currentContextItem = ((Border)sender).DataContext;
        }

        #region favorites
        private void AddContextMenus()
        {
            //lista 1
            ContextMenu contextMenu1 = new ContextMenu();

            string addToFavText = AppResources.addToFavText;
            string removeFromFavText = AppResources.removeFromFavText;
            string shareText = AppResources.shareText;
            string reportErrorText = AppResources.reportError;

            //lista1
            MenuItem menuItemAdd1 = new MenuItem { Header = addToFavText };
            contextMenu1.Items.Add(menuItemAdd1);
            menuItemAdd1.Click += new RoutedEventHandler(menuItemAdd_Click);

            if (!AppInfo.Instance.AppIsOfflineSettingValue)
            {
                MenuItem menuItemShare1 = new MenuItem { Header = shareText };
                contextMenu1.Items.Add(menuItemShare1);
                menuItemShare1.Click += new RoutedEventHandler(menuItemShare_Click);
            }

            MenuItem menuItemBrokenlink1 = new MenuItem { Header = reportErrorText };
            contextMenu1.Items.Add(menuItemBrokenlink1);
            menuItemBrokenlink1.Click += new RoutedEventHandler(menuItemBrokenlink_Click);

            ContextMenuService.SetContextMenu(firstList, contextMenu1);

            //lista 2
            ContextMenu contextMenu2 = new ContextMenu();

            MenuItem menuItemAdd2 = new MenuItem { Header = addToFavText };
            contextMenu2.Items.Add(menuItemAdd2);
            menuItemAdd2.Click += new RoutedEventHandler(menuItemAdd_Click);

            MenuItem menuItemShare2 = new MenuItem { Header = shareText };
            contextMenu2.Items.Add(menuItemShare2);
            menuItemShare2.Click += new RoutedEventHandler(menuItemShare_Click);

            MenuItem menuItemBrokenlink2 = new MenuItem { Header = reportErrorText };
            contextMenu2.Items.Add(menuItemBrokenlink2);
            menuItemBrokenlink2.Click += new RoutedEventHandler(menuItemBrokenlink_Click);

            ContextMenuService.SetContextMenu(secondList, contextMenu2);

            //lista 3
            ContextMenu contextMenu3 = new ContextMenu();
            MenuItem menuItemAdd3 = new MenuItem { Header = addToFavText };

            contextMenu3.Items.Add(menuItemAdd3);
            menuItemAdd3.Click += new RoutedEventHandler(menuItemAdd_Click);
            ContextMenuService.SetContextMenu(thirdList, contextMenu3);

            MenuItem menuItemShare3 = new MenuItem { Header = shareText };
            contextMenu3.Items.Add(menuItemShare3);
            menuItemShare3.Click += new RoutedEventHandler(menuItemShare_Click);

            MenuItem menuItemBrokenlink3 = new MenuItem { Header = reportErrorText };
            contextMenu3.Items.Add(menuItemBrokenlink3);
            menuItemBrokenlink3.Click += new RoutedEventHandler(menuItemBrokenlink_Click);

            //lista 4
            ContextMenu contextMenu4 = new ContextMenu();
            MenuItem menuItemAdd4 = new MenuItem { Header = addToFavText };

            contextMenu4.Items.Add(menuItemAdd4);
            menuItemAdd4.Click += new RoutedEventHandler(menuItemAdd_Click);
            ContextMenuService.SetContextMenu(fourthList, contextMenu4);

            MenuItem menuItemShare4 = new MenuItem { Header = shareText };
            contextMenu4.Items.Add(menuItemShare4);
            menuItemShare4.Click += new RoutedEventHandler(menuItemShare_Click);

            MenuItem menuItemBrokenlink4 = new MenuItem { Header = reportErrorText };
            contextMenu4.Items.Add(menuItemBrokenlink4);
            menuItemBrokenlink4.Click += new RoutedEventHandler(menuItemBrokenlink_Click);


            //lista preferite
            ContextMenu contextMenuFavorites = new ContextMenu();

            MenuItem menuItemRemove = new MenuItem { Header = removeFromFavText };
            contextMenuFavorites.Items.Add(menuItemRemove);
            menuItemRemove.Click += new RoutedEventHandler(menuItemRemove_Click);

            MenuItem menuItemShareFav = new MenuItem { Header = shareText };
            contextMenuFavorites.Items.Add(menuItemShareFav);
            menuItemShareFav.Click += new RoutedEventHandler(menuItemShare_Click);

            ContextMenuService.SetContextMenu(favoritesList, contextMenuFavorites);

        }

        void menuItemBrokenlink_Click(object sender, RoutedEventArgs e)
        {
            ReportBrokenLink("");
        }

        private void ReportBrokenLink(string internalError)
        {
            try
            {
                var email = new EmailComposeTask();
                email.To = "centapp@hotmail.com";
                email.Subject = string.Format("{0} - {1} ({2})", AppResources.reportError, AppInfo.Instance.AppName.ToUpper(), GenericHelper.GetAppversion());
                var episode = (_currentContextItem as ItemViewModel);


                string epTitle = string.Empty;
                if (AppInfo.Instance.UseResManager)
                {
                    epTitle = (string)(new IdToTitleConverter().Convert(episode.Id, null, null, AppInfo.Instance.NeutralCulture));
                }
                else
                {
                    epTitle = episode.Title;
                }

                email.Body = string.Format(AppResources.brokenLinkText,
                                          epTitle,
                                          (_currentContextItem as ItemViewModel).Url);

                if (!string.IsNullOrEmpty(episode.OrigId))
                {
                    email.Body += string.Format("\norig id = '{0}'", episode.OrigId);
                }

                if (!string.IsNullOrEmpty(internalError))
                {
                    email.Body += string.Format("\nError code = '{0}'", internalError);
                }

                email.Body += string.Format("\nApp version = '{0}'", _appVer);
                email.Body += string.Format("\nApp language = '{0}'", AppInfo.Instance.NeutralCulture);

                email.Show();
            }
            catch (InvalidOperationException ignored)
            {
            }
        }

        void menuItemShare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string epTitle = string.Empty;
                if (AppInfo.Instance.UseResManager)
                {
                    epTitle = (string)(new IdToTitleConverter().Convert((_currentContextItem as ItemViewModel).Id, null, null, null));
                }
                else
                {
                    epTitle = (_currentContextItem as ItemViewModel).Title;
                }

                ShareStatusTask shareStatusTask = new ShareStatusTask();
                shareStatusTask.Status = String.Format(AppResources.checkoutVideo,
                                                      epTitle,
                                                      (_currentContextItem as ItemViewModel).Url);
                shareStatusTask.Show();
            }
            catch (InvalidOperationException ignored)
            {
            }
        }

        async void menuItemAdd_Click(object sender, RoutedEventArgs e)
        {
            if (((App)Application.Current).IsTrial && !(_currentContextItem as ItemViewModel).IsAvailableInTrial)
            {
                GotoBuyPage(AppFunctionToLimit.EpisodeCount);
                return;
            }

            if (AppInfo.Instance.AppIsOfflineSettingValue)
            {
                App.ViewModel.AddToFavorites(_currentContextItem);
            }
            else
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(AppResources.FavoritesOnlyInOfflineMode, AppResources.Warning, MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    ExecBackup();
                }
            }
        }

        void menuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.RemoveFromFavorites(_currentContextItem);
        }
        #endregion


        #region "app" panorama item
        private void rateAppButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show(AppResources.noNetworkAvailable);
                    return;
                }

                FeedbackHelper.Default.Reviewed();
                MarketplaceReviewTask task = new MarketplaceReviewTask();
                task.Show();
            }
            catch (InvalidOperationException ignored)
            {

            }
            catch (Exception)
            {
                MessageBox.Show(AppResources.error_store);
            }
        }

        private void writeAnEmailButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show(AppResources.noNetworkAvailable);
                    return;
                }

                var email = new EmailComposeTask();
                email.To = "centapp@hotmail.com";
                email.Subject = string.Format(AppResources.feedbackText, AppInfo.Instance.AppName.ToUpper()) + string.Format(" ({0})", GenericHelper.GetAppversion());


                email.Show();
            }
            catch (InvalidOperationException ignored)
            {

            }
        }

        private void buyAppButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var marketplaceDetailTask = new MarketplaceDetailTask();
                marketplaceDetailTask.ContentIdentifier = null;
                marketplaceDetailTask.Show();
            }
            catch (Exception)
            {
                MessageBox.Show(AppResources.error_buy);
            }
        }
        #endregion

        #region license management
        private void GotoBuyPage(AppFunctionToLimit usage)
        {
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                              Dispatcher,
                                                              string.Format("/BuyAppPage.xaml?" + GenericHelper.UsageKeyName + "={0}", usage.ToString()));
        }
        #endregion

        private void searchEpisodesButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                              Dispatcher,
                                                              string.Format("/SearchEpisodes.xaml"));
        }


        #region offline backup

        private async void backupEpisodesButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await ExecBackup();
        }

        private async Task ExecBackup()
        {
            MessageBoxResult messageBoxResult;

            //default
            BackupSupportType backupSupportType = BackupSupportType.IsolatedStorage;

            //tests if SDcard is available
            bool isSDCardAvailable = await GenericHelper.Instance.IsSDCardAvailable();
            if (isSDCardAvailable)
            {
                string ItemPhoneMemoryTitle = AppResources.ItemPhoneMemoryTitle;
                string ItemPhoneMemoryDesc = AppResources.ItemPhoneMemoryDesc;
                string ItemSDCardTitle = AppResources.ItemSDCardTitle;
                string ItemSDCardDesc = AppResources.ItemSDCardDesc;
                string ItemCancelTitle = AppResources.ItemCancelTitle;
                string ItemCancelDesc = AppResources.ItemCancelDesc;

                MessageBoxClosedEventArgs args = await RadMessageBox.ShowAsync(new CustomHeaderedContentControl[] 
                                                                                { 
                                                                                    new CustomHeaderedContentControl() { Title = ItemPhoneMemoryTitle, Message = ItemPhoneMemoryDesc },
                                                                                    new CustomHeaderedContentControl() { Title = ItemSDCardTitle, Message = ItemSDCardDesc },
                                                                                    new CustomHeaderedContentControl() { Title = ItemCancelTitle, Message = ItemCancelDesc }
                                                                                },
                                                                                AppResources.EpisodesDownloadTitle,
                                                                                AppResources.BackupMediatypeRequest);
                
                //the user pressed the hardware back button.
                if (args.ClickedButton == null)
                {
                    return;
                }

                CustomHeaderedContentControl option = (CustomHeaderedContentControl)args.ClickedButton.Content;
                if (option.Title == ItemPhoneMemoryTitle)
                {
                    backupSupportType = BackupSupportType.IsolatedStorage;

                } else if (option.Title == ItemSDCardTitle)
                {
                    backupSupportType = BackupSupportType.SDCard;
                }
                else if (option.Title == ItemCancelTitle)
                {
                    // Return to editing the message.
                    return;
                }
            }

            if (AppInfo.Instance.AppIsOfflineSettingValue)
            {
                messageBoxResult = MessageBox.Show(AppResources.MessageAlreadyOffline, AppResources.Warning, MessageBoxButton.OKCancel);
                if (messageBoxResult != MessageBoxResult.OK)
                {
                    return;
                }
                GenericHelper.Instance.SetAppIsOffline(false);
                await GenericHelper.Instance.RemoveOfflineData();
            }

            MediaInfo mInfo = await CheckAvailableSpace(backupSupportType);

            if (!mInfo.IsBackupAvailable)
            {
                if (backupSupportType == BackupSupportType.SDCard)
                {
                    //allow user to switch to isostore save option
                    //messageBoxResult = MessageBox.Show(string.Format("$Sorry but SD card ha not available free space (required: {0}, available {1}).\nDo you want to store episode into your phone memory?", mInfo.RequiredGigaBytes, mInfo.AvailableGigaBytes),

                    messageBoxResult = MessageBox.Show(string.Format(AppResources.NotEnoughSpaceOnSDCard, mInfo.RequiredGigaBytes, mInfo.AvailableGigaBytes),
                                                        AppResources.Warning,
                                                        MessageBoxButton.OKCancel);
                    if (messageBoxResult == MessageBoxResult.OK)
                    {
                        backupSupportType = BackupSupportType.IsolatedStorage;
                    }

                    mInfo = await CheckAvailableSpace(backupSupportType);
                    //recheck isostore for free space
                    if (!mInfo.IsBackupAvailable)
                    {
                        //no space even into isostore - end
                        MessageBox.Show(string.Format(AppResources.notEnoughSpace, mInfo.RequiredGigaBytes));
                        return;
                    }
                }
                else
                {
                    //no space even into isostore - end
                    MessageBox.Show(string.Format(AppResources.notEnoughSpace, mInfo.RequiredGigaBytes));
                    return;
                }

            }

            messageBoxResult = MessageBox.Show(string.Format(AppResources.SpaceRequiredWarn, mInfo.RequiredGigaBytes), AppResources.Warning, MessageBoxButton.OKCancel);
            if (messageBoxResult != MessageBoxResult.OK)
            {
                return;
            }

            if (!NetworkInterface.GetIsNetworkAvailable() || NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.None)
            {
                MessageBox.Show(AppResources.noNetworkAvailable);
                return;
            }

#if !SIMULATE_DWN
            if (!CheckConnection())
            {
                MessageBox.Show(AppResources.wifiRequired);
                return;
            }
#endif
            if (((App)Application.Current).IsTrial)
            {
                GotoBuyPage(AppFunctionToLimit.BackupEpisodes);
                return;
            }

            //if (AppInfo.Instance.AppIsOfflineSettingValue)
            //{
            //    messageBoxResult = MessageBox.Show(AppResources.MessageAlreadyOffline, AppResources.Warning, MessageBoxButton.OKCancel);
            //    if (messageBoxResult != MessageBoxResult.OK)
            //    {
            //        return;
            //    }
            //    GenericHelper.SetAppIsOffline(false);
            //     using (IsolatedStorageFile isostore = IsolatedStorageFile.GetUserStoreForApplication())
            //    {
            //        var storedFiles = isostore.GetFileNames("ep*.mp4");
            //        foreach (var item in storedFiles)
            //        {
            //            isostore.DeleteFile(item);
            //        }
            //    }
            //    App.ViewModel.OnLoadCompleted -= new OnLoadCompletedHandler(ViewModel_OnLoadCompletedDownload);
            //    App.ViewModel.OnLoadCompleted += new OnLoadCompletedHandler(ViewModel_OnLoadCompletedDownload);
            //    App.ViewModel.LoadData();
            //}
            //else
            //{
            //    if (App.ViewModel.Items == null || App.ViewModel.Items.Count == 0)
            //    {
            //        //caso raro, si fa ripartire l'utente da zero
            //        MessageBox.Show(AppResources.ServerTemporaryUnavailable);
            //        throw new ForcedExitException("start backup error : " + AppResources.ServerTemporaryUnavailable);
            //    }
            //    GotoDownloaderPage();
            //}


            AppInfo.Instance.CurrentBackupSupport = backupSupportType;

            GotoDownloaderPage();
        }



        private bool CheckConnection()
        {
            //bool hasNetworkConnection = NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.;

            //None - The phone is not connected to any data network 
            //Wireless80211 - The phone is connected to a WiFi hotspot 
            //MobileBroadbandGsm - The phone is connected to a GSM network 
            //MobileBroadbandCdma - The phone is connected to a CDMA network (US and parts of Asia only) 
            //Ethernet - The phone has an ethernet connection (typically this is when the phone is connected to a PC via USB) 

#if DEBUG
            MessageBox.Show("connessione di rete attuale: " + NetworkInterface.NetworkInterfaceType);
            return true;
#endif

            bool isConnectionOk = (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
            if (isConnectionOk)
            {
                return true;
            }
            return false;
        }

        public async Task<UInt64> GetFreeSpace(StorageFolder folder)
        {
            var retrivedProperties = await folder.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" });
            return (UInt64)retrivedProperties["System.FreeSpace"];
        }

        private async Task<MediaInfo> CheckAvailableSpace(BackupSupportType backupType)
        {
            MediaInfo mInfo = new MediaInfo();
            mInfo.AvailableGigaBytes = 0;
            mInfo.RequiredGigaBytes = 0;

            int episodeDurationInSec = AppInfo.Instance.EpisodesLength;
            if (episodeDurationInSec == -1)
            {
                mInfo.IsBackupAvailable = false;
                return mInfo; //dato non dichiarato nell'app: non è possibile eseguire il calcolo e torna true....
            }

            Int64 curAvail = -1;
            if (backupType == BackupSupportType.IsolatedStorage)
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    curAvail = store.AvailableFreeSpace;
                }
            }
            else
            {
                await GenericHelper.Instance.InitSDBackupFolder();
                curAvail = (Int64)await GetFreeSpace(AppInfo.Instance.SDBackupFolder);
                //curAvail = (Int64)await GetFreeSpace(KnownFolders.RemovableDevices);
            }

            mInfo.AvailableGigaBytes = Math.Round((decimal)((double)curAvail / 1024d / 1024d / 1024d), 1);

            //int episodeDurationInSec = 300;

            //MP4 medium quality: 2Mb/minuto -> 1 MB = 1048576 Bytes -> 2097152 bytes/minuto -> 34952 bytes/secondo
            int bytesPerSecond = 34952;

            Int64 estimatedRequiredSpace = App.ViewModel.Items.Count * episodeDurationInSec * bytesPerSecond;
            mInfo.RequiredGigaBytes = Math.Round((decimal)((double)estimatedRequiredSpace / 1024d / 1024d / 1024d), 1);

            if (estimatedRequiredSpace >= curAvail)
            {
                mInfo.IsBackupAvailable = false;
            }

            mInfo.IsBackupAvailable = true;

            return mInfo;
        }

        private void ViewModel_OnLoadCompleted()
        {
            App.ViewModel.OnLoadCompleted -= new OnLoadCompletedHandler(ViewModel_OnLoadCompleted);
            PanoramaMainControl.DefaultItem = PanoramaMainControl.Items[0];

            //UpdateAppInfos();
            backupEpisodesButton.Visibility = AppInfo.Instance.DownloadIsAllowed ? Visibility.Visible : Visibility.Collapsed;
        }

        async void ViewModel_OnLoadCompletedDownload()
        {
            App.ViewModel.OnLoadCompleted -= new OnLoadCompletedHandler(ViewModel_OnLoadCompletedDownload);
            GotoDownloaderPage();

            //StorageFolder folder = KnownFolders.PicturesLibrary;
            //StorageFile sampleFile = await folder.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);

            //// Get the logical root folder for all external storage devices.
            //StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            //StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
            //if (sdCard != null)
            //{
            //    // An SD card is present and the sdCard variable now contains a reference to it.
            //}
            //else
            //{
            //    // No SD card is present.
            //}

        }

        private void GotoDownloaderPage()
        {
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                              Dispatcher,
                                                              string.Format("/DownloaderPage.xaml"));
        }


        #endregion

        private void otherAppsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                  Dispatcher,
                                                  string.Format("/OtherAppsPage.xaml"));

        }

        private void infoButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            var infoPage = AppInfo.Instance.InfoPageIsPivot ? "/InfoPagePivot.xaml" : "/InfoPage.xaml";
            Wp81Shared.Helpers.NavigationHelper.SafeNavigateTo(NavigationService,
                                                Dispatcher,
                                                string.Format(infoPage));
        }

    }
}