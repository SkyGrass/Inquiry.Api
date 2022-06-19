using AutoMapper;
using Backend.Api.Auth;
using Backend.Api.Entities;
using Backend.Api.Extensions;
using Backend.Api.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Backend.Api.Controllers
{
    public class AuthController : CookieHelper
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly AppAuthenticationSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _hostingEnvironment;
        public AuthController(IOptions<AppAuthenticationSettings> appSettings, IHostEnvironment hostingEnvironment,
            DncZeusDbContext dbContext, IMapper mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public IActionResult Index(string code, string state)
        {
            /*
             https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect
             */
            try
            {
                using (_dbContext)
                {
                    if (string.IsNullOrEmpty(code))
                    {
                        var query = _dbContext.ZYSoftConfig.Where(x => x.Id.Equals(_appSettings.Id)).FirstOrDefault();
                        var query1 = _dbContext.DncIcon.Where(x => x.Code.Contains("")).FirstOrDefault();
                        string url = string.Empty;
                        if (query != null)
                        {
                            url = string.Format(@"https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state={2}#wechat_redirect", query.AppId,
                    HttpUtility.HtmlEncode(_appSettings.RedirectUrl), GetRandomString());
                        }
                        return Redirect(url); //redirect auth
                    }
                    else
                    {
                        string errMsg = "";
                        if (!string.IsNullOrEmpty(code))
                        {
                            Dictionary<string, string> res = new Dictionary<string, string>();
                            if (new AppHelper(_hostingEnvironment).GetAuthInfo(code, "auth", ref res, ref errMsg))
                            {
                                string openId = res["openid"] ?? "";
                                if (!string.IsNullOrEmpty(openId))
                                {
                                    string targetUrl = ConfigurationManager.GetSetting("TargetUrl") ?? "";
                                    return Redirect(string.Format(@"{0}?openId={1}", targetUrl, openId)); //redirect auth
                                }
                                else
                                {
                                    // fail to get user openid
                                    string msg = HttpUtility.HtmlEncode("系统似乎开小差了,请关闭后打开!");
                                    return RedirectToAction("Index?tips=" + msg, "Tip");
                                }
                            }
                            else
                            {
                                string msg = HttpUtility.HtmlEncode("系统似乎开小差了!");
                                return RedirectToAction("Index?tips=" + msg, "Tip");
                            }
                        }
                    }
                }
                return View(ViewBag);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length = 10, bool useNum = true, bool useLow = true, bool useUpp = true,
            bool useSpe = false, string custom = "")
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
    }
}
