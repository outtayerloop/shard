using Shard.WiemEtBrunelle.Web.Converters.Units;
using Shard.WiemEtBrunelle.Web.Converters.Users;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        public async Task<(UnitDto, string)> TeleportUnitToAnotherShard(string userId, UnitDto newUnitData)
        {

            GenericUnit unitModelToTeleport = GetUnitModelFromUser(userId, newUnitData.Id);

            if (unitModelToTeleport == null)
                return (null, null);

            if (unitModelToTeleport.Id == null)
                return (new UnitDto(null, null, null, null), null);

            if (IsBattleUnit(unitModelToTeleport))
            {
                unitModelToTeleport.Health = GetBattleUnitFromGenericUnit(unitModelToTeleport).Health;
            }

            return await GetUnitTeleportationResult(userId, newUnitData, unitModelToTeleport);
        }

        private async Task<(UnitDto, string)> GetUnitTeleportationResult(string userId, UnitDto newUnitData, GenericUnit unitModelToTeleport)
        {
            HttpClient client = GetConfiguredTeleporationHttpClient();
            string baseUri = _configuration.GetSection("Wormholes:fake-remote:baseUri").Value;

            (User foundUser, UserDto teleportedUser) = await TryToTeleportAndReturnUser(userId, client, baseUri);

            return teleportedUser == null
                ? TeleportationFailure(newUnitData)
                : await HandleUnitTeleportation(userId, newUnitData, unitModelToTeleport, client, baseUri, foundUser);
        }

        private async Task<(UnitDto, string)> HandleUnitTeleportation(string userId, UnitDto newUnitData, GenericUnit unitModelToTeleport, HttpClient client, string baseUri, User foundUser)
        {
            (UnitDto teleportedUnit, string fullUnitTeleportationUri) = await
                TryToTeleportAndReturnUnit(userId, newUnitData, unitModelToTeleport, client, baseUri);

            return teleportedUnit == null
                ? TeleportationFailure(newUnitData)
                : TeleportationSuccess(unitModelToTeleport, foundUser, teleportedUnit, fullUnitTeleportationUri);
        }

        private (UnitDto, string) TeleportationFailure(UnitDto newUnitData)
        {
            newUnitData.DestinationShard = null;
            return (newUnitData, null);
        }

        private async Task<(UnitDto, string)> TryToTeleportAndReturnUnit(string userId,
            UnitDto newUnitData, GenericUnit unitModelToTeleport, HttpClient client, string baseUri)
        {
            UnitDto unitToTeleport = GetUnitToTeleport(newUnitData, unitModelToTeleport);

            (UnitDto teleportedUnit, string fullUnitTeleportationUri) = await
                unitModelToTeleport.TeleportToAnotherShard(userId, unitToTeleport, client, baseUri);

            return (teleportedUnit, fullUnitTeleportationUri);
        }

        private UnitDto GetUnitToTeleport(UnitDto newUnitData, GenericUnit unitModelToTeleport)
        {
            string starSystemContainingWormhole = _configuration.GetSection("Wormholes:fake-remote:system").Value;
            UnitDto unitToTeleport = UnitConverter.ConvertToUnitDto(unitModelToTeleport);
            unitToTeleport.System = starSystemContainingWormhole;
            unitToTeleport.DestinationSystem = starSystemContainingWormhole;
            unitToTeleport.EstimatedTimeOfArrival = _systemClockService.Now.ToString();
            unitToTeleport.DestinationShard = newUnitData.DestinationShard;
            return unitToTeleport;
        }

        private HttpClient GetConfiguredTeleporationHttpClient()
        {
            HttpClient client = _clientFactory.CreateClient();

            string username = _configuration.GetSection("Wormholes:fake-remote:user").Value;
            string password = _configuration.GetSection("Wormholes:fake-remote:sharedPassword").Value;
            string authenticationParameter = Convert.ToBase64String(Encoding.UTF8.GetBytes($"shard-{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationParameter);
            return client;
        }

        private async Task<(User, UserDto)> TryToTeleportAndReturnUser(string userId, HttpClient configuredClient, string baseUri)
        {
            User foundUser = _userRepository.GetUser(userId);
            UserDto userToTeleport = UserConverter.ConvertToUserDto(foundUser);
            UserDto teleportedUser = await foundUser.TeleportToAnotherShard(userToTeleport, configuredClient, baseUri);
            return (foundUser, teleportedUser);
        }

        private (UnitDto, string) TeleportationSuccess(GenericUnit unitModelToTeleport, User foundUser, UnitDto teleportedUnit, string fullUnitTeleportationUri)
        {
            foundUser.Units.Remove(unitModelToTeleport);
            _userRepository.RemoveSingleUser(foundUser);
            return (teleportedUnit, fullUnitTeleportationUri);
        }
    }
}
