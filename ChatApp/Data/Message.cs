using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Data
{
    public class Message
    {
        public Message()
        {
            this.Receivers = new HashSet<Receiver>();
        }

        [Key]
        public string Id { get; set; }

        [Required]
        [MinLength(2)]
        public string Text { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SendDate { get; set; }

        public int State { get; set; }

        public virtual ICollection<Receiver> Receivers { get; set; }
    }
}
