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
    /// DataShow 的摘要说明
    /// </summary>
    public class DataShow : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //标题
            string title = "";
            //传递参数
            SqlParameter[] parm = new SqlParameter[1];
            //存储过程名称
            string ProName = "";

            if (context.Request["data"] == "sex")
            {
                ProName = "ZZMM";
                parm = new SqlParameter[0];
            }

            DataSet dt = DBUtility.ProceureToTable(ProName, parm);

            #region 输出图形分成几份
            int count = 0;
            //分成几份传参
            if (context.Request["count"] != null)
            {
                int.TryParse(context.Request["count"], out count);
            }
            else if (dt.Tables[0] != null)
            {
                count = dt.Tables[0].Rows.Count;
            }
            #endregion

            #region 生成前台使用数据
            //生成柱状图数据---type=column
            if (context.Request["type"] == "column")
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(DataSetConvertJson.TransformToColumnJson(dt, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("yyyyMMdd"), count, title));
            }
            //生成饼图数据---type=pie
            if (context.Request["type"] == "pie")
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(DataSetConvertJson.TransformToPieJson(dt, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("yyyyMMdd"), count, title));
            }
            //生成表格所需数据---type=table
            if (context.Request["type"] == "table")
            {
                string str = "{\"items\":[" + DataSetConvertJson.DataTable2Json(dt.Tables[0]) + "]}";
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
            //分页生成表格所需数据---type=tablepaged
            if (context.Request["type"] == "tablepaged")
            {
                string str = DataSetConvertJson.Dataset2Json(dt);
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
            #endregion
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