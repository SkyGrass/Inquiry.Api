using System;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.ViewModels.Bus.Subscribe
{
    /// <summary>
    /// 
    /// </summary>
    public class ZYSoftSubscribeJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 单据号
        /// </summary> 
        public string BillNo { get; set; }

        /// <summary>
        /// 制单日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary> 
        public string DeptId { get; set; }

        /// <summary>
        /// 制单人
        /// </summary> 
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
        /// 备注
        /// </summary> 
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedOn { get; set; }
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
        public string ModifiedOn { get; set; }
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
