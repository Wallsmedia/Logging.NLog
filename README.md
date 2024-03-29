# Microsoft.Extensions.Logging.NLog

#### Version 3.1.1
 - **Supports only **Microsoft.AspNetCore.App** 3.1.101-***

#### Version 3.0.0
 - **Supports only **Microsoft.AspNetCore.App** 3.0.0-***

#### Version 2.2.0
 - Support SDK v.2.2.0

#### Version 2.1.0
 - Add support the feature turn on/off scope logging
 - Support SDK v.2.1.3

**Microsoft.Extensions.Logging.NLog** is an adapter between [NLog](https://github.com/NLog/NLog) and [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x).

It allows to simplify using NLog by utilizing [ILoggerFactory](https://github.com/aspnet/Logging) and [ILogger](https://github.com/aspnet/Logging) interfaces in an application.

[NLog](https://github.com/NLog/NLog) is a flexible and free logging platform for various .NET platforms, including .NET standard. NLog makes it easy to write to several targets. (database, file, console) and change the logging configuration on-the-fly.

## Nuget.org

- Nuget package [Microsoft.Extensions.Logging.NLog](https://www.nuget.org/packages/Wallsmedia.Microsoft.Extensions.Logging.NLog/)



## Adding Microsoft.Extensions.Logging.NLog


You have to define two configurations:

### Create the  [NLog configuration](https://github.com/NLog/NLog/wiki/Configuration-file) xml
 ``` xml
 <?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="c:\temp\internal-nlog.txt">

  <!-- https://github.com/NLog/NLog/wiki/Configuration-file  -->
  <targets>
    
    <!-- Null loggers -->
    
    <target xsi:type="Null" name="NullLog" />
    <target xsi:type="Null" name="SystemLog" />
    
    <!-- Console loggers -->
    <target xsi:type="Console" name="ConsoleInfoLog" />
    <target xsi:type="Console" name="ConsoleErrorLog" error="true" />


    <!-- File loggers -->

    <target xsi:type="File" name="CommonInfoLogFile"
            fileName="\Logs\RestWebApplication\Info\RestWebApp_CommonInfo-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception}" />
    
    <target xsi:type="File" name="BusinessErrorLogFile"
            fileName="\Logs\RestWebApplication\BusinessError\RestWebApp_BusinessError-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception}" />

    <target xsi:type="File" name="FatalErrorLogFile"
            fileName="\Logs\RestWebApplication\Error\RestWebApp_FatalError-P_${processid}-${shortdate:universalTime=true}.log"
            layout="${longdate:universalTime=true}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message}| ${exception:innerFormat=Message,Method,StackTrace:maxInnerExceptionLevel=1:format=Message,Method,StackTrace}" />
  
  </targets>

  <rules>
    <!-- Loggers application section -->

    <logger name="FatalError" writeTo="FatalErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
    <logger name="BusinessError" writeTo="BusinessErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
 
    <logger name="CommonInfo" writeTo="CommonInfoLogFile"               minlevel="Info"   final="true" enabled="true" />
    
    <logger name="ConsoleError" writeTo="ConsoleErrorLog,FatalErrorLogFile,CommonInfoLogFile" minlevel="Error"  final="true" enabled="true" />
    <logger name="ConsoleInfo" writeTo="ConsoleInfoLog,CommonInfoLogFile"  minlevel="Info"   final="true" enabled="true" />

    <!-- Other log info -->
    <logger name="Microsoft*"      minlevel="Trace" writeTo="SystemLog"      final="true" enabled="false" />
    <!-- discard all not consumed -->
    <logger name="*" minlevel="Trace" writeTo="NullLog" />
  </rules>

</nlog>
 ```
   
### Create **NLogLoggerSettings** configuration section in "appsettings.json".

The **NLogLoggerSettings** section defines the Category Name "filter" and Category Name "mapper". 

 
``` C#
{
"NLogLoggerSettings": {

    "IncludeScopes": true,

    "AcceptedCategoryNames": [ /* Filter of category name */
      "ConsoleInfo",   /* The category name is accepted as a "NLog logger name" */
      "CommonInfo",    /* The category name is accepted as a "NLog logger name" */
      "ConsoleError",  /* The category name is accepted as a "NLog logger name" */
      "FatalError",    /* The category name is accepted as a "NLog logger name" */
      "BusinessError", /* The category name is accepted as a "NLog logger name" */
      "*Error*",       /* The category name that contains "Error" is accepted as a "NLog logger name" */
      "*Info",         /* The category name that ends with "Info" is accepted as a "NLog logger name" */
      "Com*",          /* The category name that starts with "Com" is accepted as a "NLog logger name" */
      "*"              /* Any category name will be accepted  as a "NLog logger name" */
    ],

    /* Map category name "ABC" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:ABD": "ConsoleError"  
    
    /* Map category name that ends with "*Hosted" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:*Hosted": "ConsoleError"  

    /* Map category name that starts with "Microsoft.AspNetCore*" to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:Microsoft.AspNetCore*": "ConsoleError" 

    /* Map category name that contains "*AspNetCore*" to "NLog logger name" = "ConsoleError"*/
    "AcceptedAliasesCategoryNames:*AspNetCore*": "ConsoleError"

    /* Map any category  to "NLog logger name" = "ConsoleError" */
    "AcceptedAliasesCategoryNames:*": "ConsoleError"

  }
}
```
- The **AcceptedCategoryNames** - "category name filter" is used to **filter-in** category names. It is expected that the category name is exact match to **<logger name="...."**  in the NLog xml configuration.

- The **AcceptedAliasesCategoryNames** - "category name mapper" is used to **filter-in** category names and map them onto new name that expected to be match to **<logger name="..."** in the NLog xml configuration.


### Web Host Builder configuration

After defining the configurations, add in the Web Host Builder configuring of Microsoft.Extensions.Logging.LoggerFactory the following initialization code:

``` C#

     .ConfigureLogging((hostingContext, logging) =>
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

        logging.AddNLogLogger(logJsonCgf, nLoggingConfiguration);
      }
```

### Example projects

See sample of pure **NLog** style project [Using Adaptation NLog in .Net Core  Rest Web Application](https://github.com/Wallsmedia/DotNet.Logger/tree/master/samples/RestWebApplication)

See sample of pure **.Net Core Logger** => NLog style project [Using Logger + NLog in .Net Core  Rest Web Application](https://github.com/Wallsmedia/DotNet.Logger/tree/master/samples/RestWebApplication-Logger)

## Microsoft.Extensions.Logging - Configuration
If you decided to use additional filtering from **Microsoft.Extensions.Logging** over the filtering that provided  with **Microsoft.Extensions.Logging.NLog** by adding configuration:

``` C#
 logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
```
So, It is recommend to read "how to configure the Logging'" section from ASP.NET CORE web guide https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.0#configuration. 


