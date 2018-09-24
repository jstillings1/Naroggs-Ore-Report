using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Ore_Report
{
    /// <summary>
    /// A page for first scenario.
    /// </summary>


    public sealed partial class Scenario5 : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private MainPage rootPage = MainPage.Current;

        // List containing all available local HostName endpoints
        private List<LocalHostItem> localHostItems = new List<LocalHostItem>();

        public Scenario5()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }



        /// <summary>
        /// Listen handler
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private void Listen_Click(object sender, RoutedEventArgs e)
        {
            DatagramSocket listener = new DatagramSocket();
            Magic(listener);
        }

        private async void Magic(DatagramSocket listener)
        {
            AdapterList.IsEnabled = true;
            if (System.String.IsNullOrEmpty(Port.Text))
            {
                rootPage.NotifyUser("Please provide a service name.", NotifyType.ErrorMessage);
                return;
            }



            if (CoreApplication.Properties.ContainsKey("listener"))
            {
                rootPage.NotifyUser(
                    "This step has already been executed. Please move to the next one.",
                    NotifyType.ErrorMessage);
                return;
            }










            LocalHostItem selectedLocalHost = null;


            selectedLocalHost = (LocalHostItem)AdapterList.SelectedItem;
            if (selectedLocalHost == null)
            {
                rootPage.NotifyUser("Please select an address / adapter.", NotifyType.ErrorMessage);
                return;
            }

            // The user selected an address. For demo purposes, we ensure that connect will be using the same 
            // address.



            // Save the socket, so subsequent steps can use it.
            CoreApplication.Properties.Add("listener", listener);

            // Start listen operation.
            try
            {

                // Try to bind to a specific address.

                await listener.BindEndpointAsync(selectedLocalHost.LocalHost, Port.Text);
                rootPage.NotifyUser(
                        "Socket Opened on address " + selectedLocalHost.LocalHost.CanonicalName,
                        NotifyType.StatusMessage);

            }
            catch (Exception exception)
            {
                CoreApplication.Properties.Remove("listener");

                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                rootPage.NotifyUser(
                    "Start listening failed with error: " + exception.Message,
                    NotifyType.ErrorMessage);
            }
            // Dump the data to a file
            // stream whe are dumping
            // socket.OutputStream
            // Create a file picker.

            FileSavePicker picker = new FileSavePicker();

            // Set properties on the file picker such as start location and the type
            // of files to display.
            var file = await StorageFile.CreateStreamedFileAsync("file.txt", FileGrabber, null);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.DefaultFileExtension = ".txt";
            picker.FileTypeChoices.Add("Text", new string[] { ".txt" });
            StorageFile target = await NewMethod(picker);
            await file.CopyAndReplaceAsync(target);

            async void FileGrabber(StreamedFileDataRequest request)
            {
                try
                {
                    using (var stream = request.AsStreamForWrite())
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        // fancy convert from a IOutput stream into a Char s it can be writen to a file
                        for (int i = 0; i < 5000; i++)
                        {

                            try
                            {
                                byte[] bytes = ReadFully(GetOutputStream(listener));
                                char single;
                                foreach (byte number in bytes)
                                {
                                    single = Convert.ToChar(number);
                                    await streamWriter.WriteLineAsync(single);
                                }
                            }
                            catch (System.NotSupportedException ex)
                            {
                                Output.Text = "Error occurred  " + ex.Message;
                                await streamWriter.WriteLineAsync(ex.Message);
                                rootPage.NotifyUser(
                                 "Error occurred reading the file. " + ex.Message,
                                NotifyType.ErrorMessage);
                            }

                            await streamWriter.WriteLineAsync("Your data stream is blank");
                        }

                    }
                    request.Dispose();
                }
                catch (System.NotSupportedException ex)
                {
                    request.FailAndClose(StreamedFileFailureMode.Incomplete);

                    rootPage.NotifyUser(
                    "Error occurred reading the file. " + ex.Message,
                    NotifyType.ErrorMessage);
                }
            }
            // Reader stuff
            StorageFile result = file;
            if (result != null)
            {
                try
                {
                    // Retrieve the stream. This method returns a IRandomAccessStreamWithContentType.
                    var stream = await result.OpenReadAsync();

                    // Convert the stream to a .NET stream using AsStream, pass to a 
                    // StreamReader and read the stream.
                    using (StreamReader sr = new StreamReader(stream.AsStream()))
                    {
                        Output.Text = sr.ReadToEnd();
                    }
                }
                catch (System.NotSupportedException ex)
                {

                    rootPage.NotifyUser(
                    "Error occurred reading the file." + ex.Message,
                    NotifyType.ErrorMessage);
                }
            }
            else
            {

                rootPage.NotifyUser(
                    "User did not pick a file",
                    NotifyType.ErrorMessage);
            }

        }

        
    

        //Helper classes for the fancy convert!


            /// <summary>
            /// Convert Ioutput to a generic Stream
            /// </summary>
            /// <param name="socket"></param>
            /// <returns> Hopefuly a readable stream </returns>
        private Stream GetOutputStream(DatagramSocket socket)
        {
            Stream raw = null;
            try
            {
                raw = Io(socket).AsStreamForWrite();
                return raw;
            }
            catch (System.NotSupportedException ex)
            {
                Console.WriteLine(ex.Message);

            }

            return raw;

        }

        /// <summary>
        /// Takes socket and returns a Ioutput stream
        /// </summary>
        /// <param name="socket"></param>
        /// <returns> I output stream</returns>
        private IOutputStream Io (DatagramSocket socket)
        {
            return socket.OutputStream;
        }
 
        /// <summary>
        /// Crashy Crashy!
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private  byte[] ReadFully(Stream input)
        {
            
            byte[] buffer = new byte[16 * 1024];
            if (input != null)
            {

                try
                {

                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
                catch (System.NotSupportedException ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
            return buffer;
           
        }

        // end helper classes




        private static async System.Threading.Tasks.Task<StorageFile> NewMethod(FileSavePicker picker)
        {
            return await picker.PickSaveFileAsync();
        }

       


        private void AdapterList_DropDownOpened(object sender, object e)
        {


            PopulateAdapterList();
          
            

        }



        /// Notifies the user from a non-UI thread
        /// </summary>
        /// <param name="strMessage">The message</param>
        /// <param name="type">The type of notification</param>
        private void NotifyUserFromAsyncThread(string strMessage, NotifyType type)
        {
            var ignore = Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => rootPage.NotifyUser(strMessage, type));
        }

        /// <summary>
        /// Populates the NetworkAdapter list
        /// </summary>
        private void PopulateAdapterList()
        {
            localHostItems.Clear();
            AdapterList.ItemsSource = localHostItems;
            AdapterList.DisplayMemberPath = "DisplayString";
            AdapterList.SelectedValuePath = "LocalHost";

            foreach (HostName localHostInfo in NetworkInformation.GetHostNames())
            {
                if (localHostInfo.IPInformation != null)
                {
                    LocalHostItem adapterItem = new LocalHostItem(localHostInfo);
                    localHostItems.Add(adapterItem);
                }
            }
        }





        /// <summary>
        /// Helper class describing a NetworkAdapter and its associated IP address
        /// </summary>
        class LocalHostItem
        {
            public string DisplayString
            {
                get;
                private set;
            }

            public HostName LocalHost
            {
                get;
                private set;
            }

            public LocalHostItem(HostName localHostName)
            {
                if (localHostName == null)
                {
                    throw new ArgumentNullException("localHostName");
                }

                if (localHostName.IPInformation == null)
                {
                    throw new ArgumentException("Adapter information not found");
                }

                this.LocalHost = localHostName;
                this.DisplayString = "Address: " + localHostName.DisplayName +
                    " Adapter: " + localHostName.IPInformation.NetworkAdapter.NetworkAdapterId;
            }
        }

       




    
        private void HostNameForConnect_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void AdapterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdapterList.SelectedValuePath = "LocalHost";
            HostNameForConnect.Text = AdapterList.SelectedValue.ToString();
        }

        
    }
}


