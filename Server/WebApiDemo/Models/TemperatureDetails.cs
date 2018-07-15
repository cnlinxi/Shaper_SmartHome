using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class TemperatureDetails
    {
        public class TemperatureModel
        {
            public string id { get; set; }
            [Required]
            public string userName { get; set; }
            [Required]
            public string temperature { get; set; }
            [Required]
            public string humidity { get; set; }
            public string addTime { get; set; }
        }

        public interface ITemptureRepository
        {
            int CreateTempture(TemperatureModel model);
            TemperatureModel GetTempture(string userName);
        }

        public class TemptureRepository : ITemptureRepository
        {
            public int CreateTempture(TemperatureModel model)
            {
                string userName = model.userName;
                string tempturature = model.temperature;
                string humidity = model.humidity;
                string strSql = "INSERT INTO dbo.temperature(Id,UserName,Temperature,Humidity) VALUES(@id,@userName,@temperature,@humidity)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@temperature",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@humidity",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = tempturature;
                parameters[3].Value = humidity;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public TemperatureModel GetTempture(string userName)
            {
                string strSql = "SELECT TOP 1 * FROM dbo.temperature WHERE UserName=@userName ORDER BY AddTime DESC";
                SqlParameter[] parameters =
                {
                    new SqlParameter("userName",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = userName;

                TemperatureModel model = new TemperatureModel();
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    if(sdr.Read())
                    {
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.temperature =sdr["Temperature"].ToString();
                        model.humidity = sdr["Humidity"].ToString();
                        model.addTime = sdr["AddTime"].ToString();
                    }
                }
                return model;
            }
        }
    }
}