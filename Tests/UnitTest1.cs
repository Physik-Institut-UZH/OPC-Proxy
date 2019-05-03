using System;
using Xunit;
using ProxyUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using converter;

namespace Tests
{

    public class UnitTest1
    {
        JObject j = JObject.Parse("{isInMemory:false, filename:'pollo.dat', juno:'bul'}");

        [Fact]
        public void Test1()
        {
            cacheDB c = new cacheDB(j);
            Assert.NotNull(c);
        }

        [Fact]
        public void loadNodesInchacheDB(){
            Opc.Ua.NamespaceTable nt = new Opc.Ua.NamespaceTable();
            nt.Append("http://www.siemens.com/simatic-s7-opcua");
            UANodeConverter ua = new UANodeConverter("ppp", nt);
            cacheDB c = new cacheDB(j);
            ua.fillCacheDB(c);

            var q =  Enumerable.ToArray(c.nodes.Find( Query.EQ("name","ciao")));
            Assert.Equal(q.Length, 1);
            
        }
    }
}
