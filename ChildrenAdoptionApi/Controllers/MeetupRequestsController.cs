using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace ChildrenAdoptionApi.Controllers
{
    /// <summary>
    /// Запросы пользователей на встречу с ребенком
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MeetupRequestsController : ApiController
    {
        private EFDbContext db;
        public MeetupRequestsController()
        {
            db = new EFDbContext();
        }
        /// <summary>
        /// Создать запрос на встречу с ребенком
        /// </summary>
        /// <param name="childId">Ребенок</param>
        /// <param name="userId">Кандидат</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(bool))]
        public IHttpActionResult Create(Guid childId, Guid userId)
        {
            if (_GetActiveRequestId(childId) != 0)
            {
                return Ok(new { isSuccess = false });
            }
            
            var reqDate = DateTime.Now;
            var deadline = reqDate.AddDays(7);
            var req = new MeetupRequest
            {
                CreatedAt = reqDate,
                Deadline = deadline,
                ChildId = childId,
                Unread = true
            };
            db.MeetupRequests.Add(req);
            var userObj = db.AdpotionCandidates.Find(userId);
            var candidateObj = Models.ScriptExecutor.GetCandidateDetails(userObj.UserName);
            db.MeetupRequestItems.Add(new MeetupRequestItem
            {
                MeetupRequestId = req.Id,
                PortalUserId = userId,
                CreatedAt = reqDate,
                CandidateFullName = candidateObj.ApplicantPerson.LastName + " " + candidateObj.ApplicantPerson.FirstName + "" + candidateObj.ApplicantPerson.MiddleName,
                CertificateDate = candidateObj.CertificateDate
            });

            db.SaveChanges();

            return Ok(new { isSuccess = true });
        }

        /// <summary>
        /// Проверить наличие активного запроса у ребенка
        /// </summary>
        /// <param name="childId"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetActiveRequestId(Guid childId)
        {
            return Ok(_GetActiveRequestId(childId));
        }
        private int _GetActiveRequestId(Guid childId)
        {
            var reqObj = db.MeetupRequests.FirstOrDefault(x => x.ChildId == childId);
            if (reqObj != null) return reqObj.Id;
            return 0;
        }
        /// <summary>
        /// Создать запрос на встречу с ребенком
        /// </summary>
        /// <param name="meetupRequestId">Айди запроса на просмотр анкеты ребенка</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(bool))]
        public IHttpActionResult Join(int meetupRequestId, Guid userId)
        {
            var reqObj = db.MeetupRequests.Find(meetupRequestId);
            if(reqObj != null)
            {
                var userObj = db.AdpotionCandidates.Find(userId);
                var candidateObj = Models.ScriptExecutor.GetCandidateDetails(userObj.UserName);
                db.MeetupRequestItems.Add(new MeetupRequestItem
                {
                    MeetupRequestId = meetupRequestId,
                    CreatedAt = DateTime.Now,
                    CandidateFullName = candidateObj.ApplicantPerson.LastName + " " + candidateObj.ApplicantPerson.FirstName + " " + candidateObj.ApplicantPerson.MiddleName,
                    CertificateDate = candidateObj.CertificateDate,
                    PortalUserId = userId
                });

                db.SaveChanges();
            }
            return Ok(new { isSuccess = false });
        }

        [HttpGet]
public IHttpActionResult ResetDates()
        {
            var mReqs = db.MeetupRequests.ToList();
            foreach (var mReq in mReqs)
            {
                mReq.CreatedAt = mReq.CreatedAt.AddDays(-10);
                mReq.Deadline = mReq.Deadline.AddDays(-10);
                db.Entry(mReq).State = EntityState.Modified;

            }
            db.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult Clear()
        {
            var mReqs = db.MeetupRequests.ToList();
            foreach (var mReq in mReqs)
            {
                db.Entry(mReq).State = EntityState.Deleted;

            }
            db.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Просмотр запросов кандидата
        /// </summary>
        /// <param name="userId">Кандидат</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(MeetupRequest[]))]
        public IHttpActionResult GetByUserId(Guid userId)
        {
            var context = Models.ScriptExecutor.CreateContext(Models.ScriptExecutor.portal_childrenUserName, Models.ScriptExecutor.portal_childrenUserId);
            var reqs = db.MeetupRequests.Include(x => x.MeetupRequestItems).Where(x => x.MeetupRequestItems.Any(x1 => x1.PortalUserId == userId)).ToList();
            var orderedList = new List<MeetupRequestDetails>();
            foreach (var i in reqs)
            {
                var item = new MeetupRequestDetails
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    ChildId = i.ChildId,
                    Deadline = i.Deadline,
                    Unread = i.Unread,
                    MeetupRequestItems = new List<MeetupRequestItemDetails>()
                };
                int no = 1;
                foreach (var subItem in i.MeetupRequestItems.OrderBy(x => x.CertificateDate))
                {
                    subItem.OrderNo = no;
                    var newSubItem = new MeetupRequestItemDetails
                    {
                        AdpotionCandidate = subItem.AdpotionCandidate,
                        CreatedAt = subItem.CreatedAt,
                        CandidateFullName = subItem.CandidateFullName,
                        CertificateDate = subItem.CertificateDate,
                        Id = subItem.Id,
                        MeetupRequest = subItem.MeetupRequest,
                        MeetupRequestId = subItem.MeetupRequestId,
                        OrderNo = subItem.OrderNo,
                        PortalUserId = subItem.PortalUserId,
                        OrgName = context.Orgs.GetOrgName(subItem.AdpotionCandidate.OrgId),
                        DirectionId = subItem.DirectionId
                    };
                    if(subItem.VisitResultId != null)
                    {
                        newSubItem.VisitResult = Models.ScriptExecutor.GetVisitResultDetails(subItem.Id);
                    }
                    item.MeetupRequestItems.Add(newSubItem);
                    no++;
                }
                orderedList.Add(item);
            }
            return Ok(orderedList.ToArray());
        }

        /// <summary>
        /// Просмотр запросов кандидата
        /// </summary>
        /// <param name="childId">Ребенок</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetByChildId(Guid childId)
        {
            var openedRequest = db.MeetupRequests.FirstOrDefault(x => x.ChildId == childId && x.Deadline > DateTime.Now);
            if (openedRequest != null)
                return Ok(new { openedRequest.MeetupRequestItems.Count, openedRequest.Deadline });
            return Ok(new { Count = "", Deadline = "" });
        }

        /// <summary>
        /// Одобрить запрос
        /// </summary>
        /// <param name="requestItemId">Айди элемента запроса</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Accept(int requestItemId)
        {
            var reqObj = db.MeetupRequestItems.Include(x => x.AdpotionCandidate).FirstOrDefault(x => x.Id == requestItemId);
            if(reqObj != null)
            {
                if(reqObj.DirectionId != null)
                {
                    return Ok();
                }
                if(Models.ScriptExecutor.CreateDirection(reqObj.AdpotionCandidate.OrgId, reqObj.MeetupRequest.ChildId, reqObj.AdpotionCandidate.UserName, out Guid docId))
                {
                    reqObj.DirectionId = docId;
                    db.Entry(reqObj).State = EntityState.Modified;
                    db.SaveChanges();
                    return Ok();
                }
                else
                {
                    return BadRequest("Ошибка при попытке создать электронное направление в КИССП");
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Удалить запрос
        /// </summary>
        /// <param name="requestItemId">Айди заявки в запросе</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Delete(int requestItemId)
        {
            var reqObj = db.MeetupRequestItems.Find(requestItemId);
            if (reqObj != null)
            {
                db.MeetupRequestItems.Remove(reqObj);
                db.SaveChanges();
            }
            return Ok();
        }


        /// <summary>
        /// Просмотр запросов кандидата по району
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(MeetupRequestDetails[]))]
        public IHttpActionResult GetRequestsByOrgId(Guid orgId)
        {
            var context = Models.ScriptExecutor.CreateContext(Models.ScriptExecutor.portal_childrenUserName, Models.ScriptExecutor.portal_childrenUserId);
            var reqs = db.MeetupRequests.Include(x => x.MeetupRequestItems).Where(x => x.MeetupRequestItems.Any(x1 => x1.AdpotionCandidate.OrgId == orgId)).ToList();
            var orderedList = new List<MeetupRequestDetails>();
            foreach (var i in reqs)
            {
                var item = new MeetupRequestDetails
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    ChildId = i.ChildId,
                    Deadline = i.Deadline,
                    Unread = i.Unread,
                    MeetupRequestItems = new List<MeetupRequestItemDetails>()
                };
                int no = 1;
                foreach (var subItem in i.MeetupRequestItems.OrderBy(x => x.CertificateDate))
                {
                    subItem.OrderNo = no;
                    var newSubItem = new MeetupRequestItemDetails
                    {
                        AdpotionCandidate = subItem.AdpotionCandidate,
                        CreatedAt = subItem.CreatedAt,
                        CandidateFullName = subItem.CandidateFullName,
                        CertificateDate = subItem.CertificateDate,
                        Id = subItem.Id,
                        MeetupRequest = subItem.MeetupRequest,
                        MeetupRequestId = subItem.MeetupRequestId,
                        OrderNo = subItem.OrderNo,
                        PortalUserId = subItem.PortalUserId,
                        OrgName = context.Orgs.GetOrgName(subItem.AdpotionCandidate.OrgId)
                    };
                    if (subItem.VisitResultId != null)
                    {
                        newSubItem.VisitResult = Models.ScriptExecutor.GetVisitResultDetails(subItem.Id);
                    }
                    item.MeetupRequestItems.Add(newSubItem);
                    no++;
                }
                orderedList.Add(item);
            }
            return Ok(orderedList.ToArray());
        }

        /// <summary>
        /// Просмотр уведомлений на встречу
        /// </summary>
        /// <param name="orgId">Район</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(MeetupRequest[]))]
        public IHttpActionResult GetNotificationsByOrgId(Guid orgId)
        {
            var context = Models.ScriptExecutor.CreateContext(Models.ScriptExecutor.portal_childrenUserName, Models.ScriptExecutor.portal_childrenUserId);
            var reqs = db.MeetupRequests.Include(x => x.MeetupRequestItems).ToList();
            var orderedList = new List<MeetupRequestDetails>();
            foreach (var i in reqs)
            {
                var childCase = context.Documents.LoadById(i.ChildId);
                if (childCase.OrganizationId != orgId)
                {
                    continue;
                }
                var item = new MeetupRequestDetails
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    ChildId = i.ChildId,
                    Deadline = i.Deadline,
                    Unread = i.Unread,
                    MeetupRequestItems = new List<MeetupRequestItemDetails>()
                };
                int no = 1;
                foreach (var subItem in i.MeetupRequestItems.OrderBy(x => x.CertificateDate))
                {
                    subItem.OrderNo = no;
                    var newSubItem = new MeetupRequestItemDetails
                    {
                        AdpotionCandidate = subItem.AdpotionCandidate,
                        CreatedAt = subItem.CreatedAt,
                        CandidateFullName = subItem.CandidateFullName,
                        CertificateDate = subItem.CertificateDate,
                        Id = subItem.Id,
                        MeetupRequest = subItem.MeetupRequest,
                        MeetupRequestId = subItem.MeetupRequestId,
                        OrderNo = subItem.OrderNo,
                        PortalUserId = subItem.PortalUserId,
                        OrgName = context.Orgs.GetOrgName(subItem.AdpotionCandidate.OrgId)
                    };
                    if (subItem.VisitResultId != null)
                    {
                        newSubItem.VisitResult = Models.ScriptExecutor.GetVisitResultDetails(subItem.Id);
                    }
                    item.MeetupRequestItems.Add(newSubItem);
                    no++;
                }
                orderedList.Add(item);
            }
            return Ok(orderedList.ToArray());
        }

        /// <summary>
        /// Кол-во непрочитанных уведомлений
        /// </summary>
        /// <param name="orgId">Район</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetUnreadCount(Guid orgId)
        {
            var context = Models.ScriptExecutor.CreateContext(Models.ScriptExecutor.portal_childrenUserName, Models.ScriptExecutor.portal_childrenUserId);
            var allReqs = db.MeetupRequests.Where(x => x.Unread).ToArray();
            var reqs = new List<MeetupRequest>();
            foreach (var item in allReqs)
            {
                var childCase = context.Documents.LoadById(item.ChildId);
                if (childCase.OrganizationId == orgId)
                {
                    reqs.Add(item);
                }
            }
            return Ok(reqs.Count);
        }

        /// <summary>
        /// Прочитать уведомление
        /// </summary>
        /// <param name="requestId">Айди запроса</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult SetRead(int requestId)
        {
            var reqObj = db.MeetupRequests.Find(requestId);
            if(reqObj != null && reqObj.Unread)
            {
                reqObj.Unread = false;
                db.Entry(reqObj).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Ok();
        }

        /// <summary>
        /// Просмотр результата посещения по айди запроса на встречу
        /// </summary>
        /// <param name="meetupRequestItemId">Айди запроса на встречу</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(Models.ScriptExecutor.VisitResult))]
        public IHttpActionResult GetVisitResult(int meetupRequestItemId)
        {
            return Ok(Models.ScriptExecutor.GetVisitResultDetails(meetupRequestItemId));
        }

        /// <summary>
        /// Установить результат посещения ребенка
        /// </summary>
        /// <param name="result">Параметры результата</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SetVisitResult([FromBody]Models.ScriptExecutor.VisitResult result)
        {
            return Ok(Models.ScriptExecutor.SetVisitResult(result));
        }

        /// <summary>
        /// Просмотр результатов посещения района
        /// </summary>
        /// <param name="orgId">Район</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(Models.ScriptExecutor.VisitResult))]
        public IHttpActionResult GetVisitResultsByOrgId(Guid orgId)
        {
            return Ok(Models.ScriptExecutor.GetVisitResultDetails(orgId));
        }

        
    }
}