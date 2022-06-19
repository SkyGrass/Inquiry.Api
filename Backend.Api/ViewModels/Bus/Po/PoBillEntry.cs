
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.ViewModels.Bus.Po
{
    /// <summary>
    /// 采购单表头实体类
    /// </summary>
    public class PoBillEntry
    {
        public string invId { get; set; }
        public string deptId { get; set; }
        public string deptName { get; set; }

        public decimal quantity { get; set; }

        public decimal price { get; set; }

        public int partnerId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string partnerName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string remark { get; set; }
    }
}
