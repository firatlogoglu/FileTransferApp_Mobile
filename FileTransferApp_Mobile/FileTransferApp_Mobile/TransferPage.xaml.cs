﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FileTransferApp_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferPage : ContentPage
    {
        private Timer UpdateTimer;
        private int updatePeriod = 40;          ///ms
        public TransferPage()
        {
            InitializeComponent();
            //
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartUpdatingUI();
        }
        private void StartUpdatingUI()
        {
            UpdateTimer = new Timer(Timer_Tick, null, 0, updatePeriod);
            UpdateTimer.Change(0, updatePeriod);
        }
        private void StopUpdateingUI()
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                UpdateTimer.Dispose();
                UpdateTimer = null;
            }
        }
        private void Timer_Tick(object sender)
        {
            if (UpdateTimer == null)
                return;
            UpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Stopwatch stp = Stopwatch.StartNew();
            UpdateUI();
            long elapsedTime = stp.ElapsedMilliseconds;
            if(UpdateTimer!=null)
                UpdateTimer.Change(Math.Max(0, updatePeriod - elapsedTime), updatePeriod);
        }
        
        private void UpdateUI()
        {
            Dispatcher.BeginInvokeOnMainThread(() => {
                lbl_currentFileNumber.Text = Main.TransferMetrics.IndexOfCurrentFile.ToString();
                lbl_FileCount.Text = Main.TransferMetrics.CountOfFiles.ToString();
                lbl_FileSize.Text = Main.TransferMetrics.CurrentFile.FileSize.ToString("0.00") + " " + Main.TransferMetrics.CurrentFile.SizeUnit;
                lbl_FileName.Text = Main.TransferMetrics.CurrentFile.FileName;
                lbl_progress.Text = "%"+Main.TransferMetrics.Progress.ToString("0.0");
                prg_Transfer.Progress = Main.TransferMetrics.Progress / 100.0;
                lbl_transferSpeed.Text = Main.TransferMetrics.TransferSpeed.ToString("0.00") + " MB/s";
                lbl_PassedTime.Text = ((int)Main.TransferMetrics.TotalElapsedTime / 3600).ToString("00") + ":" + (((int)Main.TransferMetrics.TotalElapsedTime % 3600) / 60).ToString("00") + ":" +
                    (((int)Main.TransferMetrics.TotalElapsedTime % 3600) % 60).ToString("00");
                lbl_RemainingTime.Text = ((int)Main.TransferMetrics.EstimatedTime / 3600).ToString("00") + ":" + (((int)Main.TransferMetrics.EstimatedTime % 3600) / 60).ToString("00") + ":" +
                    (((int)Main.TransferMetrics.EstimatedTime % 3600) % 60).ToString("00");
            });
            
            if (!Main.IsTransfering)
                StopUpdateingUI();
        }
        private void btn_AbortTransfer_Clicked(object sender, EventArgs e)
        {
            Main.AbortTransfer();
        }
    }
}