using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebApiDemo.Utils
{
    public class EncriptHelper
    {
        /// <summary>
        /// 哈希加密
        /// </summary>
        /// <param name="str">需要加密的文本</param>
        /// <returns>加密后的文本</returns>
        public static string ToMd5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.Default.GetBytes(str));
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 发送方加密
        /// </summary>
        /// <param name="publicKeyXml">公钥</param>
        /// <param name="plainText">要加密的文本</param>
        /// <returns>加密的文本</returns>
        /// 公钥获得：RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        ///                  string publicKeyXml = provider.ToXmlString(false);
        public static string RSAEncrypt(string publicKeyXml,string plainText)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(publicKeyXml);
            byte[] plainData = Encoding.Default.GetBytes(plainText);
            byte[] encrytedData = provider.Encrypt(plainData, true);
            return Convert.ToBase64String(encrytedData);
        }

        /// <summary>
        /// 接收端解密
        /// </summary>
        /// <param name="privateKeyXml">私钥</param>
        /// <param name="encryptedText">已加密文本</param>
        /// <returns>解密的文本</returns>
        /// 私钥（私钥即为公/私钥对）获得：RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        ///                                                     string privateKeyXml = provider.ToXmlString(true);
        public static string RSADecrypt(string privateKeyXml,string encryptedText)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(privateKeyXml);//使用公/私钥初始化对象
            byte[] encryptedData = Convert.FromBase64String(encryptedText);
            byte[] plainData = provider.Decrypt(encryptedData, true);
            string plainText = Encoding.Default.GetString(plainData);
            return plainText;
        }
    }
}