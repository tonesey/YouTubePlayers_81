using Centapp.CartoonCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Storage;

namespace Centapp.CartoonCommon
{
    class AppInfo
    {
        public const string OfflineIndexFileNameJSON = "offline.json";
        public const string DataBackupFileName = "backup.xml";
        public const string BackupFolderOnSDCard = "PeppaPigBackup";

        private static AppInfo _instance = null;
        public static AppInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppInfo();
                }
                return _instance;
            }
        }

        #region settings persisted into user settings
        public List<int> FavoriteEpisodesIdsSettingValue { set; get; }
        public bool AppIsOfflineSettingValue { set; get; }
        public int OnlineUsagesSettingValue { set; get; }
        public BackupSupportType OfflineSupportTypeSettingValue { set; get; }
        #endregion

        #region runtime settings not persisted
        public StorageFolder SDBackupFolder { set; get; }

        public bool RecognizerInited { set; get; }
        #endregion

        #region readonly settings from app.config
        public BackupSupportType CurrentBackupSupport { get; set; }
        public string AppName { get; set; }
        public string IndexFile { get; set; }
        public bool UseJSon
        {
            get
            {
                return IndexFile != null && IndexFile.Contains("_json");
            }
        }

        public bool DownloadIsAllowed { get; set; }
        public bool InfoPageIsPivot { get; set; }
        public bool ShowOtherApps { get; set; }
        public string CustomFirstPivotItemName { get; set; }

        public CultureInfo NeutralCulture { get; set; }
        public bool IsMonoLang { get; set; }
        public bool UseResManager { get; set; }
        public int EpisodesLength { get; set; }

        #region adv/analytics
        public string MtiksId { get; set; }
        public bool IsAdvertisingEnabled { get; set; }
        public AdvProvider AdvProvider { get; set; }
        public string AdUnitId { get; set; }
        public string ApplicationId { get; set; }
        public string AdSpaceId { get; set; }
        public string AdPublisherId { get; set; }
        #endregion
        #endregion



        public bool EpisodesGroupedBySeasons { get; set; }
    }
}
