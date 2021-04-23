using LibraryCommond;

namespace LibraryCommond
{
    public class Geolocalizar : RequestGeo
    {
        private string separador = "&";
        public  string parametro()
        {
            return "?street=" + this.street + " " + this.number + separador + "city=" + this.city + separador + "county=" + this.county + separador + "state=" + this.state + separador + "country=" +  this.country 
                + separador +  "postalcode=" +  this.postalcode + separador + "format=" + this.format;
        }
    }

   
}
