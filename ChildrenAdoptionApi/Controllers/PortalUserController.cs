using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace ChildrenAdoptionApi.Controllers
{
    /// <summary>
    /// Сведения о пользователе портала
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PortalUserController : ApiController
    {
        /// <summary>
        /// Добавить пользователя
        /// </summary>
        /// <param name="userId">Айд пользователя из системы Keycloak</param>
        /// /// <param name="orgId">Айд района</param>
        /// /// <param name="userName">Имя пользователя (ПИН)</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(bool))]
        public IHttpActionResult SetUser(Guid userId, Guid orgId, string userName)
        {
            var db = new EFDbContext();
            if (!db.AdpotionCandidates.Any(x => x.PortalUserId == userId))
            {
                db.AdpotionCandidates.Add(new AdpotionCandidate
                {
                    PortalUserId = userId,
                    CreatedAt = DateTime.Now,
                    OrgId = orgId,
                    UserName = userName
                });
                db.SaveChanges();
                return Ok(true);
            }
            else
            {
                return Ok(true);
            }
        }

        /// <summary>
        /// Проверка перед созданием кандидата о наличии всего пакета документов
        /// </summary>
        /// <param name="pin">пин кандидата</param>
        /// <returns>Имеет право на доступ к порталу или нет</returns>
        [HttpGet]
        [ResponseType(typeof(bool))]
        public IHttpActionResult CanBeCreated(string pin)
        {
            return Ok(Models.ScriptExecutor.HasEnoughAttachments(pin));
        }

        /// <summary>
        /// Просмотреть весь список пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(AdpotionCandidate[]))]
        public IHttpActionResult GetAllUser()
        {
            return Ok(Models.ScriptExecutor.GetAdpotionCandidates());
        }

        /// <summary>
        /// Получить информацию о кандидате по ПИН
        /// </summary>
        /// <param name="userId">Айд кандидата</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(Models.ScriptExecutor.CandidateDetails))]
        public IHttpActionResult GetCandidateById(Guid userId)
        {
            var db = new EFDbContext();
            var candidate = db.AdpotionCandidates.Find(userId);
            if(candidate != null)
            {
                return Ok(Models.ScriptExecutor.GetCandidateDetails(candidate.UserName));
            }
            return Ok(new { });
        }
    }
}