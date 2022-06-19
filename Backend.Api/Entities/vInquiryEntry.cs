
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
    public class vInquiryEntry
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
        public string BillerId { get; set; }
        public string BillerName { get; set; }
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
        public int clsId { get; set; }
        public string clsName { get; set; }
        public string Remark { get; set; }
        public string PartnerName { get; set; }
        public int PartnerId { get; set; }
        public decimal PriceLast { get; set; }
        public decimal PriceLastConfirm { get; set; }
        public decimal PriceCurrent { get; set; }
        public decimal PriceCurrentConfirm { get; set; }
        public decimal PriceMarket { get; set; }
        public bool IsConfirm { get; set; }
        public Guid CreatedByUserGuid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
