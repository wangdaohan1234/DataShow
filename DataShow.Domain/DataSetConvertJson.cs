using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DataShow.Domain
{
    public class DataSetConvertJson
    {
        #region dataTable转换成Json格式
        /// <summary>  
        /// dataTable转换成Json格式  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static string DataTable2Json(DataTable dt)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            //jsonBuilder.Append("{");
            //jsonBuilder.Append(dt.TableName);
            //jsonBuilder.Append(":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\": \"");
                    if (dt.Rows[i][j].ToString().IndexOf("\"") != -1)
                        jsonBuilder.Append(dt.Rows[i][j].ToString().Replace("\"", ""));
                    else
                        jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            if (dt.Rows.Count > 0)
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            //jsonBuilder.Append("]");
            //jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }
        #endregion dataTable转换成Json格式

        #region DataSet转换成Json格式
        /// <summary>  
        /// DataSet转换成Json格式  
        /// </summary>  
        /// <param name="ds">DataSet</param> 
        /// <returns></returns>  
        public static string Dataset2Json(DataSet ds)
        {
            StringBuilder json = new StringBuilder();
            //第一张表
            json.Append(DataTable2Json(ds.Tables[0]));
            json.Replace('}', ',');
            json.Replace("TOTALCOUNT", "totalCount");
            //第二张表
            json.Append(ds.Tables[1].TableName);
            json.Append(":[");
            json.Append(DataTable2Json(ds.Tables[1]));
            json.Append("]");
            json.Append("}");
            ds.Dispose();
            return json.ToString();
        }
        #endregion
        #region DataSet转换成Json格式
        /// <summary>  
        /// DataSet转换成Json格式  
        /// </summary>  
        /// <param name="totalCount">总条数</param> 
        /// <param name="ds">单张表的dataset</param> 
        /// <returns></returns>  
        public static string Dataset2Json(int totalCount, DataSet ds)
        {
            StringBuilder json = new StringBuilder();
            //总条数
            json.Append("{");
            json.Append("\"TOTALCOUNT\":\"");
            json.Append(totalCount);
            json.Append("\",");
            //第二张表
            json.Append(ds.Tables[1].TableName);
            json.Append(":[");
            json.Append(DataTable2Json(ds.Tables[1]));
            json.Append("]");
            json.Append("}");
            ds.Dispose();
            return json.ToString();
        }
        #endregion

        #region 将DataSet转化为Json（柱状图/折线图）（仅支持单张表格操作）
        /// <summary>
        /// 将DataSet转化为Json（柱状图）（仅支持单张表格操作）
        /// 使用方式，data为数据，xAxis为横轴信息，time为检索时间段
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="begtime">开始时间</param>
        /// <param name="endtime">结束时间</param>
        /// <param name="count">分成几块</param>
        /// <param name="title">标题</param>
        /// <returns>Json字符串</returns>
        public static string TransformToColumnJson(DataSet dt, string begtime, string endtime, int count, string title)
        {
            string str = "{data:[";
            string[] items = new string[dt.Tables[0].Columns.Count - 1];
            string xAxis = "xAxis:[";
            Double[] sum = new Double[dt.Tables[0].Columns.Count - 1];
            Double[] qita = new Double[dt.Tables[0].Columns.Count - 1];
            for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
            {
                qita[j] = 0;
                sum[j] = 0;
            }
            for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
            {
                //大于规定块数的全部归为"其他"
                if (i > count)
                {
                    for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
                    {
                        qita[j] += Double.Parse(dt.Tables[0].Rows[i][j + 1].ToString());
                        sum[j] += Double.Parse(dt.Tables[0].Rows[i][j + 1].ToString());
                    }
                    continue;
                }
                if (i == 0)
                {
                    for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
                    {
                        items[j] += dt.Tables[0].Rows[i][j + 1];
                        sum[j] += Double.Parse(dt.Tables[0].Rows[i][j + 1].ToString());
                    }
                    xAxis += "'" + dt.Tables[0].Rows[i][0] + "'";
                }
                else
                {
                    for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
                    {
                        items[j] += "," + dt.Tables[0].Rows[i][j + 1];
                        sum[j] += Double.Parse(dt.Tables[0].Rows[i][j + 1].ToString());
                    }
                    xAxis += ",'" + dt.Tables[0].Rows[i][0] + "'";
                }
            }
            if (count < dt.Tables[0].Rows.Count)
            {
                for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
                {
                    items[j] += "," + qita[j];
                }
                xAxis += ",'其他'";
            }
            string sum_c = "sum:[";
            for (int j = 0; j < dt.Tables[0].Columns.Count - 1; j++)
            {
                if (j == 0)
                {
                    str += "{name:'" + dt.Tables[0].Columns[j + 1].ColumnName + "',data:[" + items[j] + "]}";
                    sum_c += sum[j];
                }
                else
                {
                    str += ",{name:'" + dt.Tables[0].Columns[j + 1].ColumnName + "',data:[" + items[j] + "]}";
                    sum_c += "," + sum[j];
                }
            }
            if (endtime != begtime)
                str += "]," + xAxis + "],time:\"" + begtime + "-" + endtime + "\",title:\"" + title + "\"," + sum_c + "]}";
            else
                str += "]," + xAxis + "],time:\"" + begtime + "\",title:\"" + title + "\"," + sum_c + "]}";
            return str;
        }
        #endregion 将DataSet转化为Json（/折线图）（仅支持单张表格操作）

        #region 将DataSet转化为Json（饼图）（仅支持单张表格操作）
        /// <summary>
        /// 将DataSet转化为Json（饼图）（仅支持单张表格操作）
        /// 使用方式，pie为数据，time为检索时间段
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="begtime">开始时间</param>
        /// <param name="endtime">结束时间</param>
        /// <param name="count">分成几块</param>
        /// <param name="title">标题</param>
        /// <returns>Json字符串</returns>
        public static string TransformToPieJson(DataSet dt, string begtime, string endtime, int count, string title)
        {
            string str = "{";
            string pie = "\"pie\":[";
            Double qita = 0;
            Double sum = 0;
            for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
            {
                //大于规定块数的全部归为"其他"
                if (i > count)
                {
                    qita += Double.Parse(dt.Tables[0].Rows[i][1].ToString());
                    sum += Double.Parse(dt.Tables[0].Rows[i][1].ToString());
                    continue;
                }
                if (i == 0)
                    pie += " {name: '" + dt.Tables[0].Rows[i][0].ToString() + "',y:" + dt.Tables[0].Rows[i][1].ToString() + ",sliced: true,selected: true}";
                else
                    pie += string.Format(",['{0}', {1}]", dt.Tables[0].Rows[i][0].ToString(), dt.Tables[0].Rows[i][1].ToString());

                sum += Double.Parse(dt.Tables[0].Rows[i][1].ToString());
            }
            if (qita > 0)
                pie += string.Format(",['其他', {0}]", qita);
            str += pie + "],time:\"" + begtime + "-" + endtime + "\",title:\"" + title + "\",sum:" + sum + "}";
            return str;
        }
        #endregion 将DataSet转化为Json（饼图）（仅支持单张表格操作）

        #region 将DataSet转化为校情所需数据
        /// <summary>
        /// 将DataSet转化为Json（饼图）（仅支持单张表格操作）
        /// 使用方式，data为数据，time为时间段
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="time">时间</param>
        /// <param name="title">标题</param>
        /// <returns>Json字符串</returns>
        public static string TransformToXiaoQingJson(DataSet dt, string time, string title)
        {
            string str = "{";
            string data = "'data':{";
            for (int i = 1; i < dt.Tables[0].Columns.Count; i = i + 2)
            {
                if (dt.Tables[0].Rows.Count > 0)
                {
                    if (i == 1)
                        data += "'" + dt.Tables[0].Columns[i].ColumnName + "':[ '" + dt.Tables[0].Rows[0][i].ToString() + "','" + dt.Tables[0].Rows[0][i + 1].ToString() + "']";
                    else
                        data += ",'" + dt.Tables[0].Columns[i].ColumnName + "':[ '" + dt.Tables[0].Rows[0][i].ToString() + "','" + dt.Tables[0].Rows[0][i + 1].ToString() + "']";
                }
            }
            str += data + "},time:'" + time + "',title:'" + title + "'}";
            return str;
        }
        #endregion 将DataSet转化为校情所需数据
    }
}
