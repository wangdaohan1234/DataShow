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
    /// DataShow 的摘要说明
    /// </summary>
    public class DataShow : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            //查看session
            if (HttpContext.Current.Session["user"] == null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("nosession");
                return;
            }
            if (context.Request["data"] == null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("type或data参数为空");
            }
            #region 顶部预警展示
            if (context.Request["data"] == "ew")
            {
                List<string> sts = new List<string>();
                //消费预警
                string sql1 = string.Format("SELECT YXMC, COUNT(*) FROM dbo.ORG_MM_EARLYWARNING WHERE TIME = {0} AND WARNING >= {1} "
                            + " GROUP BY YXMC ORDER BY COUNT(*) DESC", "20180425", 3);
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
                            + " WHERE BRXZ = 13 AND JDRQ >= {0}) a "
                            + " LEFT JOIN "
                            + " (SELECT XH,YXSH FROM dbo.ORG_JW_XS_XSJBSJ) b "
                            + " ON a.BLH = b.XH "
                            + " LEFT JOIN "
                            + " (SELECT CODE,NAME FROM dbo.ORG_JW_C_DEPARTMENTS) c "
                            + " ON b.YXSH = c.CODE "
                            + " WHERE NAME IS NOT NULL "
                            + " GROUP BY NAME "
                            + " ORDER BY COUNT(*) DESC", DateTime.Now.AddDays(-60).ToString("yyy-MM-dd"));
                sts.Add(sql1);
                sts.Add(sql2);
                sts.Add(sql3);
                sts.Add(sql4);
                DataSet dt1 = DBUtility.SqlToTable(sts);
                //消费预警初始化
                int count1 = 0;
                string str = "{\"ecard\":[\"";
                for (int i = 0; i < dt1.Tables[0].Rows.Count; i++)
                {
                    count1 += int.Parse(dt1.Tables[0].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt1.Tables[0].Rows[i][0].ToString(), dt1.Tables[0].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count1);
                //图书预警初始化
                count1 = 0;
                str += "\"library\":[\"";
                for (int i = 0; i < dt1.Tables[1].Rows.Count; i++)
                {
                    count1 += int.Parse(dt1.Tables[1].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt1.Tables[1].Rows[i][0].ToString(), dt1.Tables[1].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count1);
                //健康预警初始化
                count1 = 0;
                str += "\"pe\":[\"";
                for (int i = 0; i < dt1.Tables[2].Rows.Count; i++)
                {
                    count1 += int.Parse(dt1.Tables[2].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt1.Tables[2].Rows[i][0].ToString(), dt1.Tables[2].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}],", count1);
                //医务预警初始化
                count1 = 0;
                str += "\"yiliao\":[\"";
                for (int i = 0; i < dt1.Tables[3].Rows.Count; i++)
                {
                    count1 += int.Parse(dt1.Tables[3].Rows[i][1].ToString());
                    str += string.Format("<li>{0}：<span>{1}</span>人</li>", dt1.Tables[3].Rows[i][0].ToString(), dt1.Tables[3].Rows[i][1].ToString());
                }
                str += string.Format("\",{0}]", count1);
                str += ",ew:" + 33 + ",brrow:" + 33 + ",lowbill:" + 33;
                str += "}";
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
            #endregion
            #region 学生人均消费预警
            if (context.Request["data"].ToString() == "xsrjxfyj")
            {
                string sql = "";
                string st_sql = "";
                string st_user = "";
                int count = 0;
                DateTime dtime = DateTime.Now;
                if (context.Request["month"].ToString() == "三个月")
                {
                    count = 3;
                }
                if (context.Request["month"].ToString() == "六个月")
                {
                    count = 6;
                }
                if (context.Request["month"].ToString() == "九个月")
                {
                    count = 9;
                }
                if (context.Request["month"].ToString() == "十二个月")
                {
                    count = 12;
                }
                if (context.Request["user"] != null)
                {
                    st_user = context.Request["user"].ToString();
                }
                sql = string.Format("SELECT b.XBM,ROUND(AVG(ZAO_C),2),ROUND(AVG(ZHONG_C),2),ROUND(AVG(WAN_C),2),ROUND(AVG(YE_C),2),ROUND(AVG(SHITANG_C),2) FROM  "
                    + " (SELECT * FROM dbo.ORG_NE_T_MONTHMEAL "
                    + " WHERE STUEMPNO NOT LIKE '0000000%' AND TIME <= '{0}' AND TIME >= '{1}') a "
                    + " LEFT JOIN "
                    + " (SELECT XH,XM,XBM FROM dbo.ORG_JW_XS_XSJBSJ WHERE XSDQZTM = 2) b "
                    + " ON a.STUEMPNO = b.XH "
                    + " WHERE b.XH IS NOT NULL "
                    + " GROUP BY b.XBM", DateTime.Now.ToString("yyyy-MM"), DateTime.Now.AddMonths(-count).ToString("yyyy-MM"));

                st_sql = string.Format("SELECT b.XM,ROUND(AVG(ZAO_C),2),ROUND(AVG(ZHONG_C),2),ROUND(AVG(WAN_C),2),ROUND(AVG(WAN_C),2),ROUND(AVG(SHITANG_C),2),b.XBM FROM  "
                    + " (SELECT * FROM dbo.ORG_NE_T_MONTHMEAL "
                    + " WHERE STUEMPNO NOT LIKE '0000000%' AND TIME <= '{0}' AND TIME >= '{1}' AND STUEMPNO = '{2}') a "
                    + " LEFT JOIN "
                    + " (SELECT XH,XM,XBM FROM dbo.ORG_JW_XS_XSJBSJ WHERE XSDQZTM = 2) b "
                    + " ON a.STUEMPNO = b.XH "
                    + " WHERE b.XH IS NOT NULL "
                    + " GROUP BY b.XM,b.XBM", DateTime.Now.ToString("yyyy-MM"), DateTime.Now.AddMonths(-count).ToString("yyyy-MM"), st_user);

                List<string> sts = new List<string>();
                sts.Add(sql);
                sts.Add(st_sql);
                DataSet dt = DBUtility.SqlToTable(sts);

                //返回值
                string str = "{";
                string L_month = "\"L_month\":[";
                string N_month = "\"N_month\":[";
                string st_month = "\"st_month\":[";
                for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
                {
                    if (dt.Tables[0].Rows[i][0].ToString() == "1")
                        L_month += dt.Tables[0].Rows[i][1].ToString() + ',' + dt.Tables[0].Rows[i][2].ToString() + ','
                            + dt.Tables[0].Rows[i][3].ToString() + ',' + dt.Tables[0].Rows[i][4].ToString() + ',' + dt.Tables[0].Rows[i][5].ToString();
                    if (dt.Tables[0].Rows[i][0].ToString() == "2")
                        N_month += dt.Tables[0].Rows[i][1].ToString() + ',' + dt.Tables[0].Rows[i][2].ToString() + ','
                            + dt.Tables[0].Rows[i][3].ToString() + ',' + dt.Tables[0].Rows[i][4].ToString() + ',' + dt.Tables[0].Rows[i][5].ToString();
                }
                if (dt.Tables[1].Rows.Count != 0)
                {
                    string name = dt.Tables[1].Rows[0][0].ToString();
                    if (dt.Tables[1].Rows[0][6].ToString() == "1")
                        name += "(男)";
                    if (dt.Tables[1].Rows[0][6].ToString() == "2")
                        name += "(女)";
                    st_month += dt.Tables[1].Rows[0][1].ToString() + ',' + dt.Tables[1].Rows[0][2].ToString() + ','
                        + dt.Tables[1].Rows[0][3].ToString() + ',' + dt.Tables[1].Rows[0][4].ToString() + ','
                        + dt.Tables[1].Rows[0][5].ToString() + ",\"" + name + "\"";
                }
                str += L_month + "]," + N_month + "]," + st_month + "]}";
                dt.Dispose();
                //返回值
                context.Response.ContentType = "text/plain";
                context.Response.Write(str);
            }
            #endregion
            #region 校情展示
            if (context.Request["data"] == "SHOW")
            {
                List<string> sts = new List<string>();
                string card = "SELECT b.CUSTTYPENAME,COUNT(*) AS TotalCount FROM "
                            + " dbo.ORG_NE_T_CUSTOMER a "
                            + " LEFT JOIN "
                            + " dbo.ORG_NE_T_CUSTTYPE b "
                            + " ON a.CUSTTYPE = b.CUSTTYPE "
                            + " LEFT JOIN "
                            + " (SELECT CUSTID,MAX(CARDNO) AS CARDNO FROM dbo.ORG_NE_T_CARD "
                            + " WHERE CONVERT(varchar(100),STATUS) + CONVERT(varchar(100),LOSSFLAG) + "
                            + " CONVERT(varchar(100),FROZEFLAG) + CONVERT(varchar(100),BADFLAG) = '1000' "
                            + " GROUP BY CUSTID) c "
                            + " ON a.CUSTID = c.CUSTID "
                            + " WHERE c.CUSTID IS NOT NULL "
                            + " GROUP BY b.CUSTTYPENAME";
                string consume = string.Format("SELECT a.CARD,b.AMOUNT,b.TOTALCOUNT,ROUND(b.AMOUNT/b.TOTALCOUNT,2) AS AVERAGE FROM "
                                + " (SELECT COUNT(*) AS CARD FROM "
                                + " (SELECT DISTINCT STUEMPNO  FROM dbo.ORG_NE_T_TRANSDTL "
                                + " WHERE ACCDATE = '{0}') AS a) AS a, "
                                + " (SELECT SUM(AMOUNT) AS AMOUNT,COUNT(*) AS TOTALCOUNT FROM dbo.ORG_NE_T_TRANSDTL "
                                + " WHERE ACCDATE = '{0}' AND CARDBEFBAL > CARDAFTBAL) AS b", "20180415");
                string library1 = "SELECT COUNT(*) FROM dbo.ORG_BB_HOLDING";
                string library2 = string.Format("SELECT TYPENAME,TOTALCOUNT FROM "
                                + " (SELECT LOGTYPE,COUNT(*) AS TOTALCOUNT FROM dbo.ORG_BB_LOG_CIR "
                                + " WHERE CONVERT(datetime,LEFT(LOAN_TIME,10)) = '{0}' "
                                + " GROUP BY LOGTYPE) a "
                                + " LEFT JOIN "
                                + " dbo.ORG_BB_LOG_CIRTYPE b "
                                + " ON a.LOGTYPE = b.LOGTYPE", "20180415");
                sts.Add(card);
                sts.Add(consume);
                sts.Add(library1);
                sts.Add(library2);
                DataSet dt = DBUtility.SqlToTable(sts);

                //取数据并转化为Json字符串
                string Json = "\"card\":{";
                for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                        Json += "\"" + dt.Tables[0].Rows[i][0].ToString() + "\":" + dt.Tables[0].Rows[i][1].ToString();
                    else
                        Json += ",\"" + dt.Tables[0].Rows[i][0].ToString() + "\":" + dt.Tables[0].Rows[i][1].ToString();
                }
                Json += "},\"consume\":[" + dt.Tables[1].Rows[0][0].ToString() + "," + dt.Tables[1].Rows[0][1].ToString() + "," + dt.Tables[1].Rows[0][2].ToString() + "," + dt.Tables[1].Rows[0][3].ToString();
                Json += "],\"books\":[" + dt.Tables[2].Rows[0][0].ToString() + "],\"borrow\":[";
                for (int i = 0; i < dt.Tables[3].Rows.Count; i++)
                {
                    if (i == 0)
                        Json += "{\"" + dt.Tables[3].Rows[i][0].ToString() + "\":" + dt.Tables[3].Rows[i][1].ToString() + " }";
                    else
                        Json += ",{\"" + dt.Tables[3].Rows[i][0].ToString() + "\":" + dt.Tables[3].Rows[i][1].ToString() + " }";
                }
                Json += "],\"time\":[" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "]";
                Json = "{" + Json + "}";
                //Ajax返回前台
                context.Response.ContentType = "text/plain";
                context.Response.Write(Json);
            }
            #endregion
            #region 生源地分析(地图)
            if (context.Request["data"] == "stumap")
            {
                //返回字符串定义
                string str = "";
                //SQL语句
                String sql = "";
                List<string> SQLS = new List<string>();

                //全省身份证号码对应关系
                string[,] code = { { "北京", "11" }, { "天津", "12" }, { "河北", "13" }, { "山西", "14" }, { "内蒙古", "15" }, { "辽宁", "21" }, { "吉林", "22" }, { "黑龙江", "23" }, { "上海", "31" }, { "江苏", "32" }, { "浙江", "33" }, { "安徽", "34" }, { "福建", "35" }, { "江西", "36" }, { "山东", "37" }, { "河南", "41" }, { "湖北", "42" }, { "湖南", "43" }, { "广东", "44" }, { "广西", "45" }, { "海南", "46" }, { "重庆", "50" }, { "四川", "51" }, { "贵州", "52" }, { "云南", "53" }, { "西藏", "54" }, { "陕西", "61" }, { "甘肃", "62" }, { "青海", "63" }, { "宁夏", "64" }, { "新疆", "65" }, { "台湾", "71" }, { "香港", "81" }, { "澳门", "82" }, { "国外", "91" } };

                if (context.Request["grade"].ToString() == "" || context.Request["grade"].ToString() == null)
                {
                    sql = "SELECT COUNT(*) FROM dbo.ORG_NE_T_CUSTOMER";
                    //总人数
                    SQLS.Add(sql);
                    //每个省人数
                    for (int i = 0; i < 35; i++)
                    {
                        SQLS.Add(string.Format("{0} WHERE IDNO LIKE '{1}%'", sql, code[i, 1]));
                    }
                }
                else
                {
                    string st = context.Request["grade"].ToString();
                    sql = string.Format("SELECT COUNT(*) FROM dbo.ORG_NE_T_CUSTOMER WHERE STUEMPNO LIKE '{0}%'", st);
                    //总人数
                    SQLS.Add(sql);
                    //每个省人数
                    for (int i = 0; i < 35; i++)
                    {
                        SQLS.Add(string.Format("{0} AND IDNO LIKE '{1}%'", sql, code[i, 1]));
                    }
                }

                DataSet ds = DBUtility.SqlToTable(SQLS);

                str = "{";
                //全部人数
                str += string.Format("\"toalcount\":{0},", ds.Tables[0].Rows[0][0].ToString());
                str += "\"items\":[";
                //返回值处理
                for (int i = 1; i < ds.Tables.Count; i++)
                {
                    str += "{ 'hc-key': '" + code[i - 1, 0] + "', value: " + ds.Tables[i].Rows[0][0].ToString() + " },";
                }
                //Remove the last ','
                str = str.Remove(str.LastIndexOf(','));
                str += "]}";

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