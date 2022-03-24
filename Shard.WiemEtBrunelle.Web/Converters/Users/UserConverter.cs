using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Users;

namespace Shard.WiemEtBrunelle.Web.Converters.Users
{
    public static class UserConverter
    {
        public static UserDto ConvertToUserDto(User user)
        {
            if (user == null) 
                return null;

            var userDto = new UserDto(){ Id = user.Id, DateOfCreation = user.DateOfCreation.ToString("O"), Pseudo = user.Pseudo };
            userDto.ResourcesQuantity = ResourcesConverter.GetLowerCasedResources(user.ResourcesQuantity);
            return userDto;
        }
    }
}
