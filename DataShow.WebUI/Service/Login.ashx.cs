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
    /// Login 的摘要说明
    /// </summary>
    public class Login : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            string result = "Hello World!";
            if ((context.Request["username"] != null) && (context.Request["password"] != null))
            {
                string sql = "select * from dbo.ORG_MM_USER where username='{0}' and PSW = '{1}'";
                sql = string.Format(sql, context.Request["username"].ToString(), context.Request["password"].ToString());
                List<string> sts = new List<string>();
                sts.Add(sql);
                DataSet dt = DBUtility.SqlToTable(sts);
                if (dt.Tables[0].Rows.Count == 1)
                {

                    HttpContext.Current.Session["user"] = context.Request["username"].ToString();
                    HttpContext.Current.Session["GH"] = context.Request["username"].ToString();
                    HttpContext.Current.Session["name"] = dt.Tables[0].Rows[0]["truename"];
                    result = "{'result':'OK'}";
                }
                else
                {
                    result = "{'result':'NO'}";
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