﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.IntegrationTests
{
    public enum HttpProtocol
    {
        HTTP,
        HTTPS
    }

    public static class Settings
    {
#pragma warning disable CS0649 //Ignore 'field' is never assigned to.
        public static string ServerWithProto => $"{Protocol.ToString().ToLower()}://{Server}";
        public static bool ResetAfterTests = true;

        public static HttpProtocol? Protocol = null;

        public static string Server = null;
        public static string Username = null;
        public static string Password = null;

        public static string WindowsUsername;
        public static string WindowsPassword;

        public static int Probe = -1;
        public static int Group = -1;
        public static int Device = -1;

        public static string DeviceName; //Must contain "prtg" in the name
        public static string DeviceTag;

        public static int UpSensor = -1;
        public static int WarningSensor = -1;
        public static int DownSensor = -1;
        public static int DownAcknowledgedSensor = -1;
        public static int PausedSensor = -1;
        public static int PausedByDependencySensor = -1;
        public static int UnknownSensor = -1; //NetFlow sensors work best
        public static int ChannelSensor = -1;

        public static int Channel = -1;
        public static int ChannelErrorLimit = -1;   //Max: positive of value. Min: negative of value
        public static int ChannelWarningLimit = -1; //Max: positive of value. Min: negative of value
        public static string ChannelErrorMessage;
        public static string ChannelWarningMessage;

        public static int ProbesInTestServer = -1;

        public static int GroupsInTestProbe = -1;

        public static int DevicesInTestGroup = -1;
        public static int DevicesInTestProbe = -1;

        public static int SensorsInTestDevice = -1;
        public static int SensorsInTestGroup = -1;
        public static int SensorsInTestProbe = -1;
        public static int SensorsInTestServer = -1;

        public static int ChannelsInTestSensor = -1;

        public static int NotificationTiggersOnDevice = -1;
        public static int NotificationActionsInTestServer = -1;

#pragma warning restore CS0649 //Restore 'field' is never assigned to.

        //PowerShell Settings

        static Settings()
        {
            //Specify setting values below


        }
    }
}
