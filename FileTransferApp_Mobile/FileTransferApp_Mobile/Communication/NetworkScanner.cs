﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

class NetworkScanner
{
    private static List<string[]> DeviceList = new List<string[]>();
    public delegate void ScanNetworkEvent(List<string[]> availableDevices);
    public static event ScanNetworkEvent OnScanCompeleted;

    private static int numberofPingedDevice = 0;
    private static int numberofPingResponses = 0;
    public static void ScanAvailableDevices()
    {
        string gate_ip = NetworkGateway();
        string[] array = gate_ip.Split('.');
            for (int i = 2; i <= 100; i++)
            {
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
                Ping(ping_var, 8, 4000);
            }
        //Task.Run(() => {
        //    for (int i = 51; i <= 100; i++)
        //    {
        //        string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
        //        Ping(ping_var, 8, 4000);
        //    }
        //});
        //Task.Run(() => {
        //    for (int i = 101; i <= 150; i++)
        //    {
        //        string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
        //        Ping(ping_var, 8, 4000);
        //    }
        //});
        //Task.Run(() => {
        //    for (int i = 151; i <= 200; i++)
        //    {
        //        string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
        //        Ping(ping_var, 8, 4000);
        //    }
        //});
        //Task.Run(() => {
        //    for (int i = 201; i <= 255; i++)
        //    {
        //        string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
        //        Ping(ping_var, 8, 4000);
        //    }
        //});
    }
    private static string NetworkGateway()
    {
        //string ip = null;

        //foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
        //{
        //    if (f.OperationalStatus == OperationalStatus.Up)
        //    {
        //        foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
        //        {
        //            ip = d.Address.ToString();
        //        }
        //    }
        //}
        return "192.168.1.1";
    }

    private static void Ping(string host, int attempts, int timeout)
    {
        numberofPingedDevice++;
        for (int i = 0; i < attempts; i++)
        {
            new Thread(delegate ()
            {
                try
                {
                    System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                    ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                    ping.SendAsync(host, timeout, host);
                }
                catch
                {
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
            }).Start();
        }
    }
    private static void PingCompleted(object sender, PingCompletedEventArgs e)
    {
        
        string ip = (string)e.UserState;
        if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        {
            string hostname = GetHostName(ip);
            ///string macaddres = GetMacAddress(ip);
            string[] arr = new string[2];

            //store all three parameters to be shown on ListView
            arr[0] = ip;
            arr[1] = hostname;
            //arr[2] = macaddres;
            if (!DeviceList.Contains(arr))
            { 
                DeviceList.Add(arr);
                Debug.WriteLine("device: " + arr[0]);
            }
        }
        else
        {
            // MessageBox.Show(e.Reply.Status.ToString());
        }
        numberofPingResponses++;
        if (numberofPingedDevice >50 )
            OnScanCompeleted(DeviceList);
    }
    public static string GetHostName(string ipAddress)
    {
        try
        {
            IPHostEntry entry = Dns.GetHostEntry(ipAddress);
            if (entry != null)
            {
                return entry.HostName;
            }
        }
        catch (SocketException)
        {
            // MessageBox.Show(e.Message.ToString());
        }

        return null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public static string GetMacAddress(string ipAddress)
    {
        string macAddress = string.Empty;
        System.Diagnostics.Process Process = new System.Diagnostics.Process();
        Process.StartInfo.FileName = "arp";
        Process.StartInfo.Arguments = "-a " + ipAddress;
        Process.StartInfo.UseShellExecute = false;
        Process.StartInfo.RedirectStandardOutput = true;
        Process.StartInfo.CreateNoWindow = true;
        Process.Start();
        string strOutput = Process.StandardOutput.ReadToEnd();
        string[] substrings = strOutput.Split('-');
        if (substrings.Length >= 8)
        {
            macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                     + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                     + "-" + substrings[7] + "-"
                     + substrings[8].Substring(0, 2);
            return macAddress;
        }
        else
        {
            return "OWN Machine";
        }
    }
    /// <summary>
    /// Gets current device's hostname.
    /// </summary>
    /// <returns>ip as string</returns>
    public static string GetDeviceHostName()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.HostName;
    }
}
 