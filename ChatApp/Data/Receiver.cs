using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Data
{
    public class Receiver
    {
        [Key]
        public string Id { get; set; }
      
        [ForeignKey("ReceiverUser")]
        public string ReceiverId { get; set; }

        public virtual IdentityUser ReceiverUser { get; set; }

        [ForeignKey("SenderUser")]
        public string SenderUserId { get; set; }

        public virtual IdentityUser SenderUser { get; set; }
        
        public string MessageId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ReceivedDate { get; set; }

        public bool IsReceived { get; set; }
    }
}
