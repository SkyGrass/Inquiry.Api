using AutoMapper;
using Backend.Api.Entities;
using Backend.Api.Extensions;
using Backend.Api.Extensions.AuthContext;
using Backend.Api.Extensions.CustomException;
using Backend.Api.ViewModels.Bus.Inquiry;
using Backend.Api.ViewModels.Bus.Po;
using Backend.Api.ViewModels.Bus.Sign;
using Backend.Api.ViewModels.Bus.Subscribe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Controllers.Api.V1.Bus
{
    [Route("api/v1/bus/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class BusController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _hostingEnvironment;
        public BusController(DncZeusDbContext dbContext, IHostEnvironment hostingEnvironment, IMapper mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        #region 审批
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/audit/count")]
        public IActionResult GetAuditCount()
        {
            var response = ResponseModelFactory.CreateResultInstance;
            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT count(1) FROM (
                SELECT t2.BillType,t2.BillId,t2.[No],ISNULL(t2.Flag,-1)Flag,t2.UserGuid FROM dbo.ZYSoftSubscribe t1 LEFT JOIN dbo.ZYSoftAuditRecord t2 ON t2.BillType = t1.BillType
                AND t2.BillId = t1.Id WHERE t1.IsDeleted = 0 AND ISNULL(t1.Status,-1) <1 AND t2.BillId IS not NULL AND CAST(t1.[Date] AS Date)= CAST(GETDATE() AS Date)
                )a LEFT JOIN (SELECT t2.BillType,t2.BillId,t2.[No] AS [NO],ISNULL(t2.Flag,-1)Flag,t2.UserGuid FROM dbo.ZYSoftSubscribe t1 LEFT JOIN dbo.ZYSoftAuditRecord t2 ON t2.BillType = t1.BillType
                AND t2.BillId = t1.Id WHERE t1.IsDeleted = 0 AND ISNULL(t1.Status,-1) <1 AND t2.BillId IS not NULL AND CAST(t1.[Date] AS Date)= CAST(GETDATE() AS Date))b 
                    ON a.BillType = b.BillType  and a.BillId = b.BillId AND
                  a.[No]+1 = b.[No] WHERE  (a.UserGuid = '{0}' AND a.Flag <1 AND b.UserGuid IS NOT NULL )  
				  OR (ISNULL(b.Flag,-1) <1 AND  a.Flag = 1 AND b.UserGuid = '{0}')", AuthContextService.CurrentUser.Guid));

            response.SetData(new { count });
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/audit/list")]
        public IActionResult GetAuditList(int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch (type)
            {
                case 0:
                    str = string.Format(@"SELECT DISTINCT * FROM dbo.vSubscribeList t1 RIGHT JOIN (SELECT a.BillId,a.BillType FROM (
                SELECT t2.BillType,t2.BillId,t2.[No],ISNULL(t2.Flag,-1)Flag,t2.UserGuid FROM dbo.ZYSoftSubscribe t1 LEFT JOIN dbo.ZYSoftAuditRecord t2 ON t2.BillType = t1.BillType
                AND t2.BillId = t1.Id WHERE t1.IsDeleted = 0 AND ISNULL(t1.Status,-1) <1 AND t2.BillId IS not NULL AND CAST(t1.[Date] AS Date)= CAST(GETDATE() AS Date)
                )a LEFT JOIN (SELECT t2.BillType,t2.BillId,t2.[No] AS [NO],ISNULL(t2.Flag,-1)Flag,t2.UserGuid FROM dbo.ZYSoftSubscribe t1 LEFT JOIN dbo.ZYSoftAuditRecord t2 ON t2.BillType = t1.BillType
                AND t2.BillId = t1.Id WHERE t1.IsDeleted = 0 AND ISNULL(t1.Status,-1) <1 AND t2.BillId IS not NULL AND CAST(t1.[Date] AS Date)= CAST(GETDATE() AS Date))b 
                    ON a.BillType = b.BillType  and a.BillId = b.BillId AND
                  a.[No]+1 = b.[No] WHERE  (a.UserGuid = '{0}' AND a.Flag <1 AND b.UserGuid IS NOT NULL )  
				  OR (ISNULL(b.Flag,-1) <1 AND  a.Flag = 1 AND b.UserGuid = '{0}')) 
                t2 ON t2.BillType = t1.BillType AND t1.Id =t2.BillId ", AuthContextService.CurrentUser.Guid);
                    break;
                case 1:
                    str = string.Format(@"SELECT DISTINCT * FROM dbo.vSubscribeList t1 RIGHT JOIN (SELECT a.BillId,a.BillType FROM (
                SELECT t2.BillType,t2.BillId,t2.[No],ISNULL(t2.Flag,-1)Flag,t2.UserGuid FROM dbo.ZYSoftSubscribe t1 LEFT JOIN dbo.ZYSoftAuditRecord t2 ON t2.BillType = t1.BillType
                AND t2.BillId = t1.Id WHERE t1.IsDeleted = 0 AND t2.BillId IS not NULL AND t2.UserGuid ='{0}' AND ISNULL(t2.Flag,-1) >-1 AND CAST(t1.[Date] AS Date)= CAST(GETDATE() AS Date)
                )a) t2 ON t2.BillType = t1.BillType AND t1.Id =t2.BillId", AuthContextService.CurrentUser.Guid);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);

            var list = DataTableConvert.ToList<vSubscribeList>(query);
            response.SetData(list);
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/audit/do")]
        public IActionResult Audit(dynamic model)
        {
            string billType = model.billType;
            string billId = model.billId;
            string flag = model.flag;
            string remark = model.remark;
            string level = model.no;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@" SELECT * FROM dbo.ZYSoftSubscribe 
                    WHERE BillType =1 AND Id =1 AND 
                    DATEDIFF(SECOND,GETDATE(),DATEADD(DAY,1,CAST([Date] AS DATE))) >0", billType, billId)))
            {
                response.SetError("当前单据已经超期,无法审批!");
            }
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSubscribe 
                    WHERE BillType ={0} AND Id ={1} AND AND ISNULL(Status,-1) >0", billType,
                   billId)))
            {
                response.SetError("当前单据已完成审批,无需多次操作!");
            }
            else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord 
                    WHERE BillType ={0} AND BillId ={1} AND Flag >-1  AND No ='{2}'", billType,
                     billId, level)))
            {
                response.SetError("当前环节已完成审批,无需多次操作!");
            }
            else if (level.Equals("1") && !ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord 
                    WHERE BillType ={0} AND BillId ={1} AND Flag >-1  AND No <1", billType, billId)))
            {
                response.SetError("前道尚未审批,请耐心等待!");
            }
            else
            {
                List<string> list = new List<string>();
                bool isFirst = level.Equals("0");

                list.Add(string.Format(@"UPDATE dbo.ZYSoftAuditRecord SET Flag ={3},AuditDate = GETDATE(),Remark ='{4}' WHERE BillType ={0}
                    AND BillId ={1} AND UserGuid = '{2}' AND No = '{5}'", billType, billId,
                    AuthContextService.CurrentUser.Guid, flag, remark, level));

                if (!flag.Equals("1"))
                {
                    list.Add(string.Format(@"UPDATE  ZYSoftSubscribe SET Status = {0}
                        WHERE Id = {1}", AuditStatus.Refuse.GetHashCode(), billId));
                }
                else
                {
                    if (isFirst)
                    {
                        if (flag.Equals("1"))
                        {
                            list.Add(string.Format(@"UPDATE  ZYSoftSubscribe SET Status = {0}
                        WHERE Id = {1}", AuditStatus.Ing.GetHashCode(), billId));
                        }
                    }
                    else
                    {
                        if (flag.Equals("1"))
                        {
                            list.Add(string.Format(@"UPDATE  ZYSoftSubscribe SET Status = {0}
                        WHERE Id = {1}", AuditStatus.Agree.GetHashCode(), billId));
                        }
                    }
                }
                int effectRow = isFirst ?
                    ZYSoft.DB.BLL.Common.ExecuteSQLTran(list) : SummaryData(list, billId, BillType.SG.GetHashCode().ToString()); // 终审之后，写汇总
                if (effectRow > 0)
                {
                    if (isFirst)
                    {
                        DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping WHERE UserGuid IN (
                            SELECT  UserGuid FROM dbo.ZYSoftAuditRecord WHERE BillType ={1} AND BillId = {0} AND NO = 1)", billId, BillType.SG.GetHashCode()));
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow dataRow in dt.Rows)
                            {
                                string openId = dataRow["OpenId"].ToString();
                                if (openId != "")
                                {
                                    string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '您有一份申购单需要审批!|'+t2.name +'|'+ t3.name+'|'+ '申购单'+'|'+ '采购申请'+'|'+ CONVERT(VARCHAR(10),Date,23) FROM dbo.ZYSoftSubscribe t1 LEFT JOIN  dbo.vDepartment t2 ON 
                                t1.DeptId = t2.id LEFT JOIN dbo.vPerson t3 ON t1.BillerId = t3.id WHERE t1.Id = {0}", billId));
                                    string errMsg = "";
                                    SendWechatMsg(MsgType.SendToAuditer.GetHashCode(), openId, content, "id=" + billId, ref errMsg);
                                }
                            }
                        }
                    }
                    else
                    {

                    }
                    response.SetSuccess("审批成功!");
                }
                else
                {
                    response.SetSuccess("审批失败!");
                }

            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/audit/undo")]
        public IActionResult UnAudit(dynamic model)
        {
            string billType = model.billType;
            string billId = model.billId;
            string billDate = model.date;
            var response = ResponseModelFactory.CreateResultInstance;

            List<string> list = new List<string>();
            //如果下级审批了，则本级不可反审批
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND BillType = {1} AND [No]>(
                SELECT [No] FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND UserGuid='{2}') AND ISNULL(Flag,-1) >-1", billId, billType,
                        AuthContextService.CurrentUser.Guid)))
            {
                response.SetError("当前单据下级已审批,请依次反审批!");
            }
            else
            {
                //如果本级尚未审批，则不可反审批
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} 
                    AND BillType = {1} AND UserGuid='{2}' AND ISNULL(Flag,-1)=-1", billId, billType,
                       AuthContextService.CurrentUser.Guid)))
                {
                    response.SetError("当前单据尚未审批,无须反审批!");
                }
                else
                {
                    bool isFirst = ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT 1 FROM dbo.ZYSoftAuditRecord WHERE BillType ={0} 
                    AND BillId ={1} AND UserGuid = '{2}' AND NO = 0", billType, billId, AuthContextService.CurrentUser.Guid));

                    bool needAudit = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT ISNULL(needAudit,0)needAudit FROM  ZYSoftSubscribe Where Id = {0}", billId)).Equals("True");

                    //如果允许反审批的数量不足，则不可反审批
                    if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSubSummary  t1 LEFT JOIN (
                    SELECT CONVERT(VARCHAR(10),Date,23)[date],InvId ,SUM(Quantity)AS[count] FROM dbo.vSubscribeEntry WHERE Status = 1 
                    AND Id='{0}' GROUP BY InvId,Date) t2 ON t1.Date =CONVERT(VARCHAR(10),t2.date,23) AND t1.Id = t2.InvId 
                    AND t2.count>t1.FinishCount WHERE [Id] ={0} AND t1.[Date] ='{1}'", billId, billDate)))
                    {
                        response.SetError("当前单据反审批数量不足,无法反审批!");
                    }
                    else
                    {
                        if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND BillType = {1} AND [No]>(
                SELECT [No] FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND UserGuid='{2}')", billId, billType,
                        AuthContextService.CurrentUser.Guid)))
                        {
                            list.Add(string.Format(@"UPDATE ZYSoftSubscribe SET Status = {2} WHERE Id = '{0}' AND BillType = '{1}'", billId, billType,
                                needAudit ? AuditStatus.Ing.GetHashCode() : AuditStatus.NoStart.GetHashCode()));
                        }
                        else
                        {
                            list.Add(string.Format(@"UPDATE ZYSoftSubscribe SET Status = -1 WHERE Id = '{0}' AND BillType = '{1}'", billId, billType));
                        }
                        list.Add(string.Format(@"UPDATE ZYSoftAuditRecord SET Flag = -1,AuditDate = null WHERE BillId = '{0}' AND BillType = '{1}' AND UserGuid ='{2}'",
                            billId, billType, AuthContextService.CurrentUser.Guid));
                        int effectRow;
                        if (needAudit)
                        {
                            effectRow = isFirst ? ZYSoft.DB.BLL.Common.ExecuteSQLTran(list) : SummaryData(list, billId, billType, -1);
                        }
                        else
                        {
                            effectRow = SummaryData(list, billId, billType, -1);
                        }

                        if (effectRow > 0)
                        {
                            response.SetSuccess("操作完成!");
                        }
                        else { response.SetError("操作失败!"); }
                    }
                }
            }
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/audit/info")]
        public IActionResult GetAuditInfo(string billType, string billId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vAuditInfo WHERE BillType ={0} 
                    AND BillId = {1} ORDER BY [No]", billType, billId));

            var list = DataTableConvert.ToList<vAuditInfo>(query);
            response.SetData(list);
            return Ok(response);
        }

        #endregion

        #region 申购单 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/sub/one")]
        public IActionResult GetSubscribe(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vSubscribeEntry Where Id='{0}'", id));
            //var list = DataTableConvert.ToList<vSubscribeEntry>(query);
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/sub/list")]
        public IActionResult GetSubscribeList(int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch ((AuditStatus)type)
            {
                case AuditStatus.Agree:
                    str = string.Format(@"SELECT * FROM dbo.vSubscribeList WHERE CAST([Date] as Date)>= CAST(DATEADD(DAY,-1,GETDATE()) as Date) AND ISNULL(Status,-1) = {1} AND ISNULL(IsDeleted,0) = 0 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                AuthContextService.CurrentUser.Guid, type);
                    break;
                case AuditStatus.NoStart:
                case AuditStatus.Ing:
                case AuditStatus.Refuse:
                    str = string.Format(@"SELECT * FROM dbo.vSubscribeList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(Status,-1) = {1} AND ISNULL(IsDeleted,0) = 0 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                AuthContextService.CurrentUser.Guid, type);
                    break;
                case AuditStatus.Void:
                    str = string.Format(@"SELECT * FROM dbo.vSubscribeList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(IsDeleted,0) = 1 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                        AuthContextService.CurrentUser.Guid, type);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            var list = DataTableConvert.ToList<vSubscribeList>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/sub/del")]
        public IActionResult DelSubscribe(dynamic model)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string id = model.id;

            bool needAduit = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT ISNULL(needAudit,0)needAudit FROM  ZYSoftSubscribe Where Id = {0}", id)).Equals("True");

            if (needAduit)
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord 
                    WHERE BillType ={0} AND BillId ={1} AND Flag >-1", BillType.SG.GetHashCode(), id)))
                {
                    response.SetError("申请已经处于审批流程或者已审批完成,无法作废!");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                        string.Format(@"UPDATE ZYSoftSubscribe set IsDeleted =1 Where BillType ={0} and Id ={1}",
                        BillType.SG.GetHashCode(), id));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetSuccess("操作失败!");
                    }
                }
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(string.Format(@"UPDATE ZYSoftSubscribe SET Status = -1,IsDeleted =1 WHERE Id = '{0}' ", id));
                list.Add(string.Format(@"UPDATE ZYSoftAuditRecord SET Flag = -1,AuditDate = null,Remark = '' WHERE BillId = '{0}' AND BillType = '{1}' AND UserGuid ='{2}'",
                           id, BillType.SG.GetHashCode().ToString(), AuthContextService.CurrentUser.Guid));
                int effectRow = SummaryData(list, id, BillType.SG.GetHashCode().ToString(), -1);
                if (effectRow > 0)
                {
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetSuccess("操作失败!");
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/sub/save")]
        public IActionResult SaveSubscribe(SubscribeCreateModel model)
        {
            ZYSoftSubscribe subscribe = model.subscribe;
            List<ZYSoftSubscribeEntry> subscribeEntry = model.subscribeEntry;
            List<SubscribeBillEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                List<string> sqls = new List<string>();
                string billId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                    string.Format(@"EXEC dbo.L_P_GetMaxID @TableName = '{0}',@Increment = 1", "ZYSoftSubscribe"));
                string billNo = string.Format(@"SG{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
                string _serialNo = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT TOP 1 RIGHT(CONCAT( '00',(CONVERT(INT,SUBSTRING(DisplayBillNo,LEN(DisplayBillNo)-2,3))+1 )) , 3) 
                            FROM dbo.ZYSoftSubscribe WHERE [Date] ='{0}' ORDER BY id  DESC ", DateTime.Now.ToString("yyyy-MM-dd")));
                if (string.IsNullOrEmpty(_serialNo))
                {
                    _serialNo = "001";
                }
                string displayBillNo = string.Format(@"SG{0}{1}", DateTime.Now.ToString("yyyyMMdd"), _serialNo);
                sqls.Add(string.Format(@"
                 INSERT INTO dbo.ZYSoftSubscribe
                         ( Id,
                           BillType,
                           BillNo ,
                           Date ,
                           DeptId ,
                           BillerId , 
                           Status ,
                           IsDeleted ,
                           CreatedOn ,
                           CreatedByUserGuid ,
                           CreatedByUserName ,
                           NeedAudit,DisplayBillNo
                         )
                 VALUES  ( '{0}' , -- Id - varchar(50)
                           '{1}' , -- BillType - varchar(50)
                           '{2}' , -- BillNo - varchar(50) 
                           '{8}' , -- Date - datetime
                           '{3}' , -- DeptId - varchar(20)
                           '{4}' , -- BillerId - varchar(20) 
                           {5}, -- Status - int
                           0 , -- IsDeleted - int
                           GETDATE() , -- CreatedOn - datetime2
                           '{6}' , -- CreatedByUserGuid - uniqueidentifier
                           N'{7}', -- CreatedByUserName - nvarchar(max)
                           '{9}','{10}'
                         )", billId, BillType.SG.GetHashCode(), billNo, subscribe.DeptId,
         subscribe.BillerId, subscribe.NeedAudit ? AuditStatus.NoStart.GetHashCode() : AuditStatus.Agree.GetHashCode(),
         AuthContextService.CurrentUser.Guid,
         AuthContextService.CurrentUser.DisplayName, subscribe.Date, subscribe.NeedAudit, displayBillNo));
                subscribeEntry.ForEach(item =>
                {
                    sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftSubscribeEntry
                            ( BillId ,
                              InvId ,
                              UnitName,
                              Remark ,
                              Quantity ,
                              QuantityFinish ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                               '{2}',
                              '{3}' , -- Remark - varchar(100)
                              {4} , -- Quantity - decimal
                              0 , -- QuantityFinish - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{5}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{6}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.UnitName, item.Remark, item.Quantity,
                             AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                });
                if (subscribe.NeedAudit)
                {
                    sqls.Add(string.Format(@"
                    INSERT INTO dbo.ZYSoftAuditRecord
                            ( BillType ,
                              BillId ,
                              Date ,
                              No,
                              UserGuid ,
                              UserName ,
                              Flag
                            )
                    SELECT BillType,'{0}',GETDATE(),No,UserGuid,UserName,-1 FROM dbo.ZYSoftAuditConfig Where BillType ='{1}' AND UserGuid <> '{2}'",
                    billId, BillType.SG.GetHashCode(), AuthContextService.CurrentUser.Guid));
                }
                else
                {
                    sqls.Add(string.Format(@"
                    INSERT INTO dbo.ZYSoftAuditRecord
                            ( BillType ,
                              BillId ,
                              Date ,
                              No,
                              UserGuid ,
                              UserName ,
                              Flag,
                              AuditDate,
                              Remark
                            )
                    VALUES('{1}','{0}',GETDATE(),0,'{2}','{3}',1,GETDATE(),'自动审批')",
                   billId, BillType.SG.GetHashCode(), AuthContextService.CurrentUser.Guid, AuthContextService.CurrentUser.DisplayName));
                }
                sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftSubscribeLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));

                if (!subscribe.NeedAudit)
                {
                    subscribeEntry.ForEach(item =>
                    {
                        DataTable dtInv = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT name,code,specification FROM dbo.vInventory WHERE id  ={0}", item.InvId));
                        if (dtInv != null && dtInv.Rows.Count > 0)
                        {
                            string name = Convert.ToString(dtInv.Rows[0]["name"]);
                            string code = Convert.ToString(dtInv.Rows[0]["code"]);
                            string specification = Convert.ToString(dtInv.Rows[0]["specification"]);
                            sqls.Add(string.Format(@"IF EXISTS(SELECT 1 FROM ZYSoftSubSummary WHERE [Date]='{0}' AND [Id] ='{1}' AND [DeptId] = '{7}')
                        BEGIN
	                        UPDATE dbo.ZYSoftSubSummary SET [Count] = [Count] + {6} WHERE [Date]='{0}' AND [Id] ='{1}'
                        END
                        ELSE
                        BEGIN
	                        INSERT INTO dbo.ZYSoftSubSummary
	                                ( Date ,
	                                  Id ,
	                                  Name ,
	                                  Code ,
	                                  Unitname ,
	                                  Specification ,
	                                  Count ,
	                                  FinishCount,
                                      DeptId
	                                )
	                        VALUES  ( '{0}' , -- Date - varchar(10)
	                                  {1} , -- Id - int
	                                  '{2}' , -- Name - varchar(50)
	                                  '{3}' , -- Code - varchar(50)
	                                  '{4}' , -- Unitname - varchar(50)
	                                  '{5}' , -- Specification - varchar(50)
	                                  {6} , -- Count - decimal
	                                  0,  -- FinishCount - decimal
                                      '{7}' -- DeptId - int
	                                )
                        END", subscribe.Date.ToString("yyyy-MM-dd"), item.InvId, name, code, item.UnitName, specification, item.Quantity, subscribe.DeptId));
                        }
                    });
                }

                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);
                if (effectRow > 0)
                {
                    if (subscribe.NeedAudit)
                    {
                        DataTable dtOpenIds = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping t1 LEFT JOIN  
                    dbo.ZYSoftAuditRecord t2 ON t1.UserGuid = t2.UserGuid WHERE t2.BillType ={1} AND t2.BillId = {0} AND t2.No  =0", billId, BillType.SG.GetHashCode()));
                        if (dtOpenIds != null && dtOpenIds.Rows.Count > 0)
                        {
                            string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '您有一份申购单需要审批!|'+t2.name +'|'+ t3.name+'|'+ '申购单'+'|'+ '采购申请'+'|'+ CONVERT(VARCHAR(10),Date,23) FROM dbo.ZYSoftSubscribe t1 LEFT JOIN  dbo.vDepartment t2 ON 
                                t1.DeptId = t2.id LEFT JOIN dbo.vPerson t3 ON t1.BillerId = t3.id WHERE t1.Id = {0}", billId));

                            foreach (DataRow dr in dtOpenIds.Rows)
                            {
                                string openId = Convert.ToString(dr["OpenId"]);
                                if (openId != "")
                                {
                                    string errMsg = "";
                                    SendWechatMsg(MsgType.SendToAuditer.GetHashCode(), openId, content, "id=" + billId, ref errMsg);
                                }
                            }
                        }
                    }
                    response.SetSuccess("保存成功!");
                }
                else
                {
                    response.SetError("保存失败!");
                }

            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/sub/modify")]
        public IActionResult ModifySubscribe(SubscribeCreateModel model)
        {
            ZYSoftSubscribe subscribe = model.subscribe;
            List<ZYSoftSubscribeEntry> subscribeEntry = model.subscribeEntry;
            List<SubscribeBillEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                List<string> sqls = new List<string>();
                string billId = subscribe.Id.ToString();

                sqls.Add(string.Format(@"DELETE FROM ZYSoftSubscribeEntry WHERE BillId = '{0}'", billId));

                sqls.Add(string.Format(@"UPDATE dbo.ZYSoftSubscribe SET ModifiedOn = GETDATE(),ModifiedByUserGuid = '{1}',
                ModifiedByUserName = '{2}' WHERE Id = '{0}'", billId, AuthContextService.CurrentUser.Guid,
                        AuthContextService.CurrentUser.DisplayName));

                subscribeEntry.ForEach(item =>
                {
                    sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftSubscribeEntry
                            ( BillId ,
                              InvId ,
                              UnitName,
                              Remark ,
                              Quantity ,
                              QuantityFinish ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                              '{2}' , -- Remark - varchar(100)
                              '{3}',
                              {4} , -- Quantity - decimal
                              0 , -- QuantityFinish - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{5}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{6}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.UnitName, item.Remark, item.Quantity,
                             AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                });
                sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftSubscribeLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }

            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }
        #endregion

        #region 询价单    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="invId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/ask/check")]
        public IActionResult GetInvPrice(int partnerId, int invId, int billId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = billId < 0 ? ZYSoft.DB.BLL.Common.ExecuteDataTable(
               string.Format(@"EXEC L_P_GetInvPrice '{0}',{1} ", partnerId, invId)) :
                    ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"select PriceLast PriceCurrent,PriceCurrentConfirm PriceCurrentConfirm from 
                    ZYSoftInquiryEntry where BillId= {0} and PartnerId = {1} and InvId = {2}", billId, partnerId, invId));
            var list = DataTableConvert.ToList<PriceModel>(query);
            response.SetData(list);
            return Ok(response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/ask/save")]
        public IActionResult SaveAsk(InquiryCreateModel model)
        {
            ZYSoftInquiry inquiry = model.inquiry;
            List<ZYSoftInquiryEntry> inquiryEntry = model.inquiryEntry;
            List<TotalInquiryEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftInquiry
             * 2.ZYSoftInquiryEntry 
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                List<string> sqls = new List<string>();
                List<string> errList = new List<string>();
                inquiryEntry.ForEach(item =>
                {
                    string invName = CheckVaild(inquiry.StartDate, inquiry.EndDate, item.InvId, item.PartnerId.ToString());
                    if (!string.IsNullOrEmpty(invName))
                    {
                        errList.Add(string.Format(@"发现存货 {0} 在 {1} 到 {2} 期间已经存在报价!", invName, inquiry.StartDate, inquiry.EndDate));
                    }
                });
                if (errList.Count > 0)
                {
                    response.SetError(errList[0]);
                }
                else
                {
                    string billId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                            string.Format(@"EXEC dbo.L_P_GetMaxID @TableName = '{0}',@Increment = 1", "ZYSoftInquiry"));
                    string billNo = string.Format(@"XJ{0}", inquiry.BillNo);

                    string _serialNo = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT TOP 1 RIGHT(CONCAT( '00',(CONVERT(INT,SUBSTRING(DisplayBillNo,LEN(DisplayBillNo)-2,3))+1 )) , 3) 
                            FROM dbo.ZYSoftInquiry WHERE [Date] ='{0}' ORDER BY id  DESC ", DateTime.Now.ToString("yyyy-MM-dd")));
                    if (string.IsNullOrEmpty(_serialNo))
                    {
                        _serialNo = "001";
                    }
                    string displayBillNo = string.Format(@"XJ{0}{1}", DateTime.Now.ToString("yyyyMMdd"), _serialNo);
                    sqls.Add(string.Format(@"
                 INSERT INTO dbo.ZYSoftInquiry
                         ( Id,
                           BillType,
                           BillNo ,
                           Date , 
                           BillerId , 
                           Status ,
                           IsDeleted ,
                           CreatedOn ,
                           CreatedByUserGuid ,
                           CreatedByUserName ,
                           StartDate,
                           EndDate,
                           ClsId,DisplayBillNo
                         )
                 VALUES  ( '{0}' , -- Id - varchar(50)
                           '{1}' , -- BillType - varchar(50)
                           '{2}' , -- BillNo - varchar(50) 
                           GETDATE() , -- Date - datetime
                           '{3}' , -- BillerId - varchar(20) 
                           {4}, -- Status - int
                           0 , -- IsDeleted - int
                           GETDATE() , -- CreatedOn - datetime2
                           '{5}' , -- CreatedByUserGuid - uniqueidentifier
                           N'{6}', -- CreatedByUserName - nvarchar(max)
                           '{7} 00:00:00','{8} 23:59:59',{9},'{10}'
                         )", billId, BillType.XJ.GetHashCode(), billNo
                             , inquiry.BillerId, AuditStatus.Ing.GetHashCode(),
             AuthContextService.CurrentUser.Guid,
             AuthContextService.CurrentUser.DisplayName, inquiry.StartDate, inquiry.EndDate, inquiry.ClsId, displayBillNo));
                    inquiryEntry.ForEach(item =>
                    {
                        sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftInquiryEntry
                            ( BillId ,
                              PartnerId ,
                              InvId , 
                              PriceLast ,
                              PriceLastConfirm , 
                              PriceMarket ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName ,
                              Remark 
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- PartnerId - varchar(10)
                              '{2}' , -- InvId - varchar(10)
                              {3} , -- PriceLast - decimal
                              {4} , -- PriceLastConfirm - decimal
                              {5} , -- PriceMarket - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{6}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{7}', -- CreatedByUserName - nvarchar(max)
                              N'{8}' -- Remark - varchar(100)
                            )", billId, item.PartnerId, item.InvId, item.PriceLast, item.PriceLastConfirm,
                                    item.PriceMarket,
                                 AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName, item.Remark));
                    });

                    sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftInquiryLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                               AuthContextService.CurrentUser.DisplayName));

                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                    if (effectRow > 0) { response.SetData(new { id = billId }); response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/ask/modify")]
        public IActionResult ModifyAsk(InquiryCreateModel model)
        {
            ZYSoftInquiry inquiry = model.inquiry;
            List<ZYSoftInquiryEntry> inquiryEntry = model.inquiryEntry;
            List<TotalInquiryEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", inquiry.Id)))
                {
                    response.SetError("单据已经审批,无法修改!");
                }
                else
                {
                    List<string> sqls = new List<string>();
                    List<string> errList = new List<string>();
                    inquiryEntry.ForEach(item =>
                    {
                        string invName = CheckVaild(inquiry.StartDate, inquiry.EndDate, item.InvId, item.PartnerId.ToString());
                        if (!string.IsNullOrEmpty(invName))
                        {
                            errList.Add(string.Format(@"发现存货 {0} 在 {1} 到 {2} 期间已经存在报价!", invName, inquiry.StartDate, inquiry.EndDate));
                        }
                    });
                    if (errList.Count > 0)
                    {
                        response.SetError(errList[0]);
                    }
                    else
                    {
                        string billId = inquiry.Id.ToString();

                        sqls.Add(string.Format(@"DELETE FROM ZYSoftInquiryEntry WHERE BillId = '{0}'", billId));

                        sqls.Add(string.Format(@"UPDATE dbo.ZYSoftInquiry SET StartDate='{3} 00:00:00',EndDate='{4} 23:59:59',ModifiedOn = GETDATE(),ModifiedByUserGuid = '{1}',
                ModifiedByUserName = '{2}',ClsId = '{5}' WHERE Id = '{0}'", billId, AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName, inquiry.StartDate, inquiry.EndDate, inquiry.ClsId));

                        inquiryEntry.ForEach(item =>
                        {
                            sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftInquiryEntry
                            ( BillId ,
                              PartnerId ,
                              InvId ,
                              PriceLast ,
                              PriceLastConfirm , 
                              PriceMarket ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName ,
                              Remark 
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- PartnerId - varchar(10)
                              '{2}' , -- InvId - varchar(10)
                              {3} , -- PriceLast - decimal
                              {4} , -- PriceLastConfirm - decimal
                              {5} , -- PriceMarket - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{6}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{7}', -- CreatedByUserName - nvarchar(max)
                              N'{8}' -- Remark - varchar(100)
                            )", billId, item.PartnerId, item.InvId, item.PriceLast, item.PriceLastConfirm,
                                        item.PriceMarket,
                                     AuthContextService.CurrentUser.Guid,
                                     AuthContextService.CurrentUser.DisplayName, item.Remark));
                        });

                        sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftInquiryLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName));
                        int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                        if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }
                    }
                }
            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/ask/list")]
        public IActionResult GetInquiryList(int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch ((AuditStatus)type)
            {
                case AuditStatus.Ing:
                case AuditStatus.Agree:
                    str = string.Format(@"SELECT * FROM dbo.vInquiryList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(Status,-1) = {1} AND ISNULL(IsDeleted,0) = 0 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                AuthContextService.CurrentUser.Guid, type);
                    break;
                case AuditStatus.Void:
                    str = string.Format(@"SELECT * FROM dbo.vInquiryList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(IsDeleted,0) = 1 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                        AuthContextService.CurrentUser.Guid, type);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            var list = DataTableConvert.ToList<vInquiryList>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/ask/one")]
        public IActionResult GetAsk(string id, string partnerId = "")
        {
            string str = string.Format(@"SELECT * FROM dbo.vInquiryEntry Where Id='{0}'", id);
            var response = ResponseModelFactory.CreateResultInstance;
            if (!string.IsNullOrEmpty(partnerId))
            {
                str = string.Format(@"SELECT * FROM dbo.vInquiryEntry Where Id='{0}' AND 
                    PartnerId IN(SELECT PartnerId FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid = '{1}')", id, AuthContextService.CurrentUser.Guid);
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            var list = DataTableConvert.ToList<vInquiryEntry>(query);
            response.SetData(list);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/ask/record")]
        public IActionResult GetAskRecordLog(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var context = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 Context FROM dbo.ZYSoftInquiryLog Where BillId='{0}' ORDER BY CreatedOn DESC ", id));
            response.SetData(context);
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/del")]
        public IActionResult DelAsk(dynamic model)
        {
            string id = model.id;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("申请已经审批,无法作废!");
            }
            else
            {
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                    string.Format(@"UPDATE ZYSoftInquiry set IsDeleted =1 Where Id ={0}", id));
                if (effectRow > 0)
                {
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetSuccess("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/audit")]
        public IActionResult AuditAsk(dynamic model)
        {
            string id = model.id;
            string userId = model.userId;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("单据已经审批,无须多次操作!");
            }
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiryEntry 
                    WHERE BillId ={0} AND PriceCurrentConfirm<=0", id)))
            {
                response.SetError("当前询价单尚未报价,请先联系供应商!");
            }
            else
            {
                List<string> errList = new List<string>();
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT CONVERT(varchar(10),StartDate,23)StartDate, 
                CONVERT(varchar(10),EndDate,23) EndDate,InvId,PartnerId from vInquiryEntry WHERE id ={0}", id));
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        string invName = CheckVaild(dataRow["StartDate"].ToString(), dataRow["EndDate"].ToString(), dataRow["InvId"].ToString(), dataRow["PartnerId"].ToString());
                        if (!string.IsNullOrEmpty(invName))
                        {
                            errList.Add(string.Format(@"发现存货 {0} 在 {1} 到 {2} 期间已经存在报价!", invName, dataRow["StartDate"].ToString(), dataRow["EndDate"].ToString()));
                        }
                    }
                }
                if (errList.Count > 0)
                {
                    response.SetError(errList[0]);
                }
                else
                {
                    var effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                        string.Format(@"UPDATE ZYSoftInquiry set Status =1,AuditerId='{1}',AuditDate=getdate() Where Id ={0}", id, userId));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetError("操作失败!");
                    }
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/unaudit")]
        public IActionResult UnAuditAsk(dynamic model)
        {
            string id = model.id;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 0", id)))
            {
                response.SetError("单据未审批,无须多次操作!");
            }
            else
            {
                List<string> errList = new List<string>();
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT CONVERT(varchar(10),StartDate,23)StartDate, 
                CONVERT(varchar(10),EndDate,23) EndDate,InvId,PartnerId from vInquiryEntry WHERE id ={0}", id));
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        string invName = CheckInvHaveOrder(dataRow["StartDate"].ToString(), dataRow["EndDate"].ToString(), dataRow["InvId"].ToString(), dataRow["PartnerId"].ToString());
                        if (!string.IsNullOrEmpty(invName))
                        {
                            errList.Add(string.Format(@"发现存货 {0} 在 {1} 到 {2} 期间已经存在采购订单!", invName, dataRow["StartDate"].ToString(), dataRow["EndDate"].ToString()));
                        }
                    }
                }
                if (errList.Count > 0)
                {
                    response.SetError(errList[0]);
                }
                else
                {
                    var effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                        string.Format(@"UPDATE ZYSoftInquiry set Status =0,AuditerId=NULL,AuditDate=NULL Where Id ={0}", id));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetError("操作失败!");
                    }
                }
            }
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/ask/partnerasklist")]
        public IActionResult GetPartnerAskList(string partnerId, int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vInquiryList WHERE Id in(
                    SELECT Id FROM dbo.vInquiryEntry WHERE  PartnerId = {0} and status = {1}
                    )  order by CreatedOn desc", partnerId, type));
            var list = DataTableConvert.ToList<vInquiryEntry>(query);
            response.SetData(list);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/ask/partnerask")]
        public IActionResult GetPartnerAskEntry(string id, string partnerId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vInquiryEntry Where Id='{0}' And PartnerId='{1}'", id, partnerId));
            var list = DataTableConvert.ToList<vInquiryEntry>(query);
            response.SetData(list);
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/partnerconfirm")]
        public IActionResult PartnerAskConfirm(List<PartnerAskModel> list)
        {
            string id = list.FirstOrDefault().Id.ToString();

            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 0", id)))
            {
                List<string> sqlList = new List<string>();
                list.ForEach(f =>
                {
                    sqlList.Add(string.Format(@"UPDATE dbo.ZYSoftInquiryEntry SET ConfirmDate = GETDATE(),IsConfirm =1
                    WHERE BillId = {0} AND Id = {1}", f.Id, f.EntryId));
                });
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqlList);
                if (effectRow > 0)
                {
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetSuccess("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/sendpartner")]
        public IActionResult SendPartnerConfirm(dynamic model)
        {
            string id = model.id;
            string billId = model.billId;
            var response = ResponseModelFactory.CreateResultInstance;
            string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 ISNULL(t2.OpenId,'') openId FROM dbo.ZYSoftUserPartnerMapping t1 LEFT JOIN 
                dbo.ZYSoftUserWcMapping t2 ON t2.UserGuid = t1.UserGuid WHERE PartnerId = {0}", id));
            if (string.IsNullOrEmpty(openId))
            {
                response.SetError("用户未绑定微信身份，通知无法发出!");
            }
            else
            {
                string errMsg = "";
                if (SendWechatMsg(MsgType.SendToPartner.GetHashCode(), openId, "您有一份询价单待填写|询价专员|"
                    + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "|点击查看详情", "id=" + billId, ref errMsg))
                {
                    response.SetSuccess(errMsg);
                }
                else
                {
                    response.SetError(errMsg);
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/partnerupdate")]
        public IActionResult UpdatePartnerAsk(List<PartnerAskModel> list)
        {
            string id = list.FirstOrDefault().Id.ToString();
            string partnerId = list.FirstOrDefault().PartnerId.ToString();
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("当前单据已经审批,无法再次修改!");
            }
            else
            {
                List<string> sqlList = new List<string>();
                list.ForEach(f =>
                {
                    sqlList.Add(string.Format(@"UPDATE dbo.ZYSoftInquiryEntry SET PriceCurrent = '{0}'
                    WHERE BillId = {1} AND Id = {2}", f.PriceCurrent, f.Id, f.EntryId));
                });
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqlList);
                if (effectRow > 0)
                {
                    string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping WHERE UserGuid IN (
                    SELECT TOP 1 CreatedByUserGuid FROM dbo.ZYSoftInquiry WHERE  Id = {0})", id));
                    if (openId != "")
                    {
                        string partnerName = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT name FROM dbo.vPartner WHERE id ='{0}'", partnerId));
                        string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '收到【{1}】的询价单回复!|'+DisplayBillNo+'|'+CONVERT(VARCHAR(16),Date,120) 
                        FROM dbo.ZYSoftInquiry WHERE Id = {0}", id, partnerName));
                        string errMsg = "";
                        SendWechatMsg(MsgType.SendToBiller.GetHashCode(), openId, content, "id=" + id, ref errMsg);
                    }
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetError("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/partnerupdateconfirm")]
        public IActionResult ConfirmUpdatePartnerAsk(List<PartnerAskModel> list)
        {
            string id = list.FirstOrDefault().Id.ToString();

            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftInquiry 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("当前单据已经审批,无法再次修改!");
            }
            else
            {
                int entryId = 0;
                List<string> sqlList = new List<string>();
                list.ForEach(f =>
                {
                    sqlList.Add(string.Format(@"UPDATE dbo.ZYSoftInquiryEntry SET PriceCurrentConfirm = '{0}'
                    WHERE BillId = {1} AND Id = {2}", f.PriceCurrentConfirm, f.Id, f.EntryId));
                    entryId = f.EntryId;
                });
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqlList);
                if (effectRow > 0)
                {
                    string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping WHERE UserGuid IN (
                            SELECT TOP 1 t2.UserGuid FROM dbo.ZYSoftInquiryEntry t1 LEFT JOIN dbo.ZYSoftUserPartnerMapping t2 ON t1.PartnerId = t2.PartnerId Where t1.Id={0})", entryId));
                    if (openId != "")
                    {
                        string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '您好,你的报价已被收到!|'+T1.DisplayBillNo+'|-|价格有效期:'+CONVERT(VARCHAR(10),T1.STARTDATE,23)+'~'
                                    +CONVERT(VARCHAR(10),T1.ENDDATE,23) FROM ZYSoftInquiry T1 WHERE T1.ID ={0}", id));
                        string errMsg = "";
                        SendWechatMsg(MsgType.SendToPartnerConfirmPrice.GetHashCode(), openId, content, "id=" + id, ref errMsg);
                    }
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetError("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/ask/partnerconfirmprice")]
        public IActionResult NotityPartnerConfirm(dynamic model)
        {
            string clsId = model.clsId;
            string id = model.id;
            var response = ResponseModelFactory.CreateResultInstance;

            DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT idParent FROM dbo.vInventoryClassPartner WHERE idinventoryclass = {0}", clsId));
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string partnerId = Convert.ToString(dr["idParent"]);
                    string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT t2.OpenId FROM dbo.ZYSoftUserPartnerMapping t1 LEFT JOIN  dbo.ZYSoftUserWcMapping 
                        t2 ON t1.UserGuid = t2.UserGuid WHERE t1.PartnerId = 1", partnerId));
                    if (openId != "")
                    {
                        string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '您好,你的报价已被收到!|'+T1.DisplayBillNo+'|-|价格有效期:'+CONVERT(VARCHAR(10),T1.STARTDATE,23)+'~'
                                    +CONVERT(VARCHAR(10),T1.ENDDATE,23) FROM ZYSoftInquiry T1 WHERE T1.ID ={0}", id));
                        string errMsg = "";
                        SendWechatMsg(MsgType.SendToPartnerConfirmPrice.GetHashCode(), openId, content, "id=" + id, ref errMsg);
                    }
                }
            }
            response.SetSuccess("操作成功!");
            return Ok(response);
        }
        #endregion

        #region 采购单
        /// <summary>
        /// 去掉 IsConfirm =1 的限制，防止客户傻逼嘻嘻不按流程操作鬼喊没有价格
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/po/detail")]
        public IActionResult GetPoDetail(dynamic model)
        {
            string date = model.date;
            string partnerId = model.partnerId;
            string clsId = model.clsId;

            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT {1} partnerId,t1.DeptId,t3.name AS DeptName,t1.Id,t1.Name,t1.Code,t1.Unitname,t1.Specification,(t1.Count-t1.FinishCount) AS [count],ISNULL(t2.PriceCurrentConfirm ,0)AS price 
                    FROM dbo.ZYSoftSubSummary t1 LEFT JOIN (SELECT InvId,PriceCurrentConfirm  FROM  dbo.vInquiryEntry WHERE IsDeleted =0 AND
                StartDate<='{0} 00:00:00' AND EndDate>='{0} 23:59:59' AND PartnerId ={1}
                 )t2 ON t1.Id = t2.InvId LEFT JOIN  dbo.t_Department t3 ON t1.DeptId = t3.id LEFT JOIN vInventory t4 on t1.Id = t4.id 
                WHERE Date ='{0}' AND (t1.Count-t1.FinishCount)>0 and t4.idinventoryclass = '{2}'", date, partnerId, clsId));
            //var list = DataTableConvert.ToList<vInventory>(query);
            response.SetData(query);
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/po/cls")]
        public IActionResult GetPoCls(dynamic model)
        {
            string date = model.date;

            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"select distinct t2.idinventoryclass from ZYSoftSubSummary t1 left join vInventory 
                    t2 on t1.Id = t2.id  where [date] = '{0}'", date));
            response.SetData(query);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/po/record")]
        public IActionResult GetPoRecordLog(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var context = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 Context FROM dbo.ZYSoftPoLog Where BillId='{0}' ORDER BY CreatedOn DESC ", id));
            response.SetData(context);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/po/blist")]
        public IActionResult GetPoBillNoList(int partnerId)
        {
            var date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var response = ResponseModelFactory.CreateResultInstance;
            var str = string.Format(@"SELECT DISTINCT Id, BillNo,DisplayBillNo,RequiredDate FROM dbo.vPoEntry WHERE PartnerId = {0} AND ISNULL(FinishQuantity,0) =0 
                    AND RequiredDate >='{1}' AND ISNULL(Status,0) =1 AND ISNULL(IsDeleted,0)=0", partnerId, date);
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/po/list")]
        public IActionResult GetPoList(int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch ((AuditStatus)type)
            {
                case AuditStatus.Ing:
                case AuditStatus.Agree:
                    str = string.Format(@"SELECT * FROM dbo.vPoList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(Status,-1) = {1} AND ISNULL(IsDeleted,0) = 0 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                AuthContextService.CurrentUser.Guid, type);
                    break;
                case AuditStatus.Void:
                    str = string.Format(@"SELECT * FROM dbo.vPoList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(IsDeleted,0) = 1 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                        AuthContextService.CurrentUser.Guid, type);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/po/one")]
        public IActionResult GetPo(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vPoEntry Where Id='{0}'", id));
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/po/save")]
        public IActionResult SavePo(PoCreateModel model)
        {
            ZYSoftPo po = model.po;
            List<ZYSoftPoEntry> poEntry = model.poEntry;
            List<TotalPoEntry> context = model.context;

            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                List<string> sqls = new List<string>();
                string billId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                    string.Format(@"EXEC dbo.L_P_GetMaxID @TableName = '{0}',@Increment = 1", "ZYSoftPo"));
                string billNo = string.Format(@"Po{0}", po.BillNo);
                sqls.Add(string.Format(@"
                 INSERT INTO dbo.ZYSoftPo
                         ( Id,
                           BillType,
                           BillNo ,
                           Date ,
                           RequiredDate,
                           AskDate,
                           PartnerId ,
                           BillerId , 
                           Status ,
                           IsDeleted ,
                           CreatedOn ,
                           CreatedByUserGuid ,
                           CreatedByUserName 
                         )
                 VALUES  ( '{0}' , -- Id - varchar(50)
                           '{1}' , -- BillType - varchar(50)
                           '{2}' , -- BillNo - varchar(50) 
                           '{8}' , -- Date - datetime
                           '{9}' , -- RequiredDate - datetime
                           '{10}' , -- AskDate - datetime
                           '{3}' , -- PartnerId - varchar(20)
                           '{4}' , -- BillerId - varchar(20) 
                           {5}, -- Status - int
                           0 , -- IsDeleted - int
                           GETDATE() , -- CreatedOn - datetime2
                           '{6}' , -- CreatedByUserGuid - uniqueidentifier
                           N'{7}' -- CreatedByUserName - nvarchar(max)
                         )", billId, BillType.PO.GetHashCode(), billNo, po.PartnerId,
         po.BillerId, AuditStatus.Ing.GetHashCode(),
         AuthContextService.CurrentUser.Guid,
         AuthContextService.CurrentUser.DisplayName, po.Date, po.RequiredDate, po.AskDate));
                poEntry.ForEach(item =>
                {
                    sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftPoEntry
                            ( BillId ,
                              InvId ,
                              DeptId,
                              Remark ,
                              Quantity ,
                              Price ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                              '{7}' , -- DeptId - varchar(20)
                              '{2}' , -- Remark - varchar(100)
                              {3} , -- Quantity - decimal
                              {6} , -- Pirce - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{4}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{5}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.Remark, item.Quantity,
                             AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName, item.Price, item.DeptId));
                });

                sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftPoLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }

            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/po/savemuilt")]
        public IActionResult SavePopMuilt(List<PoCreateModel> list)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            List<string> sqls = new List<string>();

            try
            {
                int i = 1;
                list.ForEach(model =>
                {
                    ZYSoftPo po = model.po;
                    List<ZYSoftPoEntry> poEntry = model.poEntry;
                    List<TotalPoEntry> context = model.context;

                    string billId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                        string.Format(@"EXEC dbo.L_P_GetMaxID @TableName = '{0}',@Increment = 1", "ZYSoftPo"));
                    string billNo = string.Format(@"Po{0}", po.BillNo);

                    string _serialNo = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT TOP 1 RIGHT(CONCAT( '00',(CONVERT(INT,SUBSTRING(DisplayBillNo,LEN(DisplayBillNo)-2,3))+ {1} )) , 3) 
                            FROM dbo.ZYSoftPo WHERE [Date] ='{0}' ORDER BY id  DESC ", DateTime.Now.ToString("yyyy-MM-dd"), i));
                    if (string.IsNullOrEmpty(_serialNo))
                    {
                        _serialNo = "001";
                    }

                    string displayBillNo = string.Format(@"Po{0}", DateTime.Now.ToString("yyyyMMdd") + _serialNo); ;
                    sqls.Add(string.Format(@"
                 INSERT INTO dbo.ZYSoftPo
                         ( Id,
                           BillType,
                           BillNo ,
                           Date ,
                           RequiredDate,
                           AskDate,
                           PartnerId ,
                           BillerId , 
                           Status ,
                           IsDeleted ,
                           CreatedOn ,
                           CreatedByUserGuid ,
                           CreatedByUserName ,
                           DisplayBillNo
                         )
                 VALUES  ( '{0}' , -- Id - varchar(50)
                           '{1}' , -- BillType - varchar(50)
                           '{2}' , -- BillNo - varchar(50) 
                           '{8}' , -- Date - datetime
                           '{9}' , -- RequiredDate - datetime
                           '{10}' , -- AskDate - datetime
                           '{3}' , -- PartnerId - varchar(20)
                           '{4}' , -- BillerId - varchar(20) 
                           {5}, -- Status - int
                           0 , -- IsDeleted - int
                           GETDATE() , -- CreatedOn - datetime2
                           '{6}' , -- CreatedByUserGuid - uniqueidentifier
                           N'{7}','{11}' -- CreatedByUserName - nvarchar(max)
                         )", billId, BillType.PO.GetHashCode(), billNo, po.PartnerId,
             po.BillerId, AuditStatus.Ing.GetHashCode(),
             AuthContextService.CurrentUser.Guid,
             AuthContextService.CurrentUser.DisplayName, po.Date, po.RequiredDate, po.AskDate, displayBillNo));
                    poEntry.ForEach(item =>
                    {
                        sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftPoEntry
                            ( BillId ,
                              InvId ,
                              DeptId ,
                              UnitName,
                              Remark ,
                              Quantity ,
                              Price ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                              '{8}' , -- DeptId - varchar(20)
                              '{2}' , -- UnitName - varchar(20)
                              '{3}' , -- Remark - varchar(100)
                              {4} , -- Quantity - decimal
                              {7} , -- Pirce - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{5}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{6}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.UnitName, item.Remark, item.Quantity,
                                 AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName, item.Price, item.DeptId));
                    });

                    sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftPoLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName));

                    i++;
                });

                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }
            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/po/modify")]
        public IActionResult ModifyPo(PoCreateModel model)
        {
            ZYSoftPo po = model.po;
            List<ZYSoftPoEntry> poEntry = model.poEntry;
            List<TotalPoEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPo 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", po.Id)))
                {
                    response.SetError("单据已经审批,无法修改!");
                }
                else
                {
                    List<string> sqls = new List<string>();
                    string billId = po.Id.ToString();

                    sqls.Add(string.Format(@"DELETE FROM ZYSoftPoEntry WHERE BillId = '{0}'", billId));

                    sqls.Add(string.Format(@"UPDATE dbo.ZYSoftPo SET PartnerId={4},RequiredDate='{3}',AskDate='{5}',ModifiedOn = GETDATE(),ModifiedByUserGuid = '{1}',
                ModifiedByUserName = '{2}' WHERE Id = '{0}'", billId, AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName, po.RequiredDate, po.PartnerId, po.AskDate));

                    poEntry.ForEach(item =>
                    {
                        sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftPoEntry
                            ( BillId , 
                              InvId ,
                              Price,
                              Quantity , 
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName ,
                              Remark 
                            )
                    VALUES  ( {0} , -- BillId - int 
                              '{1}' , -- InvId - varchar(10)
                              {2} , -- Price - decimal
                              {3} , -- Quantity - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{4}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{5}', -- CreatedByUserName - nvarchar(max)
                              N'{6}' -- Remark - varchar(100)
                            )", billId, item.InvId, item.Price, item.Quantity,
                                 AuthContextService.CurrentUser.Guid,
                                 AuthContextService.CurrentUser.DisplayName, item.Remark));
                    });

                    sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftPoLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                    if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/po/del")]
        public IActionResult DelPo(dynamic model)
        {
            string id = model.id;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPo 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("申请已经审批,无法作废!");
            }
            else
            {
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                    string.Format(@"UPDATE ZYSoftPo set IsDeleted =1 Where Id ={0}", id));
                if (effectRow > 0)
                {
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetSuccess("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/po/audit")]
        public IActionResult AuditPo(dynamic model)
        {
            string id = model.id;
            string userId = model.userId;
            string date = model.date;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPo 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("单据已经审批,无须多次操作!");
            }
            else
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPoEntry t1 LEFT JOIN  dbo.ZYSoftSubSummary t2 ON t1.InvId = t2.Id
                    WHERE t1.BillId = {0} AND t2.Date ='{1}' AND t2.FinishCount+t1.Quantity >t2.Count", date, id)))
                {
                    List<string> ls = new List<string>();
                    ls.Add(string.Format(@"UPDATE ZYSoftPo set Status =1,AuditerId='{1}',AuditDate=GETDATE() Where Id ={0}", id, userId));
                    DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                        string.Format(@"SELECT InvId,Quantity,DeptId FROM dbo.ZYSoftPoEntry WHERE BillId = {0}", id));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            ls.Add(string.Format(@"UPDATE dbo.ZYSoftSubSummary SET FinishCount = FinishCount +{0} WHERE Date ='{1}' AND Id ={2} AND DeptId = '{3}'",
                                dr["Quantity"], date, dr["InvId"], dr["DeptId"]));
                        }
                    }
                    var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                    if (effectRow > 0)
                    {
                        string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping WHERE UserGuid IN (
                            SELECT TOP 1 t2.UserGuid FROM dbo.ZYSoftPo t1 LEFT JOIN dbo.ZYSoftUserPartnerMapping t2 ON t1.PartnerId = t2.PartnerId Where t1.Id={0})", id));
                        if (openId != "")
                        {
                            string deptName = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"select name from vDepartment where id in(select top 1  DeptId from ZYSoftPoEntry where BillId = '{0}')", id));
                            string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"select '您有新的采购单需要接收!|'+t2.name+'|'+t1.DisplayBillNo+'|采购单|'+CONVERT(varchar(10),t1.Date,23)+'|'
                                +CONVERT(varchar(10),t1.RequiredDate,23)+'|部门:{1}  请准时安排送货!' from ZYSoftPo  
                                t1 left join vPartner t2 on t1.PartnerId =t2.id where t1.id ={0}", id, deptName));
                            string errMsg = "";
                            SendWechatMsg(MsgType.SendToPo.GetHashCode(), openId, content, "id=" + id, ref errMsg);
                        }
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetError("操作失败!");
                    }
                }
                else
                {
                    response.SetError("采购数量检查不合法,审批失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/po/unaudit")]
        public IActionResult UnAuditPo(dynamic model)
        {
            string id = model.id;
            string userId = model.userId;
            string date = model.date;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPo 
                    WHERE Id ={0} AND ISNULL(Status,0) = 0", id)))
            {
                response.SetError("单据尚未审批,无须多次操作!");
            }
            else
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftPoEntry t1 LEFT JOIN  dbo.ZYSoftSubSummary t2 ON t1.InvId = t2.Id
                    WHERE t1.BillId = {0} AND t2.Date ='{1}' AND t1.Quantity - t2.FinishCount < t2.Count", date, id)))
                {
                    List<string> ls = new List<string>();
                    ls.Add(string.Format(@"UPDATE ZYSoftPo set Status =0,AuditerId= -1,AuditDate=NULL Where Id ={0}", id, userId));
                    DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                        string.Format(@"SELECT InvId,Quantity,DeptId FROM dbo.ZYSoftPoEntry WHERE BillId = {0}", id));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            ls.Add(string.Format(@"UPDATE dbo.ZYSoftSubSummary SET FinishCount = FinishCount - {0} WHERE Date ='{1}' AND Id ={2} AND DeptId = '{3}'",
                                dr["Quantity"], date, dr["InvId"], dr["DeptId"]));
                        }
                    }
                    var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                    if (effectRow > 0)
                    {
                        string openId = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT OpenId FROM dbo.ZYSoftUserWcMapping WHERE UserGuid IN (
                            SELECT TOP 1 t2.UserGuid FROM dbo.ZYSoftPo t1 LEFT JOIN dbo.ZYSoftUserPartnerMapping t2 ON t1.PartnerId = t2.PartnerId Where t1.Id={0})", id));
                        if (openId != "")
                        {
                            string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"select '你有一个采购订单变更|'+CONVERT(varchar(10),GETDATE(),23)+'|采购单|'+t1.DisplayBillNo+'|订单被取消如有疑问请反馈采购员!' from ZYSoftPo  
                                t1 left join vPartner t2 on t1.PartnerId =t2.id where t1.id ={0}", id));
                            string errMsg = "";
                            SendWechatMsg(MsgType.SendToPoWithUnAudit.GetHashCode(), openId, content, "id=" + id, ref errMsg);
                        }
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetError("操作失败!");
                    }
                }
                else
                {
                    response.SetError("申购数量检查不合法,反审批失败!");
                }
            }
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/po/partnerconfirmorder")]
        public IActionResult PartnerConfirmOrder(int id, int partnerId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"IF EXISTS(SELECT 1 FROM ZYSoftPo WHERE id = {0} AND PartnerId ={1} AND ConfirmDate is NULL)
                                        BEGIN
	                                        UPDATE ZYSoftPo SET ConfirmDate = GETDATE() WHERE id = {0} AND PartnerId ={1}
                                        END", id, partnerId));
            response.SetSuccess("操作成功!");
            return Ok(response);
        }
        #endregion

        #region 签收单
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        //[HttpGet("/api/v1/bus/sign/detail")]
        //public IActionResult GetSignDetail(string date, string partnerId)
        //{
        //    var response = ResponseModelFactory.CreateResultInstance;
        //    var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
        //        string.Format(@"SELECT partnerId,partnerName,Id,EntryId AS orderEntryId,InvId,code,name,deptId,deptName,unitname_s AS unitname,specification,price,Quantity - FinishQuantity AS [count]
        //            FROM dbo.vPoEntry WHERE PartnerId = {0} AND RequiredDate ='{1}' AND Status =1 AND IsDeleted =0 AND ISNULL(FinishQuantity,0)=0", partnerId, date));
        //    //var list = DataTableConvert.ToList<vInventory>(query);
        //    response.SetData(query);
        //    return Ok(response);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/sign/detail")]
        public IActionResult GetSignDetail(string orderId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT partnerId,partnerName,Id,EntryId AS orderEntryId,InvId,code,name,deptId,deptName,unitname_s AS unitname,specification,price,Quantity  AS [count]
                    FROM dbo.vPoEntry WHERE Id = {0}  AND Status =1 AND IsDeleted =0 AND ISNULL(FinishQuantity,0)=0", orderId));
            //var list = DataTableConvert.ToList<vInventory>(query);
            response.SetData(query);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/sign/record")]
        public IActionResult GetSignRecordLog(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var context = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 Context FROM dbo.ZYSoftSignLog Where BillId='{0}' ORDER BY CreatedOn DESC ", id));
            response.SetData(context);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/sign/list")]
        public IActionResult GetSignList(int type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch ((AuditStatus)type)
            {
                case AuditStatus.Ing:
                case AuditStatus.Agree:
                    str = string.Format(@"SELECT * FROM dbo.vSignList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(Status,-1) = {1} AND ISNULL(IsDeleted,0) = 0 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                AuthContextService.CurrentUser.Guid, type);
                    break;
                case AuditStatus.Void:
                    str = string.Format(@"SELECT * FROM dbo.vSignList WHERE CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(IsDeleted,0) = 1 AND CreatedByUserGuid = '{0}' ORDER BY CreatedOn desc",
                        AuthContextService.CurrentUser.Guid, type);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            response.SetData(query);
            return Ok(response);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/sign/one")]
        public IActionResult GetSign(string id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vSignEntry Where Id='{0}'", id));
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/sign/save")]
        public IActionResult SaveSign(SignCreateModel model)
        {
            ZYSoftSign sign = model.sign;
            List<ZYSoftSignEntry> signEntry = model.signEntry;
            List<TotalSignEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                List<string> sqls = new List<string>();
                string billId = ZYSoft.DB.BLL.Common.ExecuteScalar(
                    string.Format(@"EXEC dbo.L_P_GetMaxID @TableName = '{0}',@Increment = 1", "ZYSoftSign"));
                string billNo = string.Format(@"Si{0}", sign.BillNo);

                string _serialNo = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT TOP 1 RIGHT(CONCAT( '00',(CONVERT(INT,SUBSTRING(DisplayBillNo,LEN(DisplayBillNo)-2,3))+1 )) , 3) 
                            FROM dbo.ZYSoftSign WHERE [Date] ='{0}' ORDER BY id  DESC ", DateTime.Now.ToString("yyyy-MM-dd")));
                if (string.IsNullOrEmpty(_serialNo))
                {
                    _serialNo = "001";
                }
                string displayBillNo = string.Format(@"Si{0}", DateTime.Now.ToString("yyyyMMdd") + _serialNo); ;
                sqls.Add(string.Format(@"
                 INSERT INTO dbo.ZYSoftSign
                         ( Id,
                           BillType,
                           BillNo ,
                           Date ,
                           RequiredDate,
                           PartnerId ,
                           PoBillId ,
                           BillerId , 
                           Status ,
                           IsDeleted ,
                           CreatedOn ,
                           CreatedByUserGuid ,
                           CreatedByUserName ,DisplayBillNo
                         )
                 VALUES  ( '{0}' , -- Id - varchar(50)
                           '{1}' , -- BillType - varchar(50)
                           '{2}' , -- BillNo - varchar(50) 
                           '{3}' , -- Date - datetime
                           '{4}' , -- RequiredDate - datetime
                           '{5}' , -- PartnerId - int
                           '{6}' , -- PoBillId - int
                           '{7}' , -- BillerId - int
                           {8}, -- Status - int
                           0 , -- IsDeleted - int
                           GETDATE() , -- CreatedOn - datetime2
                           '{9}' , -- CreatedByUserGuid - uniqueidentifier
                           N'{10}','{11}' -- CreatedByUserName - nvarchar(max)
                         )", billId, BillType.Sign.GetHashCode(), billNo, sign.Date, sign.RequiredDate, sign.PartnerId, sign.PoBillId,
         sign.BillerId, AuditStatus.Ing.GetHashCode(),
         AuthContextService.CurrentUser.Guid,
         AuthContextService.CurrentUser.DisplayName, displayBillNo));
                signEntry.ForEach(item =>
                {
                    sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftSignEntry
                            ( BillId ,
                              InvId ,
                              DeptId ,
                              OrderId,
                              OrderEntryId,
                              IsDiff ,
                              Remark ,
                              Quantity ,
                              Price ,
                              FactQuantity,
                              Amount,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                              '{11}' , -- DeptId - varchar(20)
                             '{8}' , -- OrderId - varchar(20)
                             '{9}' , -- OrderEntryId - varchar(20)
                             '{10}' , -- IsDiff - varchar(20)
                             '{2}' , -- Remark - varchar(100) 
                              {3} , -- Quantity - decimal
                              {6} , -- Pirce - decimal
                              {7} , -- FactQuantity - decimal
                              {12} , -- Amount - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{4}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{5}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.Remark, item.Quantity,
                             AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName, item.Price,
                             item.FactQuantity, item.OrderId, item.OrderEntryId, item.IsDiff, item.DeptId, item.Price * item.FactQuantity));
                });

                sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftSignLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }

            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/bus/sign/modify")]
        public IActionResult ModifySign(SignCreateModel model)
        {
            ZYSoftSign sign = model.sign;
            List<ZYSoftSignEntry> signEntry = model.signEntry;
            List<TotalSignEntry> context = model.context;
            /*
             * 这里保存业务的时候会同时写四张表
             * 1.ZYSoftSubscribe
             * 2.ZYSoftSubscribeEntry
             * 3.ZYSoftAuditRecord
             * 4.ZYSoftSubscribeLog
             */
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSign 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", sign.Id)))
                {
                    response.SetError("单据已经审批,无法修改!");
                }
                else
                {
                    List<string> sqls = new List<string>();
                    string billId = sign.Id.ToString();

                    sqls.Add(string.Format(@"DELETE FROM ZYSoftSignEntry WHERE BillId = '{0}'", billId));

                    sqls.Add(string.Format(@"UPDATE dbo.ZYSoftPo SET Date='{5}',PartnerId={4},RequiredDate='{3}',ModifiedOn = GETDATE(),ModifiedByUserGuid = '{1}',
                ModifiedByUserName = '{2}' WHERE Id = '{0}'", billId, AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName, sign.RequiredDate, sign.PartnerId, sign.Date));

                    signEntry.ForEach(item =>
                    {
                        sqls.Add(string.Format(@"
                     INSERT INTO dbo.ZYSoftSignEntry
                            ( BillId ,
                              InvId ,
                              DeptId ,
                              OrderId,
                              OrderEntryId,
                              IsDiff,
                              Remark ,
                              Quantity ,
                              Price ,
                              FactQuantity,
                              Amount,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              '{1}' , -- InvId - varchar(20)
                              '{11}' , -- InvId - varchar(20)
                             '{8}' , -- OrderId - varchar(20)
                             '{9}' , -- OrderEntryId - varchar(20)
                              '{10}' , -- IsDiff - varchar(100)
                              '{2}' , -- Remark - varchar(100)
                              {3} , -- Quantity - decimal
                              {4} , -- Pirce - decimal
                              {5} , -- FactQuantity - decimal
                              {12} , -- FactQuantity - decimal
                              GETDATE() , -- CreatedOn - datetime2
                              '{6}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{7}' -- CreatedByUserName - nvarchar(max)
                            )", billId, item.InvId, item.Remark, item.Quantity,
                            item.Price, item.FactQuantity
                            , AuthContextService.CurrentUser.Guid,
                            AuthContextService.CurrentUser.DisplayName,
                            item.OrderId, item.OrderEntryId, item.IsDiff, item.DeptId, item.Price * item.FactQuantity));
                    });

                    sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftSignLog
                            ( BillId ,
                              Context ,
                              CreatedOn ,
                              CreatedByUserGuid ,
                              CreatedByUserName
                            )
                    VALUES  ( {0} , -- BillId - int
                              N'{1}' , -- Context - nvarchar(max)
                              GETDATE() , -- CreatedOn - datetime2
                              '{2}' , -- CreatedByUserGuid - uniqueidentifier
                              N'{3}' -- CreatedByUserName - nvarchar(max)
                            )", billId, JsonConvert.SerializeObject(model.context), AuthContextService.CurrentUser.Guid,
                             AuthContextService.CurrentUser.DisplayName));
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);

                    if (effectRow > 0) { response.SetSuccess("保存成功!"); } else { response.SetError("保存失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError(e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/sign/del")]
        public IActionResult DelSign(dynamic model)
        {
            string id = model.id;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSign 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("申请已经审批,无法作废!");
            }
            else
            {
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(
                    string.Format(@"UPDATE ZYSoftSign set IsDeleted =1 Where Id ={0}", id));
                if (effectRow > 0)
                {
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetSuccess("操作失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/sign/audit")]
        public IActionResult AuditSign(dynamic model)
        {
            string id = model.id;
            string partnerId = model.partnerId;
            string userId = model.userId;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSign 
                    WHERE Id ={0} AND ISNULL(Status,0) = 1", id)))
            {
                response.SetError("单据已经审批,无须多次操作!");
            }
            else
            {
                //允许多签收
                //if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM (SELECT * FROM dbo.ZYSoftSignEntry WHERE BillId = {0} )t1 LEFT JOIN 
                //(SELECT * FROM dbo.ZYSoftPoEntry WHERE BillId = {1}) t2 ON t1.InvId = t2.Id
                //    WHERE   t2.FinishQuantity+t1.Quantity >t2.Quantity", id, poId)))
                //{
                List<string> ls = new List<string>();
                ls.Add(string.Format(@"UPDATE ZYSoftSign set Status =1,AuditerId='{1}',AuditDate=GETDATE() Where Id ={0}", id, userId));
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                    string.Format(@"SELECT InvId,FactQuantity,OrderId,OrderEntryId FROM dbo.ZYSoftSignEntry WHERE BillId = {0}", id));
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ls.Add(string.Format(@"UPDATE dbo.ZYSoftPoEntry SET FinishQuantity = FinishQuantity +{0} WHERE InvId ={1} AND BillId = {2} AND Id = {3}",
                            dr["FactQuantity"], dr["InvId"], dr["OrderId"], dr["OrderEntryId"]));
                    }
                }
                var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                if (effectRow > 0)
                {
                    if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT 1 FROM dbo.ZYSoftSign t1 LEFT JOIN dbo.ZYSoftSignEntry 
                        t2 ON t1.Id = t2.BillId WHERE t1.Id ={0} AND ISNULL(t2.IsDiff,0) =1", id)))
                    {
                        DataTable dtOpenIds = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT OpenId FROM dbo.ZYSoftAuditConfig t1 LEFT JOIN  
                    dbo.ZYSoftUserWcMapping t2 ON t1.UserGuid = t2.UserGuid"));
                        if (dtOpenIds != null && dtOpenIds.Rows.Count > 0)
                        {
                            string content = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT '采购订单出现缺货!|'+t1.DisplayBillNo+'|'+t3.name+'|'+'-'+'|'+'请尽快安排重新采购！' FROM dbo.ZYSoftSign t1 LEFT JOIN dbo.ZYSoftSignEntry t2 ON t1.Id = t2.BillId
					LEFT JOIN  dbo.vPartner t3 ON t1.PartnerId = t3.id WHERE t1.id = {0}", id));

                            foreach (DataRow dr in dtOpenIds.Rows)
                            {
                                string openId = Convert.ToString(dr["OpenId"]);
                                if (openId != "")
                                {
                                    string errMsg = "";
                                    SendWechatMsg(MsgType.SignNotEnough.GetHashCode(), openId, content, "id=" + id + "&partnerId=" + partnerId, ref errMsg);
                                }
                            }
                        }
                    }
                    response.SetSuccess("操作成功!");
                }
                else
                {
                    response.SetError("操作失败!");
                }
                //}
                //else
                //{
                //    response.SetError("签收数量检查不合法,审批失败!");
                //}
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/sign/unaudit")]
        public IActionResult UnAuditSign(dynamic model)
        {
            string id = model.id;
            string userId = model.userId;
            string poId = model.poId;
            var response = ResponseModelFactory.CreateResultInstance;
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSign 
                    WHERE Id ={0} AND ISNULL(Status,0) = 0", id)))
            {
                response.SetError("单据尚未审批,无须多次操作!");
            }
            else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftSign 
                    WHERE Id ={0} AND ISNULL(IsSync,0) = 1", id)))
            {
                response.SetError("单据已同步,禁止操作!");
            }
            else
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM (SELECT * FROM dbo.ZYSoftSignEntry WHERE BillId = {0} )t1 LEFT JOIN 
                (SELECT * FROM dbo.ZYSoftPoEntry WHERE BillId = {1}) t2 ON t1.InvId = t2.Id
                    WHERE  t2.FinishQuantity-t1.Quantity < t2.Quantity", id, poId)))
                {
                    List<string> ls = new List<string>();
                    ls.Add(string.Format(@"UPDATE ZYSoftSign set Status =0,AuditerId= -1,AuditDate=NULL Where Id ={0}", id, userId));
                    DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                        string.Format(@"SELECT InvId,FactQuantity,OrderId,OrderEntryId FROM dbo.ZYSoftSignEntry WHERE BillId = {0}", id));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            ls.Add(string.Format(@"UPDATE dbo.ZYSoftPoEntry SET FinishQuantity = FinishQuantity - {0} WHERE InvId ={1} AND BillId = {2} AND Id = {3}",
                                dr["FactQuantity"], dr["InvId"], dr["OrderId"], dr["OrderEntryId"]));
                        }
                    }
                    var effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                    if (effectRow > 0)
                    {
                        response.SetSuccess("操作成功!");
                    }
                    else
                    {
                        response.SetError("操作失败!");
                    }
                }
                else
                {
                    response.SetError("订单数量检查不合法,反审批失败!");
                }
            }
            return Ok(response);
        }
        #endregion

        #region 备货单
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/bh/list")]
        public IActionResult GetBHList(int type, int partnerId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch (type)
            {
                case 0:
                case 1:
                    str = string.Format(@"select * from vPoEntry where  CAST([Date] as Date)= CAST(GETDATE() as Date) AND ISNULL(Status,0) =1 AND  PartnerId = {0} ORDER BY RequiredDate desc",
                        partnerId);
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            response.SetData(query);
            return Ok(response);
        }
        #endregion

        #region 基础资料
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/cls/list")]
        public IActionResult GetClsList()
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT * FROM dbo.vInventoryCls ORDER BY id"));
            var list = DataTableConvert.ToList<vInventoryCls>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/inv/list")]
        public IActionResult GetInvList(string clsId)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vInventory where idinventoryclass='{0}' ORDER BY idinventoryclass", clsId));
            var list = DataTableConvert.ToList<vInventory>(query);
            response.SetData(list);
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/inv/listbykeyword")]
        public IActionResult SearchInv(string keyword)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vInventory where code LIKE '%{0}%' OR name LIKE '%{0}%' OR chinaname LIKE '%{0}%'", keyword));
            var list = DataTableConvert.ToList<vInventory>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/dept/list")]
        public IActionResult GetDeptList()
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT * FROM dbo.vDepartment ORDER BY code"));
            var list = DataTableConvert.ToList<vDepartment>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/partner/list")]
        public IActionResult GetPartnerList(string keyword)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.IsNullOrEmpty(keyword) ?
                string.Format(@"SELECT * FROM dbo.vPartner ORDER BY code") :
                string.Format(@"SELECT* FROM dbo.vPartner WHERE code LIKE '%{0}%' OR name LIKE '%{0}%' OR chinaname LIKE '%{0}%' ORDER BY code", keyword));
            var list = DataTableConvert.ToList<vPartner>(query);
            response.SetData(list);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/base/list")]
        public IActionResult GetBaseList(int type, int notbind = 0)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var str = "";
            switch ((BaseType)type)
            {
                case BaseType.vInventory:
                    str = string.Format(@"SELECT * FROM dbo.vInventory");
                    break;
                case BaseType.vInventoryCls:
                    str = string.Format(@"SELECT * FROM dbo.vInventoryCls");
                    break;
                case BaseType.vPartner:
                    if (notbind == 0)
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPartner");
                    }
                    else
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPartner WHERE id NOT IN (SELECT PartnerId FROM dbo.ZYSoftUserPartnerMapping)");
                    }
                    break;
                case BaseType.vPerson:
                    if (notbind == 0)
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPerson");
                    }
                    else
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPerson WHERE id NOT IN (SELECT PersonId FROM dbo.ZYSoftUserPersonalMapping)");
                    }
                    break;
                case BaseType.vDepartment:
                    str = string.Format(@"SELECT * FROM dbo.vDepartment");
                    break;
            }
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            //var list = DataTableConvert.ToList<dynamic>(query);
            response.SetData(query);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bus/pc/list")]
        public IActionResult GetPartnerCls(string date = "")
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(string.IsNullOrEmpty(date) ? @"SELECT * FROM dbo.vInventoryClassPartner" : @"
                    SELECT * FROM dbo.vInventoryClassPartner where idinventoryclass in (
                     select distinct t2.idinventoryclass from vSubscribeEntry t1 left join vInventory 
                        t2 on t1.InvId = t2.id  where isnull(Status,-1)  =1 and  date ='{0}')", date));
            response.SetData(query);
            return Ok(response);
        }

        #endregion

        #region 绑定
        [HttpPost("/api/v1/bus/bind/wc")]
        public IActionResult UserBindWechat(dynamic model)
        {
            string userId = model.userId;
            string openId = model.openId;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserWcMapping WHERE UserGuid = '{0}' AND OpenId <> '{1}'", userId, openId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了一个微信");
                }
                else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserWcMapping WHERE UserGuid = '{0}' AND OpenId = '{1}'", userId, openId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了此微信");
                }
                else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserWcMapping WHERE  OpenId = '{0}'", openId)))
                {
                    response.SetError("绑定失败!原因：此微信已经绑定了别的账号,请先解绑!");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"
                        INSERT INTO dbo.ZYSoftUserWcMapping
                            ( UserGuid, OpenId, CreatedOn )
                    VALUES  ( '{0}', -- UserGuid - uniqueidentifier
                              '{1}', -- OpenId - varchar(100)
                              GETDATE()  -- CreateOn - datetime2
                              )", userId, openId));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("绑定成功!");
                    }
                    else { response.SetError("执行绑定发生错误,绑定失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("绑定失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/unbind/wc")]
        public IActionResult UserUnBindWechat(dynamic model)
        {
            string userId = model.userId;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserWcMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("解绑失败!原因：没有查询到绑定记录,请核实!");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"
                         DELETE FROM dbo.ZYSoftUserWcMapping WHERE UserGuid  ='{0}'", userId));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("解绑成功!");
                    }
                    else { response.SetError("执行解绑发生错误,解绑失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("解绑失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/bind/person")]
        public IActionResult UserBindPenson(dynamic model)
        {
            string userId = model.userId;
            string personId = model.personId;
            string personName = model.personName;
            string deptId = model.deptId;
            string deptName = model.deptName;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPersonalMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了一个职员档案");
                }
                else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了供应商档案!");
                }
                else
                {
                    List<string> ls = new List<string>();
                    ls.Add(string.Format(@"
                        INSERT INTO dbo.ZYSoftUserPersonalMapping
                            (  UserGuid ,PersonId ,DeptId ,DeptName ,CreatedOn )
                    VALUES  ( '{0}', -- UserGuid - uniqueidentifier
                              '{1}', -- PersonId - varchar(100)
                              '{2}', -- DeptId - varchar(100)
                              '{3}', -- DeptName - varchar(100)
                              GETDATE()  -- CreateOn - datetime2
                              )", userId, personId, deptId, deptName));
                    //ls.Add(string.Format(@"UPDATE dbo.DncUser SET DisplayName = '{1}' WHERE [Guid]= '{0}'", userId, personName));
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                    if (effectRow > 0)
                    {
                        response.SetSuccess("绑定成功!");
                    }
                    else { response.SetError("执行绑定发生错误,绑定失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("绑定失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/bind/persondept")]
        public IActionResult UserBindPensonDept(dynamic model)
        {
            string userId = model.userId;
            string personId = model.personId;
            string deptId = model.deptId;
            string deptName = model.deptName;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {

                List<string> ls = new List<string>();
                ls.Add(string.Format(@"
                        UPDATE dbo.ZYSoftUserPersonalMapping SET DeptId ='{2}',DeptName = '{3}' WHERE UserGuid = '{0}' AND PersonId ='{1}'", userId,
                        personId, deptId, deptName));
                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);
                if (effectRow > 0)
                {
                    response.SetSuccess("设置成功!");
                }
                else { response.SetError("执行设置发生错误,设置失败!"); }

            }
            catch (Exception e)
            {
                response.SetError("设置失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/unbind/person")]
        public IActionResult UserUnBindPerson(dynamic model)
        {
            string userId = model.userId;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPersonalMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("解绑失败!原因：没有查询到绑定记录,请核实!");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"
                         DELETE FROM dbo.ZYSoftUserPersonalMapping WHERE UserGuid  ='{0}'", userId));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("解绑成功!");
                    }
                    else { response.SetError("执行解绑发生错误,解绑失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("解绑失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/bind/partner")]
        public IActionResult UserBindPartner(dynamic model)
        {
            string userId = model.userId;
            string partnerId = model.partnerId;
            string partnerName = model.partnerName;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了一个供应商档案");
                }
                else if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPersonalMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("绑定失败!原因：您的账号已经绑定了职员档案");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"
                        INSERT INTO dbo.ZYSoftUserPartnerMapping
                            (  UserGuid ,PartnerId ,PartnerName ,CreatedOn )
                    VALUES  ( '{0}', -- UserGuid - uniqueidentifier
                              '{1}', -- PartnerId - varchar(100)
                              '{2}', -- PartnerName - varchar(100)
                              GETDATE()  -- CreateOn - datetime2
                              )", userId, partnerId, partnerName));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("绑定成功!");
                    }
                    else { response.SetError("执行绑定发生错误,绑定失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("绑定失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/bus/unbind/partner")]
        public IActionResult UserUnBindPartner(dynamic model)
        {
            string userId = model.userId;
            var response = ResponseModelFactory.CreateResultInstance;
            try
            {
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid = '{0}'", userId)))
                {
                    response.SetError("解绑失败!原因：没有查询到绑定记录,请核实!");
                }
                else
                {
                    int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"
                         DELETE FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid  ='{0}'", userId));
                    if (effectRow > 0)
                    {
                        response.SetSuccess("解绑成功!");
                    }
                    else { response.SetError("执行解绑发生错误,解绑失败!"); }
                }
            }
            catch (Exception e)
            {
                response.SetError("解绑失败!原因：" + e.Message);
            }
            return Ok(response);
        }

        [HttpGet("/api/v1/bus/bdlist/list")]
        public IActionResult GetBindPersonList(string type)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = "";
            switch (type)
            {
                case "0":
                    query =
                string.Format(@"SELECT [Guid] AS [UserGuid],DisplayName FROM dbo.DncUser 
                WHERE [Guid] NOT IN (SELECT UserGuid FROM dbo.ZYSoftUserPartnerMapping)
                AND [Guid] NOT IN (SELECT UserGuid FROM dbo.ZYSoftUserPersonalMapping) AND ISNULL(IsDeleted,0)=0");
                    break;
                case "1":
                    query = string.Format(@"SELECT t1.UserGuid,t2.DisplayName,t1.PartnerName FROM dbo.ZYSoftUserPartnerMapping t1 
                LEFT JOIN dbo.DncUser t2 ON t1.UserGuid = t2.[Guid] WHERE ISNULL(t2.IsDeleted,0)=0 ORDER BY t1.CreatedOn");
                    break;
                case "2":
                    query =
                string.Format(@"SELECT t1.UserGuid,t2.DisplayName,t1.DeptName FROM dbo.ZYSoftUserPersonalMapping t1 
                LEFT JOIN dbo.DncUser t2 ON t1.UserGuid = t2.[Guid] WHERE ISNULL(t2.IsDeleted,0)=0 ORDER BY t1.CreatedOn");
                    break;
            }
            var dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(query);
            response.SetData(dt);
            return Ok(response);
        }
        #endregion

        #region 每日清单
        [HttpPost("/api/v1/bus/rpt/daylist")]
        public IActionResult DayList(dynamic model)
        {
            string partnerId = model.partnerId;
            string date = model.date;
            int flag = model.flag;
            string[] startTimes = { "07:00:00", "18:59:59" };
            string[] endTimes = { "19:00:00", "23:59:59" };
            string startTime = string.Format(@"{0} {1}", date, startTimes[flag]);
            string endTime = string.Format(@"{0} {1}", date, endTimes[flag]);
            var response = ResponseModelFactory.CreateResultInstance;
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            string sql = string.Format(@" SELECT '{3}' RequiredDate, t1.*,t2.name AS InvName,t2.Specification,ISNULL(t2.unitname,t2.unitname2)UnitName,t3.name AS DeptName FROM (
                    SELECT  InvId,DeptId,SUM(Quantity-ISNULL(FinishQuantity,0)) AS Qty from dbo.vPoEntry WHERE PartnerId ='{0}'
                    AND ISNULL(Status,0) =1 AND ISNULL(IsDeleted,0) = 0 AND CreatedOn  >= '{1}' AND  CreatedOn <='{2}' GROUP BY deptId,InvId) t1 
                    LEFT JOIN  dbo.t_Inventory t2 ON t1.InvId = t2.id 
                    LEFT JOIN  dbo.t_Department t3 ON t1.deptId = t3.id", partnerId, startTime, endTime, date);
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(sql);
            response.SetData(query);

            return Ok(response);
        }

        [HttpPost("/api/v1/bus/rpt/subdaylist")]
        public IActionResult SubDayList(dynamic model)
        {
            string date = model.date;
            string deptId = model.deptId;
            var response = ResponseModelFactory.CreateResultInstance;
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            string sql = string.Format(@"SELECT Date AS RequiredDate,InvId,Name AS InvName,UnitName,Specification,Count AS Qty,
                DeptName FROM dbo.vSubSummeryRpt WHERE [Date]='{0}' AND DeptId ='{1}' ORDER BY InvId", date, deptId);
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(sql);
            response.SetData(query);

            return Ok(response);
        }
        #endregion

        /// <summary>
        /// 申购单审批之后，将申购明细汇总到汇总临时表
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="id"></param>
        /// <param name="billTypeId"></param>
        /// <param name="pi"></param>
        public int SummaryData(List<string> ls, string id, string billTypeId, int pi = 1)
        {
            if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT * FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND [No]>(
                SELECT[No] FROM dbo.ZYSoftAuditRecord WHERE BillId = {0} AND BillTypeId={1} AND UserGuid = '{2}')", id, billTypeId,
                     AuthContextService.CurrentUser.Guid)))
            {
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT CONVERT(VARCHAR(10),Date,23)[date],InvId id,code,name,_unitname unitname,specification,
                    SUM(Quantity) * {1} AS[count],[DeptId] FROM dbo.vSubscribeEntry WHERE Id='{0}' 
                    GROUP BY InvId,Date,code,name,_unitname,specification,DeptId", id, pi));
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ls.Add(string.Format(@"IF EXISTS(SELECT 1 FROM ZYSoftSubSummary WHERE [Date]='{0}' AND [Id] ='{1}' AND [DeptId] = '{7}')
                        BEGIN
	                        UPDATE dbo.ZYSoftSubSummary SET [Count] = [Count] + {6} WHERE [Date]='{0}' AND [Id] ='{1}' AND [DeptId] = '{7}'
                        END
                        ELSE
                        BEGIN
	                        INSERT INTO dbo.ZYSoftSubSummary
	                                ( Date ,
	                                  Id ,
	                                  Name ,
	                                  Code ,
	                                  Unitname ,
	                                  Specification ,
	                                  Count ,
	                                  FinishCount,
                                      DeptId
	                                )
	                        VALUES  ( '{0}' , -- Date - varchar(10)
	                                  {1} , -- Id - int
	                                  '{2}' , -- Name - varchar(50)
	                                  '{3}' , -- Code - varchar(50)
	                                  '{4}' , -- Unitname - varchar(50)
	                                  '{5}' , -- Specification - varchar(50)
	                                  {6} , -- Count - decimal
	                                  0,  -- FinishCount - decimal
                                      '{7}' -- DeptId - int
	                                )
                        END", dr["Date"], dr["Id"], dr["Name"],
                            dr["Code"], dr["Unitname"], dr["Specification"], dr["Count"], dr["DeptId"]));
                    }
                }
            }

            return ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls);

        }

        public bool SendWechatMsg(int id, string openId, string content, string query, ref string errMsg)
        {
            try
            {
                errMsg = "";
                string guid = Guid.NewGuid().ToString();
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.ZYSoftWcMsgLog
				(
				    id,
				    BillType,
				    OpenId,
				    SendContent,
				    SendTime, 
				    Query
				)
				VALUES
				(   '{0}',      -- id - uniqueidentifier
				    '{1}',         -- BillType - int
				    '{2}',        -- OpenId - varchar(50)
				    N'{3}',       -- SendContent - nvarchar(max)
				    GETDATE(), -- SendTime - datetime 
				    '{4}'         -- Query - varchar(max)
				    )", guid, id, openId, content, query));
                if (new AppHelper(_hostingEnvironment)
                    .PushMsg(id, openId, content, query, ref errMsg))
                {
                    errMsg = "发送成功!";
                    ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"UPDATE dbo.ZYSoftWcMsgLog SET CallBackTime = GETDATE(),Success=1 WHERE id = '{0}'", guid));
                    return true;
                }
                else
                {
                    ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"UPDATE dbo.ZYSoftWcMsgLog SET CallBackTime = GETDATE(),Success=0,Msg='{1}' WHERE id = '{0}'", guid, errMsg));
                }
                return false;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }

        public string CheckVaild(string startDate, string endDate, string invId, string partnerId)
        {
            string invName = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT t3.name+'('+t3.code+')' FROM ZYSoftInquiry t1 LEFT JOIN ZYSoftInquiryEntry t2 ON t1.Id = t2.BillId 
                        LEFT JOIN vInventory t3 ON t2.InvId = t3.id WHERE  ISNULL(t1.Status,0) =1
                        AND t2.InvId = '{0}' AND t2.PartnerId ='{1}' AND t1.StartDate <= '{2} 00:00:00' AND t1.EndDate >= '{3} 23:59:59'",
                          invId, partnerId, startDate, endDate));
            return string.IsNullOrEmpty(invName) ? "" : invName;
        }

        public string CheckInvHaveOrder(string startDate, string endDate, string invId, string partnerId)
        {
            string invName = ZYSoft.DB.BLL.Common.ExecuteScalar(string.Format(@"SELECT t3.name+'('+t3.code+')' FROM dbo.ZYSoftPo t1 LEFT JOIN dbo.ZYSoftPoEntry t2 ON t1.Id = t2.BillId 
                        LEFT JOIN vInventory t3 ON t2.InvId = t3.id WHERE  ISNULL(t1.Status,0) =1
                        AND t2.InvId = '{0}' AND t1.PartnerId ='{1}' AND t1.Date <= '{2} 00:00:00' AND t1.Date >= '{3} 23:59:59'",
                          invId, partnerId, startDate, endDate));
            return string.IsNullOrEmpty(invName) ? "" : invName;
        }
    }
}
