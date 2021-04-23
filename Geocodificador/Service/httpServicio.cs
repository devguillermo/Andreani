using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Geocodificador.Service
{
    public interface IhttpServicio
    {
        string BaseUrl { get; set; }

        Task<string> GetAsync(string parametros);
    }
    public class httpServicio : IhttpServicio
    {
        // DEFINICIÓN DE VARIABLES PRIVADAS.
        private string _baseUrl = "https://nominatim.openstreetmap.org/search";

        private HttpClient _httpClient;

        // OBTENEMOS A TRAVÉS DE CONSTRUCTOR EL HttpClient.
        public httpServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }

        // PROPIEDAD PUBLICA PARA ASIGNAR O LEER
        // LA URL-BASE DEL WEB-API.
        public string BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        //////////////////////////////////////
        // ACCIONES/OPERACIONES DEL WEB-API //
        //////////////////////////////////////

        /////////////////
        // ACCIÓN POST //
        /////////////////
        public async Task<string> GetAsync(string parmetros)
        {
            // CONSTRUIMOS LA URL DE LA ACCIÓN
            var urlBuilder_ = new StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "")
                       .Append(parmetros);

            var url_ = urlBuilder_.ToString();

            // RECUPERAMOS EL HttpClient
            var client_ = _httpClient;

            try
            {
                using (var request_ = new HttpRequestMessage())
                {
                    ///////////////////////////////////////
                    // CONSTRUIMOS LA PETICIÓN (REQUEST) //
                    ///////////////////////////////////////
                    // DEFINIMOS EL MÉTODO HTTP
                    //request_.Headers.UserAgent.Add(new ProductInfoHeaderValue("myprogram", "2.2"));

                    request_.Headers.UserAgent.Add(new ProductInfoHeaderValue("Servicio", "0.2"));
                    request_.Method = new HttpMethod("GET");

                    // DEFINIMOS LA URI
                    request_.RequestUri = new Uri(url_, System.UriKind.RelativeOrAbsolute);

                    // DEFINIMOS EL Accept, EN ESTE CASO ES "application/json"
                    request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    // ASIGNAMOS A LA CABECERA DE LA PETICIÓN EL TOKEN JWT.
                    // if (!string.IsNullOrEmpty(_bearerTokenJWT))
                    //     request_.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerTokenJWT);

                    /////////////////////////////////////////
                    // CONSTRUIMOS LA RESPUESTA (RESPONSE) //
                    /////////////////////////////////////////
                    // Utilizamos ConfigureAwait(false) para evitar el DeadLock.
                    var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    // OBTENEMOS EL Content DEL RESPONSE como un String
                    // Utilizamos ConfigureAwait(false) para evitar el DeadLock.
                    var responseText_ = await response_.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // SI ES LA RESPUESTA ESPERADA !! ...
                    if (response_.StatusCode == System.Net.HttpStatusCode.OK) // 200
                    {
                        // DESERIALIZAMOS Content DEL RESPONSE
                        //var responseBody_ = JsonConvert.DeserializeObject<List<Pais>>(responseText_);
                        //return responseBody_;
                        return responseText_;
                    }
                    else
                    // SI NO SE ESTÁ AUTORIZADO ...
                    if (response_.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401
                    {
                        throw new Exception("401 Unauthorized. No se ha indicado o es incorrecto el Token JWT de acceso. " +
                            responseText_);
                    }
                    else
                    // CUALQUIER OTRA RESPUESTA ...
                    if (response_.StatusCode != System.Net.HttpStatusCode.OK && // 200
                        response_.StatusCode != System.Net.HttpStatusCode.NoContent) // 204
                    {
                        throw new Exception((int)response_.StatusCode + ". No se esperaba el código de estado HTTP de la respuesta. " +
                            responseText_);
                    }

                    // RETORNAMOS EL OBJETO POR DEFECTO ESPERADO
                    return "";
                }
            }
            finally
            {
                // NO UTILIZAMOS CATCH, 
                // PASAMOS LA EXCEPCIÓN A LA APP.
            }
        }
    }
}
