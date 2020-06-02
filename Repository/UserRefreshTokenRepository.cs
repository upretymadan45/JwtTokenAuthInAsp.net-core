using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenAuthDemo.Models;

namespace TokenAuthDemo.Repository
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        public static Dictionary<string, string> RefreshTokenStore;
        public UserRefreshTokenRepository()
        {
            RefreshTokenStore = new Dictionary<string, string>();
        }
        public bool CheckIfRefreshTokenIsValid(string username,string refreshToken)
        {
            string refToken = "";

            RefreshTokenStore.TryGetValue(username, out refToken);

            return refToken.Equals(refreshToken);
        }

        public void SaveOrUpdateUserRefreshToken(UserRefreshToken userRefreshToken)
        {
            if (RefreshTokenStore.ContainsKey(userRefreshToken.Username))
            {
                RefreshTokenStore[userRefreshToken.Username] = userRefreshToken.RefreshToken;
            }
            else
            {
                RefreshTokenStore.Add(userRefreshToken.Username, userRefreshToken.RefreshToken);
            }
        }
    }
}
