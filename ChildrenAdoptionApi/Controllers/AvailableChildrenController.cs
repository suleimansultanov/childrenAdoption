using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using static ChildrenAdoptionApi.Models.ScriptExecutor;

namespace ChildrenAdoptionApi.Controllers
{
    /// <summary>
    /// Сервис просмотра доступных детей
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AvailableChildrenController : ApiController
    {
        private EFDbContext db;
        public AvailableChildrenController()
        {
            db = new EFDbContext();
        }
        /// <summary>
        /// Получить список активных детей
        /// </summary>
        /// <param name="userId">Айди пользователя портала (из keycloack)</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(AvailableChildrenItem[]))]
        public IHttpActionResult GetActiveList(Guid userId)
        {
            var portalUser = db.AdpotionCandidates.Find(userId);
            if (portalUser == null) return Ok();
            var refusedDecisionId = new Guid("{0D7921D2-E8AC-40D0-AAD6-93FF5BD7BE66}");
            var acceptedDecisionId = new Guid("{46CB0B76-2A82-4E20-9F55-CDFAE5393BDF}");
            var myReqs = db.MeetupRequestItems.Include(x => x.MeetupRequest).Where(x => x.PortalUserId == userId && x.VisitResultId == null).Select(x => x.MeetupRequest);
            if (myReqs.Count() > 0)
            {
                var acceptedDecisionList = new List<bool>();
                foreach (var myReq in myReqs)
                {
                    bool _isFoundSuccessVisitResult = false;
                    foreach (var mReqItem in myReq.MeetupRequestItems.Where(x => x.PortalUserId != userId && x.VisitResultId != null))
                    {
                        var visitResultOb = GetVisitResultDetails(mReqItem.Id);
                        if (visitResultOb.CandidateSolutionId == acceptedDecisionId)
                        {
                            _isFoundSuccessVisitResult = true;
                            break;
                        }
                    }
                    acceptedDecisionList.Add(_isFoundSuccessVisitResult);
                }
                if(acceptedDecisionList.Any(x => !x)) return Ok(new AvailableChildrenItem[] { });
            }
            var generalList = GetAvailableChildrenItems().ToList();

            var today = DateTime.Now;

            //условия для исключения из списка детей на просмотр
            //1 - прошло 7 дней с момента первой заявки
            var deadlinedRequests = db.MeetupRequests.ToList().Where(x => x.CreatedAt.AddDays(7) < today).ToList();
            var excludeList = deadlinedRequests.Select(x => x.ChildId).ToList();

            //условия для включения в список детей на просмотр
            //2 - прошло 7 дней но у последней заявки отказной результат
            var includeList = deadlinedRequests.Where(x => GetVisitResultDetails(x.MeetupRequestItems.LastOrDefault(x1 => x1.CertificateDate == x.MeetupRequestItems.ToArray().Max(x2 => x2.CertificateDate))?.Id ?? 0)?.CandidateSolutionId == refusedDecisionId).Select(x => x.ChildId).ToList();

            var completeExcludeList = new List<Guid>(excludeList.Where(x => !includeList.Contains(x)));

            

            return Ok(generalList.Where(x => !completeExcludeList.Contains(x.DocumentId)).ToArray());
        }


        [HttpGet]
        [ResponseType(typeof(Models.ScriptExecutor.ChildDetails))]
        public IHttpActionResult GetChildDetails(Guid childId)
        {
            return Ok(Models.ScriptExecutor.GetChildDetails(childId));
        }

        /// <summary>
        /// Фото ребенка
        /// </summary>
        /// <param name="childId">Ребенок</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPhoto(Guid childId)
        {
            var imageAttrId = new Guid("{C52CDB04-E061-4E37-A930-49100F2116CE}");
            var context = Models.ScriptExecutor.CreateContext(portal_childrenUserName, portal_childrenUserId);
            var imageData = context.Documents.GetBlobAttrData(childId, imageAttrId);

            using (MemoryStream ms = new MemoryStream())
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                if(imageData != null && imageData.Data != null)
                {
                    result.Content = new ByteArrayContent(imageData.Data);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                }

                return result;
            }
        }



        private byte[] GetResizedImage(byte[] imageData, int height, int width)
        {
            if (width <= 0 && height <= 0) return imageData;

            using (var ms = new MemoryStream(imageData))
            {
                using (var ms2 = new MemoryStream())
                {
                    ResizeImage(height, width, ms, ms2);
                    ms2.Position = 0;
                    return ms2.ToArray();
                }
            }

        }
        private static void ResizeImage(int height, int width, Stream fromStream, Stream toStream)
        {
            using (var image = System.Drawing.Image.FromStream(fromStream))
            {
                var fH = (double)height;
                var fW = (double)width;
                fH = ((fH > 0 && image.Height > fH) ? fH / image.Height : 1);
                fW = ((fW > 0 && image.Width > fW) ? fW / image.Width : 1);

                ResizeImage(Math.Min(fH, fW), image, toStream);
            }
        }
        private static void ResizeImage(double scaleFactor, Image image, Stream toStream)
        {
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);
            using (var thumbnailBitmap = new Bitmap(newWidth, newHeight))
            {
                using (var thumbnailGraph = Graphics.FromImage(thumbnailBitmap))
                {
                    thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                    thumbnailGraph.DrawImage(image, imageRectangle);

                    thumbnailBitmap.Save(toStream, image.RawFormat);
                }
            }
        }
    }
}