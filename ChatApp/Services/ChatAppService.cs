using ChatApp.Data;
using ChatApp.Models;
using ChatApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class ChatAppService : IChatAppService
    {
        private readonly ApplicationDbContext applicationDbContext;
        private static IDictionary<string, (string dbId, string name)> connectedUsers = new Dictionary<string, (string, string)>();

        public ChatAppService(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }

        public Task<string> Register(string userName, string id)
        {
            string userId = this.applicationDbContext.Users.FirstOrDefault(x => x.UserName == userName).Id;

            if (!connectedUsers.ContainsKey(id))
            {
                (string dbId, string name) value = (userId, userName);
                connectedUsers.Add(id, value);
            }

            return Task.FromResult(userId);
        }

        public Task<ICollection<MessageModel>> GetAllMessages(string id)
        {
            var result = new List<MessageModel>();

            try
            {
                var currentresult = this.applicationDbContext.Messages
                    .Where(x => x.Receivers.Where(y => y.ReceiverId == id || y.SenderUserId == id).Any())
                    .OrderBy(x => x.SendDate)
                    .ToList();

                currentresult.ForEach(x => x.Receivers = this.applicationDbContext.Receivers.Where(y => y.MessageId == x.Id).ToList());

                result = currentresult.Select(x => new MessageModel(
                         this.applicationDbContext.Users.Where(y => y.Id == x.Receivers.Select(z => z.SenderUserId).FirstOrDefault()).Select(y => y.UserName).FirstOrDefault(),
                        x.Text,
                        x.Receivers.Where(y => y.SenderUserId == id).Any(),
                        x.SendDate.ToString(),
                        x.State))
                    .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Task.FromResult<ICollection<MessageModel>>(result);
        }

        public Task<(string userName, string date, int state)> AddMessageAsync(string connectionId, string text)
        {
            var userId = string.Empty;
            string userName = string.Empty;
            var sendDate = DateTime.Now;
            int state = connectedUsers.Skip(1).Any() ? 0 : 1;

            if (connectedUsers.ContainsKey(connectionId))
            {
                userId = connectedUsers[connectionId].dbId;
                userName = connectedUsers[connectionId].name;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var receivers = new List<Receiver>();
                string msgId = Guid.NewGuid().ToString();
                
                connectedUsers.ToList().ForEach(x =>
                {
                    bool isReceived = userId == x.Value.dbId;
                    DateTime? receiveDate = isReceived ? DateTime.Now : default;
                    receivers.Add(new Receiver()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsReceived = isReceived,
                        ReceiverId = x.Value.dbId,
                        SenderUserId = userId,
                        MessageId = msgId,
                        ReceivedDate = receiveDate
                    });
                });

                try
                {
                   this.applicationDbContext.Messages.Add(new Message()
                    {
                        Id = msgId,
                        SendDate = sendDate,
                        Text = text,
                        State = state
                   });

                    this.applicationDbContext.Receivers.AddRange(receivers);

                    this.applicationDbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }

            return Task.FromResult((userName, sendDate.ToString(), state));
        }

        public Task RemoveUser(string connectionId)
        {
            if(connectedUsers.ContainsKey(connectionId))
            {
                connectedUsers.Remove(connectionId);
            }

            return Task.CompletedTask;
        }

        public Task ReceiveMessages(string connectionId, ICollection<MessageModel> notReceivedMsgs)
        {
            string userId = string.Empty;

            if (connectedUsers.ContainsKey(connectionId))
            {
                userId = connectedUsers[connectionId].dbId;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var allunReceives = this.applicationDbContext.Receivers
                    .Where(x => x.ReceiverId == userId && !x.IsReceived)
                    .ToList();

                allunReceives.ForEach(x =>
                {
                    x.IsReceived = true;
                    x.ReceivedDate = DateTime.Now;
                });

                this.applicationDbContext.SaveChanges();

                var mesageStatesForUpdates = new HashSet<string>();
                var updatedMessageIds = new HashSet<string>();

                allunReceives.ForEach(x => mesageStatesForUpdates.Add(x.MessageId));

                mesageStatesForUpdates.ToList().ForEach(x =>
                {
                    if(this.applicationDbContext.Receivers.Where(y => y.MessageId == x).ToList().All(y => y.IsReceived))
                    {
                        this.applicationDbContext.Messages.FirstOrDefault(y => y.Id == x).State = 1;
                    }
                });

                this.applicationDbContext.SaveChanges();
            }

            return Task.CompletedTask;
        }

        public Task<string> GetUserIdByConnectionId(string connectionId)
        {
            string result = string.Empty;

            if(connectedUsers.ContainsKey(connectionId))
            {
                result = connectedUsers[connectionId].dbId;
            }

            return Task.FromResult(result);
        }
    }
}
