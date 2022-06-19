using Backend.Api.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Backend.Api
{
    public class AppHelper
    {
        private IHostEnvironment _hostingEnvironment;
        public AppHelper(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        private static string Id = ConfigurationManager.GetSetting("Id");
        private static bool IsTest = Id.Equals("test");

        public bool SetCache(string type, string cache_content, int expires_in, ref string err_msg)
        {
            bool result = false;
            try
            {
                expires_in = expires_in - 5 * 60;//提前五分钟
                string cmdText = "";
                if (!ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT 1 FROM ZYSoftAccessToken WHERE Id ='{0}' AND Type ='{1}'", Id, type)))
                {
                    cmdText = (string.Format(@"INSERT INTO dbo.ZYSoftAccessToken
	                                ( Id,Type ,AccessToken ,SaveTime,ExpireTime) values('{0}','{1}','{2}','{3}','{4}')",
                                   Id, type, cache_content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                   DateTime.Now.AddSeconds(expires_in).ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {

                    cmdText = (string.Format(@"update ZYSoftAccessToken set AccessToken ='{2}',SaveTime = '{3}' ,ExpireTime ='{4}' where Id='{0}' AND Type ='{1}'",
                                                     Id, type, cache_content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                     DateTime.Now.AddSeconds(expires_in).ToString("yyyy-MM-dd HH:mm:ss")));

                }

                int effectRow = ZYSoft.DB.BLL.Common.ExecuteNonQuery(cmdText);

                result = effectRow > 0;

            }
            catch (Exception e)
            {
                err_msg = e.Message;
                WriteLog(err_msg);
            }
            return result;
        }

        public string GetCache(string type, ref string err_msg)
        {
            string cache_content = "";
            try
            {
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT AccessToken FROM dbo.ZYSoftAccessToken WHERE Id ='{0}' AND Type='{1}' AND ExpireTime > GETDATE()", Id, type));
                if (dt != null && dt.Rows.Count > 0)
                {
                    cache_content = dt.Rows[0]["AccessToken"].ToString();
                }
            }
            catch (Exception e)
            {
                err_msg = e.Message;
                WriteLog(err_msg);
            }
            return cache_content;
        }

        public bool GetAuthInfo(string code, string type, ref Dictionary<string, string> res, ref string errMsg)
        {
            res = new Dictionary<string, string>();
            if (ZYSoft.DB.BLL.Common.Exist(string.Format(@"SELECT 1 FROM ZYSoftConfig WHERE Id ='{0}'", Id)))
            {
                Dictionary<string, string> form = new Dictionary<string, string>();
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT AppId,AppSecret FROM dbo.ZYSoftConfig WHERE Id ='{0}'", Id));
                if (dt != null && dt.Rows.Count > 0)
                {
                    form.Add("code", code);
                    form.Add("grant_type", "authorization_code");
                    form.Add("appid", dt.Rows[0]["AppId"].ToString());
                    form.Add("secret", dt.Rows[0]["AppSecret"].ToString());
                }

                string resp = GetRequestApi("https://api.weixin.qq.com/sns/oauth2/access_token", ref errMsg, form);
                if (!string.IsNullOrEmpty(resp))
                {
                    res = JsonConvert.DeserializeObject<Dictionary<string, string>>(resp);
                    if (res.ContainsKey("errcode"))
                    {
                        errMsg = res["errmsg"];
                    }
                    else
                    {
                        string access_token = res["access_token"] ?? "";
                        int expires_in = int.Parse(res["expires_in"] ?? "0");
                        SetCache("auth", access_token, expires_in, ref errMsg);
                    }
                }
            }
            return string.IsNullOrEmpty(errMsg);
        }

        public bool PushMsg(int tid, string touser, string content, string query, ref string errMsg)
        {
            tid *= IsTest ? -1 : 1;
            string AccessToken = GetCache("bus", ref errMsg);
            string resp;
            Dictionary<string, string> res;
            if (string.IsNullOrEmpty(AccessToken))
            {
                DataTable config = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"SELECT AppId,AppSecret FROM dbo.ZYSoftConfig WHERE Id ='{0}'", Id));

                if (config != null && config.Rows.Count > 0)
                {
                    Dictionary<string, string> form = new Dictionary<string, string>
                    {
                        { "grant_type", "client_credential" },
                        { "appid", config.Rows[0]["AppId"].ToString() },
                        { "secret", config.Rows[0]["AppSecret"].ToString() }
                    };

                    resp = GetRequestApi("https://api.weixin.qq.com/cgi-bin/token", ref errMsg, form);
                    if (!string.IsNullOrEmpty(resp))
                    {
                        res = JsonConvert.DeserializeObject<Dictionary<string, string>>(resp);
                        if (res.ContainsKey("errcode"))
                        {
                            errMsg = res["errmsg"];
                            return false;
                        }
                        else
                        {
                            AccessToken = res["access_token"] ?? "";
                            int expires_in = int.Parse(res["expires_in"] ?? "0");
                            SetCache("bus", AccessToken, expires_in, ref errMsg);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errMsg = "没有读取到微信公众号的参数";
                    return false;
                }
            }

            DataTable template = ZYSoft.DB.BLL.Common.ExecuteDataTable(
                string.Format(@"SELECT TemplateId,Items,Url FROM dbo.ZYSoftWcMsgTemplate WHERE Id ='{0}'", tid));
            if (template != null && template.Rows.Count > 0)
            {

                JObject body = new JObject(
                    new JProperty("touser", touser),
                    new JProperty("url", string.Format(@"{0}?{1}", template.Rows[0]["Url"].ToString(), query)),
                    new JProperty("template_id", template.Rows[0]["TemplateId"].ToString())
               );

                JObject qbody = new JObject();
                string[] valus = content.Split('|');
                string[] items = template.Rows[0]["Items"].ToString().Split(',');

                for (int i = 0; i < items.Length; i++)
                {
                    string item = items[i];
                    qbody.Add(new JProperty(item, new JObject { new JProperty("value", valus.Length > i ? valus[i] : "") }));
                }
                body.Add("data", qbody);
                resp = HttpPost(string.Format(@"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}",
                    AccessToken), JsonConvert.SerializeObject(body));

                if (!string.IsNullOrEmpty(resp))
                {
                    res = JsonConvert.DeserializeObject<Dictionary<string, string>>(resp);
                    if (res.ContainsKey("errcode") && res["errcode"].Equals("0"))
                    {
                        errMsg = res["errmsg"];
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errMsg = "没有读取到通知的模板";
                return false;
            }
        }

        public bool GetUserInfoFromDb(string openId, ref Dictionary<string, string> res, ref string errMsg)
        {
            res = new Dictionary<string, string>();
            try
            {
                DataTable dt = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format(@"
                SELECT  t2.[Guid] UserId,ISNULL(t2.DisplayName,'')DisplayName,ISNULL(t3.PersonId,'')PersonId
                FROM dbo.ZYSoftUserWcMapping t1 
                 LEFT JOIN
                 dbo.DncUser t2 ON t1.UserGuid = t2.[Guid] 
                 LEFT JOIN
                 dbo.ZYSoftUserPersonalMapping t3 ON t1.UserGuid = t2.[Guid] WHERE OpenId ='{0}'", openId));
                if (dt != null && dt.Rows.Count > 0)
                {
                    //有账号，且已经关联到openId
                    res.Add("UserId", dt.Rows[0]["UserId"].ToString());
                    res.Add("DisplayName", dt.Rows[0]["DisplayName"].ToString());
                    res.Add("PersonId", dt.Rows[0]["PersonId"].ToString());
                }
                return dt != null && dt.Rows.Count > 0;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }

        public string GetRequestApi(string url, ref string errMsg, Dictionary<string, string> dic = null)
        {
            string resp = "";
            try
            {
                string query = "";
                if (dic != null && dic.Count > 0)
                {
                    foreach (var ele in dic)
                    {
                        query += string.Format(@"{0}={1}&", ele.Key, ele.Value);
                    }
                    query = query.Substring(0, query.Length - 1);
                }
                using (HttpWebResponse response = HttpHelper.HttpRequest("get", string.Format(@"{0}?{1}", url, query),
                                                    null, null, Encoding.UTF8, ""))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        resp = reader.ReadToEnd();  //得到响应结果
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return resp;
        }

        public string PostRequestApi(string url, ref string errMsg, Dictionary<string, string> dic = null)
        {
            string resp = "";
            try
            {
                using (HttpWebResponse response = HttpHelper.HttpRequest("post", url,
                    new Dictionary<string, string>(), dic, Encoding.UTF8, ""))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        resp = reader.ReadToEnd();  //得到响应结果
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return resp;
        }

        public string HttpPost(string url, string data)
        {
            try
            {
                //创建post请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                byte[] payload = Encoding.UTF8.GetBytes(data);
                request.ContentLength = payload.Length;

                //发送post的请求
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();

                //接受返回来的数据
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string value = reader.ReadToEnd();

                reader.Close();
                stream.Close();
                response.Close();

                return value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void WriteLog(string content)
        {
            try
            {
                string tracingFile = string.Format(@"{0}/logs", _hostingEnvironment.ContentRootPath);
                if (!Directory.Exists(tracingFile))
                    Directory.CreateDirectory(tracingFile);
                string fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                tracingFile += fileName;
                if (tracingFile != String.Empty)
                {
                    FileInfo file = new FileInfo(tracingFile);
                    StreamWriter debugWriter = new StreamWriter(file.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                    debugWriter.WriteLine(DateTime.Now.ToString());
                    debugWriter.WriteLine(content);
                    debugWriter.WriteLine();
                    debugWriter.Flush();
                    debugWriter.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

    }
}