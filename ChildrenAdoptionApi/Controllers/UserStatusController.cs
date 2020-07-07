using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using static ChildrenAdoptionApi.Models.ScriptExecutor;

namespace ChildrenAdoptionApi.Controllers
{
    /// <summary>
    /// Просмотр статуса пользователя
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserStatusController : ApiController
    {
        private EFDbContext db;
        public UserStatusController()
        {
            db = new EFDbContext();
        }
        /// <summary>
        /// Получить статус
        /// </summary>
        /// <param name="userId">Кандидат</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(UserStatus))]
        public IHttpActionResult Get(Guid userId)
        {
            var refusedDecisionId = new Guid("{0D7921D2-E8AC-40D0-AAD6-93FF5BD7BE66}");
            var acceptedDecisionId = new Guid("{46CB0B76-2A82-4E20-9F55-CDFAE5393BDF}");
            var status = new UserStatus
            {
                StatusDate = DateTime.MinValue,
                StatusName = "-"
            };
            
            var myReqs = db.MeetupRequestItems.Include(x => x.MeetupRequest).Where(x => x.PortalUserId == userId && x.VisitResultId == null).Select(x => x.MeetupRequest);
            bool isFoundSuccessVisitResult = false;
            if (myReqs.Count() > 0)
            {
                foreach (var myReq in myReqs)
                {
                    foreach (var mReqItem in myReq.MeetupRequestItems.Where(x => x.PortalUserId != userId && x.VisitResultId != null))
                    {
                        if (GetVisitResultDetails(mReqItem.Id).CandidateSolutionId == acceptedDecisionId)
                        {
                            isFoundSuccessVisitResult = true;
                            break;
                        }
                    }
                    if (isFoundSuccessVisitResult) break;
                }
                if (isFoundSuccessVisitResult)
                {
                    return Ok(status);
                }
            }
            var req = db.MeetupRequestItems.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x => x.PortalUserId == userId);
            if (req != null)
            {
                status.StatusDate = req.CreatedAt;
                var childObj = GetAvailableChildrenItem(req.MeetupRequest.ChildId);
                status.StatusName = string.Format("Ребенок {0} лет, пол: {1}, национальность: {2};", childObj.Age, childObj.Gender, childObj.Nationality);
            }
            else
            {
                
            }
            
            return Ok(status);
        }
    }
}