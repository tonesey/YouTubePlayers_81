using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Centapp.CartoonCommon.Helpers;
using Centapp.CartoonCommon.Converters;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;
using Centapp.CartoonCommon.JSON;
using Newtonsoft.Json;
using System.Reflection;
using Centapp.CartoonCommon.Utility;
using System.Xml;
using System.Globalization;
using Wp81Shared.Helpers;
using Windows.Web.Http;
using System.Threading.Tasks;


namespace Centapp.CartoonCommon.ViewModels
{

    public delegate void AsyncMsgHandler(string msg, bool isFatalError);
    public delegate void OnLoadCompletedHandler();

    public class MainViewModel : INotifyPropertyChanged
    {
        string _indexFileUri;

        int _dwnRetryCounter = 0;

        public event AsyncMsgHandler OnError;
        public event AsyncMsgHandler OnUserMessageRequired;

        public event OnLoadCompletedHandler OnLoadCompleted;

        private Logger _logger = new Logger();
        internal Logger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        IdToTitleConverter _cnv = new IdToTitleConverter();

        public Dispatcher CurrentDispatcher { get; set; }

        public Uri CurrentYoutubeMP4Uri { get; set; }
        public string CurrentYoutubeMP4FileName { get; set; }

        public string AppName
        {
            get
            {
                return AppInfo.Instance.AppName;
            }
        }

        public MainViewModel()
        {
            _indexFileUri = string.Format("http://centapp.altervista.org/{0}", AppInfo.Instance.IndexFile);

            //IsNetworkAvailable = true;
            this.Items = new ObservableCollection<ItemViewModel>();
            this.Items_Chunk1 = new ObservableCollection<ItemViewModel>();
            this.Items_Chunk2 = new ObservableCollection<ItemViewModel>();
            this.Items_Chunk3 = new ObservableCollection<ItemViewModel>();
            this.Items_Chunk4 = new ObservableCollection<ItemViewModel>();
        }

        #region DownloaderPage

        private BackupStageEn _BackupStage;
        public BackupStageEn BackupStage
        {
            get
            {
                return _BackupStage;
            }
            set
            {
                _logger.Log("[MainViewModel] _BackupStage = " + _BackupStage);
                if (value != _BackupStage)
                {
                    _BackupStage = value;
                    NotifyPropertyChanged("BackupStage");
                }
            }
        }


        private ItemViewModel _DwnCurEpisode { get; set; }
        public ItemViewModel DwnCurEpisode
        {
            get
            {
                return _DwnCurEpisode;
            }
            set
            {
                if (value != _DwnCurEpisode)
                {
                    _DwnCurEpisode = value;
                    NotifyPropertyChanged("DwnCurEpisode");
                }
            }
        }


        private int _DwnCurEpisodeId;
        public int DwnCurEpisodeId
        {
            get
            {
                return _DwnCurEpisodeId;
            }
            set
            {
                if (value != _DwnCurEpisodeId)
                {
                    _DwnCurEpisodeId = value;
                    NotifyPropertyChanged("DwnCurEpisodeId");
                }
            }
        }



        private string _DwnCurEpisodeTitle = "";
        public string DwnCurEpisodeTitle
        {
            get { return _DwnCurEpisodeTitle; }
            set
            {
                _DwnCurEpisodeTitle = value;
                NotifyPropertyChanged("DwnCurEpisodeTitle");
            }
        }

        private string _DwnCurEpisodeSpeed = "";
        public string DwnCurEpisodeSpeed
        {
            get { return _DwnCurEpisodeSpeed; }
            set
            {
                _DwnCurEpisodeSpeed = value;
                NotifyPropertyChanged("DwnCurEpisodeSpeed");
            }
        }

        private string _DwnCurEpisodeAmount = "";
        public string DwnCurEpisodeAmount
        {
            get { return _DwnCurEpisodeAmount; }
            set
            {
                _DwnCurEpisodeAmount = value;
                NotifyPropertyChanged("DwnCurEpisodeAmount");
            }
        }

