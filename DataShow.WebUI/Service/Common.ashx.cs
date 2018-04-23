using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using DataShow.Domain;

namespace DataShow.WebUI.Service
{
    /// <summary>
    /// Common 的摘要说明
    /// </summary>
    public class Common : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string ProName = "";
            SqlParameter[] parm = new SqlParameter[1];

            #region 年份---type=Year
            if (context.Request["type"] == "Year")
            {
                string str = "{data:[";
                int year = DateTime.Now.Year;
                while (year > 2013)
                {
                    str += "'" + year + "',";
                    year--;
                }
                str = str.Remove(str.Length - 1, 1);
                str += "]}";
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
                return;
            }
            #endregion

            DataSet dt = DBUtility.ProceureToTable(ProName, parm);
            context.Response.ContentType = "text/plain";
            context.Response.Write(TransformToJson(dt));
        }

        public string TransformToJson(DataSet dt)
        {
            string str = "{data:[";
            for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                    str += "'" + dt.Tables[0].Rows[i][0] + "'";
                else
                    str += ",'" + dt.Tables[0].Rows[i][0] + "'";
            }
            str += "]}";
            return str;
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