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
        public LiteDatabase db = null;
        public MemoryStream mem = null;
        
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

            db = (_config.isInMemory) ? new LiteDatabase(mem) : new LiteDatabase(@_config.filename) ;
            
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

        /// <summary>
        /// Update the cache with the new value of that variable
        /// </summary>
        /// <param name="name">name of variable</param>
        /// <param name="value"> updated value of variable</param>
        /// <param name="time"> timestamp of when it changed</param>
        public void updateBuffer(string name, object value, DateTime time){
            try{
                dbVariableValue var_idx = latestValues.FindOne(Query.EQ("name",name));

                // if not found then search in nodes list
                if(var_idx == null) 
                    var_idx = this._initVarValue(name);

                var_idx.value = Convert.ChangeType(value, Type.GetType(var_idx.systemType));
                var_idx.timestamp = time;
                latestValues.Upsert(var_idx);

            } 
            catch (Exception e){
                Console.Error.WriteLine("Error in updating value for variable " + name);
                Console.Error.WriteLine(e.StackTrace);
            }           
        }

        /// <summary>
        /// Read a variable value from the DB cache given the name
        /// </summary>
        /// <param name="name"> Name of the variable</param>
        /// <returns>Returns A dbVariable</returns>
        public dbVariableValue readValue(string name){
            
            dbVariableValue read_var = new dbVariableValue();

            try{
                var temp =  latestValues.FindOne(Query.EQ("name",name));
                if(temp == null) throw new Exception("Variable does not exist in DB: " + name);
                
                read_var = temp;
            }
            catch(Exception e) {
                Console.Error.WriteLine("Error in reading value for variable " + name);
                Console.Error.WriteLine(e.StackTrace);
            }
            return read_var;
        }

        /// <summary>
        /// Initialization of the variable value in DB, this is used if the variable does not exist yet, 
        /// then one looks into the nodelist.
        /// </summary>
        /// <param name="name">name of the variable value to initialize</param>
        /// <returns></returns>
        private dbVariableValue _initVarValue(string name){
            dbNode var_idx = nodes.FindOne(Query.EQ("name",name));
            
            if(var_idx == null)  {
                Console.WriteLine("ma porxca miseria " + name);
                throw new Exception("variable does not exist: "+name );
            }
            else {
                dbVariableValue new_var = new dbVariableValue {
                    Id = var_idx.Id,
                    name = var_idx.name,
                    systemType = var_idx.systemType,
                };
                return new_var;
            }
        }
        
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
        public string systemType {get;set;}
        public DateTime timestamp {get;set;}

        public dbVariableValue(){
            this.Id = -9;
            this.name = "does_not_exist";
            this.value = -9;
            this.systemType = "null";
            this.timestamp = DateTime.Now;
        }
    }
}