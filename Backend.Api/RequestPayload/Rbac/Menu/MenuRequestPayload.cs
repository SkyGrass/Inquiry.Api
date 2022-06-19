






using System;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.RequestPayload.Rbac.Menu
{
    /// <summary>
    /// 
    /// </summary>
    public class MenuRequestPayload : RequestPayload
    {
        /// <summary>
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 上级菜单GUID
        /// </summary>
        public Guid? ParentGuid { get; set; }
    }
}
