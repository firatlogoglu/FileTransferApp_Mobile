﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FileTransfer;
using FileTransfer.Communication;

namespace FileTransferApp_Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReceivingPage : ContentPage
    {

        public ReceivingPage()
        {
            InitializeComponent();
            loader.Easing = Easing.CubicIn;
            //if (!Admob.TestMode)
            //    BannerView.AdsId = Admob.BannerAdID;
        }
        protected override void OnAppearing()
        {
            //Admob.AdjustBannerView(BannerView);
            TransferEngine.OnClientRequested += Main_OnClientRequested;
        }
        protected override void OnDisappearing()
        {
            TransferEngine.OnClientRequested -= Main_OnClientRequested;
        }
        private void Main_OnClientRequested(string totalTransferSize, string senderDevice, bool isAlreadyAccepted)
        {
            /// Show file transfer request and ask for permission here
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PushAsync(new TransferPermissionPage(totalTransferSize, senderDevice));
            });
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            NetworkScanner.GetDeviceAddress(out string DeviceIP, out string DeviceHostName);
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                lbl_IP.Text = DeviceIP;
                lbl_HostName.Text = Parameters.DeviceName;
            });
        }
    }
}