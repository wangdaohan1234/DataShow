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

            if (context.Request["data"] == "EW")
            {
                List<string> sts = new List<string>();
                //消费预警
                string sql1 = string.Format("SELECT YXMC, COUNT(*) FROM dbo.ORG_MM_EARLYWARNING WHERE TIME = {0} AND COUNT >= {1} "
                            + " GROUP BY YXMC ORDER BY COUNT(*) DESC", DateTime.Now.AddDays(-1000).ToString("yyyyMMdd"), 3);
                //图书预警
                string sql2 = string.Format("SELECT NAME,COUNT(*) FROM "
                            + " (SELECT XH,YXSH FROM dbo.ORG_JW_XS_XSJBSJ WHERE XSDQZTM =2) a "
                            + " LEFT JOIN "
                            + " (SELECT CODE,NAME FROM dbo.ORG_JW_C_DEPARTMENTS) b "
                            + " ON a.YXSH = b.CODE "
                            + " WHERE XH NOT IN "
                            + " (SELECT DISTINCT DATA2 FROM dbo.ORG_BB_LOG_CIR "
                            + " WHERE LOGTYPE = '30001' AND LOAN_TIME >= CONVERT(datetime, {0}) AND DATA2 IS NOT NULL) "
                            + " GROUP BY NAME ORDER BY COUNT(*) DESC", DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd"));
                //健康预警
                string sql3 = "SELECT NAME,COUNT(*) FROM "
                            + " (SELECT HABI_1 FROM dbo.ORG_SP_HABITUS "
                            + " WHERE HABI_8 LIKE '%肥胖%') a "
                            + " LEFT JOIN "
                            + " (SELECT XH,YXSH FROM dbo.ORG_JW_XS_XSJBSJ) b "
                            + " ON a.HABI_1 = b.XH "
                            + " LEFT JOIN "
                            + " (SELECT CODE,NAME FROM dbo.ORG_JW_C_DEPARTMENTS) c "
                            + " ON b.YXSH = c.CODE "
                            + " GROUP BY NAME "
                            + " ORDER BY COUNT(*) DESC ";
                //医务预警
                string sql4 = string.Format("SELECT NAME,COUNT(*) FROM "
                            + " (SELECT BLH FROM dbo.ORG_YWS_XT_BRJBXX "
                            + " WHERE BRXZ = 13 AND CONVERT(datetime,left(JDRQ,(charindex(' ',JDRQ)-1))) >= {0}) a "
                            + " LEFT JOIN "
                            + " (SELECT XH,YXSH FROM dbo.ORG_JW_XS_XSJBSJ) b "
                            + " ON a.BLH = b.XH "
                            + " LEFT JOIN "
                            + " (SELECT CODE,NAME FROM dbo.ORG_JW_C_DEPARTMENTS) c "
                            + " ON b.YXSH = c.CODE "
                            + " GROUP BY NAME "
                            + " ORDER BY COUNT(*) DESC", DateTime.Now.AddDays(-60).ToString("yyy-MM-dd"));
                sts.Add(sql1);
                sts.Add(sql2);
                sts.Add(sql3);
                sts.Add(sql4);
                DataSet dt = DBUtility.SqlToTable(sts);
                //消费预警初始化
                int count = 0;
                string str = "{\"ecard\":[\"";
                for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
                {
                    count += int.Parse(dt.Tables[0].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt.Tables[0].Rows[i][0].ToString(), dt.Tables[0].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count);
                //图书预警初始化
                count = 0;
                str += "\"library\":[\"";
                for (int i = 0; i < dt.Tables[1].Rows.Count; i++)
                {
                    count += int.Parse(dt.Tables[1].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt.Tables[1].Rows[i][0].ToString(), dt.Tables[1].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count);
                //健康预警初始化
                count = 0;
                str += "\"pe\":[\"";
                for (int i = 0; i < dt.Tables[2].Rows.Count; i++)
                {
                    count += int.Parse(dt.Tables[2].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt.Tables[2].Rows[i][0].ToString(), dt.Tables[2].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count);
                //医务预警初始化
                count = 0;
                str += "\"yiliao\":[\"";
                for (int i = 0; i < dt.Tables[3].Rows.Count; i++)
                {
                    count += int.Parse(dt.Tables[3].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt.Tables[3].Rows[i][0].ToString(), dt.Tables[3].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}]", count);
                str += ",ew:" + 33 + ",brrow:" + 33 + ",lowbill:" + 33;
                str += "}";
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
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