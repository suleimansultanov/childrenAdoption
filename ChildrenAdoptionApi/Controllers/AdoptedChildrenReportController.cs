using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace ChildrenAdoptionApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AdoptedChildrenReportController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(AdoptedChildrenReportResponse))]
        public IHttpActionResult Get()
        {
            var response = RequestHttp<AdoptedChildrenReportResponse>();
            response.Total = response.ByGeography.Sum(x => x.Total);
            return Ok(response);
        }
        private T RequestHttp<T>()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost/CISSA-WEB-API/api/Custom/AdoptedChildren");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "GET";
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var responseText = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseText = streamReader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<T>(responseText);
        }
    }
    public class AdoptedChildrenReportResponse
    {
        public AdoptedChildrenReportItem[] ByAge { get; set; }
        public AdoptedChildrenReportItem[] ByNationalities { get; set; }
        public AdoptedChildrenReportItem[] ByGeography { get; set; }
        public int? Total { get; set; }
        public class AdoptedChildrenReportItem
        {
            public int No { get; set; }
            public string Name { get; set; }
            public int Boys { get; set; }
            public int Girls { get; set; }
            public int Total { get; set; }
        }
    }
}