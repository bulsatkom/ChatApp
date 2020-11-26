using ChatApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Interfaces
{
    public interface IChatAppService
    {
        Task<ICollection<MessageModel>> GetAllMessages(string id);

        Task<string> Register(string userName, string id);

        Task<(string userName, string date, int state)> AddMessageAsync(string connectionId, string text);

        Task RemoveUser(string connectionId);

        Task ReceiveMessages(string connectionId);

        Task<string> GetUserIdByConnectionId(string connectionId);
    }
}
