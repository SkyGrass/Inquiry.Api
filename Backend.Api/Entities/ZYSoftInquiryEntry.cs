
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 询价单表头实体类
    /// </summary>
    public class ZYSoftInquiryEntry
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
        /// 
        /// </summary>
        public int PartnerId { get; set; }

        /// <summary>
        /// 存货ID
        /// </summary> 
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string InvId { get; set; }
          
        /// <summary>
        /// 上期报价
        /// </summary> 
        public decimal PriceLast { get; set; }

        /// <summary>
        /// 上期定价
        /// </summary> 
        public decimal PriceLastConfirm { get; set; }

        /// <summary>
        /// 本期报价
        /// </summary> 
        public decimal PriceCurrent { get; set; }

        /// <summary>
        /// 本期定价
        /// </summary> 
        public decimal PriceCurrentConfirm { get; set; }

        /// <summary>
        /// 市场价
        /// </summary> 
        public decimal PriceMarket { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndDate { get; set; }

        public bool IsConfirm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Remark { get; set; } 

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
