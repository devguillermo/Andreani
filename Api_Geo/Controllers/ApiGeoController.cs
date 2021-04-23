using Api_Geo.Domain;
using Api_Geo.Models;
using LibraryCommond;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api_Geo.Controllers
{
    [ApiController]

    public class ApiGeoController : ControllerBase
    {
        //private IOptions<AppSettings> _settings;

        private readonly ILogger<ApiGeoController> _logger;
        private IServiceRabbitMq _ServiceRabbitMq;
        private IServiceMongo _serviceMongo;
        public ApiGeoController(IServiceMongo serviceMongo, IServiceRabbitMq ServiceRabbitMq, ILogger<ApiGeoController> logger)
        {
            _logger = logger;
            //_settings = settings;
            _serviceMongo = serviceMongo;
            _ServiceRabbitMq = ServiceRabbitMq;
        }

        [HttpGet]
        [Route("/geolocalizar/{calle}/{numero}/{ciudad}/{cp}/{provincia}/{pais}")]
        public ActionResult Get(string calle, string numero, string ciudad, string cp,
            string provincia, string pais)
        {
            string mesaje = $@"Mensaje {calle}, {numero}, {ciudad}, {pais}";

            var id = ObjectId.GenerateNewId();

            RequestGeo geo = new RequestGeo
            {
                id = id,
                idStr = id.ToString(),
                city = ciudad,
                country = pais,
                county = ciudad,
                street = calle,
                number = numero,
                state = provincia,
                postalcode = cp,
                GeoLocState = false
            };

            _serviceMongo.addDocumentRequest(geo);


            string jsonGeo = JsonSerializer.Serialize(geo);

            _ServiceRabbitMq.Send(jsonGeo);

            Body body = new Body { id = geo.id + jsonGeo };
            return Ok(body);

        }

        [HttpGet]
        [Route("/geolocalizar/{id}")]
        public ActionResult Get(string Id)
        {




            RequestGeo geo = _serviceMongo.SelectDocument(Id);


            ProcessStatus processStatus = new ProcessStatus
            {
                id = geo.id.ToString(),
                latitud = geo.ResponseOps.lat,
                longitud = geo.ResponseOps.lon,
                estado = geo.GeoLocState.ToString()
            };


            


            return Ok(processStatus);

        }
    }
}
