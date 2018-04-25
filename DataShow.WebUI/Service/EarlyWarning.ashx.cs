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
    /// 预警系统的数据请求处理
    /// </summary>
    public class EarlyWarning : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //标题
            string title = "";
            //传递参数
            SqlParameter[] parm = new SqlParameter[1];
            //存储过程名称
            string ProName = "";
            //获取分页及单页面数据条数限制
            int totalCount = 0;
            int page = 1;
            int limit = 20;
            if ((context.Request.QueryString["page"] != null) && (context.Request.QueryString["limit"] != null))
            {
                page = int.Parse(context.Request.QueryString["page"].ToString());
                limit = int.Parse(context.Request.QueryString["limit"].ToString());
            }

            #region 学生实时预警统计
            if (context.Request["data"] == "xsssyjtj")
            {
                //存储过程名称
                ProName = "EW_XSYJByXYAndNJ";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[3];
                parm[0] = new SqlParameter("@time", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
                parm[1] = new SqlParameter("@waring", 3);
                parm[2] = new SqlParameter("@lastestgrade", 2017);
            }
            #endregion
            #region 学生预警（按辅导员统计）
            if (context.Request["data"] == "xsyj")
            {
                //存储过程名称
                ProName = "EW_XSYJByFDY";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[2];
                parm[0] = new SqlParameter("@time", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
                parm[1] = new SqlParameter("@waring", 3);
            }
            #endregion
            #region 晚归学生分布情况
            if (context.Request["data"] == "wgxsfbqk")
            {
                //存储过程名称
                ProName = "EW_WGXS";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[2];
                parm[0] = new SqlParameter("@begintime", "20140415 22:00:00");
                parm[1] = new SqlParameter("@endtime", "20140416 04:00:00");
            }
            #endregion
            #region 预警学生详情
            if (context.Request["data"] == "EWDetail")
            {
                //存储过程名称
                ProName = "EW_StudentDetail";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[11];
                parm[0] = new SqlParameter("@xh", context.Request["xh"]);
                parm[1] = new SqlParameter("@xm", context.Request["xm"]);
                parm[2] = new SqlParameter("@yxmc", context.Request["yxmc"]);
                parm[3] = new SqlParameter("@zymc", context.Request["zymc"]);
                parm[4] = new SqlParameter("@bjmc", context.Request["bjmc"]);
                parm[5] = new SqlParameter("@time", context.Request["time"]);
                parm[6] = new SqlParameter("@warning", context.Request[("warning")]);
                parm[7] = new SqlParameter("@fdy", context.Request[("fdy")]);
                parm[8] = new SqlParameter("@page_size", limit);
                parm[9] = new SqlParameter("@page_num", page);
                parm[10] = new SqlParameter("@total_record", SqlDbType.Int);
                parm[10].Direction = ParameterDirection.Output;
            }
            #endregion

            //运行SQL
            DataSet dt = DBUtility.ProceureToTable(ProName, parm);
            if (context.Request["data"] == "EWDetail")
                totalCount = Convert.ToInt32(parm[10].Value);

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