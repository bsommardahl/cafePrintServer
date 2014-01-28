using System;
using System.Configuration;
using Topshelf;

namespace CafePrintServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"] ?? "9000");

            HostFactory.Run(
                x =>
                    {
                        x.Service<PrintServerService>(
                            s =>
                                {
                                    s.ConstructUsing(
                                        name =>
                                        new PrintServerService(port));
                                    s.WhenStarted(tc => tc.Start());
                                    s.WhenStopped(tc => tc.Stop());
                                });
                        x.RunAsLocalSystem();

                        x.SetDisplayName("Cafe Print Server");
                        x.SetDescription("Receipt printing server for the Cafe application.");
                        x.SetServiceName("CafePrintServer");
                    });
        }
    }
}