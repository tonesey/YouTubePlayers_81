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
            StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

            if (sdCard != null)
            {
                //An SD card is present and the sdCard variable now contains a reference to it.

                var sdFiles = (await sdCard.GetFilesAsync()).ToList();

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


                //using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(curEpisodeFileName, System.IO.FileMode.Create, _isoStore))
                //{
                //    byte[] buffer = new byte[1024];
                //    //while (e.Result.Read(buffer, 0, buffer.Length) > 0)
                //    //{
                //    //    stream.Write(buffer, 0, buffer.Length);
                //    //}
                //    //18-07-2013 - così non viene scritto tutto il buffer ma solo i dati realmente necessari
                //    int bytesRead;
                //    while ((bytesRead = e.Result.Read(buffer, 0, buffer.Length)) > 0)
                //    {
                //        stream.Write(buffer, 0, bytesRead);
                //    }
                //}

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

        async void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            var stream = e.Result;
            //http://stackoverflow.com/questions/7669311/is-there-a-way-to-convert-a-system-io-stream-to-a-windows-storage-streams-irando

            //Windows.Storage.Streams.IInputStream inStream = stream.AsInputStream();
            //Windows.Storage.Streams.IOutputStream outStream = stream.AsOutputStream();




            //using (var outputStream = outStream)
            //{
            //    DataWriter dataWriter = new DataWriter(outputStream);
            //    dataWriter.WriteString("sd file sample content");
            //    await dataWriter.StoreAsync();
            //    await outputStream.FlushAsync();
            //}

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.OpenReadAsync(new Uri("http://TODO"));


            var imageUrl = "http://www.microsoft.com/global/en-us/news/publishingimages/logos/MSFT_logo_Web.jpg";
            var client = new HttpClient();
            Stream stream = await client.GetStreamAsync(imageUrl);
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);

        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
        }
    }
}