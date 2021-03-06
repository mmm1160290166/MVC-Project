﻿using MZ_CORE;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace MZ_API
{
    /// <summary>
    /// 权限过滤器【签名】
    /// </summary>
    public class ActionFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext context)
        {
            base.OnActionExecuting(context);

            #region 参数
            string usercode = string.Empty;
            string timestamp = string.Empty;
            string random = string.Empty;
            string signature = string.Empty;
            string sessionkey = string.Empty;
            string platform = string.Empty;

            //用户唯一标识
            if (context.Request.Headers.Contains("usercode"))
            {
                usercode = HttpUtility.UrlDecode(context.Request.Headers.GetValues("usercode").FirstOrDefault());
            }
            //时间戳
            if (context.Request.Headers.Contains("timestamp"))
            {
                timestamp = HttpUtility.UrlDecode(context.Request.Headers.GetValues("timestamp").FirstOrDefault());
            }
            //随机数
            if (context.Request.Headers.Contains("random"))
            {
                random = HttpUtility.UrlDecode(context.Request.Headers.GetValues("random").FirstOrDefault());
            }
            //签名参数
            if (context.Request.Headers.Contains("signature"))
            {
                signature = HttpUtility.UrlDecode(context.Request.Headers.GetValues("signature").FirstOrDefault());
            }
            //平台参数
            if (context.Request.Headers.Contains("platform"))
            {
                platform = HttpUtility.UrlDecode(context.Request.Headers.GetValues("platform").FirstOrDefault());
            }
            #endregion
            if (string.IsNullOrEmpty(usercode))
            {
                context.Response = CORE.Msg.ResponseJson(CORE.Msg.ToJson(CORE.Msg.Result(CORE.Msg.RST.ERR, "账号不存在")));
            }
            else if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(random) || string.IsNullOrEmpty(signature))
            {
                context.Response = CORE.Msg.ResponseJson(CORE.Msg.ToJson(CORE.Msg.Result(CORE.Msg.RST.ERR, "参数错误")));
            }
            else
            {
                #region 判断token是否有效
                CORE.token token = CORE.CacheHelper.GetToken(platform == "webchat" ? CORE.CacheHelper.TokenKey.BPM_P8_API_MOBILE_TOKEN : CORE.CacheHelper.TokenKey.BPM_P8_API_TOKEN, usercode);
                if (token != null)
                {
                    #region 生成令牌
                    string data = "";
                    if (HttpContext.Current.Request.HttpMethod == "POST")
                    {
                        switch (HttpContext.Current.Request.ContentType)
                        {
                            case "application/x-www-form-urlencoded;charset=utf-8":
                                data = HttpContext.Current.Request.Form.ToString();
                                break;
                            case "application/json;charset=utf-8":
                                using (Stream sm = HttpContext.Current.Request.InputStream)
                                {
                                    using (StreamReader sr = new StreamReader(sm, Encoding.UTF8))
                                    {
                                        data = sr.ReadToEnd();
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        data = HttpContext.Current.Request.QueryString.NVCToQueryString();
                    }
                    string newsignature = CORE.ApiHelper.GetSignture(timestamp, random, usercode, token.SignToken.ToString(), data);
                    #endregion
                    if (newsignature != signature)
                    {
                        context.Response = CORE.Msg.ResponseJson(CORE.Msg.ToJson(CORE.Msg.Result(CORE.Msg.RST.ERR, "令牌验证失败", "$.P8.common().relogin();")));
                    }
                }
                else
                {
                    context.Response = CORE.Msg.ResponseJson(CORE.Msg.ToJson(CORE.Msg.Result(CORE.Msg.RST.ERR, "token已失效", "$.P8.common().relogin();")));
                }
                #endregion
            }
        }
    }
}