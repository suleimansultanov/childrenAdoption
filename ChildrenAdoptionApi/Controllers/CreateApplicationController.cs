using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace ChildrenAdoptionApi.Controllers
{
    public class CreateApplicationController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new CreateAppViewModel() { ActionNo = 1 };
            var districts = GetDistrictItems();
            ViewBag.DistrictId = new SelectList(districts, "ID", "Name", "RegionName", districts[0]);
            return View(model);
        }
        static List<DistrictItem> GetDistrictItems()
        {
            return new List<DistrictItem>()
            {
                new DistrictItem
                {
                    ID = new Guid("{20FF5245-DB58-443A-A31B-061200E939AA}"),
                    Name = "Октябрьский район",
                    RegionName = "г. Бишкек"
                },
                new DistrictItem
                {
                    ID = new Guid("{E3BFB034-7638-4C14-8C0F-EA823CCA7A5B}"),
                    Name = "Ленинский район",
                    RegionName = "г. Бишкек"
                },
                new DistrictItem
                {
                    ID = new Guid("{D4C5884F-1820-4DF5-9BF8-C48202ACB801}"),
                    Name = "Первомайский район",
                    RegionName = "г. Бишкек"
                },
                new DistrictItem
                {
                    ID = new Guid("{52315837-1B8F-4B80-9D8C-160214F4681A}"),
                    Name = "Свердлвский район",
                    RegionName = "г. Бишкек"
                },
                new DistrictItem
                {
                    ID = new Guid("{668D6957-1F76-49DE-B9AD-0F2C635DF2DD}"),
                    Name = "Кеминский район",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{DC2174B4-479B-4F6F-9A9D-9BA28AB715CD}"),
                    Name = "Аламединский район",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{046FF80D-4D26-4E30-A41F-17873C0E2C8F}"),
                    Name = "Жети-Огузского района",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{7866D332-C471-4873-8F8B-6DA9EDC3683A}"),
                    Name = "Ыссык-Атинского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{C8641D03-B553-4F95-ADFB-EC002FD64E06}"),
                    Name = "Московского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{30658F5A-DC22-4A7A-AAF6-63DFB7070C50}"),
                    Name = "Чуйского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{8B6510F7-3B6B-4C9D-B8A1-2349B78B2CF3}"),
                    Name = "город Токмок",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{7F311898-D254-4668-8184-D1794E695A19}"),
                    Name = "Бакай-Атинского района",
                    RegionName = "Талас"
                },
                new DistrictItem
                {
                    ID = new Guid("{19F7870D-4B5E-457A-B159-528F71374E1E}"),
                    Name = "Жайылского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{093AFB08-3F18-4E5C-8EB6-86EF0DD711B9}"),
                    Name = "Сокулукского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{5DF10BBC-D256-40F8-B641-5EF31CE11540}"),
                    Name = "Панфиловского района",
                    RegionName = "Чуй"
                },
                new DistrictItem
                {
                    ID = new Guid("{4313A514-83B6-4FED-9731-4D838A6E91CD}"),
                    Name = "Нарынского района",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{93DF61DD-A2FE-4F52-B961-F1B27BD27A8C}"),
                    Name = "город Нарын",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{69EAAC3D-3525-4CE3-8C5E-42123B4ED9CB}"),
                    Name = "Атбашинского района",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{51C381F2-6AD6-4382-81F9-3D36E9ABEA08}"),
                    Name = "Кочкорского района",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{B3EA3DE1-5D76-461C-90AC-68D312B7B665}"),
                    Name = "Жумгальского района",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{B3C93041-BBEA-445D-B7E1-D42D9F3692C3}"),
                    Name = "Акталинского района",
                    RegionName = "Нарын"
                },
                new DistrictItem
                {
                    ID = new Guid("{C6D92271-6FF2-4641-A83F-D02610060C43}"),
                    Name = "Кара-Бууринского района",
                    RegionName = "Талас"
                },
                new DistrictItem
                {
                    ID = new Guid("{3BB3CB8F-A747-4026-9031-C6B67EA9735B}"),
                    Name = "город Талас",
                    RegionName = "Талас"
                },
                new DistrictItem
                {
                    ID = new Guid("{16FD5BFF-7AB4-4B40-AAFB-0282BA5BE32E}"),
                    Name = "Таласского района",
                    RegionName = "Талас"
                },
                new DistrictItem
                {
                    ID = new Guid("{B8DACAD8-0118-4147-8BD2-30DCE891E5EB}"),
                    Name = "Манасского района",
                    RegionName = "Талас"
                },
                new DistrictItem
                {
                    ID = new Guid("{3277F73D-B02D-4F35-AD1A-80B349EECF6B}"),
                    Name = "Тонского района",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{1F892B98-97B4-4B58-B61F-7DFECBAC3B5E}"),
                    Name = "Ак-Сууйского района",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{AB27C82D-8348-4DBA-BA9C-CEAE3A67704E}"),
                    Name = "город Каракол",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{23DB562E-C0B7-4B7E-AC16-967C96500444}"),
                    Name = "город Балыкчи",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{14EB7AEF-C331-4085-A83E-0F5EEBF2EAA3}"),
                    Name = "Иссык-Кульского района",
                    RegionName = "И-К"
                },
                new DistrictItem
                {
                    ID = new Guid("{5EE1AC59-52A1-45E3-84EC-54AD09FC9F1E}"),
                    Name = "Баткенского района",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{D328865B-EDE4-4674-92C6-C134F55C3C6F}"),
                    Name = "город Баткен",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{879F3420-E45A-4E11-9394-D0B1A0091805}"),
                    Name = "Кадамжайского района",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{C7B6AA81-A4B7-4AD6-BE03-E4B54E8084DA}"),
                    Name = "Лейлекский район",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{32AF3EA3-4100-406F-8825-20CB8A1877AA}"),
                    Name = "город Кизыл-Кия",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{FF3B071C-FC51-4F64-8BD4-11A5377E93D2}"),
                    Name = "город Сулюкта",
                    RegionName = "Баткен"
                },
                new DistrictItem
                {
                    ID = new Guid("{8B739BFD-CF8F-4380-A246-8A34BCE9A231}"),
                    Name = "город Ош",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{B382A964-BBD7-4540-B6E4-BC9250AB7005}"),
                    Name = "Араванского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{A2317371-CC1D-4EB3-8ADF-D7BA33B75F87}"),
                    Name = "Кара-Кульджинского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{59FF6F9C-F047-4E25-B837-669AA0596B07}"),
                    Name = "Карасуйского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{A3282FFC-BAFE-4B4E-99D0-C68509BE6133}"),
                    Name = "Ноокатского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{E425497A-3C82-424E-9691-71EAAD4759D8}"),
                    Name = "Узгенского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{D62E0B83-E589-491D-B0A4-8BEA42178231}"),
                    Name = "город Узген",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{E5411CDD-3CAF-4798-A7BF-FD57C856C82B}"),
                    Name = "Алайского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{C1A54A26-CA15-4C29-8DEB-957F5B0FD002}"),
                    Name = "Чон-Алайского района",
                    RegionName = "Ош"
                },
                new DistrictItem
                {
                    ID = new Guid("{18DBE2BE-BBB6-4BC2-9A33-8B25856C0382}"),
                    Name = "город Джалал-Абад",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{B42B4F13-2AA5-43D2-B738-CB02CA647D47}"),
                    Name = "город Кок-Жангак",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{9AFA2870-4609-4D8A-8417-11C2DBDEC94B}"),
                    Name = "город Майлуу-Суу",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{9D8B0CA0-D618-4157-AF15-5E7CCA7D9F23}"),
                    Name = "город Таш-Кумыр",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{AA65C2F7-3091-40E5-939D-6A18250E125A}"),
                    Name = "город Кара-Куль",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{DC448CE3-49F4-43F1-AEAA-F782341C62C0}"),
                    Name = "Сузакского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{17F5601A-36B7-4FAC-A980-07F349239D5B}"),
                    Name = "Базар-Коргонского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{E4964796-6658-46A9-BD49-A34B3275CB1F}"),
                    Name = "Ноокенского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{2765827A-88EF-4144-8050-9A2FD66FB136}"),
                    Name = "Аксыйский района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{60F22545-CB84-4795-81A3-2EEEEB0FB88A}"),
                    Name = "Ала-Букинского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{A598BEA0-37C9-4DC2-9BC7-FB3AA1097E14}"),
                    Name = "Чаткальского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{64ECD259-A848-474B-8E8D-571612A7E43B}"),
                    Name = "Токтогульского района",
                    RegionName = "Ж-А"
                },
                new DistrictItem
                {
                    ID = new Guid("{B0A96501-F5F4-48A4-8103-6C7A7BA290C4}"),
                    Name = "Тогуз-Тороузского района",
                    RegionName = "Ж-А"
                }
            };
        }
        public ActionResult Index(CreateAppViewModel model)
        {
            var districts = GetDistrictItems();
            ViewBag.DistrictId = new SelectList(districts, "ID", "Name", "RegionName", model.DistrictId);
            if (string.IsNullOrEmpty(model.ApplicantPIN) || string.IsNullOrEmpty(model.PassportNo) || string.IsNullOrEmpty(model.PassportSeries))
            {
                ViewBag.Error = "Необходимо заполнить все поля";
                return View(model);
            }
            
            if (SendRequest(new
            {
                clientId = "8d8461a4-9d3e-4136-98a7-66697078371d",
                orgName = "ПОРТАЛ-ГБД",
                request = new
                {
                    passportDataByPSN = new
                    {
                        request = new
                        {
                            pin = model.ApplicantPIN,
                            series = model.PassportSeries,
                            number = model.PassportNo
                        }
                    }
                }
            }, "http://localhost/ServiceConstructor/SoapClient/SendRequest2", "POST", out dynamic response, out string errorMessage))
            {
                if (response.response.passportDataByPSNResponse.response != null)
                {
                    var r = response.response.passportDataByPSNResponse.response;
                    model.passportPerson = ((JObject)r).ToObject<CreateAppViewModel._passportPerson>();
                }
                else
                {
                    //ViewBag.Error = "Данные о браке отсутствуют";
                }
            }

            if (SendRequest(new
            {
                clientId = "731537be-3e88-4f9a-8f2b-4aea6b5afc01",
                orgName = "ПОРТАЛ-ГБД",
                request = new {
                zagsPinMarriageAct = new {
                    request = new {
                        pin = model.ApplicantPIN
                    }
                }
            }
        }, "http://localhost/ServiceConstructor/SoapClient/SendRequest2", "POST", out response, out errorMessage))
            {
                if(response.response.zagsPinMarriageActResponse.response != null)
                {
                    var r = response.response.zagsPinMarriageActResponse.response;
                    model.zagsData = ((JObject)r).ToObject<CreateAppViewModel._zagsData>();
                }
                else
                {
                    //ViewBag.Error = "Данные о браке отсутствуют";
                }
            }
            else
            {
                ViewBag.Error = errorMessage;
            }

            if(model.ActionNo == 1)
            {
                model.ActionNo = 2;
            }
            else if (model.ActionNo == 2)
            {
                Models.ScriptExecutor.CreateCandidateApp(model);
                var msg = @"Ваше заявление успешно отправлено в {orgName} по адресу {address}. Вам необходимо проверять статус своего заявления в течении следующих 10 рабочих дней. Для дополнительной консультации можете обратиться по тел. {contact}";
                foreach (var el in XDocument.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/organizations.xml")).Root.Elements())
                {
                    var orgName = el.Attribute("name").Value;
                    var newOrgId = Guid.Parse(el.Attribute("newOrgId").Value);
                    var address = el.Attribute("address").Value;
                    var contact = el.Attribute("contact").Value;
                    if (newOrgId == model.DistrictId)
                    {
                        msg = msg.Replace("{orgName}", orgName);
                        msg = msg.Replace("{address}", address);
                        msg = msg.Replace("{contact}", contact);
                        break;
                    }
                }
                ViewBag.Message = msg;
                //model = new CreateAppViewModel {ActionNo = 1 };
            }

            return View(model);
        }
        
        public ActionResult GetStatus(string pin)
        {
            if(SendRequest(null, "http://localhost/CISSA-WEB-API/api/Custom/GetCandidateStatus?pin=" + pin, "GET", out dynamic result, out string error))
            {
                ViewBag.Message = result.Message;
                ViewBag.Documents = result.Documents;
            }
            else
            {
                ViewBag.Message = error;
            }
            return View();
        }

        private bool SendRequest(object request, string url, string httpmethod, out dynamic result, out string errorMessage)
        {
            errorMessage = "";
            result = new object();
            try
            {
                string webAddr = url;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = httpmethod;

                if(request != null)
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(request);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseText = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    responseText = streamReader.ReadToEnd();
                }

                result = JObject.Parse(responseText);

                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.GetBaseException() + ", trace: " + e.StackTrace;
                return false;
            }
        }
    }
    public class DistrictItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string RegionName { get; set; }
    }
    public class CreateAppViewModel
    {
        public string ApplicantPIN { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNo { get; set; }
        public string Telephone { get; set; }
        public _passportPerson passportPerson { get; set; }
        public _zagsData zagsData { get; set; }

        public class _zagsData
        {
            public _zagsPerson groom { get; set; }
            public _zagsPerson bride { get; set; }

            public class _zagsPerson
            {
                public string pin { get; set; }
                public string surname { get; set; }
                public string firstName { get; set; }
                public string patronymic { get; set; }
                public string newSurname { get; set; }
                public string newFirstName { get; set; }
                public string newPatronymic { get; set; }
                public string nationality { get; set; }
                public string citizenship { get; set; }
                public string placeOfBirth { get; set; }
            }
        }

        public class _passportPerson
        {
            public string pin { get; set; }
            public string surname { get; set; }
            public string name { get; set; }
            public string patronymic { get; set; }
            public string nationality { get; set; }
            public DateTime dateOfBirth { get; set; }
            public string passportAuthority { get; set; }
            public DateTime issuedDate { get; set; }
            public DateTime expiredDate { get; set; }
            public string gender { get; set; }
            public string addressRegion { get; set; }
            public string addressLocality { get; set; }
            public string addressStreet { get; set; }
            public string addressHouse { get; set; }
        }

        public Guid DistrictId { get; set; }
        public string Address { get; set; }
        public int ActionNo { get; set; }
    }
}