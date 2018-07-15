using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApiDemo.Utils;

namespace WebApiDemo.Models
{
    public class InformationDetails
    {
        public class InformationModel
        {
            public string Id { get; set; }
            public string Content { get; set; }
        }

        public interface IInformationRepository
        {
            /// <summary>
            /// 创建信息
            /// </summary>
            /// <param name="model">信息实体</param>
            /// <returns>影响行数</returns>
            int CreateInformation(InformationModel model);
            /// <summary>
            /// 获取信息
            /// </summary>
            /// <param name="id">信息Id</param>
            /// <returns>信息实体</returns>
            InformationModel GetModel(string id);
            /// <summary>
            /// 更新信息
            /// </summary>
            /// <param name="model">信息实体</param>
            /// <param name="id">信息id</param>
            /// <returns>影响行数</returns>
            int Update(InformationModel model, string id);
        }

        public class ConfigurationRepository : IInformationRepository
        {
            public int CreateInformation(InformationModel model)
            {
                string id = model.Id;
                string content = model.Content;
                string strSql = "INSERT INTO dbo.information VALUES(@id,@content)";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@content",System.Data.SqlDbType.NVarChar,2000)
                };

                parameters[0].Value = id;
                parameters[1].Value = content;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }

            public InformationModel GetModel(string id)
            {
                string strSql = "SELECT * FROM dbo.information WHERE Id=@id";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50)
                };
                parameters[0].Value = id;
                using (SqlDataReader sdr = DBHelper.ExecuteSqlReader(strSql, parameters))
                {
                    if (sdr.Read())
                    {
                        InformationModel model = new InformationModel();
                        model.Id = sdr["Id"].ToString();
                        model.Content = sdr["Content"].ToString();
                        return model;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public int Update(InformationModel model, string id)
            {
                string strSql = "UPDATE dbo.information SET Content=@content WHERE Id=@id";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@id",System.Data.SqlDbType.VarChar,50),
                    new SqlParameter("@content",System.Data.SqlDbType.NVarChar,2000)
                };
                parameters[0].Value = id;
                parameters[1].Value = model.Content;

                return DBHelper.ExecuteNonQuery(strSql, parameters);
            }
        }
    }
}