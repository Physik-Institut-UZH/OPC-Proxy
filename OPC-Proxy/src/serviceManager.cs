using NetCoreConsoleClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NLog;
using converter;

namespace ProxyUtils{

    /// <summary>
    /// Class that manages the comunication between all the services, dbCache, OPCclient, TCP server, kafka server.
    /// Takes care of initialization of all services and of setting up event handlers.
    /// </summary>
    public class serviceManager : logged{
        private cacheDB db;
        private OPCclient opc;
        
        public serviceManager(JObject config){
        
            opc = new OPCclient(config);
            db = new cacheDB(config);

            // setting up the comunication line back to the manager
            opc.setPointerToManager(this);
            db.setPointerToManager(this);
        }

        public void connectOpcClient(){
            opc.connect();
        }

        public void subscribeOpcNodes(){

            opc.subscribe( db.getDbNodes() );
        }

        public void browseNodesFillCache(){
            
            UANodeConverter ua = new UANodeConverter("nodeset.xml", opc.session.NamespaceUris);
            ua.fillCacheDB(db);
        }

    }


    public class Managed : logged {
        private serviceManager _serviceManager;

        public Managed(){
            _serviceManager = null;
        }

        public void setPointerToManager(serviceManager ser){
            _serviceManager = ser;
        }
    }

    public class logged{
        public static Logger logger = LogManager.GetCurrentClassLogger();
    }
}