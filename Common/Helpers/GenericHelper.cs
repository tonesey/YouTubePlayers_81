using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Globalization;
using Windows.Storage;
using System.Threading.Tasks;

namespace Centapp.CartoonCommon.Helpers
{

    public enum BackupSupportType
    {
        Undefined = -1,
        IsolatedStorage = 0,
        SDCard = 1
    }

    public enum VersionFormat
    {
        V,
        VR,
        VRB
    }

    public class GenericHelper
    {

        public const string FavoriteEpisodesKey = "FavoriteEpisodesIds";
        public const string AppIsOfflineKey = "AppIsOffline";
        public const string OnlineUsagesKey = "OnlineUsages";
        public const string OfflineSupportTypeKey = "OfflineSupportType";

        public const string UsageKeyName = "usage";
        public const string MaxNumberKeyName = "number";

        private static GenericHelper _instance = null;
        public static GenericHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GenericHelper();
                }
                return _instance;
            }
        }

        public async Task ReadAppSettings()
        {
            object favoriteEpisodes = Readkey(FavoriteEpisodesKey);
            AppInfo.Instance.FavoriteEpisodesIdsSettingValue = favoriteEpisodes == null ? new List<int>() : (List<int>)favoriteEpisodes;

            object appIsOffline = Readkey(AppIsOfflineKey);
            AppInfo.Instance.AppIsOfflineSettingValue = appIsOffline == null ? false : (bool)appIsOffline;

            object onlineUsagesCount = Readkey(OnlineUsagesKey);
            AppInfo.Instance.OnlineUsagesSettingValue = (onlineUsagesCount == null || string.IsNullOrEmpty(onlineUsagesCount.ToString())) ? 0 : int.Parse(onlineUsagesCount.ToString());

            object offlineSupportType = Readkey(OfflineSupportTypeKey);
            AppInfo.Instance.OfflineSupportTypeSettingValue = offlineSupportType == null ? BackupSupportType.IsolatedStorage : (BackupSupportType)offlineSupportType;

            if (AppInfo.Instance.OfflineSupportTypeSettingValue == BackupSupportType.SDCard)
            {
                bool sdInitOk = await InitSDBackupFolder();
                if (!sdInitOk)
                {
                    //MessageBox.Show("$error while initing SD card, app will run in online mode");
                    MessageBox.Show(AppResources.SDCardErrorInitGoOnline);
                    //non viene scritto il setting, potrebbe essere una mancanza temporanea di SD, agli avvii successivi si ritenta di lavorare offline su SD
                    //SetAppIsOffline(false); 
                    AppInfo.Instance.AppIsOfflineSettingValue = false;
                    AppInfo.Instance.OfflineSupportTypeSettingValue = BackupSupportType.Undefined;
                }
            }
        }

        internal void IncrementOnlineUsagesCount()
        {
            AppInfo.Instance.OnlineUsagesSettingValue++;
            Writekey(OnlineUsagesKey, AppInfo.Instance.OnlineUsagesSettingValue);
        }

        //internal void SetOfflineBackupType(BackupSupportType backupType)
        //{
        //    AppInfo.Instance.OfflineSupportTypeSettingValue = backupType;
        //    Writekey(OfflineSupportTypeKey, AppInfo.Instance.OfflineSupportTypeSettingValue);
        //}

        internal void SetAppIsOffline(bool isOffline, BackupSupportType backupType = BackupSupportType.Undefined)
        {
            AppInfo.Instance.AppIsOfflineSettingValue = isOffline;
            Writekey(AppIsOfflineKey, AppInfo.Instance.AppIsOfflineSettingValue);

            if (isOffline)
            {
                AppInfo.Instance.OfflineSupportTypeSettingValue = backupType;
                Writekey(OfflineSupportTypeKey, backupType);
            }
            else
            {
                Writekey(OfflineSupportTypeKey, BackupSupportType.Undefined);
            }
        }


        public void Writekey(string key, object value)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            }
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public object Readkey(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key) ? IsolatedStorageSettings.ApplicationSettings[key] : null;
        }

        #region SD card
        internal async Task RemoveOfflineEpisodes()
        {
            if (AppInfo.Instance.OfflineSupportTypeSettingValue == BackupSupportType.IsolatedStorage)
            {
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (var item in isoStore.GetFileNames("ep*.mp4"))
                    {
                        isoStore.DeleteFile(item);
                    }
                    //foreach (var item in isoStore.GetFileNames("thumb*.png"))
                    //{
                    //    isoStore.DeleteFile(item);
                    //}
                }
            }
            else
            {
                var folderFiles = await AppInfo.Instance.SDBackupFolder.GetFilesAsync();
                foreach (StorageFile ep in folderFiles.Where(ep => ep.Name.StartsWith("ep")))
                {
                    await ep.DeleteAsync();
                }
            }
        }

        internal async Task<bool> IsSDCardAvailable()
        {
            try
            {
                StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
                StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
                return sdCard != null;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        internal async Task<bool> InitSDBackupFolder()
        {
            try
            {
                AppInfo.Instance.SDBackupFolder = await InitSDBackupFolderImpl();
            }
            catch (Exception ex)
            {
                //sd not present
                AppInfo.Instance.SDBackupFolder = null;
                return false;
            }
            return true;
        }

        private async Task<StorageFolder> InitSDBackupFolderImpl()
        {
            try
            {
                StorageFolder backupFolder = null;
                StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
                StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
                try
                {
                    backupFolder = await sdCard.GetFolderAsync(AppInfo.BackupFolderOnSDCard);
                }
                catch (Exception)
                {
                }
                if (backupFolder == null)
                {
                    backupFolder = (await sdCard.CreateFolderAsync(AppInfo.BackupFolderOnSDCard));
                }
                return backupFolder;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region misc
        internal static string GetOfflineFileName(string id)
        {
            return string.Format("ep_{0}.mp4", id);
        }

        internal static string GetAppversion(VersionFormat format)
        {
            var appEl = XDocument.Load("WMAppManifest.xml").Root.Element("App");
            var ver = new Version(appEl.Attribute("Version").Value);

            switch (format)
            {
                case VersionFormat.V:
                    return string.Format("{0}", ver.Major);
                case VersionFormat.VR:
                    return string.Format("{0}.{1}", ver.Major, ver.Minor);
            }

            return string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        internal static string GetAppversion()
        {
            return GetAppversion(VersionFormat.VRB);
        }

        internal static string GetYoutubeID(string uri)
        {
            ////http://www.youtube.com/watch?v=1CuGUN_rmpE
            //string id = "Uh_tZEkIVS4";
            if (string.IsNullOrEmpty(uri)) throw new ArgumentException("uri not valid");
            return uri.Substring(uri.IndexOf('=') + 1);
        }
        #endregion


        //public static MyResourceManager Resourcemanager { get; set; }
    }
}
