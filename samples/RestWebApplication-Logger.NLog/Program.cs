// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright (c) Wallsmedia 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.NLog;
using Microsoft.Extensions.Options;
using NLog.Config;

namespace RestWebApplication
{
    public class Program
    {

        public static void AppConfigure(WebHostBuilderContext hostingContext, ILoggingBuilder logging)
        {

            // ** Add Microsoft.Extensions.Logging.NLog

            string logPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, $"nlog.{hostingContext.HostingEnvironment.EnvironmentName}.config");
            if (!File.Exists(logPath))
            {
                throw new MissingMemberException($"Missing NLog configuration file '{logPath}'");
            }
            var nLoggingConfiguration = new XmlLoggingConfiguration(logPath);

            var logJsonCgf = hostingContext.Configuration.GetSection(nameof(NLogLoggerSettings));
            if (!logJsonCgf.Exists())
            {
                throw new MissingMemberException($"Missing configuration section '{nameof(NLogLoggerSettings)}'");
            }

            logging.ClearProviders();
            logging.AddNLogLogger(logJsonCgf, nLoggingConfiguration);

            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            //logging.AddConsole();
            //logging.AddDebug();
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(AppConfigure);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
