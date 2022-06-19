
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 申购单表头实体类
    /// </summary>
    public class vSubscribeList
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; } 
        public int BillType { get; set; }  
        public string BillNo { get; set; }
        public string DisplayBillNo { get; set; }
        public DateTime Date { get; set; } 
        public string DeptId { get; set; } 
        public string BillerId { get; set; }
        public string BillerName { get; set; }
        public string DeptName { get; set; }
        public string AuditState { get; set; }
        public AuditStatus Status { get; set; } 
        public IsDeleted IsDeleted { get; set; }
        public bool NeedAudit { get; set; }
    }
}
