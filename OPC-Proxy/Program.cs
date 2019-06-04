using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ProxyUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NetCoreConsoleClient;

using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua.Client.Controls;

using NLog;

namespace OPC_Proxy
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static int Main(string[] args)
        {
            init_logging();

            logger.Info("OPC-Proxy starting up...");

            JObject config = JObject.Parse(
                "{isInMemory:true, filename:'pollo.dat', stopTimeout:-1, autoAccept:false, endpointURL:'opc.tcp://xeplc.physik.uzh.ch:4840/s7OPC'}"
            );
            
            serviceManager man = new serviceManager(config);
            
            man.connectOpcClient();
            man.browseNodesFillCache();
            man.subscribeOpcNodes();

            ManualResetEvent quitEvent = new ManualResetEvent(false);
            try
            {
                Console.CancelKeyPress += (sender, eArgs) =>
                {
                    quitEvent.Set();
                    eArgs.Cancel = true;
                };

            }
            catch
            {
            }

            // wait for timeout or Ctrl-C
            quitEvent.WaitOne(-1);
            
                       
            return (int)OPCclient.ExitCode;

            // db.Dispose();
        }



        public static void init_logging(){
            // Logging 
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            //var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
    }
}
