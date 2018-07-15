using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class DeviceInitializationDetails
    {
        public class DeviceInitializationModel
        {
            public string id { get; set; }
            public string userName { get; set; }
            public string authCode { get; set; }
        }

        public interface IDeviceInitializationRepository
        {
            int CreateDeviceInitialization(DeviceInitializationModel model);
            DeviceInitializationModel GetModel(string authCode);
            int Update(string userName, DeviceInitializationModel model);
        }

        public class DeviceInitializationRepositoty : IDeviceInitializationRepository
        {
            public int CreateDeviceInitialization(DeviceInitializationModel model)
            {
                string userName = model.userName;
                string authCode = "1";//将authCode初始化为1
                string strSql = "INSERT INTO dbo.deviceInitialization(Id,UserName,AuthCode) VALUES(@id,@userName,@authCode)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@authCode",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = authCode;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public DeviceInitializationModel GetModel(string authCode)
            {
                string strSql = "SELECT TOP 1 * FROM dbo.deviceInitialization WHERE AuthCode=@authCode AND DateDiff(n,AddTime,getdate())<30 AND DateDiff(n,AddTime,getdate())>=0 ORDER BY AddTime DESC";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@authCode",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = authCode;

                DeviceInitializationModel model = new DeviceInitializationModel();
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    if (sdr.Read())
                    {
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.authCode = sdr["AuthCode"].ToString();
                    }
                }
                return model;
            }

            public int Update(string userName, DeviceInitializationModel model)
            {
                string strSql = "UPDATE dbo.deviceInitialization SET AuthCode=@authCode,AddTime=getdate() WHERE UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@authCode",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = model.authCode;
                parameters[1].Value = userName;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }
        }
    }
}