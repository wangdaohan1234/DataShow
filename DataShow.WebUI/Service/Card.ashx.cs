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
    /// Card 的摘要说明
    /// </summary>
    public class Card : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //标题
            string title = "";
            //传递参数
            SqlParameter[] parm = new SqlParameter[1];
            //存储过程名称
            string ProName = "";
            //初始化检索
            int datecount = 7;
            //初始化时间
            DateTime endtime = DateTime.Now.AddDays(-1);
            DateTime begtime = endtime;
            if (context.Request["time"] != null)
            {
                if (context.Request["time"] == "一周")
                {
                    datecount = 7;
                }
                else if (context.Request["time"] == "两周")
                {
                    datecount = 14;
                }
                else if (context.Request["time"] == "一月")
                {
                    datecount = 30;
                }
                else if (context.Request["time"] == "一年")
                {
                    datecount = 366;
                }

                begtime = endtime.AddDays(-datecount);
            }
            if ((context.Request["begtime"] != null) && (context.Request["endtime"] != null))
            {
                begtime = DateTime.Parse(context.Request["begtime"].ToString());
                endtime = DateTime.Parse(context.Request["endtime"].ToString());
                datecount = (endtime - begtime).Days;
            }
            //获取分页及单页面数据条数限制
            int totalCount = 0;
            int page = 1;
            int limit = 20;
            if ((context.Request.QueryString["page"] != null) && (context.Request.QueryString["limit"] != null))
            {
                page = int.Parse(context.Request.QueryString["page"].ToString());
                limit = int.Parse(context.Request.QueryString["limit"].ToString());
            }
            #region 用户信息详情
            if (context.Request["data"] == "CustomerDetail")
            {
                //存储过程名称(需要注意在哪个用户下)
                ProName = "Card_CustomerDetail";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[10];
                parm[0] = new SqlParameter("@stuempno", context.Request["stuempno"]);
                parm[1] = new SqlParameter("@custname", context.Request["custname"]);
                parm[2] = new SqlParameter("@sex", context.Request["sex"]);
                parm[3] = new SqlParameter("@custtypename", context.Request["custtypename"]);
                parm[4] = new SqlParameter("@idno", context.Request["idno"]);
                parm[5] = new SqlParameter("@deptname", context.Request["deptname"]);
                parm[6] = new SqlParameter("@classname", context.Request[("classname")]);
                parm[7] = new SqlParameter("@page_size", limit);
                parm[8] = new SqlParameter("@Page_num", page);
                parm[9] = new SqlParameter("@total_record", SqlDbType.Int);
                parm[9].Direction = ParameterDirection.Output;
            }
            #endregion

            //运行SQL
            DataSet dt = DBUtility.ProceureToTable(ProName, parm);
            if (context.Request["data"] == "CustomerDetail")
                totalCount = Convert.ToInt32(parm[9].Value);

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
                string str = "{\"items\":[" + DataSetConvertJson.DataTable2Json(dt.Tables[0]) + "],";
                str += "totalCount:" + "'" + totalCount.ToString() + "'" + "}";
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
            //导出表格数据到Excel---type=export_excel
            if (context.Request["type"] == "export_excel")
            {
                ExportClass.DataGridViewExportToExcel(dt, title);
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