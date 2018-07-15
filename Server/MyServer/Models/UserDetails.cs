using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class UserDetails
    {
        /// <summary>
        /// 用户模型
        /// </summary>
        public class UserModel
        {
            public string Id { get; set; }
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
        }

        public interface IUserRepository
        {
            /// <summary>
            /// 是否存在该用户名
            /// </summary>
            /// <param name="UserName">用户名</param>
            /// <returns>存在为true</returns>
            bool IsExists(string UserName);
            /// <summary>
            /// 创建用户
            /// </summary>
            /// <param name="model">用户模型</param>
            /// <returns>受影响的行数</returns>
            int CreateUser(UserModel model);
            /// <summary>
            /// 获取用户对象
            /// </summary>
            /// <param name="UserName">用户名</param>
            /// <returns>用户模型</returns>
            UserModel GetModel(string UserName);
            /// <summary>
            /// 更新用户对象
            /// </summary>
            /// <param name="model">用户模型</param>
            /// <param name="UserName">用户名</param>
            /// <returns>受影响的行数</returns>
            int Update(UserModel model, string UserName);
        }

        public class UserRepository : IUserRepository
        {

            /// <summary>
            /// 根据Account查询是否存在记录
            /// </summary>
            /// <param name="Account">用户名</param>
            /// <returns>存在:true;不存在:false</returns>
            public bool IsExists(string UserName)
            {
                string strSql = "SELECT COUNT(*) FROM dbo.UserInfo WHERE UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };

                parameters[0].Value = UserName;//参数赋值

                return Convert.ToInt32(DBHelper.ExcuteSqlScalar(strSql, parameters)) > 0 ? true : false;
            }

            public int CreateUser(UserModel model)
            {
                if (!IsExists(model.UserName))
                {
                    string guid = Guid.NewGuid().ToString();
                    string userName = model.UserName;
                    string password = EncriptHelper.ToMd5(model.Password);
                    string strSql = "INSERT INTO dbo.UserInfo VALUES(@id,@userName,@password)";
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                        new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                        new SqlParameter("@password",System.Data.SqlDbType.VarChar,50)
                    };
                    parameters[0].Value = guid;
                    parameters[1].Value = userName;
                    parameters[2].Value = password;
                    return DBHelper.ExecuteNonQuery(strSql, parameters);
                }
                return -1;
            }

            public UserModel GetModel(string UserName)
            {
                string strSql = "SELECT * FROM dbo.UserInfo WHERE UserName=@userName";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = UserName;

                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    if(sdr.Read())
                    {
                        UserModel model = new UserModel();
                        model.UserName = sdr["UserName"].ToString();
                        model.Password = sdr["Password"].ToString();

                        return model;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// 更新一条用户记录
            /// </summary>
            /// <param name="model">用户实体</param>
            /// <param name="UserName">用户名（更新前的用户名）</param>
            /// <returns>影响的行数</returns>
            public int Update(UserModel model, string UserName)
            {
                string strSql = "UPDATE dbo.UserInfo SET UserName=@userName,Password=@password WHERE UserName=@userName_Old";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userName",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@password",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@userName_Old",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = model.UserName;
                parameters[1].Value = model.Password;
                parameters[2].Value = UserName;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }
        }
    }
}