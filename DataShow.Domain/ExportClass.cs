using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DataShow.Domain
{
    public class ExportClass
    {
        /// <summary>
        /// DataSet数据集导出到Excel
        /// </summary>
        /// <param name="dgv">DataSet控件</param>
        /// <param name="strTitle">导出的Excel标题</param>
        public static void DataGridViewExportToExcel(DataSet ds, string strTitle)
        {
            if (strTitle == "")
                strTitle = DateTime.Now.ToString("yyyyMMdd");
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-xls";
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            DataGrid dg = new DataGrid();
            dg.DataSource = ds;
            dg.DataBind();
            //格式化为字符串
            for (int i = 0; i < dg.Items.Count; i++)
                dg.Items[i].Cells[0].Attributes.Add("style", "vnd.ms-excel.numberformat:@");
            dg.RenderControl(htmlWrite);
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(strTitle) + ".xls");
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
        }
    }
}
