using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class ConfigurationDetails
    {
        public class ConfigurationModel
        {
            public string id { get; set; }
            [Required]
            public string userName { get; set; }
            [Required]
            public string channelUri { get; set; }
            public string addTime { get; set; }
            public DateTime expirationTime { get; set; }
        }

        public interface IConfigurationRepository
        {
            /// <summary>
            /// 创建配置
            /// </summary>
            /// <param name="model">配置实体</param>
            /// <returns>影响行数</returns>
            int CreateConfiguration(ConfigurationModel model);
            /// <summary>
            /// 获取配置
            /// </summary>
            /// <param name="userName">用户名</param>
            /// <returns>Channel实体</returns>
            List<ConfigurationModel> GetModel(string userName);
            /// <summary>
            /// 更新配置
            /// </summary>
            /// <param name="model">配置实体</param>
            /// <param name="key">配置键名</param>
            /// <returns>影响行数</returns>
            int Update(ConfigurationModel model, string userName);
        }

        public class ConfigurationRepository : IConfigurationRepository
        {
            public int CreateConfiguration(ConfigurationModel model)
            {
                string userName = model.userName;
                string chanelUri = model.channelUri;
                DateTime expirationTime = model.expirationTime;
                string strSql = "INSERT INTO dbo.notification(Id,UserName,ChannelUri,ExpirationTime) VALUES(@id,@userName,@channelUri,@expirationTime)";
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

            public List<ConfigurationModel> GetModel(string userName)
            {
                List<ConfigurationModel> list = new List<ConfigurationModel>();
                string strSql = "SELECT * FROM dbo.notification WHERE DATEDIFF(day,AddTime,GETDATE())>=0 AND DATEDIFF(day,GETDATE(),ExpirationTime)>=0 AND UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = userName;
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    while(sdr.Read())
                    {
                        ConfigurationModel model = new ConfigurationModel();
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.channelUri = sdr["ChannelUri"].ToString();
                        model.addTime = sdr["AddTime"].ToString();
                        list.Add(model);
                    }
                    return list;
                }
            }

            public int Update(ConfigurationModel model, string userName)
            {
                string strSql = "UPDATE dbo.notification SET ChannelUri=@channelUri,AddTime=GETDATE(),ExpirationTime=@expirationTime WHERE UserName=@userName";
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
