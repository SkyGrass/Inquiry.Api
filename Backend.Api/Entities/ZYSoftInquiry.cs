
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
    public class ZYSoftInquiry
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
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
        /// 制单人
        /// </summary>
        [Column(TypeName = "nvarchar(10)")]
        public string BillerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Remark { get; set; }
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
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int ClsId { get; set; }
    }
}
