using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace loT4WebApiSample.Helpers
{
    public class BlobHelper
    {
        //本地数据容器，重装可同步
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //多平台同步容器
        private StorageFolder roamdingFolder = ApplicationData.Current.RoamingFolder;
        ApplicationDataContainer roamdingSettings = ApplicationData.Current.RoamingSettings;
        string ContainerName { get; set; }
        CloudBlobContainer cloudBlobContainer { get; set; }
        public BlobHelper() { }
        public BlobHelper(string storageAccountName,string storageAccountKey,string containerName)
        {
            cloudBlobContainer = SetUpContainer(storageAccountName, storageAccountKey, containerName);
            ContainerName = containerName;
        }

        /// <summary>
        /// 初始化BlobContainer
        /// </summary>
        /// <param name="storageAccountName"></param>
        /// <param name="storageAccountKey"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private CloudBlobContainer SetUpContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            string connectionString = string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                    storageAccountName, storageAccountKey);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            return cloudBlobContainer;
        }

        /// <summary>
        /// 此方法可初始化BlobMethod类，并且保证Blob权限在有URL即可访问内容
        /// </summary>
        /// <param name="storageAccountName"></param>
        /// <param name="storageAccountKey"></param>
        /// <param name="containerName"></param>
        public async void RunatAppStartUp(string storageAccountName, string storageAccountKey, string containerName)
        {
            cloudBlobContainer = 
                SetUpContainer(storageAccountName, storageAccountKey, containerName);
            //await cloudBlobContainer.CreateIfNotExistsAsync();

            ContainerName = containerName;

            BlobContainerPermissions permission = new BlobContainerPermissions();
            permission.PublicAccess = BlobContainerPublicAccessType.Blob;
            await cloudBlobContainer.SetPermissionsAsync(permission);
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="strFileName">图片传入Blob的文件名</param>
        /// <param name="imageFile">StorageFile类型的File对象</param>
        /// <returns></returns>
        public async Task uploadImage(string strFileName,StorageFile imageFile)
        {
            CloudBlockBlob blob =
                cloudBlobContainer.GetBlockBlobReference(strFileName);
            await blob.UploadFromFileAsync(imageFile);
            roamdingSettings.Values["isAvatarModify"] = true;
        }

        /// <summary>
        /// 从Blob获取文件流
        /// </summary>
        /// <param name="strFileName">Blob上文件名称</param>
        /// <returns>IRandomAccessStream</returns>
        public async Task<IRandomAccessStream> downloadFile(string strFileName)
        {
            CloudBlockBlob blobSource =
                cloudBlobContainer.GetBlockBlobReference(strFileName);
            await blobSource.FetchAttributesAsync();
            long fileLength = blobSource.Properties.Length;
            byte[] bytes = new byte[fileLength];
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            DataWriter datawriter = new DataWriter(memoryStream.GetOutputStreamAt(0));
            datawriter.WriteBytes(bytes);
            await datawriter.StoreAsync();
            //IRandomAccessStream stream = new InMemoryRandomAccessStream();
            //await blobSource.DownloadToStreamAsync(stream);
            return memoryStream;
        }

        /// <summary>
        /// 向应用的本地文件夹存储Blob文件
        /// </summary>
        /// <param name="strFileName">Blob上文件名称</param>
        /// <returns></returns>
        public async Task downloadFileAndStorage(string strFileName)
        {
            CloudBlockBlob blobSource =
                cloudBlobContainer.GetBlockBlobReference(strFileName);
            await blobSource.FetchAttributesAsync();
            DateTimeOffset? lastModifyTime = blobSource.Properties.LastModified;
            //App.avatarLastModifyTime = Convert.ToDateTime(lastModifyTime);
            bool isBlobExist = await blobSource.ExistsAsync();
            if (isBlobExist)
            {
                roamdingSettings.Values["avatarLastModifyTime"] = lastModifyTime;
                StorageFile file =
                    await localFolder.CreateFileAsync(strFileName, CreationCollisionOption.ReplaceExisting);
                await blobSource.DownloadToFileAsync(file);
            }
            //if (isBlobExist)
            //{
            //    string localPath = Path.Combine(downloadFolder,blobSource.Name.Replace(@"/",@"\"));
            //    string dirPath = Path.GetDirectoryName(localPath);
            //    if(!Directory.Exists(dirPath))
            //    {
            //        Directory.CreateDirectory(dirPath);
            //    }
            //}
        }
    }
}
