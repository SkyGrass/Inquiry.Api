
namespace Backend.Api.Entities.Enums
{
    /// <summary>
    /// 通用枚举类
    /// </summary>
    public class CommonEnum
    {
        /// <summary>
        /// 是否已删
        /// </summary>
        public enum IsDeleted
        {
            /// <summary>
            /// 所有
            /// </summary>
            All = -1,
            /// <summary>
            /// 否
            /// </summary>
            No = 0,
            /// <summary>
            /// 是
            /// </summary>
            Yes = 1
        }

        /// <summary>
        /// 是否已被锁定
        /// </summary>
        public enum IsLocked
        {
            /// <summary>
            /// 未锁定
            /// </summary>
            UnLocked = 0,
            /// <summary>
            /// 已锁定
            /// </summary>
            Locked = 1
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public enum IsEnabled
        {
            /// <summary>
            /// 否
            /// </summary>
            No = 0,
            /// <summary>
            /// 是
            /// </summary>
            Yes = 1
        }


        /// <summary>
        /// 用户状态
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// 未指定
            /// </summary>
            All = -1,
            /// <summary>
            /// 已禁用
            /// </summary>
            Forbidden = 0,
            /// <summary>
            /// 正常
            /// </summary>
            Normal = 1
        }
        /// <summary>
        /// 审批状态
        /// </summary>
        public enum AuditStatus
        {
            None = -99,
            /// <summary>
            /// 未开始
            /// </summary>
            NoStart = -1,
            /// <summary>
            /// 审批中
            /// </summary>
            Ing = 0,
            /// <summary>
            /// 同意
            /// </summary>
            Agree = 1,
            /// <summary>
            /// 拒绝
            /// </summary>
            Refuse = 2,
            /// <summary>
            /// 作废
            /// </summary>
            Void = 3
        }

        /// <summary>
        /// 权限类型
        /// </summary>
        public enum PermissionType
        {
            /// <summary>
            /// 菜单
            /// </summary>
            Menu = 0,
            /// <summary>
            /// 按钮/操作/功能
            /// </summary>
            Action = 1
        }

        /// <summary>
        /// 是否枚举
        /// </summary>
        public enum YesOrNo
        {
            /// <summary>
            /// 所有
            /// </summary>
            All = -1,
            /// <summary>
            /// 否
            /// </summary>
            No = 0,
            /// <summary>
            /// 是
            /// </summary>
            Yes = 1
        }

        public enum BillType
        {
            SG = 1, //申购单
            XJ = 2,//询价单
            PO = 3,//采购单
            Sign = 4//签收单
        }

        public enum BaseType
        {
            vInventory = 0,
            vInventoryCls = 1,
            vPartner = 2,
            vPerson = 3,
            vDepartment = 4,
        }

        public enum MsgType
        {
            SendToPartner = 1,
            SendToAuditer = 2,
            SendToBiller = 3,
            SendToPo = 4,
            SendToPoWithUnAudit = 5,
            SendToPartnerConfirmPrice = 6,
            SignNotEnough = 7
        }
    }

}
