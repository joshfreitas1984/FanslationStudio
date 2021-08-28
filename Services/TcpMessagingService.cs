using Caliburn.Micro;
using FanslationStudio.UserExperience.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FanslationStudio.Services
{
    public class TcpMessagingService
    {
        private static TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 500);
        private static bool listen = true; // <--- boolean flag to exit loop

        public async void RunServer(IEventAggregator eventAggregator)
        {
            listener.Start();

            while (listen)
            {
                if (listener.Pending())
                    await HandleClient(await listener.AcceptTcpClientAsync(), eventAggregator);
                else
                    await Task.Delay(100); //<--- timeout
            }
        }

        private static async Task HandleClient(TcpClient client, IEventAggregator eventAggregator)
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream);
            string msg = await reader.ReadToEndAsync();

            //Right now going to just handle the script id - should probably extend this to a Json Payload on both sides
            await eventAggregator.PublishOnCurrentThreadAsync(new ScriptViewedInGameEvent()
            {
                LineId = msg
            });
        }
    }
}
