using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace CafePrintServer
{
    public class MyBootstrapper : DefaultNancyBootstrapper
    {
        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            Console.WriteLine("Incoming Request: {0} {1}", context.Request.Method, context.Request.Url);

            AllowAccessToConsumingSite(pipelines);

            base.RequestStartup(container, pipelines, context);
        }

        static void AllowAccessToConsumingSite(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(x =>
            {
                x.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                x.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
                x.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            });
        }
    }
}