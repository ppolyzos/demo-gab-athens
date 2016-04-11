using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GwabDemo2016.Hubs
{
    [HubName("photos")]
    public class PhotosHub : Hub
    {
        private Random random = new Random();

        public void Notify(string name)
        {
            Clients.All.notify(name);
        }

        public void GenerateMessage()
        {
            Clients.All.notify($"{DateTime.Now.ToString("HH:mm:ss")}: Random Message id: {random.Next(1, 100)}");
        }
    }
}