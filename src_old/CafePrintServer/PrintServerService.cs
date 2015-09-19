using System;
using System.Configuration;
using Nancy.Hosting.Self;
using log4net;

namespace CafePrintServer
{
    public class PrintServerService
    {
        readonly int _port;
        readonly ILog _log;
        NancyHost _nancyHost;

        public PrintServerService(int port, ILog log)
        {
            _port = port;
            _log = log;
        }

        public void Start()
        {
            string url = "http://localhost:" + _port;
            var host = ConfigurationManager.AppSettings["host"];
            if (host != null)
                url = host;
            
            try
            {
                _nancyHost = new NancyHost(new HostConfiguration
                                               {
                                                   UrlReservations = new UrlReservations
                                                                         {
                                                                             CreateAutomatically = true
                                                                         },
                                                                         RewriteLocalhost = true,
                                                                         

                                               }, new Uri(url));
                _nancyHost.Start();                    
                Console.WriteLine("Listening at {0}...", url);
            }
            catch (Exception ex)
            {
                _log.Error("Could not start server.", ex);
                throw ex;
            }
        }

        public void Stop()
        {
            _nancyHost.Dispose();
        }
    }
}