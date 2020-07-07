using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ChildrenAdoptionApi
{
    public class EFDbContext : DbContext
    {
        public EFDbContext()
            : base("DefaultConnection")
        {

        }
        
        public DbSet<AdpotionCandidate> AdpotionCandidates { get; set; }
        public DbSet<MeetupRequest> MeetupRequests { get; set; }
        public DbSet<MeetupRequestItem> MeetupRequestItems { get; set; }
        public DbSet<AdoptionOrder> AdoptionOrders { get; set; }
    }

    /// <summary>
    /// Кандидат-пользователь
    /// </summary>
    [Table("AdpotionCandidates")]
    public class AdpotionCandidate
    {
        /// <summary>
        /// Айди пользователя
        /// </summary>
        [Key]
        public Guid PortalUserId { get; set; }
        /// <summary>
        /// Айди УСР
        /// </summary>
        public Guid OrgId { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Логин кандидата
        /// </summary>
        public string UserName { get; set; }
    }


    /// <summary>
    /// Запрос на встречу с ребенком
    /// </summary>
    [Table("MeetupRequests")]
    public class MeetupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Дата запроса
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Айди ребенка
        /// </summary>
        public Guid ChildId { get; set; }
        /// <summary>
        /// Прочитано?
        /// </summary>
        public bool Unread { get; set; } = true;

        public DateTime Deadline { get; set; }
        /// <summary>
        /// Заявки кандидатов на встречу
        /// </summary>
        public virtual ICollection<MeetupRequestItem> MeetupRequestItems { get; set; }
    }

    public class MeetupRequestDetails
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Дата запроса
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Айди ребенка
        /// </summary>
        public Guid ChildId { get; set; }
        /// <summary>
        /// Прочитано?
        /// </summary>
        public bool Unread { get; set; } = true;

        public DateTime Deadline { get; set; }
        /// <summary>
        /// Заявки кандидатов на встречу
        /// </summary>
        public virtual List<MeetupRequestItemDetails> MeetupRequestItems { get; set; }
    }

    /// <summary>
    /// Заявка кандидата к запросу на встречу
    /// </summary>
    [Table("MeetupRequestItems")]
    public class MeetupRequestItem
    {
        public int Id { get; set; }
        /// <summary>
        /// Айди пользователя
        /// </summary>
        [ForeignKey("AdpotionCandidate")]
        public Guid PortalUserId { get; set; }
        [JsonIgnore]
        public virtual AdpotionCandidate AdpotionCandidate { get; set; }

        [ForeignKey("MeetupRequest")]
        public int MeetupRequestId { get; set; }
        public virtual MeetupRequest MeetupRequest { get; set; }
        /// <summary>
        /// Номер позиции на встречу
        /// </summary>
        public int? OrderNo { get; set; }
        /// <summary>
        /// Дата заявки
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Кандидат ФИО
        /// </summary>
        public string CandidateFullName { get; set; }
        /// <summary>
        /// Дата выдачи заключения
        /// </summary>
        public DateTime CertificateDate { get; set; }
        /// <summary>
        /// Айди направления
        /// </summary>
        public Guid? DirectionId { get; set; }
        /// <summary>
        /// Айди результата посещения
        /// </summary>
        public Guid? VisitResultId { get; set; }
    }
    public class MeetupRequestItemDetails
    {
        public int Id { get; set; }
        /// <summary>
        /// Айди пользователя
        /// </summary>
        [ForeignKey("AdpotionCandidate")]
        public Guid PortalUserId { get; set; }
        [JsonIgnore]
        public virtual AdpotionCandidate AdpotionCandidate { get; set; }

        [ForeignKey("MeetupRequest")]
        public int MeetupRequestId { get; set; }
        public virtual MeetupRequest MeetupRequest { get; set; }
        /// <summary>
        /// Номер позиции на встречу
        /// </summary>
        public int? OrderNo { get; set; }
        /// <summary>
        /// Дата заявки
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Кандидат ФИО
        /// </summary>
        public string CandidateFullName { get; set; }
        /// <summary>
        /// Кандидат Район
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// Дата выдачи заключения
        /// </summary>
        public DateTime CertificateDate { get; set; }
        /// <summary>
        /// Айди результата посещения
        /// </summary>
        public Models.ScriptExecutor.VisitResult VisitResult { get; set; }
        /// <summary>
        /// Айди направления на посещения
        /// </summary>
        public Guid? DirectionId { get; set; }
    }


    /// <summary>
    /// Заявка кандидата к запросу на встречу
    /// </summary>
    [Table("AdoptionOrders")]
    public class AdoptionOrder
    {
        /// <summary>
        /// Айди приказа
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Регистрационный номер приказа
        /// </summary>
        public string No { get; set; }
        /// <summary>
        /// Дата вступления приказа (+ 1 день с момента согласи на усыновление)
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Айди заявки из запроса на ребенка
        /// </summary>
        [ForeignKey("MeetupRequestItem")]
        public int? MeetupRequestItemId { get; set; }
        /// <summary>
        /// Данные заявки из запроса на ребенка
        /// </summary>
        public virtual MeetupRequestItem MeetupRequestItem { get; set; }
        /// <summary>
        /// Отклонен?
        /// </summary>
        public bool IsRejected { get; set; }
        /// <summary>
        /// Причина отклонения
        /// </summary>
        public string RejectReason { get; set; }
    }
}