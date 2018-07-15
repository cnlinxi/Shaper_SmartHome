using myWebJobDemo.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace myWebJobDemo.Model
{
    /// <summary>
    /// Model写错
    /// </summary>
    public class UserFaceIdDetails
    {
        public string UserName { get; set; }
        public string FaceId { get; set; }
    }

    public interface IUserFaceIdRepository
    {
        int CreateFaceId(UserFaceIdDetails model);
        UserFaceIdDetails GetModel(string userName);
        int UpdateFaceId(UserFaceIdDetails model, string userName);
    }

    public class UserFaceIdRepository : IUserFaceIdRepository
    {
        public int CreateFaceId(UserFaceIdDetails model)
        {
            string userName = model.UserName;
            string faceId = model.FaceId;
            string strSql = "INSERT INTO dbo.faceid VALUES(@userName,@faceId)";
            SqlParameter[] parameters =
            {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@faceId",System.Data.SqlDbType.VarChar,100)
            };

            parameters[0].Value = userName;
            parameters[1].Value = faceId;

            return DBHelper.ExecuteNonQuery(strSql, parameters);
        }

        public UserFaceIdDetails GetModel(string userName)
        {
            string strSql = "SELECT * FROM dbo.faceid WHERE UserName=@userName";
            SqlParameter[] parameters =
            {
                new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
            };
            parameters[0].Value = userName;
            using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
            {
                if (sdr.Read())
                {
                    UserFaceIdDetails model = new UserFaceIdDetails();
                    model.UserName = sdr["UserName"].ToString();
                    model.FaceId = sdr["FaceId"].ToString();
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        public int UpdateFaceId(UserFaceIdDetails model, string userName)
        {
            string strSql = "UPDATE dbo.faceid SET FaceId=@faceId WHERE UserName=@userName";
            SqlParameter[] parameters =
            {
                new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                new SqlParameter("@faceId",System.Data.SqlDbType.VarChar,100)
            };
            parameters[0].Value = model.UserName;
            parameters[1].Value = model.FaceId;

            return DBHelper.ExecuteNonQuery(strSql, parameters);
        }
    }
}