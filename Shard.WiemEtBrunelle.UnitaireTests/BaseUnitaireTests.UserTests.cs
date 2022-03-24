using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Frameworks;
using Shard.Shared.Web.IntegrationTests.Clock;
using Shard.WiemEtBrunelle.Web.Converters.Units;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Users;
using Shard.WiemEtBrunelle.Web.Repositories;
using Shard.WiemEtBrunelle.Web.Repositories.Buildings;
using Shard.WiemEtBrunelle.Web.Repositories.Users;
using Shard.WiemEtBrunelle.Web.Services.Users;
using System.Threading.Tasks;
using Xunit;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    public partial class BaseUnitaireTests
    {



        [Fact]
        public void UserNotFound()
        {
            string userId = GetIdForUser();
            UserDto userDto = userService.GetUserById(userId);

            Assert.Null(userDto);
        }


        //CREATION DU USER
        [Fact]
        public void CreateUser()
        {

            string userId = GetIdForUser();
            UserDto user = new UserDto() { Id = userId, DateOfCreation = dateTimeOffset.ToString("O") };

            userService.CreateOrUpdateUser(user);
            UserDto userCreated = userService.GetUserById(userId);

            Assert.NotNull(userCreated.DateOfCreation);
            Assert.Equal(dateTimeOffset.ToString("O"), userCreated.DateOfCreation);
            Assert.NotNull(userCreated.Id);
            Assert.Equal(userId, userCreated.Id);
            Assert.NotNull(userCreated.ResourcesQuantity);
            Assert.Null(userCreated.Pseudo);

        }

        [Fact]
        public void UpdateUser()
        {
            string userId = GetIdForUser();
            UserDto user = new() { Id = userId, DateOfCreation = dateTimeOffset.ToString("O") };
            user.Pseudo = GetPseudoForUser();

            userService.CreateOrUpdateUser(user);

            string newPseudo = GetNewPseudoForUser();
            user.Pseudo = newPseudo;

            userService.CreateOrUpdateUser(user);

            Assert.Equal(newPseudo, user.Pseudo);
            Assert.Equal(userId, user.Id);
        }

        [Fact]
        public void GetAllUnitsFromUser()
        {
            UserDto user = new UserDto() { Id = GetIdForUser(), DateOfCreation = dateTimeOffset.ToString("O") };
            userService.CreateOrUpdateUser(user);

            Task<List<UnitDto>> units = userService.GetAllUnitsFromUser(user.Id);

            Assert.NotNull(units);
        }

    }
}
