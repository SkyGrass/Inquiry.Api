
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 询价单表头实体类
    /// </summary>
    public class vInquiryList
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        public string BillNo { get; set; }
        public string DisplayBillNo { get; set; }
        public DateTime Date { get; set; }
        public int AuditerId { get; set; }
        public string AuditerName { get; set; }
        public DateTime AuditDate { get; set; }
        public string BillerId { get; set; }
        public string BillerName { get; set; }
        public string AuditState { get; set; }
        public AuditStatus Status { get; set; }
        public IsDeleted IsDeleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
