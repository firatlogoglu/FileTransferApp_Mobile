﻿using FileTransferApp_Mobile.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FileTransfer;
using FileTransfer.Communication;

namespace FileTransferApp_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class SendingPage : ContentPage
    {
        private string TargetDeviceIP;
        private string TargetDeviceName;
        private bool isRequestSent = false;
        public SendingPage()
        {
            InitializeComponent();
            //if (!Admob.TestMode)
            //    BannerView.AdsId = Admob.BannerAdID;
        }
        protected override void OnAppearing()
        {
            //Admob.AdjustBannerView(BannerView);
            list_Devices.SeparatorColor = Color.AliceBlue;
            list_Devices.RefreshControlColor = Color.Black;

            NetworkScanner.OnScanCompleted += NetworkScanner_OnScanCompleted;
            TransferEngine.OnTransferResponded += Main_OnTransferResponded;
            bool isAnyDeviceAvailable = false;
            if (NetworkScanner.PublisherDevices != null && NetworkScanner.PublisherDevices.Count > 0)
            {

                list_Devices.ItemsSource = NetworkScanner.DeviceNames.ToArray();
                list_Devices.SelectedItem = NetworkScanner.DeviceNames[0];
                list_Devices_ItemSelected(null, null);
                isAnyDeviceAvailable = true;
            }
            if (!isAnyDeviceAvailable)
                btn_ScanDevices_Clicked(null, null);
        }
        protected override void OnDisappearing()
        {
            NetworkScanner.OnScanCompleted -= NetworkScanner_OnScanCompleted;
            TransferEngine.OnTransferResponded -= Main_OnTransferResponded;
        }
        protected override bool OnBackButtonPressed()
        {
            if(!isRequestSent)
                return base.OnBackButtonPressed();
            else
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(AppResources.Send_Warning_Goback);
                return true;
            }
        }
        private void Main_OnTransferResponded(bool isAccepted)
        {
            Debug.WriteLine("Receiver Response: " + isAccepted);
            isRequestSent = false;
            if (isAccepted)
            {
                Device.InvokeOnMainThreadAsync(() =>
                {
                    TransferEngine.IsSending = true;
                    Navigation.PushAsync(new TransferPage());
                });
            }
            else
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert(AppResources.Send_Warning_rejected +" "+ TargetDeviceName);
            }
        }

        private void NetworkScanner_OnScanCompleted()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (NetworkScanner.DeviceNames != null)
                {
                    if (NetworkScanner.DeviceNames.Count > 0)
                    {
                        list_Devices.ItemsSource = NetworkScanner.DeviceNames.ToArray();
                        list_Devices.SelectedItem = NetworkScanner.DeviceNames[0];
                        list_Devices_ItemSelected(null, null);
                    }
                }
            });
        }

       
        private void btn_SendFile_Clicked(object sender, EventArgs e)
        {
            if (!isRequestSent)
            {
                Task.Run(() =>
                {
                    using (var progress = Acr.UserDialogs.UserDialogs.Instance.Loading(""))
                    {
                        while (isRequestSent)
                        {
                            //progress.PercentComplete = i;
                            Thread.Sleep(10);
                        }
                    }
                });
                isRequestSent = true;
                TransferEngine.SendFileTo(txt_ClientIP.Text);
                if(NetworkScanner.DeviceNames.Count>0)
                {
                    int index = NetworkScanner.PublisherDevices.FindIndex(x=>x.IP.Equals(txt_ClientIP.Text.ToString()));
                    if(index>=0&& index< NetworkScanner.DeviceNames.Count)
                        TargetDeviceName = NetworkScanner.DeviceNames[index];
                }
            }
        }

        private void list_Devices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (NetworkScanner.DeviceNames.Count <= 0)
                return;
            TargetDeviceIP = NetworkScanner.PublisherDevices.Find(x => x.Hostname.Equals(list_Devices.SelectedItem.ToString())).IP;
            txt_ClientIP.Text = TargetDeviceIP;//list_Clients.SelectedItem.ToString();
        }

        private void btn_ScanDevices_Clicked(object sender, EventArgs e)
        {
            if (NetworkScanner.IsScanning)
                return;
            NetworkScanner.ScanAvailableDevices();
            
            Task.Run(() =>
            {
                Thread.Sleep(500);
                using (var progress = Acr.UserDialogs.UserDialogs.Instance.Progress(AppResources.Send_Scanning))
                {
                    while (NetworkScanner.IsScanning)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (NetworkScanner.DeviceNames != null)
                                if (NetworkScanner.DeviceNames.Count > 0)
                                    list_Devices.ItemsSource = NetworkScanner.DeviceNames.ToArray();

                            progress.PercentComplete = NetworkScanner.ScanPercentage;
                        });
                        Thread.Sleep(100);
                    }
                }
            });
        }
    }
}