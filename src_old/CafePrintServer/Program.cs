using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Topshelf;
using log4net;
using log4net.Config;

namespace CafePrintServer
{
    class Program
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            int port = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"] ?? "9000");

            LogIpAddresses();

            HostFactory.Run(
                x =>
                    {
                        x.Service<PrintServerService>(
                            s =>
                                {
                                    s.ConstructUsing(
                                        name =>
                                        new PrintServerService(port, Log));

                                    s.WhenStarted(tc =>
                                                      {
                                                          tc.Start();
                                                          Log.Info("CafePrintServer Started");
                                                      });

                                    s.WhenStopped(tc =>
                                                      {
                                                          tc.Stop();
                                                          Log.Info("CafePrintServer Stopped");
                                                      });
                                });
                        x.RunAsLocalSystem();

                        x.SetDisplayName("Cafe Print Server");
                        x.SetDescription("Receipt printing server for the Cafe application.");
                        x.SetServiceName("CafePrintServer");
                    });
        }

        static void LogIpAddresses()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            var ips = new List<string>();
            for (int i = 0; i < addr.Length; i++)
            {
                IPAddress ipAddress = addr[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    ips.Add(ipAddress.ToString());
            }
            Log.Info(string.Format("Starting cafe print server with the following ip addresses: {0}",
                                   string.Join(", ", ips)));
        }
    }
}