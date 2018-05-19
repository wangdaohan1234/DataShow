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
    /// PersonalMonthBill 的摘要说明
    /// </summary>
    public class PersonalMonthBill : IHttpHandler, IReadOnlySessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            //用户
            string id = "";
            string xm = "\"name\":['{0}']";

            if (context.Request["id"] == null)
            {
                //查看session
                if (HttpContext.Current.Session["user"] == null)
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.Write("nosession");
                    return;
                }
                else
                {
                    id = HttpContext.Current.Session["user"].ToString();
                }
            }
            else
            {
                id = context.Request["id"].ToString();
            }
            //取姓名
            string sql1 = string.Format("SELECT STUEMPNO,CUSTNAME,SEX FROM dbo.ORG_NE_T_CUSTOMER WHERE STUEMPNO = '{0}'", id.PadLeft(11, '0'));
            List<string> sts = new List<string>();
            sts.Add(sql1);
            DataSet dt = DBUtility.SqlToTable(sts);
            if (dt.Tables[0].Rows.Count == 0)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("非法请求");
                return;
            }
            xm = string.Format(xm, dt.Tables[0].Rows[0][1].ToString());

            //初始化检索
            int datecount = 7;
            //初始化时间
            DateTime endtime = DateTime.Now;
            DateTime begintime = endtime;
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
                else if (context.Request["time"] == "一个月")
                {
                    datecount = 30;
                }
                begintime = endtime.AddDays(-datecount);
            }
            if ((context.Request["begtime"] != null) && (context.Request["endtime"] != null))
            {
                begintime = DateTime.Parse(context.Request["begtime"].ToString());
                endtime = DateTime.Parse(context.Request["endtime"].ToString());
                datecount = (endtime - begintime).Days;
            }
            //三个操作谁查看了谁的账单信息
            string sql = string.Format("select e.shopname,sum(amount),count(*),max(amount),min(amount),ROUND(avg(amount),2) " +
                                        " from (select * from dbo.ORG_NE_T_TRANSDTL where refno > '{0}' and refno <'{1}' and  custid in " +
                                        " (select custid from dbo.ORG_NE_T_CUSTOMER where STUEMPNO like '{2}')  and transflag = 2 and amount >0  order by " +
                                        " accdate,acctime) a left join dbo.ORG_NE_T_TRANSCODE b on a.TRANSCODE=b.TRANSCODE " +
                                        " left join DCUSER.org_ne_t_device c on a.TERMID = c.DEVICEID " +
                                        " left join (select * FROM dbo.ORG_NE_T_SHOPPOS where status = 1 ) d ON d.DEVICEID = c.DEVICEID " +
                                        " left join (select * from  dbo.ORG_NE_T_SHOP where status =1) e on e.SHOPID = d.SHOPID " +
                                        " group by e.shopname order by sum(amount) desc", begintime.ToString("yyyyMMdd"), endtime.ToString("yyyyMMdd"), id);
            List<string> sts1 = new List<string>();
            sts.Add(sql);
            DataSet da = DBUtility.SqlToTable(sts1);
            string str = "{";
            string shop = "\"shop\":[";
            string sum = "\"sum\":[";
            string pie = "\"pie\":[";
            double shitang = 0;
            double jiaochao = 0;
            double guoshu = 0;
            double yiliao = 0;
            double shangpu = 0;
            double qita = 0;
            string _shitang = "<li class=\"fo\">消费详情如下:</li>";
            string _jiaochao = "<li class=\"fo\">消费详情如下:</li>";
            string _guoshu = "<li class=\"fo\">消费详情如下:</li>";
            string _yiliao = "<li class=\"fo\">消费详情如下:</li>";
            string _shangpu = "<li class=\"fo\">消费详情如下:</li>";
            string _qita = "<li class=\"fo\">消费详情如下:</li>";
            for (int i = 0; i < da.Tables[0].Rows.Count; i++)
            {
                if ((da.Tables[0].Rows[i][0].ToString() == "一食堂") || (da.Tables[0].Rows[i][0].ToString() == "二食堂") || (da.Tables[0].Rows[i][0].ToString() == "三食堂") ||
                    (da.Tables[0].Rows[i][0].ToString() == "四食堂") || (da.Tables[0].Rows[i][0].ToString() == "五食堂") || (da.Tables[0].Rows[i][0].ToString() == "教师餐厅") ||
                    (da.Tables[0].Rows[i][0].ToString() == "清真食堂") || (da.Tables[0].Rows[i][0].ToString() == "教授餐厅") || (da.Tables[0].Rows[i][0].ToString() == "熟食卤菜"))
                {
                    _shitang += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    shitang += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
                else if ((da.Tables[0].Rows[i][0].ToString() == "河南教育超市") || (da.Tables[0].Rows[i][0].ToString() == "河东教育超市"))
                {
                    _jiaochao += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    jiaochao += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
                else if ((da.Tables[0].Rows[i][0].ToString() == "东二食堂蔬菜展示厅") || (da.Tables[0].Rows[i][0].ToString() == "西一水果展示厅"))
                {
                    _guoshu += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    guoshu += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
                else if ((da.Tables[0].Rows[i][0].ToString() == "体检") || (da.Tables[0].Rows[i][0].ToString() == "医务室") || (da.Tables[0].Rows[i][0].ToString() == "医务室收费"))
                {
                    _yiliao += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    yiliao += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
                else if ((da.Tables[0].Rows[i][0].ToString() == "泰洁干洗店") || (da.Tables[0].Rows[i][0].ToString() == "理发店") || (da.Tables[0].Rows[i][0].ToString() == "一食堂小卖部")
                    || (da.Tables[0].Rows[i][0].ToString() == "电脑屋") || (da.Tables[0].Rows[i][0].ToString() == "亮点眼镜店") || (da.Tables[0].Rows[i][0].ToString() == "佳意面包房")
                    || (da.Tables[0].Rows[i][0].ToString() == "三食堂小卖部") || (da.Tables[0].Rows[i][0].ToString() == "三食堂面包房"))
                {
                    _shangpu += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    shangpu += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
                else
                {
                    _qita += "<li>" + da.Tables[0].Rows[i][0].ToString() + ":&nbsp;" + da.Tables[0].Rows[i][1].ToString() + "元</li>";
                    qita += Math.Round(double.Parse(da.Tables[0].Rows[i][1].ToString()), 2);
                }
            }
            shop += "'" + _shitang + "','" + _jiaochao + "','" + _guoshu + "','" + _shangpu + "','" + _yiliao + "','" + _qita + "'";
            sum += shitang + "," + jiaochao + "," + guoshu + "," + shangpu + "," + yiliao + "," + qita;
            pie += " {name: '食堂',y:" + shitang + ",sliced: true,selected: true},['教育超市', " + jiaochao + "],['果蔬', " + guoshu + "],['商铺', " + shangpu + "],['医疗', " + yiliao + "],['其他', " + qita + "]";

            //消费排名
            string dbsum = (shitang + jiaochao + guoshu + shangpu + yiliao + qita).ToString();
            string ranking = GetRanking(id, dbsum, shitang, guoshu, dt.Tables[0].Rows[0][2].ToString(), begintime.ToString("yyyy-MM"));

            //字符串拼接
            str += shop + "]," + sum + "]," + pie + "]," + xm + "," + ranking + ",\"photo\":\"" + "none" + "\"}";

            context.Response.ContentType = "text/plain";
            context.Response.Write(str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sum"></param>
        /// <summary>
        /// 获取排名
        /// </summary>
        /// <param name="id">学号</param>
        /// <param name="sum">个人消费总额</param>
        /// <param name="stsum">食堂消费总额</param>
        /// <param name="guoshu">果蔬消费总额</param>
        /// <param name="sex">性别</param>
        /// <param name="time">时间</param>
        /// <returns></returns>
        protected string GetRanking(string id, string sum, double stsum, double guoshu, string sex, string time)
        {
            string str = "\"rank\":['";
            string rankst = "\"rankst\":['";
            string percent = "\"percent\":['{0}<span style=\"font-size:20px;\">%</span>'],";
            string percentst = "\"percentst\":['{0}<span style=\"font-size:20px;\">%</span>'],";
            string conclusion1 = "\"conclusion1\":['本月您超过了<span class=\"outstand\">{0}%</span>的人'],";
            string conclusion2 = "\"conclusion2\":['被誉为:<span class=\"outstand\">{0}</span><span class=\"outstand\">{1}</span>'],";
            //综合消费比重
            List<string> strs = new List<string>();
            if (id.IndexOf("000000") != -1)
            {
                string sql1 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where sum < {0} and STUNO like '000000%' and time = '{1}' and sum <> 0", sum, time);
                string sql2 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where STUNO like '000000%' and time = '{0}' and sum <> 0", time);
                strs.Add(sql1);
                strs.Add(sql2);
            }
            else
            {
                string sql1 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where sum < {0} and STUNO not like '000000%' and time = '{1}' and sum <> 0", sum, time);
                string sql2 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where STUNO not like '000000%' and time = '{0}' and sum <> 0", time);
                strs.Add(sql1);
                strs.Add(sql2);
            }
            //食堂消费比重
            List<string> st = new List<string>();
            if (id.IndexOf("000000") != -1)
            {
                string sql1 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where shitang < {0} and STUNO like '000000%' and time = '{1}' and sum <> 0", stsum, time);
                string sql2 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where STUNO like '000000%' and time = '{0}' and sum <> 0", time);
                st.Add(sql1);
                st.Add(sql2);
            }
            else
            {
                string sql1 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where shitang < {0} and STUNO not like '000000%' and time = '{1}' and sum <> 0", stsum, time);
                string sql2 = string.Format("select count(*) from dbo.ORG_NE_T_BILL where STUNO not like '000000%' and time = '{0}' and sum <> 0", time);
                st.Add(sql1);
                st.Add(sql2);
            }
            try
            {
                DataSet dt = DBUtility.SqlToTable(strs);
                DataSet stdt = DBUtility.SqlToTable(st);
                //消费百分百
                double rk = Math.Round(double.Parse(dt.Tables[0].Rows[0][0].ToString()) / double.Parse(dt.Tables[1].Rows[0][0].ToString()), 2);
                double strk = Math.Round(double.Parse(stdt.Tables[0].Rows[0][0].ToString()) / double.Parse(stdt.Tables[1].Rows[0][0].ToString()), 2);
                percent = string.Format(percent, rk * 100);
                percentst = string.Format(percentst, strk * 100);

                //结论
                conclusion1 = string.Format(conclusion1, rk * 100);
                conclusion2 = string.Format(conclusion2, conclusionST(Math.Round((stsum + guoshu) / double.Parse(sum), 2)), conclusionZH(rk));

                //综合图形展示
                double zhanshi = Math.Round(rk * 10, 0);
                for (int i = 1; i <= 10; i++)
                {
                    if (sex == "2")
                    {
                        if (i <= zhanshi)
                            str += "<li class=\"girl girlwin\"></li>";
                        else
                            str += "<li class=\"girl\"></li>";
                    }
                    else
                    {
                        if (i <= zhanshi)
                            str += "<li class=\"boy boywin\"></li>";
                        else
                            str += "<li class=\"boy\"></li>";
                    }
                }
                //食堂图形展示
                double stzhanshi = Math.Round(strk * 10, 0);
                for (int j = 1; j <= 10; j++)
                {
                    if (sex == "2")
                    {
                        if (j <= stzhanshi)
                            rankst += "<li class=\"girl girlwin\"></li>";
                        else
                            rankst += "<li class=\"girl\"></li>";
                    }
                    else
                    {
                        if (j <= stzhanshi)
                            rankst += "<li class=\"boy boywin\"></li>";
                        else
                            rankst += "<li class=\"boy\"></li>";
                    }
                }
                str = percent + percentst + conclusion1 + conclusion2 + rankst + "']," + str;
            }
            catch (Exception e)
            {
                throw e;
            }
            return str + "']";
        }
        /// <summary>
        /// 总消费结论
        /// </summary>
        /// <param name="rk">比例</param>
        /// <returns></returns>
        protected string conclusionZH(double rk)
        {
            string str = "";
            if (rk >= 0.9)
            {
                str = "钻石卡";
            }
            else if (rk >= 0.8)
            {
                str = "金卡";
            }
            else if (rk >= 0.7)
            {
                str = "银卡";
            }
            else if (rk >= 0.6)
            {
                str = "普通卡";
            }
            else if (rk > 0)
            {
                str = "勤俭卡";
            }
            return str;
        }
        /// <summary>
        /// 食堂消费结论
        /// </summary>
        /// <param name="rk">比例</param>
        /// <returns></returns>
        protected string conclusionST(double rk)
        {
            string str = "";
            if (rk >= 0.8)
            {
                str = "持家型";
            }
            else if (rk >= 0.6)
            {
                str = "均衡型";
            }
            else if (rk > 0)
            {
                str = "享受型";
            }
            else
            {
                str = "没有消费";
            }
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