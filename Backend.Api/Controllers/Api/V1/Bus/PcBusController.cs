using Backend.Api.Entities;
using Backend.Api.Extensions;
using Backend.Api.Extensions.AuthContext;
using Backend.Api.Extensions.CustomException;
using Backend.Api.RequestPayload.PcBus.Ask;
using Backend.Api.RequestPayload.PcBus.Report;
using Backend.Api.RequestPayload.PcBus.Base;
using Backend.Api.RequestPayload.PcBus.Bind;
using Backend.Api.ViewModels.Bus.Inquiry;
using Backend.Api.ViewModels.Bus.Subscribe;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.Controllers.Api.V1.Bus
{
    [Route("api/v1/pcbus/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class PcBusController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        public PcBusController(DncZeusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region 询价单   

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("/api/v1/pcbus/ask/list")]
        public IActionResult List(AskRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" BillNo Like '%{0}%' AND ", payload.Kw);
            }

            if (payload.IsDeleted > Entities.Enums.CommonEnum.IsDeleted.All)
            {
                sqlWhere += string.Format(@" ISNULL(IsDeleted,0) = {0} AND ",
                    payload.IsDeleted.GetHashCode());
            }

            if (payload.Status > Entities.Enums.CommonEnum.Status.All)
            {
                sqlWhere += string.Format(@" ISNULL(Status,0) = {0}",
                    payload.Status.GetHashCode());
            }
            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }
            if (sqlWhere.EndsWith(" AND "))
            {
                sqlWhere = sqlWhere[0..^5];
            }
            DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"select * from vInquiryList where 1=1 {0}
                order by Id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize));

            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"select count(1) from vInquiryList where 1=1 {0} ",
                sqlWhere));
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);

        }

        /// <summary>
        /// 供应商查看询价单
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/pcbus/ask/plist")]

        public IActionResult ListForPartner(AskRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" BillNo Like '%{0}%' AND ", payload.Kw);
            }

            if (payload.IsDeleted > Entities.Enums.CommonEnum.IsDeleted.All)
            {
                sqlWhere += string.Format(@" ISNULL(IsDeleted,0) = {0} AND ",
                    payload.IsDeleted.GetHashCode());
            }

            if (payload.Status > Entities.Enums.CommonEnum.Status.All)
            {
                sqlWhere += string.Format(@" ISNULL(Status,0) = {0}",
                    payload.Status.GetHashCode());
            }
            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }
            if (sqlWhere.EndsWith(" AND "))
            {
                sqlWhere = sqlWhere[0..^5];
            }

            DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"select * from vInquiryList where 1=1 AND ID IN (SELECT DISTINCT BillId FROM dbo.ZYSoftInquiryEntry WHERE
                    EXISTS (SELECT TOP 1 PartnerId FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid='{3}')) {0}
                order by Id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize, AuthContextService.CurrentUser.Guid));

            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"select count(1) from vInquiryList where 1=1 AND ID IN (SELECT DISTINCT BillId FROM dbo.ZYSoftInquiryEntry WHERE
                    EXISTS (SELECT TOP 1 PartnerId FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid='{1}')) {0} ",
                sqlWhere, AuthContextService.CurrentUser.Guid));
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);

        }

        [HttpPost("/api/v1/pcbus/ask/delmulit")]
        public IActionResult Delete(dynamic model)
        {
            string ids = model.ids;
            var response = ResponseModelFactory.CreateResultInstance;
            if (string.IsNullOrEmpty(ids))
            {
                response.SetError("没有指定要删除的订单!");
            }
            else
            {
                List<string> sqls = new List<string>();
                string[] array = ids.Split(',');
                foreach (string id in array)
                {
                    sqls.Add(string.Format(@" 
                        IF EXISTS(SELECT 1 FROM dbo.ZYSoftInquiry WHERE id ='{0}' AND ISNULL(Status,0)=0)
                        BEGIN
		                        DELETE FROM dbo.ZYSoftInquiry WHERE Id='{0}' AND ISNULL([Status],0)= 0
		                        DELETE FROM dbo.ZYSoftInquiryEntry WHERE BillId='{0}'
		                        DELETE FROM dbo.ZYSoftInquiryLog WHERE BillId='{0}'
                        END", id));
                }

                int effectRow = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);
                if (effectRow > 0)
                {
                    response.SetSuccess("删除成功!");
                }
                else
                {
                    response.SetError("删除失败!");
                }
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/ask/confirm")]
        public IActionResult Confirm(List<PriceCurrentConfirm> list)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            if (list.Count <= 0)
            {
                response.SetError("没有要保存的数据!");
            }
            else
            {
                List<string> sqls = new List<string>();
                list.ForEach(f =>
                {
                    sqls.Add(string.Format(@"UPDATE dbo.ZYSoftInquiryEntry SET PriceCurrentConfirm = {3} WHERE  BillId ={0} AND PartnerId = {1} AND InvId ={2}",
                        f.BillId, f.PartnerId, f.InvId, f.ConfirmPrice));
                });

                int effectRows = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);
                if (effectRows > 0)
                {
                    response.SetSuccess("保存成功!");
                }
                else
                {
                    response.SetError("保存失败!");
                }
            }
            return Ok(response);
        }

        [HttpGet("/api/v1/pcbus/ask/periodisvalid")]
        public IActionResult CheckPeriodIsVaild(int clsId, string startDate, string endDate)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            response.SetData("");
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clsId"></param> 
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/pcbus/ask/check")]
        public IActionResult GetInvLastPrice(int clsId, int billId = -1)
        {
            /*
             * 1.根据大类ID，取出全部存货
             * 2.根据大类ID，取出全部供应商
             * 3.循环存货 和  供应商，取出 当前存货在不同供应商下，上个报价期的价格
             */
            var response = ResponseModelFactory.CreateResultInstance;

            string str = string.Format(@"SELECT * FROM dbo.vInventory WHERE idinventoryclass= '{0}'", clsId);
            DataTable dtInvs = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);

            str = string.Format(@" SELECT idParent FROM dbo.vInventoryClassPartner WHERE idinventoryclass = '{0}'", clsId);
            DataTable dtPartners = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            string partnerId, invId;

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            if (dtInvs != null && dtInvs.Rows.Count > 0)
            {
                if (dtPartners != null && dtPartners.Rows.Count > 0)
                {
                    foreach (DataRow drInv in dtInvs.Rows)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        invId = Convert.ToString(drInv["id"]);
                        dic.Add("id", invId);
                        dic.Add("code", Convert.ToString(drInv["code"].ToString()));
                        dic.Add("name", Convert.ToString(drInv["name"].ToString()));
                        dic.Add("unitname", Convert.ToString(drInv["unitname"].ToString()));
                        dic.Add("unitname2", Convert.ToString(drInv["unitname2"].ToString()));
                        foreach (DataRow drPartner in dtPartners.Rows)
                        {
                            partnerId = Convert.ToString(drPartner["idParent"]);
                            PriceModel model = GetInvLastPrice(partnerId, invId, billId);
                            if (model != null)
                            {
                                dic.Add("p_last_price_" + partnerId, model.PriceCurrent.ToString());
                                dic.Add("p_last_confirm_price_" + partnerId, model.PriceCurrentConfirm.ToString());
                                //dic.Add("p_market_price_" + partnerId, "0");
                                //if (model.PriceCurrentConfirm.ToString().Equals("0") &&
                                //    !model.PriceCurrent.ToString().Equals("0"))
                                //{
                                //    dic.Add("p_current_price_" + partnerId, model.PriceCurrent.ToString());
                                //}
                                //else
                                //{
                                //    dic.Add("p_current_price_" + partnerId, model.PriceCurrentConfirm.ToString());
                                //}
                            }
                            else
                            {
                                dic.Add("p_last_price_" + partnerId, "0");
                                dic.Add("p_last_confirm_price_" + partnerId, "0");
                                //dic.Add("p_market_price_" + partnerId, "0");
                                //dic.Add("p_current_price_" + partnerId, "0");
                            }
                        }

                        list.Add(dic);
                    }
                }
            }
            response.SetData(list);
            return Ok(response);
        }

        public PriceModel GetInvLastPrice(string partnerId, string invId, int billId)
        {
            var query = billId < 0 ? ZYSoft.DB.BLL.Common.ExecuteDataTable(
               string.Format(@"EXEC L_P_GetInvPrice '{0}',{1} ", partnerId, invId)) :
                    ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"select PriceLast PriceCurrent,PriceCurrentConfirm PriceCurrentConfirm from 
                    ZYSoftInquiryEntry where BillId= {0} and PartnerId = {1} and InvId = {2}", billId, partnerId, invId));
            var list = DataTableConvert.ToList<PriceModel>(query);

            if (list != null && list.Count > 0)
            {
                return list.FirstOrDefault();
            }
            else
            {
                return default;
            }
        }

        public PriceModel GetInvLastPrice(string partnerId, string invId, string startDate, string endDate)
        {
            string lastPrice = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 t2.PriceCurrent FROM dbo.ZYSoftInquiry t1 LEFT JOIN dbo.ZYSoftInquiryEntry t2 ON
                    t1.Id = t2.BillId WHERE ISNULL(t1.IsDeleted,0)=0 AND ISNULL(t1.Status,0)=1 AND t2.PartnerId ='{0}' AND t2.InvId = '{1}' 
                    AND t1.StartDate <= GETDATE() AND t1.EndDate> GETDATE() ", partnerId, invId));

            string currentPrice = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"SELECT TOP 1 t2.PriceCurrent FROM dbo.ZYSoftInquiry t1 LEFT JOIN dbo.ZYSoftInquiryEntry t2 ON
                    t1.Id = t2.BillId WHERE ISNULL(t1.IsDeleted,0)=0 AND ISNULL(t1.Status,0)=1 AND t2.PartnerId ='{0}' AND t2.InvId = '{1}' 
                    AND t1.StartDate = '{2}' AND t1.EndDate = '{3}' ", partnerId, invId, startDate, endDate));

            return new PriceModel()
            {
                PriceCurrent = decimal.Parse(string.IsNullOrEmpty(lastPrice) ? "0" : lastPrice),
                PriceCurrentConfirm = decimal.Parse(string.IsNullOrEmpty(currentPrice) ? "0" : currentPrice),
            };
        }
        #endregion

        #region 申购审批流

        [HttpGet("/api/v1/pcbus/flow/sginfo")]
        public IActionResult GetSgAuditFlowInfo()
        {
            var response = ResponseModelFactory.CreateResultInstance;
            var query = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT * FROM dbo.ZYSoftAuditConfig WHERE BillType =1"));
            response.SetData(query);
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/flow/sg")]
        public IActionResult AuditFlow(List<AuditFlow> list)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            if (list.Count <= 0)
            {
                response.SetError("没有要保存的数据!");
            }
            else
            {
                List<string> sqls = new List<string>
                {
                    string.Format(@"DELETE FROM dbo.ZYSoftAuditConfig WHERE BillType = {0}", BillType.SG.GetHashCode())
                };
                list.ForEach(f =>
                {
                    sqls.Add(string.Format(@"INSERT INTO dbo.ZYSoftAuditConfig
                                        (
                                            BillType,
                                            UserGuid,
                                            UserName,
                                            No,
                                            Remark
                                        )
                                        VALUES
                                        (   {0},    -- BillType - int
                                            '{1}', -- UserGuid - uniqueidentifier
                                            '{2}',   -- UserName - varchar(50)
                                            {3},    -- No - int
                                            ''  -- Remark - varbinary(50)
                                            )", BillType.SG.GetHashCode(), f.UserGuid, f.UserName, f.No));
                });

                int effectRows = ZYSoft.DB.BLL.Common.ExecuteSQLTran(sqls);
                if (effectRows > 0)
                {
                    response.SetSuccess("保存成功!");
                }
                else
                {
                    response.SetError("保存失败!");
                }
            }
            return Ok(response);
        }
        #endregion

        #region 基础资料-分页
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("/api/v1/pcbus/base/list")]
        public IActionResult GetBaseList(BaseRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            string str = "", strCount = "", sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                if (payload.Type != 1)
                {
                    sqlWhere += string.Format(@" code like '%{0}%' or name like '%{0}%' or chinaname like '%{0}%' ", payload.Kw);
                }
                else
                {
                    sqlWhere += string.Format(@" name like '%{0}%' or chinaname like '%{0}%' ", payload.Kw);
                }
            }

            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }

            switch ((BaseType)payload.Type)
            {
                case BaseType.vInventory:
                    str = string.Format(@"SELECT * FROM dbo.vInventory where 1=1 {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                    strCount = string.Format(@"SELECT count(1) FROM dbo.vInventory where 1=1 {0}", sqlWhere);
                    break;
                case BaseType.vInventoryCls:
                    str = string.Format(@"SELECT * FROM dbo.vInventoryCls where 1=1 {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                    strCount = string.Format(@"SELECT count(1) FROM dbo.vInventoryCls where 1=1 {0}", sqlWhere);
                    break;
                case BaseType.vPartner:
                    if (payload.Notbind == 0)
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPartner where 1=1 {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                        strCount = string.Format(@"SELECT count(1) FROM dbo.vPartner where 1=1 {0}", sqlWhere);
                    }
                    else
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPartner WHERE id NOT IN (SELECT PartnerId FROM dbo.ZYSoftUserPartnerMapping) {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);

                        strCount = string.Format(@"SELECT count(1) FROM dbo.vPartner WHERE id NOT IN (SELECT PartnerId FROM dbo.ZYSoftUserPartnerMapping) {0}", sqlWhere);
                    }
                    break;
                case BaseType.vPerson:
                    if (payload.Notbind == 0)
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPerson where 1=1 {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                        strCount = string.Format(@"SELECT count(1) FROM dbo.vPerson where 1=1 {0}", sqlWhere);
                    }
                    else
                    {
                        str = string.Format(@"SELECT * FROM dbo.vPerson WHERE id NOT IN (SELECT PersonId FROM dbo.ZYSoftUserPersonalMapping) {0} 
                order by id offset {1} rows fetch next {2} rows only", sqlWhere, (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                        strCount = string.Format(@"SELECT count(1) FROM dbo.vPerson WHERE id NOT IN (SELECT PersonId FROM dbo.ZYSoftUserPersonalMapping) {0}", sqlWhere);
                    }
                    break;
                case BaseType.vDepartment:
                    str = string.Format(@"SELECT * FROM dbo.vDepartment where 1=1 {0} order by id offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);
                    strCount = string.Format(@"SELECT count(1) FROM dbo.vDepartment where 1=1 {0}", sqlWhere);
                    break;
            }

            var dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(strCount);
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);
        }
        #endregion

        #region 用户绑定
        [HttpPost("/api/v1/pcbus/bind/upm")]
        public IActionResult GetUserPartnerMapping(BindRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            string str = "", strCount = "", sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" LoginName LIKE '%{0}%' OR DisplayName LIKE '%{0}%'  OR PartnerName LIKE '%{0}%'  ", payload.Kw);
            }

            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }

            str = string.Format(@"SELECT * FROM dbo.vUserPartnerMapping where 1=1 {0} order by Guid offset {1} rows fetch next {2} rows only", sqlWhere,
        (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);

            strCount = string.Format(@"SELECT count(1) FROM dbo.vUserPartnerMapping where 1=1 {0}", sqlWhere);


            var dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(strCount);
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/unbind/upm")]
        public IActionResult UnBindUserPartnerMapping(dynamic model)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string userId = model.userId ?? "";
            string partnerId = model.partnerId ?? "";

            int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"DELETE FROM dbo.ZYSoftUserPartnerMapping WHERE UserGuid ='{0}' AND PartnerId = '{1}'", userId, partnerId));
            if (effectRow > 0)
            {
                response.SetSuccess("解绑成功!");
            }
            else
            {
                response.SetError("解绑失败!");
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/bind/upem")]
        public IActionResult GetUserPersonMapping(BindRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            string str = "", strCount = "", sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" LoginName LIKE '%{0}%' OR DisplayName LIKE '%{0}%'  OR DeptName LIKE '%{0}%'  ", payload.Kw);
            }

            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }

            str = string.Format(@"SELECT * FROM dbo.vUserPersonMapping where 1=1 {0} order by Guid offset {1} rows fetch next {2} rows only", sqlWhere,
        (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);

            strCount = string.Format(@"SELECT count(1) FROM dbo.vUserPersonMapping where 1=1 {0}", sqlWhere);


            var dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(strCount);
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/unbind/upem")]
        public IActionResult UnBindUserPersonMapping(dynamic model)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string userId = model.userId ?? "";
            string personId = model.personId ?? "";

            int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"DELETE FROM dbo.ZYSoftUserPersonalMapping WHERE UserGuid ='{0}' AND PersonId = '{1}'", userId, personId));
            if (effectRow > 0)
            {
                response.SetSuccess("解绑成功!");
            }
            else
            {
                response.SetError("解绑失败!");
            }
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/bind/uwm")]
        public IActionResult GetUseWechatMapping(BindRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            string str = "", strCount = "", sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" LoginName LIKE '%{0}%' OR DisplayName LIKE '%{0}%'  OR OpenId LIKE '%{0}%'  ", payload.Kw);
            }

            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }

            str = string.Format(@"SELECT * FROM dbo.vUserWechatMapping where 1=1 {0} order by Guid offset {1} rows fetch next {2} rows only", sqlWhere,
        (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize);

            strCount = string.Format(@"SELECT count(1) FROM dbo.vUserWechatMapping where 1=1 {0}", sqlWhere);


            var dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(str);
            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(strCount);
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);
        }

        [HttpPost("/api/v1/pcbus/unbind/uwm")]
        public IActionResult UnBindUserWechatMapping(dynamic model)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string userId = model.userId ?? "";
            string openId = model.openId ?? "";

            int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(string.Format(@"DELETE FROM dbo.ZYSoftUserWcMapping WHERE UserGuid ='{0}' AND OpenId = '{1}'", userId, openId));
            if (effectRow > 0)
            {
                response.SetSuccess("解绑成功!");
            }
            else
            {
                response.SetError("解绑失败!");
            }
            return Ok(response);
        }
        #endregion

        #region 报表
        [HttpPost("/api/v1/pcbus/rpt/subsummery")]
        public IActionResult SubSummeryRpt(SubSummeryRequestPayload payload)
        {
            var response = ResponseModelFactory.CreateResultInstance;

            string sqlWhere = " AND ";
            if (!string.IsNullOrEmpty(payload.Kw))
            {
                sqlWhere += string.Format(@" Name Like '%{0}%' AND ", payload.Kw);
            }

            if (!string.IsNullOrEmpty(payload.DeptId) && payload.DeptId != "-1")
            {
                sqlWhere += string.Format(@" DeptId ='{0}' AND ", payload.DeptId);
            }

            if (string.IsNullOrEmpty(payload.StartDate))
            {
                sqlWhere += string.Format(@" Date >= CONVERT(VARCHAR(10),dateadd(month, datediff(month, 0, getdate()), 0),23) AND ");
            }
            else
            {
                sqlWhere += string.Format(@" Date >= '{0}' AND ", payload.StartDate);
            }

            if (string.IsNullOrEmpty(payload.EndDate))
            {
                sqlWhere += string.Format(@" Date <= CONVERT(VARCHAR(10),dateadd(month, datediff(month, 0, dateadd(month, 1, getdate())), -1),23) AND ");
            }
            else
            {
                sqlWhere += string.Format(@" Date <= '{0}' AND ", payload.EndDate);
            }

            if (sqlWhere == " AND ")
            {
                sqlWhere = "";
            }

            if (sqlWhere.EndsWith(" AND "))
            {
                sqlWhere = sqlWhere[0..^5];
            }
            DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"select * from vSubSummeryRpt where 1=1 {0}
                order by deptName,code,Date offset {1} rows fetch next {2} rows only", sqlWhere,
                (payload.CurrentPage - 1) * payload.PageSize, payload.PageSize));

            string count = ZYSoft.DB.BLL.Common.ExecuteScalar(
                string.Format(@"select count(1) from vSubSummeryRpt where 1=1 {0} ",
                sqlWhere));
            response.SetData(dt, dt != null && dt.Rows.Count > 0 ? int.Parse(count) : 0);
            return Ok(response);
        }
        #endregion
    }
}
