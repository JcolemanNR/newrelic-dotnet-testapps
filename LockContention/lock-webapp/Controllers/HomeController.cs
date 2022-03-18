﻿using NewRelic.Api.Agent;
using shared_remoting_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace lock_webapp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Test()
        {
            ViewBag.Message = "Your Test page.";
            var rnd = new Random(Environment.TickCount);
            var depth = rnd.Next(100);
            var result = new ContentResult();

            NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("ExpectedSpans", depth + 2);

            result.ContentType = "text";
            result.Content = InstrumentedMethod(0, depth);

            return View();
        }

        private static bool RemotingInitialized = false;
        private void InitializeRemoting()
        {
            if (!RemotingInitialized)
            {
                // Create the channel.
                IpcChannel channel = new IpcChannel();

                // Register the channel.
                System.Runtime.Remoting.Channels.ChannelServices.
                    RegisterChannel(channel, false);

                // Register as client for remote object.
                System.Runtime.Remoting.WellKnownClientTypeEntry remoteType =
                    new System.Runtime.Remoting.WellKnownClientTypeEntry(
                        typeof(RemoteObject),
                        "ipc://localhost:9090/RemoteObject.rem");
                System.Runtime.Remoting.RemotingConfiguration.
                    RegisterWellKnownClientType(remoteType);

                // Create a message sink.
                string objectUri;
                System.Runtime.Remoting.Messaging.IMessageSink messageSink =
                    channel.CreateMessageSink(
                        "ipc://localhost:9090/RemoteObject.rem", null,
                        out objectUri);
                Console.WriteLine("The URI of the message sink is {0}.",
                    objectUri);

                if (messageSink != null)
                {
                    Console.WriteLine("The type of the message sink is {0}.",
                        messageSink.GetType().ToString());
                }
                RemotingInitialized = true;
            }
        }

        public ActionResult ThrowException()
        {
            throw new Exception("OH NO!!!!");

            return View();
        }

        public ActionResult DoIpcStuff()
        {
            InitializeRemoting();

            // Create an instance of the remote object.
            RemoteObject service = new RemoteObject();

            ViewBag.Message = $"The remote object has been called {service.GetCount()} times.";

            return View();
        }

        public ActionResult CreateFailedDbConnectionViaIPC()
        {
            InitializeRemoting();

            // Create an instance of the remote object.
            RemoteObject service = new RemoteObject();

            ViewBag.Message = $"The remote object has been called, and returned message: {service.CreateFailedDbConnection()} times.";

            return View();
        }

        [Transaction]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static string InstrumentedMethod(int level, int maxLevel)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(2));

            var sbResult = new StringBuilder()
                .AppendLine($"{level + 1} of {maxLevel}");

            if (level + 1 < maxLevel)
            {
                sbResult.AppendLine(InstrumentedMethod(level + 1, maxLevel));
            }

            return sbResult.ToString();
        }

    }
}