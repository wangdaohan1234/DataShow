using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DataShow.Domain
{
    public class DBUtility
    {
        private static string connectstring = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static DataSet ProceureToTable(string ProName, SqlParameter[] parm)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connectstring);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProName;
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
    }
}