        private int _DwnCurEpisodePercentage = 0;
        public int DwnCurEpisodePercentage
        {
            get { return _DwnCurEpisodePercentage; }
            set
            {
                _DwnCurEpisodePercentage = value;
                NotifyPropertyChanged("DwnCurEpisodePercentage");
            }
        }

        private int _DwnOverallPercentage = 0;
        public int DwnOverallPercentage
        {
            get { return _DwnOverallPercentage; }
            set
            {
                _DwnOverallPercentage = value;
                NotifyPropertyChanged("DwnOverallPercentage");
            }
        }

        private bool _DwnInProgress = false;
        public bool DwnInProgress
        {
            get { return _DwnInProgress; }
            set
            {
                _DwnInProgress = value;
                NotifyPropertyChanged("DwnInProgress");
            }
        }


        #endregion

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public bool AlreadyAskedRating
        {
            get;
            set;
        }

        private bool _isDataLoading = true;
        public bool IsDataLoading
        {
            get { return _isDataLoading; }
            set
            {
                _isDataLoading = value;
                NotifyPropertyChanged("IsDataLoading");
                NotifyPropertyChanged("IsNotDataLoading");
            }
        }

        public bool IsNotDataLoading
        {
            get { return !_isDataLoading; }
        }

        private ItemViewModel _selectedEpisode;
        public ItemViewModel SelectedEpisode
        {
            get
            {
                return _selectedEpisode;
            }
            set
            {
                _selectedEpisode = value;
                NotifyPropertyChanged("SelectedEpisode");
            }
        }

        #region items groupment
        private ObservableCollection<ItemViewModel> _items;
        public ObservableCollection<ItemViewModel> Items
        {
            get
            {
                return _items;
            }
            private set
            {
                _items = value;
                NotifyPropertyChanged("Items");
            }
        }

        private ObservableCollection<ItemViewModel> _items_chunk1;
        public ObservableCollection<ItemViewModel> Items_Chunk1
        {
            get
            {
                return _items_chunk1;
            }
            private set
            {
                _items_chunk1 = value;
                NotifyPropertyChanged("Items_Chunk1");
            }
        }

        private ObservableCollection<ItemViewModel> _items_chunk2;
        public ObservableCollection<ItemViewModel> Items_Chunk2
        {
            get
            {
                return _items_chunk2;
            }
            private set
            {
                _items_chunk2 = value;
                NotifyPropertyChanged("Items_Chunk2");
            }
        }

        private ObservableCollection<ItemViewModel> _items_chunk3;
        public ObservableCollection<ItemViewModel> Items_Chunk3
        {
            get
            {
                return _items_chunk3;
            }
            private set
            {
                _items_chunk3 = value;
                NotifyPropertyChanged("Items_Chunk3");
            }
        }


        private ObservableCollection<ItemViewModel> _items_chunk4;
        public ObservableCollection<ItemViewModel> Items_Chunk4
        {
            get
            {
                return _items_chunk4;
            }
            private set
            {
                _items_chunk4 = value;
                NotifyPropertyChanged("Items_Chunk4");
            }
        }

        #endregion

        public List<ItemViewModel> FavoriteEpisodes
        {
            get
            {
                return _items.Where(s => s.IsFavorite).ToList();
            }
        }

        #region online management
        public async Task DownloadItemsAsynch()
        {
#if !NOINTERNET
            //WebClient client = new WebClient();
            //client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            //client.OpenReadAsync(new Uri(indexFileUrl + "?" + Guid.NewGuid()), UriKind.Absolute);

            try
            {
                HttpClient client = new HttpClient();
                string data = await client.GetStringAsync(new Uri(_indexFileUri + "?" + Guid.NewGuid()));
                SaveIndexToIsostoreJSON(data);
                BuildItemsFromJson(data, false);
            }
            catch (Exception)
            {
                if (OnError != null) OnError(AppResources.ServerTemporaryUnavailable, true);
                throw;
            }

#else
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream localStream = asm.GetManifestResourceStream("Centapp.CartoonCommon.videosrc.json");
            client_OpenReadCompleted(localStream, null);
#endif
        }

