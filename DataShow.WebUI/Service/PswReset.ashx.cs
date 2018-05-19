using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using DataShow.Domain;
using System.Web.SessionState;

namespace DataShow.WebUI.Service
{
    /// <summary>
    /// PswReset 的摘要说明
    /// </summary>
    public class PswReset : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            string result = "Hello World!";
            if ((HttpContext.Current.Session["user"] != null) && (context.Request["oldpassword"] != null))
            {
                string sql = "select * from dbo.ORG_MM_USER where username='{0}' and PSW = '{1}'";
                sql = string.Format(sql, HttpContext.Current.Session["user"].ToString(), context.Request["oldpassword"].ToString());
                List<string> sts = new List<string>();
                sts.Add(sql);
                DataSet dt = DBUtility.SqlToTable(sts);
                if (dt.Tables[0].Rows.Count == 1)
                {
                    sql = "update dbo.ORG_MM_USER set PSW = '{1}' where username='{0}'";
                    sql = string.Format(sql, HttpContext.Current.Session["user"].ToString(), context.Request["password"].ToString());
                    int dt2 = DBUtility.RunSQL(sql);
                    if (dt2 == 1)
                    {
                        result = "{'result':'OK','message':'密码修改成功！'}";
                    }
                    else
                    {
                        result = "{'result':'NO','message':'密码修改失败请联系管理员处理！'}";
                    }
                }
                else
                {
                    result = "{'result':'NO','message':'旧密码输入错误！'}";
                }
            }
            context.Response.ContentType = "text/plain";
            context.Response.Write(result);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}