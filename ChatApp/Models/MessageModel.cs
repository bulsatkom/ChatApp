using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class MessageModel
    {
        public MessageModel(string username, string body, bool mine, string date, int isReceived)
        {
            this.Username = username;
            this.Body = body;
            this.Mine = mine;
            this.Date = date;
            this.IsReceived = isReceived;
        }

        public string Username { get; set; }
        public string Body { get; set; }
        public bool Mine { get; set; }

        public bool IsNotice => Body.StartsWith("[Notice]");

        public string CSS => Mine ? "sent" : "received";

        public string Date { get; set; }

        public int IsReceived { get; set; }
    }
}
