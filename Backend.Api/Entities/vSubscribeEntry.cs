
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
    public class vSubscribeEntry
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        public int BillType { get; set; }
        public string BillNo { get; set; }
        public DateTime Date { get; set; }
        public string DeptId { get; set; }
        public string BillerId { get; set; }
        public string BillerName { get; set; }
        public string DeptName { get; set; }
        public string AuditState { get; set; }
        public AuditStatus Status { get; set; }
        public IsDeleted IsDeleted { get; set; }
        public int EntryId { get; set; }
        public string InvId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string specification { get; set; }
        public int idunit { get; set; }
        public string unitname { get; set; }
        public string _unitname { get; set; }
        public int clsId { get; set; }
        public string clsName { get; set; }
        public bool needAudit { get; set; }
        public decimal Quantity { get; set; }
        public string Remark { get; set; }
        public Guid CreatedByUserGuid { get; set; }
    }
}
