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


            #region 学生实时预警统计
            if (context.Request["data"] == "xsssyjtj")
            {
                //存储过程名称
                ProName = "EW_XSYJByXYAndNJ";
                //具体申请几个空间看存储过程中需要几个参数
                parm = new SqlParameter[3];
                parm[0] = new SqlParameter("@time", 20180409);
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
                parm[0] = new SqlParameter("@time", 20180409);
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

            //运行SQL
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