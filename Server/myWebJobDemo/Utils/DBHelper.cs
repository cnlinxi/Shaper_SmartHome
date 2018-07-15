using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace myWebJobDemo.Utils
{
    public class DBHelper
    {
        private readonly static string connectionString = ConfigurationManager.ConnectionStrings["ConnectionStrings"].ConnectionString;

        private static SqlCommand PrepareCommand(SqlConnection con,string cmdText,CommandType type,SqlParameter[] parameters)
        {
            if(con.State!=ConnectionState.Open)
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand(cmdText, con);
            cmd.CommandType = type;
            if(cmd.Parameters!=null)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        /// <summary>
        /// 执行查询，并返回查询结果
        /// </summary>
        /// <param name="strSql">待查询语句</param>
        /// <returns>查询结果</returns>
        public static SqlDataReader ExecuteSqlReader(string strSql)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand(strSql, con);
            return cmd.ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// 执行带参数查询，并返回查询结果
        /// </summary>
        /// <param name="strSql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteSqlReader(string strSql, SqlParameter[] parameters)
        {
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand sqlCmd = PrepareCommand(con, strSql, CommandType.Text, parameters);
            return sqlCmd.ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// 执行非查询的Sql语句
        /// </summary>
        /// <param name="strSql">待执行的Sql语句</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string strSql)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(strSql, con);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行非查询带参数的Sql语句
        /// </summary>
        /// <param name="strSql">待执行Sql语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string strSql, SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = PrepareCommand(con, strSql, CommandType.Text, parameters);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}