        //        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        //        {
        //            string errMsg = string.Empty;
        //            bool indexFileLoadedFromIsostore = false;
        //            Stream webStream = null;
        //            App.ViewModel.Logger.Log("[client_OpenReadCompleted]");
        //            if (AppInfo.Instance.UseJSon)
        //            {
        //#if DEBUGOFFLINE
        //                webStream = (Stream)sender;
        //#endif
        //                if (e.Error != null)
        //                {
        //                    if (OnError != null) OnError(AppResources.ServerTemporaryUnavailable, true);
        //                    return;
        //                }
        //                try
        //                {
        //                    webStream = e.Result;
        //                    using (StreamReader reader = new StreamReader(webStream))
        //                    {
        //                        var data = reader.ReadToEnd();
        //                        SaveIndexToIsostoreJSON(data);
        //                        BuildItemsFromJson(data, false);
        //                    }
        //                }
        //                finally
        //                {
        //                    if (webStream != null)
        //                    {
        //                        webStream.Close();
        //                    }
        //                }
        //            }
        //        }

        #region load/save index isostore

        #region json
        private static void SaveIndexToIsostoreJSON(string json)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(AppInfo.OfflineIndexFileNameJSON))
                    {
                        isoStore.DeleteFile(AppInfo.OfflineIndexFileNameJSON);
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(AppInfo.OfflineIndexFileNameJSON, FileMode.Create, isoStore))
                    {
                        using (TextWriter writer = new StreamWriter(isoStream))
                        {
                            writer.WriteLine(json);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.ViewModel.Logger.Log(string.Format("[SaveIndexToIsostore] error - {0}", ex.Message));
                throw ex;
            }
        }

       // static int internalcount = 0;

