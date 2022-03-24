using Microsoft.Extensions.Configuration;
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using Shard.WiemEtBrunelle.Web.Repositories.Buildings;
using Shard.WiemEtBrunelle.Web.Repositories.Users;
using Shard.WiemEtBrunelle.Web.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    public partial class BaseUnitaireTests
    {
        UserRepository userRepository;
        BuildingRepository buildingRepository;
        private readonly IUserService userService;
        private readonly IClock fakeClock;
        private static IConfiguration configuration;


        public BaseUnitaireTests()
        {
            userRepository = new UserRepository();
            buildingRepository = new BuildingRepository();
            configuration = InitConfiguration();
            fakeClock = new FakeClock();
            userService = new UserService(configuration, fakeClock, userRepository, buildingRepository);
        }

        private IConfiguration InitConfiguration()
            => new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();

        private readonly DateTimeOffset dateTimeOffset = new DateTimeOffset(new DateTime(2020, 10, 31));

        private string GetTheNameOfBuildingUnit()
        => "builder";

        private string GetWrongUnitType()
        => "bidule";

        private string GetWrongBuildingType()
        => "notMine";

        private string GetWrongResourceCategory()
        => "wrongCategory";

        private string GetIdForUser()
        => "userId";

        private string GetNameOfBuildingType()
        => "mine";

        private string GetPseudoForUser()
        => GetRandomEntityFromList(pseudo);

        private string GetNewPseudoForUser()
        => GetRandomEntityFromList(newPseudo);

        private T GetRandomEntityFromList<T>(List<T> entityList)
        {
            var random = new Random();
            int index = random.Next(entityList.Count);
            T randomEntity = entityList.ElementAt(index);
            return randomEntity;
        }

        private List<string> pseudo = new List<string>() { "nelle", "wiwi" };
        private List<string> newPseudo = new List<string>() { "patate", "rico" };

    }
}
