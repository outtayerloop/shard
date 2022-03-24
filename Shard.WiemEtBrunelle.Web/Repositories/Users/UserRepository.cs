using MongoDB.Driver;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Shard.WiemEtBrunelle.Web.Constants.Resources;
using Shard.WiemEtBrunelle.Web.Database.Services;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Repositories.Users
{
    public class UserRepository : IUserRepository
    {

        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(MongoDbConnection mongoDbConnectionService)
        {
            mongoDbConnectionService.Database.DropCollection("user");
            mongoDbConnectionService.Database.CreateCollection("user");
            _userCollection = mongoDbConnectionService.Database.GetCollection<User>("user");
        }

        public User GetUser(string userId)
            => _userCollection.Find(user => user.Id == userId).FirstOrDefault();

        public void AddUser(User newUser) => _userCollection.InsertOne(newUser);

        public User ReplaceUserWithNewData(User userToUpdate)
        {
            _userCollection.ReplaceOne(GetUserFilterById(userToUpdate.Id), userToUpdate);
            return userToUpdate;
        }

        public User UpdateUserResourcesQuantity(UserDto newUserData, string userId)
        {
            User updateUser = GetUser(userId);

            foreach (string resource in newUserData.ResourcesQuantity.Keys)
            {
                updateUser.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] = newUserData.ResourcesQuantity[resource];
            }

            UpdateUserInDatabaseById(userId, "ResourcesQuantity", newUserData.ResourcesQuantity);

            ChangeAdministratorStatusToUnauthenticated();
            return updateUser;
        }

        public List<User> GetAllUsers()
            => _userCollection.Find(FilterDefinition<User>.Empty).ToList();

        public void RemoveSingleUser(User userToRemove)
            => _userCollection.DeleteOne(GetUserFilterById(userToRemove.Id));

        private void UpdateUserInDatabaseById<T>(string userId, string fieldNameToUpdate, T updateValue)
        {
            UpdateDefinition<User> userUpdateDefinition = Builders<User>.Update.Set(fieldNameToUpdate, updateValue);
            _userCollection.UpdateOne(GetUserFilterById(userId), userUpdateDefinition);
        }

        private FilterDefinition<User> GetUserFilterById(string userId)
            => Builders<User>.Filter.Eq("Id", userId);

        private void ChangeAdministratorStatusToUnauthenticated()
            => RequestConstants.AdminIsAuthenticated = false;
    }
}
