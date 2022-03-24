using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Repositories.Users
{
    public interface IUserRepository
    {
        User GetUser(string userId);

        void AddUser(User newUser);

        User ReplaceUserWithNewData(User userToUpdate);

        User UpdateUserResourcesQuantity(UserDto newUserData, string userId);

        List<User> GetAllUsers();

        void RemoveSingleUser(User userToRemove);

    }
}
