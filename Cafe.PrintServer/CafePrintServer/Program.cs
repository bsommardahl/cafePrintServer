using System;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Routing;
using Microsoft.Owin.Hosting;

namespace CafePrintServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:9000/";            
            WebApp.Start<Startup>(url: baseAddress);
            Console.WriteLine("Listening at {0}...", baseAddress);
            while(true){}
        }
    }
}