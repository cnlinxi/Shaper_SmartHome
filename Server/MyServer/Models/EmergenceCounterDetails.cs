using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class EmergenceCounterDetails
    {
        public class EmergenceCounterModel
        {
            public string id { get; set; }
            public string userName { get; set; }
            public string fireCounter { get; set; }
            public string strangerCounter { get; set; }
            public string counter { get; set; }
            public string addTime { get; set; }
        }

        public interface IEmergenceCounterRepository
        {
            int CreateEmergenceCounter(EmergenceCounterModel model);
            List<EmergenceCounterModel> GetModels(string userName);
        }

        public class EmergenceCounterRepository : IEmergenceCounterRepository
        {
            public int CreateEmergenceCounter(EmergenceCounterModel model)
            {
                string userName = model.userName;
                string counter = model.counter;
                string fireCounter = model.fireCounter;
                string strangerCounter = model.strangerCounter;
                string strSql = "INSERT INTO dbo.emergenceCounter(Id,UserName,FireCounter,StrangerCounter,Counter) VALUES(@id,@userName,@fireCounter,@strangerCounter,@counter)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@fireCounter",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@strangerCounter",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@counter",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = fireCounter;
                parameters[3].Value = strangerCounter;
                parameters[4].Value = Convert.ToInt32(fireCounter) + Convert.ToInt32(strangerCounter);

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public List<EmergenceCounterModel> GetModels(string userName)
            {
                List<EmergenceCounterModel> list = new List<EmergenceCounterModel>();
                string strSql = "SELECT TOP 3 * FROM dbo.emergenceCounter WHERE DATEDIFF(day,AddTime,GETDATE())>=0 AND DATEDIFF(day,AddTime,GETDATE())<=3 AND UserName=@userName ORDER BY AddTime DESC";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = userName;
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    while (sdr.Read())
                    {
                        EmergenceCounterModel model = new EmergenceCounterModel();
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.fireCounter = sdr["FireCounter"].ToString();
                        model.strangerCounter = sdr["StrangerCounter"].ToString();
                        model.counter = sdr["Counter"].ToString();
                        model.addTime = sdr["AddTime"].ToString();
                        list.Add(model);
                    }
                    return list;
                }
            }
        }
    }
}