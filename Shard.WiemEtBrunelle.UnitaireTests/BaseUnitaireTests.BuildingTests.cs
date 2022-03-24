using System;
using System.Collections.Generic;
using Shard.Shared.Web.IntegrationTests.Clock;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Repositories.Buildings;
using Shard.WiemEtBrunelle.Web.Repositories.Users;
using Shard.WiemEtBrunelle.Web.Services;
using Shard.WiemEtBrunelle.Web.Services.Users;
using System.Linq;
using Xunit;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    partial class BaseUnitaireTests
    {
        



        [Fact]
        public void CreateBuildingWithWrongTypeOfBuilder()
        {
            BuildingDto buildingData = new BuildingDto();
            UnitDto unitDto = new UnitDto();
            unitDto.Type = GetWrongUnitType();

            BuildingDto buildingCreated = userService.OrderBuildingCreation(buildingData, unitDto);

            Assert.Null(buildingCreated);
        }

        [Fact]
        public void CreateBuilding()
        {

            BuildingDto buildingData = new BuildingDto();
            buildingData.Type = GetNameOfBuildingType();
            UnitDto unitDto = new UnitDto();

            unitDto.Type = GetTheNameOfBuildingUnit();
            buildingData.ResourceCategory = GetRandomEntityFromList(BuildingConstants.resourcesCategory);
            BuildingDto buildingCreated = userService.OrderBuildingCreation(buildingData, unitDto);

            Assert.NotNull(buildingCreated.Id);
            Assert.False(buildingCreated.IsBuilt);

        }

        [Fact]
        public void CreateBuildingWithWrongTypeOfResourceCategory()
        {
            BuildingDto buildingData = new BuildingDto();
            buildingData.ResourceCategory = GetWrongResourceCategory();

            UnitDto unitDto = new UnitDto();

            BuildingDto buildingCreated = userService.OrderBuildingCreation(buildingData, unitDto);

            Assert.Null(buildingCreated);
        }


        [Theory]
        [InlineData("gaseous")]
        [InlineData("liquid")]
        [InlineData("solid")]

        public void CreateBuildingForGaseousResource(string resourceCategory)
        {
            BuildingDto buildingData = new BuildingDto();
            buildingData.ResourceCategory = resourceCategory;
            buildingData.Type = GetNameOfBuildingType();

            UnitDto unitDto = new UnitDto();
            unitDto.Type = GetTheNameOfBuildingUnit();

            BuildingDto buildingCreated = userService.OrderBuildingCreation(buildingData, unitDto);

            string buildingType = GetNameOfBuildingType();

            Assert.NotNull(buildingCreated);
            Assert.Equal(resourceCategory, buildingCreated.ResourceCategory);
            Assert.Equal(buildingType, buildingCreated.Type);

        }

        [Fact]
        public void CreateBuildingWithWrongTypeOfBuilding()
        {
            BuildingDto buildingData = new BuildingDto();
            buildingData.ResourceCategory = GetWrongBuildingType();
            buildingData.Type = GetNameOfBuildingType();

            UnitDto unitDto = new UnitDto();
            unitDto.Type = GetTheNameOfBuildingUnit();

            BuildingDto buildingCreated = userService.OrderBuildingCreation(buildingData, unitDto);


            Assert.Null(buildingCreated);



        }
    }
}