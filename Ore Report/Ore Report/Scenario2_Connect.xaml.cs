//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using System;
using System.IO;

namespace Ore_Report
{
    /// <summary>
    /// A page for second scenario.
    /// </summary>
    public sealed partial class Scenario2 : Page
    {

        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        MainPage rootPage = MainPage.Current;
        // List containing all available local HostName endpoints
        private List<LocalHostItem> loopHostItems = new List<LocalHostItem>();

        public Scenario2()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Make sure we're using the correct server address if an adapter was selected in scenario 1
            object serverAddress;
            if (CoreApplication.Properties.TryGetValue("serverAddress", out serverAddress))
            {
                if (serverAddress is string)
                {
                    HostNameForConnect.Text = serverAddress as string;
                }
            }
        }

        /// <summary>
        /// This is the click handler for the 'ConnectSocket' button.
        /// </summary>
        /// <param name="sender">Object for which the event was generated.</param>
        /// <param name="e">Event's parameters.</param>
        private async void ConnectSocket_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ServiceNameForConnect.Text))
            {
                rootPage.NotifyUser("Please provide a service name.", NotifyType.ErrorMessage);
                return;
            }

            // By default 'HostNameForConnect' is disabled and host name validation is not required. When enabling the
            // text box validating the host name is required since it was received from an untrusted source (user 
            // input). The host name is validated by catching ArgumentExceptions thrown by the HostName constructor for
            // invalid input.
            HostName hostName;
            try
            {
                hostName = new HostName(HostNameForConnect.Text);
            }
            catch (ArgumentException)
            {
                rootPage.NotifyUser("Error: Invalid host name.", NotifyType.ErrorMessage);
                return;
            }
            if (CoreApplication.Properties.ContainsKey("clientSocket"))
            {
                rootPage.NotifyUser(
                    "This step has already been executed. Please move to the next one.",
                    NotifyType.ErrorMessage);
                return;
            }

            DatagramSocket socket = new DatagramSocket();

            if (DontFragment.IsOn)
            {
                // Set the IP DF (Don't Fragment) flag.
                // This won't have any effect when running both client and server on localhost.
                // Refer to the DatagramSocketControl class' MSDN documentation for the full list of control options.
                socket.Control.DontFragment = true;
            }

            socket.MessageReceived += MessageReceived;

            // Save the socket, so subsequent steps can use it.
            CoreApplication.Properties.Add("clientSocket", socket);

            rootPage.NotifyUser("Connecting to: " + HostNameForConnect.Text, NotifyType.StatusMessage);

            try
            {
                // Connect to the server (by default, the listener we created in the previous step).
                await socket.ConnectAsync(hostName, ServiceNameForConnect.Text);

                rootPage.NotifyUser("Connected", NotifyType.StatusMessage);

                // Mark the socket as connected. Set the value to null, as we care only about the fact that the 
                // property is set.
                CoreApplication.Properties.Add("connected", null);
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                rootPage.NotifyUser("Connect failed with error: " + exception.Message, NotifyType.ErrorMessage);
            }
            // https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-reading-and-writing-files
        }





        //Helper classes


        private Stream GetOutputStream(DatagramSocket socket)
        {
            Stream raw = Io(socket).AsStreamForWrite();

            return raw;
        }

        private IOutputStream Io(DatagramSocket socket)
        {

            return socket.OutputStream;
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
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

        // end helper classes



        async void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            // Dump the data to a file
            // stream whe are dumping
            // socket.OutputStream
            // Create a file picker.

            // Create a file picker.
            FileSavePicker picker = new FileSavePicker();

            // Set properties on the file picker such as start location and the type
            // of files to display.
            var file = await StorageFile.CreateStreamedFileAsync("file.txt", FileGrabber, null);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.DefaultFileExtension = ".txt";
            picker.FileTypeChoices.Add("Text", new string[] { ".txt" });
            var target = await picker.PickSaveFileAsync();
            await file.CopyAndReplaceAsync(target);

            async void FileGrabber(StreamedFileDataRequest request)
            {
                try
                {
                    using (var stream = request.AsStreamForWrite())
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        for (int i = 0; i < 5000; i++)
                        {
                            byte[] bytes = ReadFully(GetOutputStream(socket));
                            char single;
                            foreach (byte number in bytes)
                            {
                                single = Convert.ToChar(number);
                                await streamWriter.WriteLineAsync(single);
                            }
                        }
                    }
                    request.Dispose();
                }
                catch (Exception)
                {
                    request.FailAndClose(StreamedFileFailureMode.Incomplete);
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
                        TextBlock1.Text = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    TextBlock1.Text = "Error occurred reading the file. " + ex.Message;
                }
            }
            else
            {
                TextBlock1.Text = "User did not pick a file";
            }


      

        
            try
            {
                // Interpret the incoming datagram's entire contents as a string.
                uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                string receivedMessage = eventArguments.GetDataReader().ReadString(stringLength);

                NotifyUserFromAsyncThread(
                    "Received data from remote peer: \"" +
                    receivedMessage + "\"",
                    NotifyType.StatusMessage);
            }
            catch (Exception exception)
            {
                SocketErrorStatus socketError = SocketError.GetStatus(exception.HResult);
                if (socketError == SocketErrorStatus.ConnectionResetByPeer)
                {
                    // This error would indicate that a previous send operation resulted in an 
                    // ICMP "Port Unreachable" message.
                    NotifyUserFromAsyncThread(
                        "Peer does not listen on the specific port. Please make sure that you run step 1 first " +
                        "or you have a server properly working on a remote server.",
                        NotifyType.ErrorMessage);
                }
                else if (socketError != SocketErrorStatus.Unknown)
                {
                    NotifyUserFromAsyncThread(
                        "Error happened when receiving a datagram: " + socketError.ToString(),
                        NotifyType.ErrorMessage);
                }
                else
                {
                    throw;
                }
            }
        }

        private void NotifyUserFromAsyncThread(string strMessage, NotifyType type)
        {
            var ignore = Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => rootPage.NotifyUser(strMessage, type));
        }


        private void HostNameForConnect_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            HostName hostName;
            try
            {
                hostName = new HostName(HostNameForConnect.Text);
            }
            catch (ArgumentException)
            {
                rootPage.NotifyUser("Error: Invalid host name.", NotifyType.ErrorMessage);
                return;
            }
        }



        private void BindToAddress_Checked(object sender, RoutedEventArgs e)
        {
            CoreApplication.Properties.Remove("serverAddress");
            LocalHostItem selectedLoopHost = null;
            AdapterList.IsEnabled = true;
            selectedLoopHost = (LocalHostItem)AdapterList.SelectedItem;
            if (selectedLoopHost == null)
            {
                rootPage.NotifyUser("Please select an address / adapter.", NotifyType.ErrorMessage);
                return;
            }


        }

        private void AdapterList_DropDownOpened(object sender, object e)
        {


            PopulateAdapterList();

        }
        /// <summary>
        /// Populates the NetworkAdapter list
        /// </summary>
        public void PopulateAdapterList()
        {

            LoopHostItems.Clear();
            AdapterList.ItemsSource = LoopHostItems;
            AdapterList.DisplayMemberPath = "DisplayString";
            AdapterList.SelectedValuePath = "LocalHost";


            foreach (HostName loopHostInfo in NetworkInformation.GetHostNames())
            {
                if (loopHostInfo.IPInformation != null)
                {
                    LocalHostItem adapterItem = new LocalHostItem(loopHostInfo);
                    LoopHostItems.Add(adapterItem);

                }
            }
          

        }
        /// <summary>
        /// Helper class describing a NetworkAdapter and its associated IP address
        /// </summary>
        public string HostString { get; private set; }
        private List<LocalHostItem> LoopHostItems { get => loopHostItems; set => loopHostItems = value; }

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


            public LocalHostItem(HostName loopHostName)
            {

                if (loopHostName == null)
                {
                    throw new ArgumentNullException("loopHostName");
                }

                if (loopHostName.IPInformation == null)
                {
                    throw new ArgumentException("Adapter information not found");
                }

                this.LocalHost = loopHostName;
                this.DisplayString = "Address: " + loopHostName.DisplayName +
                    " Adapter: " + loopHostName.IPInformation.NetworkAdapter.NetworkAdapterId;

            }


        }


        private void AdapterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            AdapterList.SelectedValuePath = "LocalHost";
            HostNameForConnect.Text = AdapterList.SelectedValue.ToString();
             
        }

    }
}