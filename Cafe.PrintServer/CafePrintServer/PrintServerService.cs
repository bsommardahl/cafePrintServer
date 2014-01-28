using System;
using Microsoft.Owin.Hosting;

namespace CafePrintServer
{
    public class PrintServerService
    {
        readonly int _port;
        IDisposable _app;

        public PrintServerService(int port)
        {
            _port = port;
        }

        public void Start()
        {
            string url = "http://*:" + _port;
            _app = WebApp.Start<Startup>(url);
            Console.WriteLine("Listening at {0}...", url);
        }

        public void Stop()
        {
            _app.Dispose();
        }
    }
}