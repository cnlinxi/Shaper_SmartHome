using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WebApiSample.Common
{
    public class HttpServerService : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        HttpServer httpServer;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();

            httpServer = new HttpServer(8000);
            httpServer.StartServer();
        }
    }

    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private int port = 8000;
        private StreamSocketListener listener;
        private AppServiceConnection appServiceConnection;

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            listener.Control.KeepAlive = true;
            listener.Control.NoDelay = true;

            port = serverPort;
            listener.ConnectionReceived += async (s, e) => { await ProcessRequestAsync(e.Socket); };
        }

        public void StartServer()
        {
            Task.Run(async () =>
            {
                await listener.BindServiceNameAsync(port.ToString());

                // Initialize the AppServiceConnection
                appServiceConnection = new AppServiceConnection();
                appServiceConnection.PackageFamilyName = "BlinkyWebService_1w720vyc4ccym";
                appServiceConnection.AppServiceName = "App2AppComService";

                // Send a initialize request 
                var res = await appServiceConnection.OpenAsync();
                if (res != AppServiceConnectionStatus.Success)
                {
                    throw new Exception("Failed to connect to the AppService");
                }
            });
        }

        private async Task ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            StringBuilder request = new StringBuilder();
            byte[] data = new byte[BufferSize];
            IBuffer buffer = data.AsBuffer();
            uint dataRead = BufferSize;
            using (IInputStream input = socket.InputStream)
            {
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            string requestAsString = request.ToString();
            if(requestAsString.Length>0)
            {
                WriteResponse(requestAsString, socket);
            }
            //string[] splitRequestAsString = requestAsString.Split('\n');
            //if (splitRequestAsString.Length != 0)
            //{
            //    string requestMethod = splitRequestAsString[0];
            //    string[] requestParts = requestMethod.Split(' ');
            //    if (requestParts.Length > 1)
            //    {
            //        if (requestParts[0] == "GET")
            //            WriteResponse(requestParts[1], socket);
            //        else
            //            throw new InvalidDataException("HTTP method not supported: "
            //                + requestParts[0]);
            //    }
            //}
        }

        private void WriteResponse(string requestContent, StreamSocket socket)
        {
            var updateMessage = new ValueSet();
            updateMessage.Add("Command", requestContent);
#pragma warning disable CS4014
            appServiceConnection.SendMessageAsync(updateMessage);
#pragma warning restore CS4014

            // See if the request is for blinky.html, if yes get the new state
            //            string state = "Unspecified";
            //            bool stateChanged = false;
            //            if (request.Contains("blinky.html?state=on"))
            //            {
            //                state = "On";
            //                stateChanged = true;
            //            }
            //            else if (request.Contains("blinky.html?state=off"))
            //            {
            //                state = "Off";
            //                stateChanged = true;
            //            }

            //            if (stateChanged)
            //            {
            //                var updateMessage = new ValueSet();
            //                updateMessage.Add("Command", state);
            //#pragma warning disable CS4014
            //                appServiceConnection.SendMessageAsync(updateMessage);
            //#pragma warning restore CS4014
            //            }

            //string html = state == "On" ? onHtmlString : offHtmlString;
            //byte[] bodyArray = Encoding.UTF8.GetBytes(html);
            // Show the html 
            //using (var outputStream = socket.OutputStream)
            //{
            //    using (Stream resp = outputStream.AsStreamForWrite())
            //    {
            //        using (MemoryStream stream = new MemoryStream(bodyArray))
            //        {
            //            string header = String.Format("HTTP/1.1 200 OK\r\n" +
            //                                "Content-Length: {0}\r\n" +
            //                                "Connection: close\r\n\r\n",
            //                                stream.Length);
            //            byte[] headerArray = Encoding.UTF8.GetBytes(header);
            //            resp.Write(headerArray, 0, headerArray.Length);
            //            stream.CopyTo(resp);
            //            resp.Flush();
            //        }
            //    }
            //}
        }

        public void Dispose()
        {
            listener.Dispose();
        }
    }
}
