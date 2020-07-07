using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChildrenAdoptionApi.Models
{
    public static class ScriptExecutor
    {
        public static Guid portal_childrenUserId = new Guid("{72B9AB33-3FAF-455B-8347-E6AA5B6ECFEB}");
        public static string portal_childrenUserName = "portal_children";
        static IAppServiceProvider InitProvider(string username, Guid userId)
        {
            var dataContextFactory = DataContextFactoryProvider.GetFactory();

            var dataContext = dataContextFactory.CreateMultiDc("DataContexts");
            BaseServiceFactory.CreateBaseServiceFactories();
            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = providerFactory.Create(dataContext);
            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(userId, username));
            return provider;
        }
        public static WorkflowContext CreateContext(string username, Guid userId)
        {
            return new WorkflowContext(new WorkflowContextData(Guid.Empty, userId), InitProvider(username, userId));
        }

        internal static bool HasEnoughAttachments(string pin)
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var candidateId = GetCandidateCaseId(pin);
            if (candidateId == Guid.Empty) return false;
            var docRepo = context.Documents;
            var candidate = docRepo.LoadById(candidateId);
            var cert = docRepo.LoadById((Guid)candidate["Certificate"]);
            var appId = (Guid)cert["CandidateApplication"];
            var attachTypeList = new List<Guid>();
            var attachDefId = new Guid("{FE71B6E0-C73B-48B2-A7E1-FC896E86CB5D}");
            var qb = new QueryBuilder(attachDefId);
            qb.Where("CandidateApplication").Eq(appId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            query.AddAttribute("DocumentAttachmentType");
            query.AddGroupAttribute("DocumentAttachmentType");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                {
                    if (!reader.IsDbNull(1))
                        attachTypeList.Add(reader.GetGuid(1));
                }
            }

            return attachTypeList.Count >= 9;
        }

        /// <summary>
        /// Элемент списка детей
        /// </summary>
        public class AvailableChildrenItem
        {
            /// <summary>
            /// Дата постановки на учет
            /// </summary>
            public DateTime RegDate { get; set; }
            /// <summary>
            /// Айди организации УСР
            /// </summary>
            public Guid OrgId { get; set; }
            /// <summary>
            /// Айди записи
            /// </summary>
            public Guid DocumentId { get; set; }
            /// <summary>
            /// Возраст ребенка
            /// </summary>
            public int Age { get; set; }
            /// <summary>
            /// Пол ребенка
            /// </summary>
            public Guid Gender { get; set; }
            /// <summary>
            /// Национальность
            /// </summary>
            public Guid Nationality { get; set; }
            /// <summary>
            /// Приметы
            /// </summary>
            public string PrimetyRebenka { get; set; }
            /// <summary>
            /// Характер
            /// </summary>
            public string Character { get; set; }
            /// <summary>
            /// Состояние здоровья
            /// </summary>
            public string Diagnosis { get; set; }

        }

        
        public class AvailableChildrenItemDetail
        {
            /// <summary>
            /// Айди организации УСР
            /// </summary>
            public string OrgName { get; set; }
            /// <summary>
            /// Возраст ребенка
            /// </summary>
            public int Age { get; set; }
            /// <summary>
            /// Пол ребенка
            /// </summary>
            public string Gender { get; set; }
            /// <summary>
            /// Национальность
            /// </summary>
            public string Nationality { get; set; }
            /// <summary>
            /// Приметы
            /// </summary>
            public string PrimetyRebenka { get; set; }
            /// <summary>
            /// Характер
            /// </summary>
            public string Character { get; set; }
            /// <summary>
            /// Состояние здоровья
            /// </summary>
            public string Diagnosis { get; set; }

        }
        static Guid childDefId = new Guid("{2CFF5093-F55F-4335-ADAA-EC951C41A770}");
        static Guid registeredStateId = new Guid("{C1414D0C-417A-45AB-8B57-01D30A567F08}");
        static Guid sentToMTSRStateId = new Guid("{3B18BB86-3A8C-41A6-BC90-583F200C167D}");
        static Guid personDefId = new Guid("{D71CE61A-9B59-4B5E-8713-8131DBB5BA02}");
        public static AvailableChildrenItem[] GetAvailableChildrenItems()
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var qb = new QueryBuilder(childDefId);
            qb.Where("&State").In(new object[] { registeredStateId, sentToMTSRStateId });
            var query = context.CreateSqlQuery(qb.Def);
            var personSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Person");
            
            query.AddAttribute(personSrc, "BirthDate");
            query.AddAttribute(personSrc, "Gender");
            query.AddAttribute(query.Source, "HealthDescription");
            query.AddAttribute(query.Source, "&Id");
            query.AddAttribute(query.Source, "&OrgId");
            query.AddAttribute(query.Source, "PrimetyRebenka");
            query.AddAttribute(query.Source, "Character");
            query.AddAttribute(personSrc, "Nationality");
            query.AddAttribute(query.Source, "RegDate");
            var table = new DataTable();
            using (var reader = context.CreateSqlReader(query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            var list = new List<AvailableChildrenItem>();
            foreach (DataRow r in table.Rows)
            {
                var bd = r[0] is DBNull ? DateTime.MinValue : (DateTime)r[0];
                var genderId = r[1] is DBNull ? Guid.Empty : (Guid)r[1];
                var diagnosis = r[2] is DBNull ? string.Empty : r[2].ToString();
                var docId = (Guid)r[3];
                var orgId = (Guid)r[4];
                var PrimetyRebenka = r[5] is DBNull ? string.Empty : r[5].ToString();
                var Character = r[6] is DBNull ? string.Empty : r[6].ToString();
                var Nationality = r[7] is DBNull ? Guid.Empty : (Guid)r[7];
                var regDate = r[8] is DBNull ? DateTime.MaxValue : (DateTime)r[8];
                if (bd == DateTime.MinValue)
                {
                    continue;
                }
                //if (genderId == Guid.Empty) continue;
                var age = DateTime.Today.Year - bd.Year;
                if (DateTime.Today > new DateTime(DateTime.Today.Year, bd.Month, bd.Day)) age--;

                list.Add(new AvailableChildrenItem
                {
                    RegDate = regDate,
                    Age = age,
                    Gender = genderId,
                    Diagnosis = diagnosis,
                    DocumentId = docId,
                    OrgId = orgId,
                    PrimetyRebenka = PrimetyRebenka,
                    Character = Character,
                    Nationality = Nationality
                });

            }

            return list.ToArray();
        }
        public static AvailableChildrenItemDetail GetAvailableChildrenItem(Guid childId)
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var qb = new QueryBuilder(childDefId);
            qb.Where("&Id").Eq(childId);
            var query = context.CreateSqlQuery(qb.Def);
            var personSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Person");

            query.AddAttribute(personSrc, "BirthDate");
            query.AddAttribute(personSrc, "Gender");
            query.AddAttribute(query.Source, "HealthDescription");
            query.AddAttribute(query.Source, "&Id");
            query.AddAttribute(query.Source, "&OrgId");
            query.AddAttribute(query.Source, "PrimetyRebenka");
            query.AddAttribute(query.Source, "Character");
            query.AddAttribute(personSrc, "Nationality");
            var table = new DataTable();
            using (var reader = context.CreateSqlReader(query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            var list = new List<AvailableChildrenItemDetail>();
            foreach (DataRow r in table.Rows)
            {
                var bd = r[0] is DBNull ? DateTime.MinValue : (DateTime)r[0];
                var genderId = r[1] is DBNull ? Guid.Empty : (Guid)r[1];
                var diagnosis = r[2] is DBNull ? string.Empty : r[2].ToString();
                var docId = (Guid)r[3];
                var orgId = (Guid)r[4];
                var PrimetyRebenka = r[5] is DBNull ? string.Empty : r[5].ToString();
                var Character = r[6] is DBNull ? string.Empty : r[6].ToString();
                var Nationality = r[7] is DBNull ? Guid.Empty : (Guid)r[7];
                if (bd == DateTime.MinValue)
                {
                    continue;
                }
                //if (genderId == Guid.Empty) continue;
                var age = DateTime.Today.Year - bd.Year;
                if (DateTime.Today > new DateTime(DateTime.Today.Year, bd.Month, bd.Day)) age--;

                list.Add(new AvailableChildrenItemDetail
                {
                    Age = age,
                    Gender = GetEnumValue(context, genderId),
                    Diagnosis = diagnosis,
                    OrgName = context.Orgs.GetOrgName(orgId),
                    PrimetyRebenka = PrimetyRebenka,
                    Character = Character,
                    Nationality = GetEnumValue(context, Nationality)
                });

            }
            if (list.Count > 0)
                return list[0];
            else return null;
        }
        static Dictionary<Guid, string> EnumDic { get; set; }
        public static string GetEnumValue(WorkflowContext context, Guid enumId)
        {
            if (EnumDic == null) EnumDic = new Dictionary<Guid, string>();
            if (EnumDic.ContainsKey(enumId)) return EnumDic[enumId];
            else
            {
                var val = enumId == Guid.Empty ? "-" : context.Enums.GetValue(enumId).Value;
                EnumDic.Add(enumId, val);
                return val;
            }
        }



        public class UserStatus
        {
            public string StatusName { get; set; }
            public DateTime StatusDate { get; set; }
            public string AdditionalInfo { get; set; }
        }


        public static AdpotionCandidate[] GetAdpotionCandidates()
        {
            var db = new EFDbContext();
            return db.AdpotionCandidates.ToArray();
        }

        /// <summary>
        /// Анкета кандидата
        /// </summary>
        public class CandidateDetails
        {
            /// <summary>
            /// УСР кандидата
            /// </summary>
            public string OrgName { get; set; }
            /// <summary>
            /// Айди района
            /// </summary>
            public Guid OrgId { get; set; }
            /// <summary>
            /// Дата выдачи заключения
            /// </summary>
            public DateTime CertificateDate { get; set; }
            /// <summary>
            /// Начальник УСР
            /// </summary>
            public string ManagerName { get; set; }
            /// <summary>
            /// Дата постановки на учет
            /// </summary>
            public string RegDate { get; set; }
            /// <summary>
            /// Заявитель
            /// </summary>
            public Person ApplicantPerson { get; set; }
            /// <summary>
            /// Телефон
            /// </summary>
            public string Telephone { get; set; }
            /// <summary>
            /// Место работы, вид деятельности заявителя
            /// </summary>
            public string ApplicantJobDetails { get; set; }
            /// <summary>
            /// Заявитель работает
            /// </summary>
            public string IsApplicantEmployed { get; set; }
            /// <summary>
            /// Причины не занятости заявителя
            /// </summary>
            public string ApplicantUnemployedReason { get; set; }
            /// <summary>
            /// Характер работы заявителя
            /// </summary>
            public string ApplicantWorkNature1 { get; set; }
            /// <summary>
            /// Тип работы заявителя
            /// </summary>
            public string ApplicantWorkNature2 { get; set; }
            /// <summary>
            /// Супруг (а)
            /// </summary>
            public Person SpousePerson { get; set; }
            /// <summary>
            /// Место работы, вид деятельности супруга (и)
            /// </summary>
            public string SpouseJobDetails { get; set; }
            /// <summary>
            /// Супруг(а) работает
            /// </summary>
            public string IsSpouseEmployed { get; set; }
            /// <summary>
            /// Причины не занятости супруга (и)
            /// </summary>
            public string SpouseUnemployedReason { get; set; }
            /// <summary>
            /// Характер работы супруга(и)
            /// </summary>
            public string SpouseWorkNature1 { get; set; }
            /// <summary>
            /// Тип работы супруга(и)
            /// </summary>
            public string SpouseWorkNature2 { get; set; }

            /// <summary>
            /// Адрес фактического проживания
            /// </summary>
            public AddressDetails AddressDetails { get; set; }

        }

        /// <summary>
        /// Адресные данные
        /// </summary>
        public class AddressDetails
        {
            /// <summary>
            /// Область
            /// </summary>
            public string Area { get; set; }
            /// <summary>
            /// Район
            /// </summary>
            public string District { get; set; }
            /// <summary>
            /// Айылный аймак
            /// </summary>
            public string Settlement { get; set; }
            /// <summary>
            /// Населенный пункт
            /// </summary>
            public string Village { get; set; }
            /// <summary>
            /// Улица (мкр, ж-м)
            /// </summary>
            public string Street { get; set; }
            /// <summary>
            /// Дом (корпус, строение)
            /// </summary>
            public string House { get; set; }
            /// <summary>
            /// Квартира
            /// </summary>
            public string Apartment { get; set; }
        }

        /// <summary>
        /// Физ. лицо
        /// </summary>
        public class Person
        {
            /// <summary>
            /// ПИН
            /// </summary>
            public string PIN { get; set; }
            /// <summary>
            /// Фамилия
            /// </summary>
            public string LastName { get; set; }
            /// <summary>
            /// Имя
            /// </summary>
            public string FirstName { get; set; }
            /// <summary>
            /// Отчество
            /// </summary>
            public string MiddleName { get; set; }
            /// <summary>
            /// Номер, серия паспорта
            /// </summary>
            public string PassportNo { get; set; }
            /// <summary>
            /// Выдавший орган
            /// </summary>
            public string PassportOrg { get; set; }
            /// <summary>
            /// Дата выдачи
            /// </summary>
            public string PassportDate { get; set; }
        }

        static Guid candidateCaseDefId = new Guid("{35B15C63-13ED-4B17-A57A-539C4D38A985}");
        static Guid certificateDefId = new Guid("{B7EF62BF-4E3C-4225-AC4C-E286C4F519C2}");
        static Guid candidateAppDefId = new Guid("{4810668C-6A34-4BF4-9113-3DB107812832}");
        static Guid childHouseDefId = new Guid("{C0C2A073-DE36-4B84-A370-81D264501760}");
        static Guid areaDefId = new Guid("{99D6F2C3-C138-4BDD-BD5B-BCAE0EF11AC6}");
        static Guid districtDefId = new Guid("{A3FCA356-82A9-4BBD-872A-8333BEC6E41A}");
        static Guid settlementDefId = new Guid("{80CC229E-05FC-45A5-B2ED-F101465ADD1E}");
        static Guid villageDefId = new Guid("{BD0E1850-FFE9-4EBA-B86E-98069DF7B885}");
        public static CandidateDetails GetCandidateDetails(string pin)
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var qb = new QueryBuilder(candidateAppDefId);
            var query = context.CreateSqlQuery(qb.Def);
            var appPersonSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "ApplicantPerson");
            var certSrc = query.JoinSource(query.Source, certificateDefId, SqlSourceJoinType.Inner, "CandidateApplication");
            var areaSrc = query.JoinSource(query.Source, areaDefId, SqlSourceJoinType.LeftOuter, "Area");
            var districtSrc = query.JoinSource(query.Source, districtDefId, SqlSourceJoinType.LeftOuter, "District");
            var settlementSrc = query.JoinSource(query.Source, settlementDefId, SqlSourceJoinType.LeftOuter, "Settlement");
            var villageSrc = query.JoinSource(query.Source, villageDefId, SqlSourceJoinType.LeftOuter, "Village");

            query.AddCondition(ExpressionOperation.And, personDefId, "PIN", ConditionOperation.Equal, pin);
            query.AddAttribute(appPersonSrc, "LastName");
            query.AddAttribute(appPersonSrc, "FirstName");
            query.AddAttribute(appPersonSrc, "MiddleName");
            query.AddAttribute(query.Source, "Telephone");
            query.AddAttribute(query.Source, "RegDate");
            query.AddAttribute(query.Source, "ApplicantJobDetails");
            query.AddAttribute(query.Source, "IsApplicantEmployed");
            query.AddAttribute(query.Source, "ApplicantUnemployedReason");
            query.AddAttribute(query.Source, "ApplicantWorkNature1");
            query.AddAttribute(query.Source, "ApplicantWorkNature2");
            query.AddAttribute(query.Source, "SpousePerson");
            query.AddAttribute(query.Source, "SpouseJobDetails");
            query.AddAttribute(query.Source, "IsSpouseEmployed");
            query.AddAttribute(query.Source, "SpouseUnemployedReason");
            query.AddAttribute(query.Source, "SpouseWorkNature1");
            query.AddAttribute(query.Source, "SpouseWorkNature2");
            query.AddAttribute(areaSrc, "Name");
            query.AddAttribute(districtSrc, "Name");
            query.AddAttribute(settlementSrc, "Name");
            query.AddAttribute(villageSrc, "Name");
            query.AddAttribute(query.Source, "Street");
            query.AddAttribute(query.Source, "House");
            query.AddAttribute(query.Source, "Apartment");
            query.AddAttribute(query.Source, "&OrgId");
            query.AddAttribute(appPersonSrc, "PassportSeries");
            query.AddAttribute(appPersonSrc, "PassportNo");
            query.AddAttribute(appPersonSrc, "IssuedBy");
            query.AddAttribute(appPersonSrc, "DateOfIssue");
            query.AddAttribute(certSrc, "RegDate");
            CandidateDetails model = null;
            using (var reader = context.CreateSqlReader(query))
            {
                if (reader.Read())
                {
                    model = new CandidateDetails
                    {
                        ApplicantPerson = new Person
                        {
                            PIN = pin,
                            LastName = !reader.IsDbNull(0) ? reader.GetString(0) : "",
                            FirstName = !reader.IsDbNull(1) ? reader.GetString(1) : "",
                            MiddleName = !reader.IsDbNull(2) ? reader.GetString(2) : "",
                        },
                        Telephone = !reader.IsDbNull(3) ? reader.GetString(3) : "",
                        RegDate = !reader.IsDbNull(4) ? reader.GetDateTime(4).ToString("dd.MM.yyyy") : "",
                        ApplicantJobDetails = !reader.IsDbNull(5) ? reader.GetString(5) : "",
                        IsApplicantEmployed = GetEnumValue(context, !reader.IsDbNull(6) ? reader.GetGuid(6) : Guid.Empty),
                        ApplicantUnemployedReason = !reader.IsDbNull(7) ? reader.GetString(7) : "",
                        ApplicantWorkNature1 = GetEnumValue(context, !reader.IsDbNull(8) ? reader.GetGuid(8) : Guid.Empty),
                        ApplicantWorkNature2 = GetEnumValue(context, !reader.IsDbNull(9) ? reader.GetGuid(9) : Guid.Empty),
                        SpousePerson = InitPerson(context, !reader.IsDbNull(10) ? reader.GetGuid(10) : Guid.Empty),
                        SpouseJobDetails = !reader.IsDbNull(11) ? reader.GetString(11) : "",
                        IsSpouseEmployed = GetEnumValue(context, !reader.IsDbNull(12) ? reader.GetGuid(12) : Guid.Empty),
                        SpouseUnemployedReason = !reader.IsDbNull(13) ? reader.GetString(13) : "",
                        SpouseWorkNature1 = GetEnumValue(context, !reader.IsDbNull(14) ? reader.GetGuid(14) : Guid.Empty),
                        SpouseWorkNature2 = GetEnumValue(context, !reader.IsDbNull(15) ? reader.GetGuid(15) : Guid.Empty),
                        AddressDetails = new AddressDetails
                        {
                            Area = !reader.IsDbNull(16) ? reader.GetString(16) : "",
                            District = !reader.IsDbNull(17) ? reader.GetString(17) : "",
                            Settlement = !reader.IsDbNull(18) ? reader.GetString(18) : "",
                            Village = !reader.IsDbNull(19) ? reader.GetString(19) : "",
                            Street = !reader.IsDbNull(20) ? reader.GetString(20) : "",
                            House = !reader.IsDbNull(21) ? reader.GetString(21) : "",
                            Apartment = !reader.IsDbNull(22) ? reader.GetString(22) : "",
                        },
                        OrgName = context.Orgs.GetOrgName(reader.GetGuid(23)),
                        OrgId = reader.GetGuid(23),
                    };
                    var PassportSeries = !reader.IsDbNull(24) ? reader.GetString(24) : "";
                    var PassportNo = !reader.IsDbNull(25) ? reader.GetString(25) : "";
                    var IssuedBy = !reader.IsDbNull(26) ? reader.GetString(26) : "";
                    var DateOfIssue = !reader.IsDbNull(27) ? reader.GetDateTime(27).ToString("dd.MM.yyyy") : "";
                    var certDate = !reader.IsDbNull(28) ? reader.GetDateTime(28) : DateTime.MaxValue;
                    model.CertificateDate = certDate;
                    model.ApplicantPerson.PassportNo = PassportSeries + PassportNo;
                    model.ApplicantPerson.PassportOrg = IssuedBy;
                    model.ApplicantPerson.PassportDate = DateOfIssue;
                }
            }
            if(model != null)
            {
                var ui = context.Orgs.Get(model.OrgId);
                var proxyObj = new CissaMeta.MetaProxy();
                var ministryOrgTypeId = new Guid("{1E480AD8-F4C9-4FC3-8B51-061EF8DD92F3}");
                var districtOfficeOrgTypeId = new Guid("{1E00777A-E352-4FDD-9DD8-E569ACF4FD93}");
                var managerPositionId = ui.TypeId == ministryOrgTypeId ? new Guid("{208F787B-F729-4E05-8944-38ABCEA44D74}") : new Guid("{2773E423-5E4C-4D14-A37A-241E2CC55111}");
                var managers = proxyObj.GetUsersByPositionId(managerPositionId, ui.Id);
                var managerName = "[начальник не найден]";
                if (managers != null && managers.Count() > 0)
                {
                    var managerInfo = managers.First();
                    if (managerInfo.Person != null)
                    {
                        if (!string.IsNullOrEmpty(managerInfo.Person.Last_Name))
                            managerName = managerInfo.Person.Last_Name;
                        if (!string.IsNullOrEmpty(managerInfo.Person.First_Name))
                            managerName += " " + (managerInfo.Person.First_Name.Length > 0 ? managerInfo.Person.First_Name.Trim()[0].ToString().ToUpper() + "." : "");

                    }
                }
                model.ManagerName = managerName;
            }
            
            return model;
        }
        public static Guid GetCandidateCaseId(string pin)
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var qb = new QueryBuilder(candidateCaseDefId);
            qb.Where("Certificate").Include("CandidateApplication").Include("ApplicantPerson").Include("PIN").Eq(pin).End().End().End();
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttribute("&Id");
            using (var reader = context.CreateSqlReader(query))
            {
                if (reader.Read()) return reader.GetGuid(0);
            }
            return Guid.Empty;
        }
        static Person InitPerson(WorkflowContext context, Guid personId)
        {
            if(personId != Guid.Empty)
            {
                var p = context.Documents.LoadById(personId);
                var person = new Person
                {
                    PIN = p["PIN"] != null ? p["PIN"].ToString() : "",
                    LastName = p["LastName"] != null ? p["LastName"].ToString() : "",
                    FirstName = p["FirstName"] != null ? p["FirstName"].ToString() : "",
                    MiddleName = p["MiddleName"] != null ? p["MiddleName"].ToString() : ""
                };
                return person;
            }
            return new Person();
        }

        /// <summary>
        /// Анкета ребенка
        /// </summary>
        public class ChildDetails
        {
            /// <summary>
            /// Район ребенка
            /// </summary>
            public string OrgName { get; set; }
            /// <summary>
            /// ПИН ребенка
            /// </summary>
            public string PIN { get; set; }
            /// <summary>
            /// Фамилия
            /// </summary>
            public string LastName { get; set; }
            /// <summary>
            /// Имя
            /// </summary>
            public string FirstName { get; set; }
            /// <summary>
            /// Возраст ребенка
            /// </summary>
            public int Age { get; set; }
            /// <summary>
            /// Пол ребенка
            /// </summary>
            public string Gender { get; set; }
            /// <summary>
            /// Национальность
            /// </summary>
            public string Nationality { get; set; }
            /// <summary>
            /// Приметы
            /// </summary>
            public string PrimetyRebenka { get; set; }
            /// <summary>
            /// Характер
            /// </summary>
            public string Character { get; set; }
            /// <summary>
            /// Состояние здоровья
            /// </summary>
            public string Diagnosis { get; set; }
            /// <summary>
            /// Детское учреждение
            /// </summary>
            public string HouseName { get; set; }
            /// <summary>
            /// Адрес учреждения
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// Причина отсутствия родительского попечительства
            /// </summary>
            public _OrphanageReason OrphanageReason { get; set; }
            /// <summary>
            /// Дата постановки на учет
            /// </summary>
            public string RegDate { get; set; }
            /// <summary>
            /// Причины отсутствия родительского попечительства
            /// </summary>
            public class _OrphanageReason
            {
                /// <summary>
                /// Смерть родителей
                /// </summary>
                public bool OrphanageReason1 { get; set; }
                /// <summary>
                /// Ограничение в родительских правах
                /// </summary>
                public bool OrphanageReason2 { get; set; }
                /// <summary>
                /// Местонахождение не известно
                /// </summary>
                public bool OrphanageReason3 { get; set; }
                /// <summary>
                /// Лишение родительских прав
                /// </summary>
                public bool OrphanageReason4 { get; set; }
                /// <summary>
                /// Заявление о согласии на усыновление/удочерение
                /// </summary>
                public bool OrphanageReason5 { get; set; }
                /// <summary>
                /// Оставленный/брошенный
                /// </summary>
                public bool OrphanageReason6 { get; set; }
                /// <summary>
                /// В связи с признанием родителей недееспособными
                /// </summary>
                public bool OrphanageReason7 { get; set; }
                /// <summary>
                /// Родители, в местах лишения свободы
                /// </summary>
                public bool OrphanageReason8 { get; set; }
            }

            /// <summary>
            /// Статус сирота
            /// </summary>
            public string OrphanType { get; set; }
            /// <summary>
            /// Возможные формы устройства ребенка
            /// </summary>
            public string PossibleAdoptionType { get; set; }


        }
        public static ChildDetails GetChildDetails(Guid childId)
        {
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            var qb = new QueryBuilder(childDefId);
            qb.Where("&Id").Eq(childId);
            var query = context.CreateSqlQuery(qb.Def);
            var personSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Person");
            var childHouseSrc = query.JoinSource(query.Source, childHouseDefId, SqlSourceJoinType.LeftOuter, "ChildrenHouse");
            query.AddAttribute(personSrc, "BirthDate");
            query.AddAttribute(personSrc, "Gender");
            query.AddAttribute(query.Source, "HealthDescription");
            query.AddAttribute(query.Source, "&Id");
            query.AddAttribute(query.Source, "&OrgId");
            query.AddAttribute(query.Source, "PrimetyRebenka");
            query.AddAttribute(query.Source, "Character");
            query.AddAttribute(personSrc, "Nationality");
            query.AddAttribute(personSrc, "PIN");
            query.AddAttribute(personSrc, "LastName");
            query.AddAttribute(personSrc, "FirstName");
            query.AddAttribute(childHouseSrc, "Name");
            query.AddAttribute(childHouseSrc, "Address");
            query.AddAttribute(query.Source, "RegDate");
            query.AddAttribute(query.Source, "OrphanageReason1");
            query.AddAttribute(query.Source, "OrphanageReason2");
            query.AddAttribute(query.Source, "OrphanageReason3");
            query.AddAttribute(query.Source, "OrphanageReason4");
            query.AddAttribute(query.Source, "OrphanageReason5");
            query.AddAttribute(query.Source, "OrphanageReason6");
            query.AddAttribute(query.Source, "OrphanageReason7");
            query.AddAttribute(query.Source, "OrphanageReason8");
            query.AddAttribute(query.Source, "OrphanType");
            query.AddAttribute(query.Source, "PossibleAdoptionType");


            var table = new DataTable();
            using (var reader = context.CreateSqlReader(query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            var list = new List<ChildDetails>();
            foreach (DataRow r in table.Rows)
            {
                var bd = r[0] is DBNull ? DateTime.MinValue : (DateTime)r[0];
                var genderId = r[1] is DBNull ? Guid.Empty : (Guid)r[1];
                var diagnosis = r[2] is DBNull ? string.Empty : r[2].ToString();
                var docId = (Guid)r[3];
                var orgId = (Guid)r[4];
                var PrimetyRebenka = r[5] is DBNull ? string.Empty : r[5].ToString();
                var Character = r[6] is DBNull ? string.Empty : r[6].ToString();
                var Nationality = r[7] is DBNull ? Guid.Empty : (Guid)r[7];
                var PIN = r[8] is DBNull ? string.Empty : r[8].ToString();
                var LastName = r[9] is DBNull ? string.Empty : r[9].ToString();
                var FirstName = r[10] is DBNull ? string.Empty : r[10].ToString();
                var house = r[11] is DBNull ? string.Empty : r[11].ToString();
                var address = r[12] is DBNull ? string.Empty : r[12].ToString();
                var regDate = r[13] is DBNull ? DateTime.MinValue : (DateTime)r[13];
                var reason1 = r[14] is DBNull ? false : (bool)r[14];
                var reason2 = r[15] is DBNull ? false : (bool)r[15];
                var reason3 = r[16] is DBNull ? false : (bool)r[16];
                var reason4 = r[17] is DBNull ? false : (bool)r[17];
                var reason5 = r[18] is DBNull ? false : (bool)r[18];
                var reason6 = r[19] is DBNull ? false : (bool)r[19];
                var reason7 = r[20] is DBNull ? false : (bool)r[20];
                var reason8 = r[21] is DBNull ? false : (bool)r[21];
                var orphanTypeId = r[22] is DBNull ? Guid.Empty : (Guid)r[22];
                var possibleAdoptionType = r[23] is DBNull ? string.Empty : r[23].ToString();

                if (bd == DateTime.MinValue)
                {
                    continue;
                }
                //if (genderId == Guid.Empty) continue;
                var age = DateTime.Today.Year - bd.Year;
                if (DateTime.Today > new DateTime(DateTime.Today.Year, bd.Month, bd.Day)) age--;

                list.Add(new ChildDetails
                {
                    Age = age,
                    Gender = GetEnumValue(context, genderId),
                    Diagnosis = diagnosis,
                    PrimetyRebenka = PrimetyRebenka,
                    Character = Character,
                    Nationality = GetEnumValue(context, Nationality),
                    PIN = PIN,
                    LastName = LastName,
                    FirstName = FirstName,
                    HouseName = house,
                    Address = address,
                    RegDate = regDate.ToString("dd.MM.yyyy"),
                    OrphanType = GetEnumValue(context, orphanTypeId),
                    PossibleAdoptionType = possibleAdoptionType,
                    OrphanageReason = new ChildDetails._OrphanageReason
                    {
                        OrphanageReason1 = reason1,
                        OrphanageReason2 = reason2,
                        OrphanageReason3 = reason3,
                        OrphanageReason4 = reason4,
                        OrphanageReason5 = reason5,
                        OrphanageReason6 = reason6,
                        OrphanageReason7 = reason7,
                        OrphanageReason8 = reason8
                    },
                    OrgName = context.Orgs.GetOrgName(orgId)
                });

            }
            if (list.Count > 0)
                return list[0];
            else return null;
        }
        static Guid childVisitDirectionDefId = new Guid("{8294306B-ADAB-4CDF-9602-7BFA709CC312}");
        public static bool CreateDirection(Guid orgId, Guid childId, string candidatePIN, out Guid docId)
        {
            docId = Guid.Empty;
            if (!GetUserInfo(orgId, out Guid userId, out string userName))
            {
                return false;
            }
            var userContext = CreateContext(userName, userId);
            var userDocRepo = userContext.Documents;

            var direction = userDocRepo.New(childVisitDirectionDefId);
            
            direction["Child"] = childId;
            direction["RegDate"] = DateTime.Now;
            var regNo = GeneratorRepository.GetNewId(userContext.DataContext, orgId, direction.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;
            direction["RegNo"] = "ГБД-ПОРТАЛ-" + regNo;
            direction["Candidate"] = GetCandidateCaseId(candidatePIN);
            direction["OrgProfile"] = GetOrgProfileId(userContext, orgId);
            userDocRepo.Save(direction);
            userDocRepo.SetDocState(direction.Id, registeredStateId);
            docId = direction.Id;
            return true;
        }
        static bool GetUserInfo(Guid orgId, out Guid userId, out string userName)
        {
            userId = Guid.Empty;
            userName = "";
            string data_DB_connectionString = "Data Source=192.168.0.2;Initial Catalog=cissa-meta;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Max Pool Size=3500;Connect Timeout=300;";
            SqlConnection sqlConnection1 = new SqlConnection(data_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = "select w.Id, w.User_Name from Object_Defs od inner join Workers w on w.Id = od.Id where w.OrgPosition_Id is not null and od.Parent_Id = '" + orgId + "'";

            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                    userName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                }
            }

            sqlConnection1.Close();

            return userId != Guid.Empty && !string.IsNullOrEmpty(userName);
        }
        static Guid GetOrgProfileId(WorkflowContext context, Guid orgId)
        {
            Guid orgProfileDefId = new Guid("{BB23529B-072D-4E85-87D4-337473458EC2}");
            var qb = new QueryBuilder(orgProfileDefId, context.UserId);
            qb.Where("&OrgId").Eq(orgId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttribute("&Id");
            using (var reader = context.CreateSqlReader(query))
            {
                if (reader.Read())
                {
                    var orgProfileId = reader.GetGuid(0);
                    return orgProfileId;
                }
            }
            var orgInfo = context.Orgs.Get(orgId);

            throw new ApplicationException("Профиль организации не найден для \"" + orgInfo.Name + "\"!");
        }

        /// <summary>
        /// Результат посещения ребенка
        /// </summary>
        public class VisitResult
        {
            /// <summary>
            /// Дата регистрации
            /// </summary>
            public DateTime RegDate { get; set; }
            /// <summary>
            /// Айди запроса встречи
            /// </summary>
            public int MeetupRequestItemId { get; set; }
            /// <summary>
            /// Решение кандидата
            /// </summary>
            public Guid CandidateSolutionId { get; set; }
            /// <summary>
            /// Причина отказа
            /// </summary>
            public string RefuseReason { get; set; }
        }
        /// <summary>
        /// Просмотр результата посещений
        /// </summary>
        public class VisitResultDetails
        {
            /// <summary>
            /// Направление
            /// </summary>
            public MeetupRequest MeetupRequest { get; set; }
            /// <summary>
            /// Решение кандидата
            /// </summary>
            public string CandidateSolution { get; set; }
            /// <summary>
            /// Причина отказа
            /// </summary>
            public string RefuseReason { get; set; }
        }
        static Guid visitResultDefId = new Guid("{FBE67FBF-906B-4B77-98ED-FC0EBB18081A}");
        public static bool SetVisitResult(VisitResult visitResult)
        {
            var refusedDecisionId = new Guid("{0D7921D2-E8AC-40D0-AAD6-93FF5BD7BE66}");
            var acceptedDecisionId = new Guid("{46CB0B76-2A82-4E20-9F55-CDFAE5393BDF}");
            var db = new EFDbContext();
            var reqObj = db.MeetupRequestItems.FirstOrDefault(x => x.Id == visitResult.MeetupRequestItemId);
            
            if(reqObj.VisitResultId != null)
            {
                return false;
            }

            if (!GetUserInfo(reqObj.AdpotionCandidate.OrgId, out Guid userId, out string userName))
            {
                return false;
            }
            var userContext = CreateContext(userName, userId);
            var userDocRepo = userContext.Documents;

            var visit = userDocRepo.New(visitResultDefId);

            visit["ChildVisitDirection"] = reqObj.DirectionId;
            visit["CandidateSolution"] = visitResult.CandidateSolutionId;
            visit["RefusedBy"] = visitResult.RefuseReason;
            visit["RegDate"] = visitResult.RegDate;
            var regNo = GeneratorRepository.GetNewId(userContext.DataContext, visit.OrganizationId.Value, visit.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;
            visit["RegNo"] = "ГБД-ПОРТАЛ-" + regNo;
            visit["OrgProfile"] = GetOrgProfileId(userContext, visit.OrganizationId.Value);
            userDocRepo.Save(visit);
            userDocRepo.SetDocState(visit.Id, registeredStateId);
            reqObj.VisitResultId = visit.Id;
            db.Entry(reqObj).State = EntityState.Modified;

            if(visitResult.CandidateSolutionId == acceptedDecisionId)
            {
                db.AdoptionOrders.Add(new AdoptionOrder
                {
                    Date = visitResult.RegDate.AddDays(1),
                    MeetupRequestItemId = visitResult.MeetupRequestItemId,
                    No = "ГБД-ПОРТАЛ-" + regNo
                });
            }

            db.SaveChanges();
            
            return true;
        }
        public static VisitResult GetVisitResultDetails(int meetupRequestItemId)
        {
            var db = new EFDbContext();
            var reqObj = db.MeetupRequestItems.Include(x => x.AdpotionCandidate).FirstOrDefault(x => x.Id == meetupRequestItemId);
            if (reqObj != null)
            {
                if (reqObj.VisitResultId != null)
                {
                    var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
                    var visitResultDoc = context.Documents.LoadById(reqObj.VisitResultId.Value);
                    return new VisitResult
                    {
                        CandidateSolutionId = (Guid)visitResultDoc["CandidateSolution"],
                        MeetupRequestItemId = meetupRequestItemId,
                        RefuseReason = visitResultDoc["RefusedBy"] != null ? visitResultDoc["RefusedBy"].ToString() : "",
                        RegDate = (DateTime)visitResultDoc["RegDate"]
                    };
                }
                else return null;
            }
            else return null;
        }
        public static VisitResult[] GetVisitResultDetails(Guid orgId)
        {
            var db = new EFDbContext();
            var reqObjs = db.MeetupRequestItems.Include(x => x.AdpotionCandidate).Where(x => x.AdpotionCandidate.OrgId == orgId).ToList();
            var list = new List<VisitResult>();
            var context = CreateContext(portal_childrenUserName, portal_childrenUserId);
            foreach (var reqObj in reqObjs)
            {
                if (reqObj.VisitResultId != null)
                {
                    var visitResultDoc = context.Documents.LoadById(reqObj.VisitResultId.Value);
                    list.Add(new VisitResult
                    {
                        CandidateSolutionId = (Guid)visitResultDoc["CandidateSolution"],
                        MeetupRequestItemId = reqObj.Id,
                        RefuseReason = visitResultDoc["RefusedBy"] != null ? visitResultDoc["RefusedBy"].ToString() : "",
                        RegDate = (DateTime)visitResultDoc["RegDate"]
                    });
                }
            }
            return list.ToArray();
        }

        public static bool CreateCandidateApp(Controllers.CreateAppViewModel model)
        {
            if (!GetUserInfo(model.DistrictId, out Guid userId, out string userName))
            {
                return false;
            }
            var userContext = CreateContext(userName, userId);
            var userDocRepo = userContext.Documents;

            var candidateApp = userDocRepo.New(candidateAppDefId);

            candidateApp["RegDate"] = DateTime.Now;
            var regNo = GeneratorRepository.GetNewId(userContext.DataContext, candidateApp.OrganizationId.Value, candidateApp.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;
            candidateApp["RegNo"] = "ГБД-ПОРТАЛ-" + regNo;
            candidateApp["OrgProfile"] = GetOrgProfileId(userContext, candidateApp.OrganizationId.Value);

            var applicantPerson = userDocRepo.New(personDefId);
            applicantPerson["PIN"] = model.passportPerson.pin;
            applicantPerson["LastName"] = model.passportPerson.surname;
            applicantPerson["FirstName"] = model.passportPerson.name;
            applicantPerson["MiddleName"] = model.passportPerson.patronymic;
            applicantPerson["BirthDate"] = model.passportPerson.dateOfBirth;
            applicantPerson["PassportSeries"] = model.PassportSeries;
            applicantPerson["PassportNo"] = model.PassportNo;
            applicantPerson["DateOfIssue"] = model.passportPerson.issuedDate;
            applicantPerson["IssuedBy"] = model.passportPerson.passportAuthority;

            userDocRepo.Save(applicantPerson);
            candidateApp["ApplicantPerson"] = applicantPerson.Id;
            candidateApp["Telephone"] = model.Telephone;

            var cissaAppDefId = new Guid("{9F44B322-E9FD-4EE1-BF72-C02B8A2D225F}");
            var cissaApp = userDocRepo.New(cissaAppDefId);
            regNo = GeneratorRepository.GetNewId(userContext.DataContext, cissaApp.OrganizationId.Value, cissaApp.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;
            cissaApp["No"] = "ГБД-ПОРТАЛ-" + regNo;
            cissaApp["PIN"] = model.ApplicantPIN;
            var enumItems = userContext.Enums.GetEnumItems(new Guid("{0552D296-1EA9-4A65-B7B5-764BA78D2D86}"));
            var pSeriesObj = enumItems.FirstOrDefault(x => x.Value.Trim().ToUpper() == model.PassportSeries.Trim().ToUpper());
            if (pSeriesObj != null)
                cissaApp["PassportSeries"] = pSeriesObj.Id;
            cissaApp["PassportNo"] = model.PassportNo;
            userDocRepo.Save(cissaApp);

            userDocRepo.Save(candidateApp);
            userDocRepo.SetDocState(candidateApp.Id, registeredStateId);
            return true;
        }
    }
    public static class CreatePdf
    {
        static iTextSharp.text.Phrase GetPhrase(string text)
        {
            return new iTextSharp.text.Phrase(text, fntHead);
        }
        static iTextSharp.text.Font fntHead = null;
        public static void Execute(int meetupItemId)
        {
            var db = new EFDbContext();
            var reqObj = db.MeetupRequestItems.Find(meetupItemId);
            if (reqObj.DirectionId == null) return;
            var directionId = reqObj.DirectionId.Value;
            var context = ScriptExecutor.CreateContext(ScriptExecutor.portal_childrenUserName, ScriptExecutor.portal_childrenUserId);
            var docRepo = context.Documents;
            var visit = docRepo.LoadById(directionId);
            var candidateId = Guid.Empty;
            var childId = Guid.Empty;
            if (visit["ChildSearchItem"] != null)
            {
                var searchItem = docRepo.LoadById((Guid)visit["ChildSearchItem"]);
                var search = docRepo.LoadById((Guid)searchItem["ChildSearchResult"]);
                candidateId = (Guid)search["Candidate"];
                childId = (Guid)searchItem["Child"];
            }
            else if (visit["Candidate"] != null)
            {
                candidateId = (Guid)visit["Candidate"];
                childId = (Guid)visit["Child"];
            }
            var candidate = docRepo.LoadById(candidateId);
            var child = docRepo.LoadById(childId);
            var cert = docRepo.LoadById((Guid)candidate["Certificate"]);
            var app = docRepo.LoadById((Guid)cert["CandidateApplication"]);
            var person = docRepo.LoadById((Guid)app["ApplicantPerson"]);
            var childPerson = docRepo.LoadById((Guid)child["Person"]);

            iTextSharp.text.Rectangle pgeSize = new iTextSharp.text.Rectangle(595, 792);
            var pTypesEnumDefId = new Guid("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
            var cTypesEnumDefId = new Guid("{9FF88649-11F9-4842-BD05-E0568F552724}");
            using (var fs = new System.IO.FileStream("C:\\CissaFiles\\VisitDirection_" + directionId.ToString("N") + ".pdf", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                var doc = new iTextSharp.text.Document();
                doc.SetPageSize(iTextSharp.text.PageSize.A4);
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();

                iTextSharp.text.pdf.BaseFont bfR;
                string myFont = @"C:\Windows\Fonts\Times.ttf";
                bfR = iTextSharp.text.pdf.BaseFont.CreateFont(myFont,
                  iTextSharp.text.pdf.BaseFont.IDENTITY_H,
                  iTextSharp.text.pdf.BaseFont.EMBEDDED);

                iTextSharp.text.BaseColor clrBlack = new iTextSharp.text.BaseColor(0, 0, 0);
                fntHead = new iTextSharp.text.Font(bfR, 12, iTextSharp.text.Font.NORMAL, clrBlack);

                iTextSharp.text.Paragraph pgr = new iTextSharp.text.Paragraph("Направление на посещение ребенка", fntHead);
                doc.Add(pgr);
                doc.Add(new iTextSharp.text.Paragraph("\n"));
                doc.Add(new iTextSharp.text.Paragraph("\n"));
                pgr = new iTextSharp.text.Paragraph("Начальник: ___________ ______________", fntHead);
                doc.Add(pgr);
                doc.Close();
            }
            return;
            var ui = context.GetUserInfo();
            context["OrgName"] = ui.OrganizationName;

            string passSeriesNo = person["PassportSeries"] != null && !string.IsNullOrEmpty(person["PassportSeries"].ToString().Trim()) ? person["PassportSeries"].ToString() : "[серия]";
            passSeriesNo += person["PassportNo"] != null && !string.IsNullOrEmpty(person["PassportNo"].ToString().Trim()) ? person["PassportNo"].ToString() : "[номер]";
            string issuedInfo = person["IssuedBy"] != null && !string.IsNullOrEmpty(person["IssuedBy"].ToString().Trim()) ? person["IssuedBy"].ToString() : "[выдавший орган]";
            issuedInfo += " от " + (person["DateOfIssue"] != null ? ((DateTime)person["DateOfIssue"]).ToShortDateString() : "[дата выдачи]");
            passSeriesNo += ", " + issuedInfo;

            context["CandidateName"] = string.Format("{0}, паспорт: {1}", GetPersonName(person), passSeriesNo);

            context["ChildName"] = GetPersonName(childPerson);
            context["ChildAddress"] = child["Address"];

            var regDate = (DateTime?)visit["RegDate"] ?? DateTime.MinValue;
            string regDay = "[--]";
            string regMonth = "[------]";
            string regYear = "[----]";
            if (regDate != DateTime.MinValue)
            {
                regDay = regDate.Day.ToString();
                regMonth = GetMonthName(regDate.Month);
                regYear = regDate.Year.ToString();
            }
            context["RegDay"] = regDay;
            context["RegMonth"] = regMonth;
            context["RegYear"] = regYear;

            var proxyObj = new CissaMeta.MetaProxy();
            var ministryOrgTypeId = new Guid("{1E480AD8-F4C9-4FC3-8B51-061EF8DD92F3}");
            var districtOfficeOrgTypeId = new Guid("{1E00777A-E352-4FDD-9DD8-E569ACF4FD93}");
            var managerPositionId = ui.OrganizationTypeId == ministryOrgTypeId ? new Guid("{208F787B-F729-4E05-8944-38ABCEA44D74}") : new Guid("{2773E423-5E4C-4D14-A37A-241E2CC55111}");
            var managers = proxyObj.GetUsersByPositionId(managerPositionId, ui.OrganizationId.Value);
            var managerName = "[начальник не найден]";
            if (managers != null && managers.Count() > 0)
            {
                var managerInfo = managers.First();
                if (managerInfo.Person != null)
                {
                    if (!string.IsNullOrEmpty(managerInfo.Person.Last_Name))
                        managerName = managerInfo.Person.Last_Name;
                    if (!string.IsNullOrEmpty(managerInfo.Person.First_Name))
                        managerName += " " + (managerInfo.Person.First_Name.Length > 0 ? managerInfo.Person.First_Name.Trim()[0].ToString().ToUpper() + "." : "");

                }
            }
            context["ManagerName"] = managerName;
        }
        static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "январь";
                case 2:
                    return "февраль";
                case 3:
                    return "март";
                case 4:
                    return "апрель";
                case 5:
                    return "май";
                case 6:
                    return "июнь";
                case 7:
                    return "июль";
                case 8:
                    return "август";
                case 9:
                    return "сентябрь";
                case 10:
                    return "октябрь";
                case 11:
                    return "ноябрь";
                case 12:
                    return "декабрь";
                default:
                    return "месяц " + month;
            }
        }

        static string GetPersonName(Doc person)
        {
            string name = string.Format("{0} {1}{2}",
                (person["LastName"] != null && !string.IsNullOrEmpty(person["LastName"].ToString().Trim()) ? person["LastName"].ToString() : "[фамилия]"),
                (person["FirstName"] != null && !string.IsNullOrEmpty(person["FirstName"].ToString().Trim()) ? person["FirstName"].ToString() : "[имя]"),
                (person["MiddleName"] != null && !string.IsNullOrEmpty(person["MiddleName"].ToString().Trim()) ? " " + person["MiddleName"].ToString() : ""));
            return name;
        }
    }

    public static class CreateTempApp
    {
        public class CandidateApplication
        {
            public int requestID { get; set; }
            public Person ApplicantPerson { get; set; }
            public string Telephone { get; set; }
            public JobDetails ApplicantJobDetails { get; set; }

            public Person SpousePerson { get; set; }
            public JobDetails SpouseJobDetails { get; set; }
            public AddressDetails Address { get; set; }
            public string HouseNature { get; set; }
            public string HouseNature2 { get; set; }
            public double TotalArea { get; set; }
            public double UsefulArea { get; set; }
            public int RoomAmount { get; set; }
            public string HouseOwnership { get; set; }
            public string AdditionalInformation { get; set; }
        }

        public class Person
        {
            public string PIN { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string PassportSeries { get; set; }
            public string PassportNo { get; set; }
            public DateTime DateOfIssue { get; set; }
            public string IssuedBy { get; set; }
            public string NationalityText { get; set; }
        }
        public class JobDetails
        {
            public string JobDetailsText { get; set; }
            public string WorkNature1 { get; set; }
            public string WorkNature2 { get; set; }
            public string HasLongBusinessTrips { get; set; }
            public string WorkSchedule { get; set; }
            public bool IsPensioner { get; set; }
            public bool IsEmployed { get; set; }
            public string UnemployedReason { get; set; }
        }

        public class AddressDetails
        {
            public string RegionId { get; set; }
            public string DistrictId { get; set; }
            public string SettlementId { get; set; }
            public string VillageId { get; set; }
            public string Street { get; set; }
            public string House { get; set; }
            public string Apartment { get; set; }
        }
    }
}