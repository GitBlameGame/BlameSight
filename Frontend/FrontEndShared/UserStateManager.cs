
using VeryUsefulServer.Models;

namespace FrontEndShared.Services
{
    public class UserStateManager
    {
        List<UserState> users {  get; set; }
        public UserStateManager() {
            users = [];
                
                }

        public bool userStateExists(string userID)
        {
            return users == null ? false : users.Any(user => user.UserID == userID);
        }

        public UserState getUserState(string userID)
        {
            if(users == null)
            {
                throw new ArgumentNullException();
            }
            return users.First(user => user.UserID == userID) ?? throw new IOException("User not found");
        }

        public void putUserState(UserState userState)
        {
            if(userStateExists(userState.UserID))
            {
                if (userState.jwt != null)
                {
                    userState.currentState = State.LOGGED_IN;
                }
                else if (userState.accessToken != null)
                {
                    userState.currentState = State.BACKEND_AUTH;
                }
                else if (userState.deviceCode != null)
                {
                    userState.currentState = State.GITHUB_AUTH;
                }
                users.Remove(users.First(user => user.UserID == userState.UserID));
                users.Add(userState);
            }
            else
            {
                users.Add(userState);
            }
        }
    }
}
