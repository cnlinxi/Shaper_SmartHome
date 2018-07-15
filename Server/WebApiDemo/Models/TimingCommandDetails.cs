using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class TimingCommandDetails
    {
        public class TimingCommandModel
        {
            public string id { get; set; }
            [Required]
            public string userName { get; set; }
            [Required]
            public string command { get; set; }
            public string addTime { get; set; }
        }

        public interface ITimingCommandRepository
        {
            int CreateCommand(TimingCommandModel model);
            TimingCommandModel GetModel(string userName);
            int Update(string userName, TimingCommandModel model);
        }

        public class TimingCommandRepository : ITimingCommandRepository
        {
            public int CreateCommand(TimingCommandModel model)
            {
                string userName = model.userName;
                string command = model.command;
                string strSql = "INSERT INTO dbo.timingCommand(Id,UserName,Command) VALUES(@id,@userName,@command)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@command",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = command;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public TimingCommandModel GetModel(string userName)
            {
                string strSql = "SELECT TOP 1 * FROM dbo.timingCommand WHERE UserName=@userName ORDER BY AddTime DESC";
                SqlParameter[] parameters =
                {
                    new SqlParameter("userName",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = userName;

                TimingCommandModel model = new TimingCommandModel();
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    if (sdr.Read())
                    {
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.command = sdr["Command"].ToString();
                        model.addTime =sdr["AddTime"].ToString();
                    }
                }
                return model;
            }

            public int Update(string userName, TimingCommandModel model)
            {
                string strSql = "UPDATE dbo.timingCommand SET Command=@command WHERE UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@command",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = model.command;
                parameters[1].Value = userName;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }
        }
    }
}