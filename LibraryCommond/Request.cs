using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;


namespace LibraryCommond
{
    public class RequestGeo
    {
        [BsonId]
        public ObjectId id { get; set; }

        public string idStr { get; set; }

        public bool GeoLocState { get; set; }
        //public string _id { get; set; }
        public string street { get; set; }

        public string number { get; set; }

        public string state { get; set; }

        public string city { get; set; }

        public string postalcode { get; set; }

        public string county { get; set; }

        public string country { get; set; }

       public string format { get; set; }

        
        

        public ResponseOps ResponseOps { get; set; }

    }

}
