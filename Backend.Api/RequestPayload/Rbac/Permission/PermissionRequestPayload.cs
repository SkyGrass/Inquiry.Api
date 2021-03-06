






using System;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.RequestPayload.Rbac.Permission
{
    /// <summary>
    /// 
    /// </summary>
    public class PermissionRequestPayload : RequestPayload
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
        /// 关联菜单GUID
        /// </summary>
        public Guid? MenuGuid { get; set; }
    }
}
