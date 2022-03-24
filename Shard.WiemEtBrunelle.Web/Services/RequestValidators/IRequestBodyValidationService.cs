using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Dto.Units;

namespace Shard.WiemEtBrunelle.Web.Services.RequestValidators
{
    public interface IRequestBodyValidationService
    {
        string GetUnitUpdateRequestValidity(string unitId, UnitDto unitBodyData);

        string GetUserUpdateOrCreateRequestValidity(string userId, UserDto userBodyData);

        string GetBuildingRequestValidity(BuildingDto buildData, UnitDto existingBuilder);

        string GetBuildingRequestValidity(string buildingId);
    }
}
