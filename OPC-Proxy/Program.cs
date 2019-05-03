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


namespace OPC_Proxy
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            JObject j = JObject.Parse("{isInMemory:false, filename:'pollo.dat', juno:'bul'}");
            cacheDB db = new cacheDB(j);

            dbNode node = new dbNode() {
                classType = "pi",
                name = "h",
                internalIndex = 0,
                systemType = "kkk"
            };
            
                        // command line options
            int stopTimeout = Timeout.Infinite;
            bool autoAccept = false;

            string endpointURL;
            endpointURL = "opc.tcp://xeplc.physik.uzh.ch:4840/s7OPC";

            MySampleClient client = new MySampleClient(endpointURL, autoAccept, stopTimeout);
            Console.WriteLine("----> ", TypeInfo.GetBuiltInType("i=4").ToString());
                Console.WriteLine("----> ", TypeInfo.GetBuiltInType("i=63").ToString());
            client.Run();
            
           
            
            return (int)MySampleClient.ExitCode;

            // db.Dispose();
        }
    }
}
