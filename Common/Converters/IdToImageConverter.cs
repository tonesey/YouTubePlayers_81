using Centapp.CartoonCommon.Helpers;
using Centapp.CartoonCommon.ViewModels;
using MyToolkit.Multimedia;
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace Centapp.CartoonCommon.Converters
{
    public class IdToImageConverter : IValueConverter
    {
        private readonly object _readLock = new object();

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

#if DEBUGOFFLINE
            return null;
#endif

            if (value == null) return null;

            if (!AppInfo.Instance.AppIsOfflineSettingValue)
            {
                return new BitmapImage(YouTube.GetThumbnailUri(GenericHelper.GetYoutubeID((value as ItemViewModel).Url)));
            }

            BitmapImage image = new BitmapImage();
            lock (_readLock)
            {
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var curThumbName = string.Format("thumb_{0}.png", (value as ItemViewModel).Id);
                    if (!isoStore.FileExists(curThumbName))
                    {
                        //prevedere un img di default? in teorian non dovrebbe MAI passare da qua se offline
                        curThumbName = string.Format("thumb_1.png");
                    }
                    //using (var stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(curThumbName, System.IO.FileMode.Open))
                    using (var stream = new IsolatedStorageFileStream(curThumbName, FileMode.Open, FileAccess.Read, FileShare.None, isoStore))
                    {
                        image.SetSource(stream);
                    }
                }
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}