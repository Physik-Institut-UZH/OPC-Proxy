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

    public class cacheDBTest
    {
        JObject j;
        cacheDB cDB;

        public cacheDBTest(){
            j = JObject.Parse("{isInMemory:true, filename:'pollo.dat', juno:'bul'}");
            cDB = new cacheDB(j);

            Opc.Ua.NamespaceTable nt = new Opc.Ua.NamespaceTable();
            nt.Append("http://www.siemens.com/simatic-s7-opcua");
            UANodeConverter ua = new UANodeConverter("ppp", nt);
            ua.fillCacheDB(cDB);

        }

        [Fact]
        public void dbExist()
        {
            Assert.NotNull(cDB);
        }

        [Fact]
        public void loadNodesInchacheDB(){
            
            Assert.Equal(22, cDB.nodes.Count());

        }

        [Fact]
        public void fillDBWithNewVar(){
            cDB.updateBuffer("ciao",72,DateTime.Now);
            var q = cDB.latestValues.FindOne(Query.EQ("name","ciao"));
            Assert.NotNull(q);
            Assert.Equal(72, q.value);

        }

        [Fact]
        public void readFromDB(){
            cDB.updateBuffer("ciao",72,DateTime.Now);
            var q = cDB.readValue("ciao");
            Assert.NotNull(q);
            Assert.Equal(72, q.value);

            var p = cDB.readValue("ciao1");
            Assert.Equal(-9, p.value);
            Assert.Equal("does_not_exist", p.name);
            Assert.Equal("null", p.systemType);
            Assert.Equal(DateTime.Now.Second, p.timestamp.Second);
        }
    }
}
