using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class EmergencePictureDetails
    {
        public class EmergencePictureModel
        {
            public string id { get; set; }
            [Required]
            public string userName { get; set; }
            [Required]
            public string pictureUri { get; set; }
            public string addTime { get; set; }
        }

        public interface IEmergencePictureRepository
        {
            int CreateEmergencePicture(EmergencePictureModel model);
            List<EmergencePictureModel> GetModels(string userName);
            int DeleteEmergencePictureUri(string userName,string pictureUri);
        }

        public class EmergencePictureRepository : IEmergencePictureRepository
        {
            public int CreateEmergencePicture(EmergencePictureModel model)
            {
                string userName = model.userName;
                string pictureUri = model.pictureUri;
                string strSql = "INSERT INTO dbo.emergence_picture(Id,UserName,PictureUri) VALUES(@id,@userName,@pictureUri)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@pictureUri",System.Data.SqlDbType.VarChar,200),
                };

                parameters[0].Value = Guid.NewGuid().ToString("N");
                parameters[1].Value = userName;
                parameters[2].Value = pictureUri;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public int DeleteEmergencePictureUri(string userName,string pictureUri)
            {
                string strSql = "DELETE FROM dbo.emergence_picture WHERE PictureUri=@pictureUri AND UserName=@userName";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@pictureUri",System.Data.SqlDbType.VarChar,200),
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = pictureUri;
                parameters[1].Value = userName;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public List<EmergencePictureModel> GetModels(string userName)
            {
                List<EmergencePictureModel> list = new List<EmergencePictureModel>();
                string strSql = "SELECT * FROM dbo.emergence_picture WHERE UserName=@userName ORDER BY AddTime DESC";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = userName;
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    while (sdr.Read())
                    {
                        EmergencePictureModel model = new EmergencePictureModel();
                        model.id = sdr["Id"].ToString();
                        model.userName = sdr["UserName"].ToString();
                        model.pictureUri = sdr["PictureUri"].ToString();
                        model.addTime = sdr["AddTime"].ToString();
                        list.Add(model);
                    }
                    return list;
                }
            }
        }
    }
}