using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeryUsefulServer.Models;

namespace FrontEndShared.Models
{
    public class InputMessage
    {
        public string JWT { get; set; }
        public UserState UserState { get; set; }
        public string Message { get; set; }

        public InputMessage(
            string jwt, string msg, UserState user
            ) {
            this.JWT = jwt;
            this.Message = msg;
            this.UserState = user;
        }
    }
}
