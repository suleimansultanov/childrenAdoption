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
    /// Электронные приказы о передаче на временное опекунство
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AdoptionOrdersController : ApiController
    {
        private EFDbContext db;
        public AdoptionOrdersController()
        {
            db = new EFDbContext();
        }


        /// <summary>
        /// Список приказов кандидата
        /// </summary>
        /// <param name="userId">Кандидат</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(AdoptionOrder[]))]
        public IHttpActionResult GetByUserId(Guid userId)
        {
            var orders = db.AdoptionOrders.Include(x => x.MeetupRequestItem.AdpotionCandidate).Where(x => x.MeetupRequestItem.AdpotionCandidate.PortalUserId == userId).ToArray();
            return Ok(orders);
        }

        /// <summary>
        /// Список приказов по району
        /// </summary>
        /// <param name="orgId">Район</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(AdoptionOrder[]))]
        public IHttpActionResult GetByOrgId(Guid orgId)
        {
            var context = Models.ScriptExecutor.CreateContext(Models.ScriptExecutor.portal_childrenUserName, Models.ScriptExecutor.portal_childrenUserId);
            var childIdList = db.AdoptionOrders.Include(x => x.MeetupRequestItem.MeetupRequest).Select(x => x.MeetupRequestItem.MeetupRequest.ChildId).ToList().Distinct();
            var myChilds = new List<Guid>();
            foreach (var childId in childIdList)
            {
                var childCase = context.Documents.LoadById(childId);
                if (childCase.OrganizationId == orgId) myChilds.Add(childCase.Id);
            }
            var orders = db.AdoptionOrders.Include(x => x.MeetupRequestItem.AdpotionCandidate).Where(x => x.MeetupRequestItem.AdpotionCandidate.OrgId == orgId).ToList();

            if (myChilds.Count > 0)
            {
                orders.AddRange(db.AdoptionOrders.Include(x => x.MeetupRequestItem.MeetupRequest).Where(x => myChilds.Contains(x.MeetupRequestItem.MeetupRequest.ChildId)).ToList());
            }
            
            return Ok(orders);
        }

        /// <summary>
        /// Отклонить приказ о временной опеке
        /// </summary>
        /// <param name="orderId">Айди приказа</param>
        /// <param name="reason">Причина отклонения</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Reject(int orderId, string reason)
        {
            var orderObj = db.AdoptionOrders.Find(orderId);
            if(orderObj != null)
            {
                if(orderObj.Date > DateTime.Now)
                {
                    orderObj.IsRejected = true;
                    orderObj.RejectReason = reason;
                    db.Entry(orderObj).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    return BadRequest("Срок отклонения истек!");
                }
            }
            return NotFound();
        }
    }
}