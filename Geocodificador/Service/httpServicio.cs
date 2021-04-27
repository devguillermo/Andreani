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
        
        private string _baseUrl = "https://nominatim.openstreetmap.org/search";

        private HttpClient _httpClient;

        
        public httpServicio(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }

        
        public string BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        public async Task<string> GetAsync(string parmetros)
        {
            
            var urlBuilder_ = new StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "")
                       .Append(parmetros);

            var url_ = urlBuilder_.ToString();

            
            var client_ = _httpClient;

            try
            {
                using (var request_ = new HttpRequestMessage())
                {
            
                    request_.Headers.UserAgent.Add(new ProductInfoHeaderValue("Servicio", "0.2"));
                    request_.Method = new HttpMethod("GET");

                    
                    request_.RequestUri = new Uri(url_, System.UriKind.RelativeOrAbsolute);

                    request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    var responseText_ = await response_.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response_.StatusCode == System.Net.HttpStatusCode.OK) // 200
                    {
                        return responseText_;
                    }
                    else
                    if (response_.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401
                    {
                        throw new Exception("401 Unauthorized. No se ha indicado o es incorrecto el Token JWT de acceso. " +
                            responseText_);
                    }
                    else
                    if (response_.StatusCode != System.Net.HttpStatusCode.OK && // 200
                        response_.StatusCode != System.Net.HttpStatusCode.NoContent) // 204
                    {
                        throw new Exception((int)response_.StatusCode + ". No se esperaba el código de estado HTTP de la respuesta. " +
                            responseText_);
                    }

                    return "";
                }
            }
            finally
            {
            }
        }
    }
}
