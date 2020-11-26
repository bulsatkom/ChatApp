using ChatApp.Models;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public const string HubUrl = "/chat";
        private readonly IChatAppService chatAppService;

        public ChatHub(IChatAppService chatAppService)
        {
            this.chatAppService = chatAppService;
        }

        public async Task Broadcast(string connectionId, string message)
        {
            if (!string.IsNullOrEmpty(connectionId))
            {
                var result = await this.chatAppService.AddMessageAsync(connectionId, message);
                await Clients.All.SendAsync("Broadcast", result.userName, message, result.date, result.state);
            }
        }

        public async Task ReceiveMessages(string messagesAsString, string connectionId)
        {
            var messages = JsonConvert.DeserializeObject<ICollection<MessageModel>>(messagesAsString);

            if (messages != null && messages.Any())
            {
                await this.chatAppService.ReceiveMessages(connectionId);
                string id = await this.chatAppService.GetUserIdByConnectionId(connectionId);
                var msgs = await this.chatAppService.GetAllMessages(id);

                var msgsAsString = JsonConvert.SerializeObject(msgs);

                await Clients.Caller.SendAsync("ReceivedMessage", msgsAsString);
                await Clients.All.SendAsync("SyncData", string.Empty);
            }
        }

        public async Task SyncData(string connectionId)
        {
            string Id = await this.chatAppService.GetUserIdByConnectionId(connectionId);
            var messages = await this.chatAppService.GetAllMessages(Id);
            var msgsAsString = JsonConvert.SerializeObject(messages);

            await Clients.Caller.SendAsync("ReceivedMessage", msgsAsString);
        }

        public async Task Disconnect(string connectionId)
        {
            await this.chatAppService.RemoveUser(connectionId);
        }

        public async Task Connect(string name)
        {
            var UserId = Guid.NewGuid().ToString(); 

            var id = await this.chatAppService.Register(name, UserId);
            var msgs = await this.chatAppService.GetAllMessages(id);

            var msgsAsString = JsonConvert.SerializeObject(msgs);
            await Clients.Caller.SendAsync("Connect", name, UserId, msgsAsString);
        }
    }
}
