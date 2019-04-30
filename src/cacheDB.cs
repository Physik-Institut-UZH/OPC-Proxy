using System;
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
    class cacheDB {
        LiteDatabase db;
        MemoryStream mem;

        dbConfig _config;
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

            mem = new MemoryStream();

            db = (_config.isInMemory) ? new LiteDatabase(@_config.filename) : new LiteDatabase(mem);
            
            createCollections();
        }
        private void createCollections(){
            Console.WriteLine("file {0} , memory {1}, overrride {2}", _config.filename, _config.isInMemory, _config.overwrite);
        }
    }
    


    /// <summary>
    /// <see cref="dbConfig"/>
    /// Configuration handler for DBcache
    /// </summary>
    class dbConfig{
        public bool isInMemory { get; set; }
        public string filename { get; set;}
        public bool overwrite{get; set;}

        public dbConfig(){
            isInMemory = true;
            filename = "DBcache.opcproxy.dat";
            overwrite = false;
        }
    }

}