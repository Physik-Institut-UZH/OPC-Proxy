using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProxyUtils{

    /// <summary>
    /// Class that holds the in memory cache database. LiteDB is used as chache DB.
    /// </summary>
    public class cacheDB {
//    class cacheDB : IDisposable {
        public double p;
        LiteDatabase db = null;
        MemoryStream mem = null;
        
        public LiteCollection<dbNode> nodes {get; private set;}
        public LiteCollection<dbNamespace> namespaces {get;  set;}
        public LiteCollection<dbVariableValue> latestValues {get; set;}
        public LiteCollection<dbVariableValue> bufferValues {get; set;}
        //bool disposed = false;

        dbConfig _config = null;

        /// <summary>
        /// Constructor for cacheDB. config is as follows:
        /// </summary>
        /// 
        /// <para>
        ///  [isInMemory] - Boolean : if false the DB is persisted on disk (file), and can be loaded later. Warning - performance degrade. Default is true.
        /// 
        ///  [filename] - String : name of the file DB should be written to. Deafult "DBcache.opcproxy.dat" 
        /// 
        ///  [overwrite] - Boolean : if true force overwrite of DB file, false will load from file. Default false.
        /// </para>
        public cacheDB( JObject config ){
            _config = config.ToObject<dbConfig>();
            init();
        }

        private void createCollections(){

            nodes           = db.GetCollection<dbNode>("nodes");
            namespaces      = db.GetCollection<dbNamespace>("namespaces");
            latestValues    = db.GetCollection<dbVariableValue>("latestValues");
            bufferValues    = db.GetCollection<dbVariableValue>("bufferValues");

            // Creating indexes
            nodes.EnsureIndex("name");
            namespaces.EnsureIndex("URI");
            namespaces.EnsureIndex("internalIndex");
            namespaces.EnsureIndex("currentServerIndex");
            latestValues.EnsureIndex("name");
            bufferValues.EnsureIndex("name");
            bufferValues.EnsureIndex("timestamp");
        }

        private void init(){
            mem = new MemoryStream();

            db = (_config.isInMemory) ? new LiteDatabase(@_config.filename) : new LiteDatabase(mem);
            
            createCollections();
        }

        public void clear(){
            db.Dispose();
            mem.Dispose();
        }
        /// <summary>
        /// Drops the memory/file streams and the db and re-initialize a fresh empty db instance. 
        /// Usefull when one wants to re-load nodes in case node XML file changed.
        /// </summary>
        public void refresh(){
            clear();
            init();
        }

        /*/public void Dispose(){
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing){
            Console.WriteLine("dispose");

            if(disposed) return ;
            if (disposing) {
                db.Dispose();   
                mem.Dispose();
            }
            disposed = true;
        }*/
    }
    
    /// <summary>
    /// Configuration handler for DBcache
    /// </summary>
    public class dbConfig{
    
        public bool isInMemory { get; set; }
        public string filename { get; set;}
        public bool overwrite{get; set;}

        public dbConfig(){
            isInMemory = true;
            filename = "DBcache.opcproxy.dat";
            overwrite = false;
        }
    }

    /// <summary>
    /// Representation of an OPC Server Node. 
    /// </summary>
    public class dbNode{
         public int Id { get; set; }
        public string name {get;set;}
        public string identifier {get;set;}
        public int internalIndex{get;set;}
        public string classType {get;set;}
        public string systemType {get;set;}
        public string[] references{get;set;}
    }
    /// <summary>
    /// Node internal and server related namespace: the nodes in the DB are stored referring to a namespaceIndex which 
    /// is genereted internally at creation time. This table holds the current server namespace 
    /// index for that URI (which can change at any new session) and the internal node index assigned 
    /// at the node insertion time (which will not change).
    /// </summary>
    public class dbNamespace{
        public int Id { get; set; }
        public string URI {get;set;}
        public int currentServerIndex {get;set;}
    }

    /// <summary>
    /// Variable stored value
    /// </summary>
    public class dbVariableValue{
        public int Id { get; set; }
        public string name{get;set;}
        public object value{get;set;}
        public Type systemType {get;set;}
        public DateTime timestamp {get;set;}
    }
}