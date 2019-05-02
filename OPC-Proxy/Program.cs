using System;
using ProxyUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OPC_Proxy
{
    class Program
    {
        static void Main(string[] args)
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
            // db.Dispose();
        }
    }
}
