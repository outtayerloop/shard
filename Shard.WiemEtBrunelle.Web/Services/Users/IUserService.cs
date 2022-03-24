using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public interface IUserService
    {
        Task<List<UnitDto>> GetAllUnitsFromUser(string userId);

        Task<UnitDto> GetSingleUnitFromUser(string userId, string unitId);

        UnitDto UpdateSingleUnitFromUser(string userId, UnitDto unitData);

        UserDto CreateOrUpdateUser(UserDto userData);

        UserDto GetUserById(string userId);

        Task<GenericUnitLocationDto> GetSingleUnitDetailsForUser(string userId, string unitId);

        Task<UnitDto> GetUnitOfSpecificType(string userId, string unitType);

        BuildingDto OrderBuildingCreation(BuildingDto buildingData, UnitDto builder);

        List<BuildingDto> GetAllBuildingsFromUser(string userId);

        Task<BuildingDto> GetSingleBuildingFromUser(string userId, string buildingId);

        Task<UnitDto> StartPortCreatesUnitAsync(string userId, string startportId, UnitDto newUnit);

        UnitDto PutUnitAsAdministrator(string userId, UnitDto newUnit);

        UnitDto ReceivingJumpingCargo(string userId, UnitDto jumpingUnit);

        Task<(UnitDto, string)> TeleportUnitToAnotherShard(string userId, UnitDto newUnitData);
    }
}