
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.ViewModels.Bus.Sign
{
    /// <summary>
    /// 采购单表头实体类
    /// </summary>
    public class SignBillEntry
    {
        public string invId { get; set; }
        public int deptId { get; set; }
        public string orderId { get; set; }
        public string orderEntryId { get; set; }
        public bool isDiff { get; set; }
        public decimal quantity { get; set; }
        public decimal factQuantity { get; set; }

        public decimal price { get; set; }

        public int partnerId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string partnerName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string remark { get; set; }
    }
}
