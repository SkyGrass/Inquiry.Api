
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 采购单表头实体类
    /// </summary>
    public class ZYSoftPoEntry
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 头表主ID
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 存货ID
        /// </summary>  
        [Column(TypeName = "nvarchar(20)")]
        public string InvId { get; set; }
        public string DeptId { get; set; }
        public string UnitName { get; set; }
        /// <summary>
        /// 总数量
        /// </summary> 
        public decimal Quantity { get; set; }

        /// <summary>
        /// 已下发数量
        /// </summary> 
        public decimal Price { get; set; }
        /// <summary>
        /// 头表主ID
        /// </summary>
        public int PartnerId { get; set; }

        /// <summary>
        /// 存货ID
        /// </summary>  
        [Column(TypeName = "nvarchar(20)")]
        public string PartnerName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Remark { get; set; }
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
