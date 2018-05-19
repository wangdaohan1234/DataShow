using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace DataShow.Domain
{
    public class DBUtility
    {
        private static string connectstring = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static DataSet SqlToTable(List<string> sts)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 180;
            SqlConnection con = new SqlConnection(connectstring);
            try
            {
                con.Open();
                for(int i = 0; i < sts.Count; i++)
                {
                    cmd.Connection = con;
                    cmd.CommandText = sts[i];
                    da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);
                    dt = new DataTable();
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    da.Dispose();
                    con.Close();
                    con.Dispose();
                }
            }
            return ds;
        }
        public static DataSet ProceureToTable(string ProName, SqlParameter[] parm)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connectstring);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProName;
            cmd.CommandTimeout = 180;
            try
            {
                con.Open();
                cmd.Connection = con;
                for (int i = 0; i < parm.Length; i++)
                {
                    cmd.Parameters.Add(parm[i]);
                }
                da.SelectCommand = cmd;
                da.Fill(ds);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
            return ds;
        }
        public static int RunSQL(string st)
        {
            int count = 0;
            SqlConnection con = new SqlConnection(connectstring);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = st;
                count = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                con.Dispose();
            }
            return count;
        }
        // MD5　32位加密
        public static string MD5Encrypt(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 

                pwd = pwd + s[i].ToString("X").PadLeft(2, '0');

            }
            return pwd;
        }
    }
}
