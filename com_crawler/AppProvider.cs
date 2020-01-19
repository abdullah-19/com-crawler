﻿// This source code is a part of Community Crawler Project.
// Copyright (C) 2020. rollrat. Licensed under the MIT Licence.

using com_crawler.Cache;
using com_crawler.Log;
using com_crawler.Network;
using com_crawler.Postprocessor;
using com_crawler.Proxy;
using com_crawler.Setting;
using com_crawler.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace com_crawler
{
    /// <summary>
    /// Community Crawler App-Provider
    /// </summary>
    public class AppProvider
    {
        public static string ApplicationPath = Directory.GetCurrentDirectory();
        public static string DefaultSuperPath = Directory.GetCurrentDirectory();

        public static Dictionary<string, object> Instance =>
            InstanceMonitor.Instances;

        public static NetScheduler Scheduler { get; set; }

        public static PostprocessorScheduler PPScheduler { get; set; }

        public static bool Initialized { get; set; }

        public static bool Initialize()
        {
            if (Initialized)
            {
                Logs.Instance.Push("App provider already initialized!");
                return true;
            }

            // Initialize logs instance
            Logs.Instance.Push("App provider initializing...");

            // Check program crashed.
            if (ProgramLock.ProgramCrashed)
                Logs.Instance.Push("Program is terminated abnormally.");

#if DEBUG
            // Check exists instances.
            if (Instance.Count > 1)
                throw new Exception("You must wait for app-provider initialization procedure before using instance-lazy!\n" +
                    "For more informations, see the development documents.");
#endif

            // GC Setting
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            // Extends Connteion Limit
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            // Initialize Scheduler
            Scheduler = new NetScheduler(Settings.Instance.Model.ThreadCount);

            // Initialize Postprocessor Scheduler
            PPScheduler = new PostprocessorScheduler(Settings.Instance.Model.PostprocessorThreadCount);

            Logs.Instance.Push("App provider starts.");

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            if (Settings.Instance.Network.UsingFreeProxy && FreeProxy.Instance.IsBuildRequire())
            {
                if (FreeProxy.Instance.IsBuildRequire())
                {
                    Logs.Instance.Push("You must build proxy to use free-proxy. Please run with --build-free-proxy before using free-proxy.");
                }
                else
                {
                    FreeProxy.Instance.Load();
                }
                FreeProxyPass.Init();
            }

            Initialized = true;

            return true;
        }

        public static void Deinitialize()
        {
            Logs.Instance.Push("App provider de-initialized.");
        }
    }
}