        private static string LoadIndexFromIsostoreJSON()
        {
            string str = null;
            try
            {
                //internalcount++;
                //if (internalcount % 2 != 0)
                //{
                //    throw new Exception("eccezione malefica!!");
                //}

                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(AppInfo.OfflineIndexFileNameJSON))
                    {
                        using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(AppInfo.OfflineIndexFileNameJSON, FileMode.Open, isoStore))
                        {
                            using (TextReader reader = new StreamReader(isoStream))
                            {
                                str = reader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("internal error" + AppInfo.OfflineIndexFileNameJSON + " not found");
                    }
                }
            }
            catch (Exception ex)
            {
                App.ViewModel.Logger.Log(string.Format("[LoadIndexFromIsostore] error - {0}", ex.Message));
                throw ex;
            }
            return str;
        }
        #endregion

        #endregion

        private void InitResManager(XDocument doc)
        {
            if (AppInfo.Instance.IsMonoLang)
            {
                //forza la cultura corrente ad essere quella dichiarata dall'AssmblyInfo 
                App.ResManager = new MyResourceManager(doc, AppInfo.Instance.NeutralCulture, AppInfo.Instance.NeutralCulture);
            }
            else
            {
                App.ResManager = new MyResourceManager(doc, Thread.CurrentThread.CurrentCulture, AppInfo.Instance.NeutralCulture);
            }
        }

        private void BuildItemsFromJson(string json, bool appIsOffline)
        {
            //senza status
            //List<Season> seasons = JsonConvert.DeserializeObject<List<Season>>(json);

            //con status
            var root = JsonConvert.DeserializeObject<RootObject>(json);
            List<Season> seasons = root.seasons;

            if (!appIsOffline)
            {
                #region status management
                var elStatus = root.statusmsg;
                if (!string.IsNullOrEmpty(elStatus))
                {
                    if (!string.IsNullOrEmpty(root.value) && root.value != "ok")
                    {
                        bool mustExit = false;
                        bool msgRequired = true;

                        if (root.value == "ko")
                        {
                            mustExit = true;
                        }
                        else if (root.value == "warn")
                        {
                            mustExit = false;
                        }

                        if (!string.IsNullOrEmpty(root.targetVer) && !string.IsNullOrEmpty(root.op))
                        {
                            var targetVer = root.targetVer;
                            var op = root.op;

                            if (!string.IsNullOrEmpty(targetVer) && !string.IsNullOrEmpty(op))
                            {
                                Version serverVer = new Version(targetVer);
                                VersionFormat formatRequired = VersionFormat.VRB;
                                if (serverVer.Major != -1 && serverVer.Minor != -1 && serverVer.Build != -1)
                                {
                                    //VRB
                                    formatRequired = VersionFormat.VRB;
                                }
                                else if (serverVer.Major != -1 && serverVer.Minor != -1 && serverVer.Build == -1)
                                {
                                    //VR
                                    formatRequired = VersionFormat.VR;
                                }
                                else if (serverVer.Major != -1 && serverVer.Minor == -1 && serverVer.Build == -1)
                                {
                                    //V
                                    formatRequired = VersionFormat.V;
                                }

                                Version appVer = new Version(GenericHelper.GetAppversion(formatRequired));

                                switch (op.ToUpper())
                                {
                                    case "EQ":
                                        msgRequired = appVer == serverVer;
                                        break;
                                    case "LT":
                                        msgRequired = appVer < serverVer;
                                        break;
                                    case "GT":
                                        msgRequired = appVer > serverVer;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        if (msgRequired)
                        {
                            OnUserMessageRequired(root.statusmsg, mustExit);
                            if (mustExit)
                            {
                                return;
                            }
                        }
                    }
                }

                #endregion
            }

            ObservableCollection<ItemViewModel> items = new ObservableCollection<ItemViewModel>();
            //int seasonCount = 0;
            // int index = 0;
            foreach (var season in seasons)
            {
                foreach (var episode in season.episodes)
                {
                    var item = new ItemViewModel()
                               {
                                   Id = episode.id,
                                   Url = YouTubeHelper.BuildYoutubeID(episode.youtube_id),
                                   IsAvailableInTrial = appIsOffline ? true : episode.id <= 5,
                                   Title = episode.name,
                                   SeasonId = season.season
                               };
                    items.Add(item);
                }
                //seasonCount++;
            }

            #region favorites management
            for (int i = 1; i <= items.Count; i++)
            {
                ItemViewModel curEpisode = items.ElementAt(i - 1);
                int episodeId = curEpisode.Id;
                //curEpisode.Title = _cnv.Convert(episodeId);
                if (AppInfo.Instance.FavoriteEpisodesIdsSettingValue.Contains(episodeId))
                {
                    curEpisode.IsFavorite = true;
                }
                if (appIsOffline)
                {
                    curEpisode.OfflineFileName = GenericHelper.GetOfflineFileName(episodeId.ToString());
                }
            }
            #endregion

            Items = items;

            Items_Chunk1 = new ObservableCollection<ItemViewModel>(items.Where(i => i.SeasonId == 1));
            Items_Chunk2 = new ObservableCollection<ItemViewModel>(items.Where(i => i.SeasonId == 2));
            Items_Chunk3 = new ObservableCollection<ItemViewModel>(items.Where(i => i.SeasonId == 3));
            Items_Chunk4 = new ObservableCollection<ItemViewModel>(items.Where(i => i.SeasonId == 4));

            this.IsDataLoaded = true;
            IsDataLoading = false;

            NotifyPropertyChanged("FavoriteEpisodes");
            if (OnLoadCompleted != null) OnLoadCompleted();
        }

        #endregion

        public async Task LoadData()
        {
            App.ViewModel.Logger.Log("[MainViewModel][LoadData]");
            IsDataLoading = true;
            if (!AppInfo.Instance.AppIsOfflineSettingValue)
            {
                App.ViewModel.Logger.Log("[MainViewModel][LoadData] online");
                DownloadItemsAsynch();
            }
            else
            {
                App.ViewModel.Logger.Log("[MainViewModel][LoadData] offline");
                string json = string.Empty;
                int retryCounter = 0;
                bool dataReadOk = false;
                bool dwnRecoverRequired = false;
                Exception lastException = null;

                do
                {
                    retryCounter++;
                    try
                    {
                        App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] START loading data from isostore (retry {0})", retryCounter));
                        json = LoadIndexFromIsostoreJSON();
                        App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] LoadIndexFromIsostoreJSON OK (retry {0})", retryCounter));
                        BuildItemsFromJson(json, true);
                        throw new Exception("Errore malvagio");
                        App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] BuildItemsFromJson OK (retry {0})", retryCounter));
                        dataReadOk = true;
                        App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] END loading data from isostore OK! (retry {0})", retryCounter));
                    }
                    catch (Exception ex)
                    {
                        App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] error! (retry {0}): {1}", retryCounter, ex.Message + "\n" + ex.StackTrace));
                        lastException = ex;
                        FlurryHelper.LogException("[MainViewModel][LoadData] retry = " + retryCounter, ex);
                        if (retryCounter <= 3)
                        {
                            dwnRecoverRequired = true;
                        }
                    }

                    if (dwnRecoverRequired)
                    {
                        try
                        {
                            App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] dwn recover START (retry {0})", retryCounter));
                            HttpClient client = new HttpClient();
                            string data = await client.GetStringAsync(new Uri(_indexFileUri + "?" + Guid.NewGuid()));
                            SaveIndexToIsostoreJSON(data);
                            App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] dwn recover END (retry {0})", retryCounter));
                        }
                        catch (Exception ex)
                        {
                            App.ViewModel.Logger.Log(string.Format("[MainViewModel][LoadData] dwn recover error! (retry {0}): {1}", retryCounter, ex.Message + "\n" + ex.StackTrace));
                        }
                    }
                } while (retryCounter <= 3 && !dataReadOk);

                if (!dataReadOk)
                {
                   // if (OnError != null) OnError(AppResources.ServerTemporaryUnavailable, true);
                    throw lastException;
                }
            }
        }

        #region favorites
        internal void AddToFavorites(object item)
        {
            int id = (item as ItemViewModel).Id;
            (item as ItemViewModel).IsFavorite = true;
            if (!AppInfo.Instance.FavoriteEpisodesIdsSettingValue.Contains(id))
            {
                AppInfo.Instance.FavoriteEpisodesIdsSettingValue.Add(id);
            }
            GenericHelper.Instance.Writekey(GenericHelper.FavoriteEpisodesKey, AppInfo.Instance.FavoriteEpisodesIdsSettingValue);
            NotifyPropertyChanged("FavoriteEpisodes");
        }

        internal void RemoveFromFavorites(object item)
        {
            int id = (item as ItemViewModel).Id;
            (item as ItemViewModel).IsFavorite = false;
            if (AppInfo.Instance.FavoriteEpisodesIdsSettingValue.Contains(id))
            {
                AppInfo.Instance.FavoriteEpisodesIdsSettingValue.Remove(id);
            }
            GenericHelper.Instance.Writekey(GenericHelper.FavoriteEpisodesKey, AppInfo.Instance.FavoriteEpisodesIdsSettingValue);
            NotifyPropertyChanged("FavoriteEpisodes");
        }
        #endregion

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null == handler)
            {
                return;
            }
            if (CurrentDispatcher != null)
            {
                CurrentDispatcher.BeginInvoke(() =>
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


    }
}