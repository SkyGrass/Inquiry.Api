
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 采购单表头实体类
    /// </summary>
    public class ZYSoftSign
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        public int BillType { get; set; }
        /// <summary>
        /// 单据号
        /// </summary> 
        [Column(TypeName = "nvarchar(50)")]
        public string BillNo { get; set; }

        /// <summary>
        /// 制单日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 制单日期
        /// </summary>
        public DateTime RequiredDate { get; set; }

        public int PartnerId { get; set; }

        public int PoBillId { get; set; }
        public int BillerId { get; set; }

        public string AuditerId { get; set; }

        public DateTime AuditDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AuditStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IsDeleted IsDeleted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid CreatedByUserGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? ModifiedByUserGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ModifiedByUserName { get; set; }
    }
}
