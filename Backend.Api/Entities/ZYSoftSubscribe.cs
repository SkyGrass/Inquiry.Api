
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
    public class ZYSoftSubscribe
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        public int BillType { get; set; }
        public bool NeedAudit { get; set; }
        public string BillNo { get; set; }
        public DateTime Date { get; set; }

        public string DeptId { get; set; }

        public string BillerId { get; set; }

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
