using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenAuthDemo.Models;

namespace TokenAuthDemo.Repository
{
    public interface IUserRefreshTokenRepository
    {
        void SaveOrUpdateUserRefreshToken(UserRefreshToken userRefreshToken);
        bool CheckIfRefreshTokenIsValid(string username,string refreshToken);
    }
}
