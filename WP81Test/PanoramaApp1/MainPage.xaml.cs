using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.BackgroundAudio;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using System.Windows.Media.Imaging;

namespace PanoramaApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }


        // http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh758325.aspx

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // var files = (await KnownFolders.MusicLibrary.GetFilesAsync()).ToList();
            // var files1 = (await KnownFolders.RemovableDevices.GetFilesAsync()).ToList();

            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            // Get the first child folder, which represents the SD card.
            StorageFolder firstFolder = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
            StorageFolder sdCard = null;
            sdCard = await firstFolder.GetFolderAsync("MyBackup111");
            if (sdCard == null)
            {
                sdCard = (await firstFolder.CreateFolderAsync("MyBackup111"));
            }
            
            if (sdCard != null)
            {
                //An SD card is present and the sdCard variable now contains a reference to it.

                //var sdFiles = (await sdCard.GetFilesAsync()).ToList();

                //remove
                var testFile = (await sdCard.GetFileAsync("sample.txt"));
                if (testFile != null) {
                    testFile.DeleteAsync(StorageDeleteOption.Default);
                }

                //StorageFolder folder = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

                //file write
                StorageFile sampleFile = await sdCard.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);
                IRandomAccessStream stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    DataWriter dataWriter = new DataWriter(outputStream);
                    dataWriter.WriteString("sd file sample content");
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
                //file read
                //var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                StorageFile readFile = await sdCard.GetFileAsync("sample.txt");
                string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
                MessageBox.Show(text);

            }
            else
            {
                // No SD card is present.
                MessageBox.Show("SD card not present");
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.OpenReadAsync(new Uri("http://www.microsoft.com/global/en-us/news/publishingimages/logos/MSFT_logo_Web.jpg"));

            //var imageUrl = "http://www.microsoft.com/global/en-us/news/publishingimages/logos/MSFT_logo_Web.jpg";
            //var client = new HttpClient();
            //Stream stream = await client.GetStreamAsync(imageUrl);
            //var memStream = new MemoryStream();
            //await stream.CopyToAsync(memStream);

        }
        async void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            var stream = e.Result;
            //http://stackoverflow.com/questions/7669311/is-there-a-way-to-convert-a-system-io-stream-to-a-windows-storage-streams-irando

            Windows.Storage.Streams.IInputStream inStream = stream.AsInputStream();

            StorageFolder pictLibrary = Windows.Storage.KnownFolders.PicturesLibrary;
            StorageFolder storageFolder = (await pictLibrary.GetFoldersAsync()).FirstOrDefault();

            if (storageFolder != null)
            {
                StorageFile sampleFile = await storageFolder.CreateFileAsync("sample.png", CreationCollisionOption.ReplaceExisting);
                IRandomAccessStream outStream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                using (var outputStream = outStream.GetOutputStreamAt(0))
                {
                    DataWriter dataWriter = new DataWriter(outputStream);
                    //dataWriter.WriteString("sd file sample content");

                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    //classic stream
                    //while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    //{
                    //    //stream.Write(buffer, 0, bytesRead);
                    //    dataWriter.WriteBytes(buffer);
                    //}

                    //new stream
                    while ((bytesRead = inStream.AsStreamForRead().Read(buffer, 0, buffer.Length)) > 0)
                    {
                        //stream.Write(buffer, 0, bytesRead);
                        dataWriter.WriteBytes(buffer);
                    }

                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();

                    //var memStream = new MemoryStream();
                    //await stream.CopyToAsync(memStream);
                    //memStream.Position = 0;
                    //var bitmap = new BitmapImage();
                    //bitmap.SetSource(memStream.AsRandomAccessStream());
                    //image.Source = bitmap;
                }
            }
            
          

        }

      

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
        }
    }
}