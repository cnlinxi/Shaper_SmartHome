using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class CommandDetails
    {
         public class CommandChannelModel
        {
            public string id { get; set; }
            [Required]
            public string userName { get; set; }
            [Required]
            public string channelUri { get; set; }
            public string addTime { get; set; }
            public DateTime expirationTime { get; set; }
        }

        public class CommandModel
        {
            public string userName { get; set; }
            public string commandTarget { get; set; }
            public string commandContent { get; set; }
        }

        public interface ICommandRepository
        {
            /// <summary>
            /// 创建命令通道
            /// </summary>
            /// <param name="model">命令通道实体</param>
            /// <returns>影响行数</returns>
            int CreateCommandChannel(CommandChannelModel model);
            /// <summary>
            /// 获取命令通道实体
            /// </summary>
            /// <param name="userName">用户名</param>
            /// <returns>命令通道实体</returns>
            List<CommandChannelModel> GetModel(string userName);
            /// <summary>
            /// 更新命令通道
            /// </summary>
            /// <param name="model">配置命令通道实体</param>
            /// <param name="userName">用户名</param>
            /// <returns>影响行数</returns>
            int Update(CommandChannelModel model, string userName);
        }

        public class CommandRepository : ICommandRepository
        {
            public int CreateCommandChannel(CommandChannelModel model)
            {
                string userName = model.userName;
                string chanelUri = model.channelUri;
                DateTime expirationTime = model.expirationTime;
                string strSql = "INSERT INTO dbo.command(Id,UserName,ChannelUri,ExpirationTime) VALUES(@id,@userName,@channelUri,@expirationTime)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@channelUri",System.Data.SqlDbType.VarChar,500),
                    new SqlParameter("@expirationTime",System.Data.SqlDbType.DateTime)
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = chanelUri;
                parameters[3].Value = expirationTime;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public List<CommandChannelModel> GetModel(string userName)
            {
                List<CommandChannelModel> list = new List<CommandChannelModel>();
                string strSql = "SELECT * FROM dbo.command WHERE DATEDIFF(day,AddTime,GETDATE())>=0 AND DATEDIFF(day,GETDATE(),ExpirationTime)>=0 AND UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = userName;
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    while (sdr.Read())
                    {
                        CommandChannelModel model = new CommandChannelModel();
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.channelUri = sdr["ChannelUri"].ToString();
                        model.addTime = sdr["AddTime"].ToString();
                        list.Add(model);
                    }
                    return list;
                }
            }

            public int Update(CommandChannelModel model, string userName)
            {
                string strSql = "UPDATE dbo.command SET ChannelUri=@channelUri,AddTime=GETDATE(),ExpirationTime=@expirationTime WHERE UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@channelUri",System.Data.SqlDbType.VarChar,500),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@expirationTime",System.Data.SqlDbType.DateTime)
                };
                parameters[0].Value = model.channelUri;
                parameters[1].Value = userName;
                parameters[2].Value = model.expirationTime;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }
        }
    }
}