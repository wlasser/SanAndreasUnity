﻿using System;
using System.Linq;
using System.Security.Cryptography;
// ReSharper disable once RedundantUsingDirective
using UnityEngine;

namespace Facepunch.Networking
{
    static class NetConfig
    {
        public static bool IsDefinedByCommandLine { get; private set; }

        public static bool IsClient { get; set; }

        public static bool IsServer { get; set; }

        public static int Port { get; set; }

        public static int RconPort { get; set; }

        public static String RconPassword { get; set; }

        public static int MaxConnections { get; set; }

        public static String Hostname { get; set; }

        public static bool AutoUpdate { get; set; }

        // TODO: replace this with something better?
        public static uint IP
        {
            get
            {
                uint ip = 0;

                if (!string.IsNullOrEmpty(Hostname))
                {
                    var addressList = System.Net.Dns.GetHostEntry(Hostname).AddressList;

                    if (addressList.Length == 0)
                    {
                        return ip;
                    }

                    System.Net.IPAddress.Parse(Hostname).GetAddressBytes();

                    var ipBytes = addressList[0].GetAddressBytes();
                    ip = (uint)ipBytes[0] << 24;
                    ip += (uint)ipBytes[1] << 16;
                    ip += (uint)ipBytes[2] << 8;
                    ip += (uint)ipBytes[3];
                }

                return ip;
            }
        }

        public static void ListenServer(int port, int rconPort, int maxConnections)
        {
            IsClient = true;
            IsServer = true;

            Port = port;
            RconPort = rconPort;
            MaxConnections = maxConnections;

            Hostname = "localhost";
        }

        public static void DedicatedServer(int port, int rconPort, int maxConnections)
        {
            IsClient = false;
            IsServer = true;

            Port = port;
            RconPort = rconPort;
            MaxConnections = maxConnections;
        }

        public static void Client(String hostname, int port)
        {
            IsClient = true;
            IsServer = false;

            Port = port;
            Hostname = hostname;
        }

        static NetConfig()
        {
#if UNITY_EDITOR
            RconPassword = "IDGKGRGdkKqZuBdv";
            ListenServer(Application.DefaultPort, Application.DefaultRconPort, 8);
#else
            Port = Application.DefaultPort;
            RconPort = Application.DefaultRconPort;
            Hostname = "localhost";
            MaxConnections = 8;

            var bytes = new byte[8];
            RandomNumberGenerator.Create().GetBytes(bytes);

            RconPassword = String.Join("", bytes.Select(x => x.ToString("x2")).ToArray());

            var args = Environment.GetCommandLineArgs();
            for (var i = 1; i < args.Length - 1; ++i) {
                switch (args[i]) {
                    case "--client":
                        IsClient = true;
                        IsDefinedByCommandLine = true;
                        break;
                    case "--server":
                        IsServer = true;
                        IsDefinedByCommandLine = true;
                        break;
                    case "--port":
                        int port;
                        if (int.TryParse(args[++i], out port)) {
                            Port = port;
                            break;
                        }

                        Debug.LogErrorFormat("Invalid port '{0}'.", args[i]);
                        break;
                    case "--rcon-port":
                        int rconPort;
                        if (int.TryParse(args[++i], out rconPort)) {
                            RconPort = rconPort;
                            break;
                        }

                        Debug.LogErrorFormat("Invalid rcon port '{0}'.", args[i]);
                        break;
                    case "--rcon-password":
                        RconPassword = args[++i];
                        break;
                    case "--max-connections":
                        int maxConnections;
                        if (int.TryParse(args[++i], out maxConnections)) {
                            MaxConnections = maxConnections;
                            break;
                        }

                        Debug.LogErrorFormat("Invalid max connection count '{0}'.", args[i]);
                        break;
                    case "--hostname":
                        Hostname = args[++i];
                        break;
                    case "--autoupdate":
                        AutoUpdate = true;
                        break;
                }
            }
#endif
        }
    }
}
