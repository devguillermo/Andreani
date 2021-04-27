using MongoDB.Driver;
using LibraryCommond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Api_Geo.Models
{
    public interface IServiceMongo
    {
        public bool addDocumentRequest(RequestGeo documento);
        public bool UpdateDocumentRequest(RequestGeo documento,  string id);

        public RequestGeo SelectDocument(string id);
    }
        
    class ServiceMongo : IServiceMongo
    {
        
        MongoClient dbClient;

        private IOptions<AppSettings> _settings;
        private IMongoDatabase db;

        public ServiceMongo(IOptions<AppSettings> settings)
        {
            _settings = settings;
            MongoClientSettings settingsBase = new MongoClientSettings();
            settingsBase.Server = new MongoServerAddress(_settings.Value.HostDb, _settings.Value.PortDb);
            settingsBase.UseTls = false;
            settingsBase.SslSettings = new SslSettings();
            settingsBase.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity("admin", _settings.Value.UserDb);
            MongoIdentityEvidence evidence = new PasswordEvidence(_settings.Value.PasswordDb);

            settingsBase.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            dbClient = new MongoClient(settingsBase);

            db = dbClient.GetDatabase(_settings.Value.NameDb);
        }

        public bool addDocumentRequest(RequestGeo documento)
        {
            IMongoCollection<RequestGeo> dbCollection = db.GetCollection<RequestGeo>("RequestGeo");

            dbCollection.InsertOne(documento);
                      
            return true;
        }

        public bool UpdateDocumentRequest(RequestGeo documento, string id)

        {
           
            IMongoCollection<RequestGeo> dbCollection = db.GetCollection<RequestGeo>("RequestGeo");
            //MongoDB.Bson.ObjectId.Parse(id)
            var filter = Builders<RequestGeo>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));

            //var update = Builders<RequestGeo>.Update.Combine(documento);
            var update = Builders<RequestGeo>.Update.Set("ResponseOps", documento.ResponseOps).Set("GeoLocState", true);

            dbCollection.UpdateOne(filter, update);

            
            return true;
                
        }

        public string getAll()
        {//IMongoCollection<Game> dbCollection = db.GetCollection<Game>("boardgames");
            var dbList = dbClient.ListDatabases().ToList();

            Console.WriteLine("The list of databases are:");

            foreach (var item in dbList)
            {
                Console.WriteLine(item);
            }

            return dbList.ToString();
        }

        public RequestGeo SelectDocument(string id)
        {
            var idObj = new ObjectId(id);

            IMongoCollection<RequestGeo> dbCollection = db.GetCollection<RequestGeo>("RequestGeo");


            var filter = Builders<RequestGeo>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
            var requestGeo = dbCollection.Find(filter).FirstOrDefault();

            return requestGeo;
        }
    }
}
