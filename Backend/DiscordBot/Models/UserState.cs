using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public enum State
    {
        NEW,
        GITHUB_AUTH,
        BACKEND_AUTH,
        LOGGED_IN
    }
    public class UserState
    {
        public string UserID { get; set; }
        public string? deviceCode { get; set; }
        public string? accessToken { get; set; }
        public string? jwt {  get; set; }
        public State currentState {  get; set; }
        public UserState(string userID) { 
            this.currentState = State.NEW; 
            this.UserID = userID;
        }
    }
}
