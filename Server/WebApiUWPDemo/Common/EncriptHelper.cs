using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace WebApiUWPDemo
{
    public class EncriptHelper
    {
        public static string ToMd5(string str)
        {
            // Create a string that contains the name of the hashing algorithm to use.
            String strAlgName = HashAlgorithmNames.Md5;

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a CryptographicHash object. This object can be reused to continually
            // hash new messages.
            CryptographicHash objHash = objAlgProv.CreateHash();

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            objHash.Append(buffMsg);
            IBuffer buffHash = objHash.GetValueAndReset();

            // Convert the hashes to string values (for display);
            //return CryptographicBuffer.EncodeToBase64String(buffHash);
            byte[] bytes = new byte[buffHash.Capacity];
            CryptographicBuffer.CopyToByteArray(buffHash, out bytes);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

       public async Task<string> ProtectDataAsync(string strMsg,string strDescriptor,BinaryStringEncoding encoding)
        {
            DataProtectionProvider provider = new DataProtectionProvider(strDescriptor);
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);
            IBuffer buffProtect = await provider.ProtectAsync(buffMsg);
            return CryptographicBuffer.ConvertBinaryToString(encoding, buffProtect);
        }

        public async Task<string> UnprotectDataAsync(string strProtect,BinaryStringEncoding encoding)
        {
            DataProtectionProvider provider = new DataProtectionProvider();
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffProtect = CryptographicBuffer.ConvertStringToBinary(strProtect, encoding);
            IBuffer buffMsg = await provider.UnprotectAsync(buffProtect);
            return CryptographicBuffer.ConvertBinaryToString(encoding, buffMsg);
        }
    }
}