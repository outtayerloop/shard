using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto.Users
{
    public class UserDto
    {
        public string Id { get; set; }

        public string Pseudo { get; set; }

        public string DateOfCreation { get; set; }



        public Dictionary<string, int> ResourcesQuantity { get; set; }
    }
}
