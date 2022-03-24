using Microsoft.AspNetCore.Mvc;
using Shard.WiemEtBrunelle.Web.Dto;
using Shard.WiemEtBrunelle.Web.Models;
using Shard.WiemEtBrunelle.Web.Services;
using Shard.WiemEtBrunelle.Web.Services.Problem;
using System.Collections.Generic;

/*********************************
 * 
 * Nom: SystemsController
 * 
 * Rôle: Contrôleur de la route /Systems
 * 
 * Date: 08/09/2020
 * 
 ********************************/


namespace Shard.WiemEtBrunelle.Web.Controllers
{

    [Route("[controller]")]
    public class SystemsController : ShardBaseController
    {
        public SystemsController(ISectorService sectorService, IProblemDescriptionService problemDescriptionService) 
            : base(sectorService, problemDescriptionService){}

        /// <summary>
        /// Retourne les informations du secteur et l'ensemble des informations détaillées de ses systèmes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<List<StarSystemDto>> GetAllSystems()
            => _sectorService.GetAllSystems();

        /// <summary>
        /// Récupère les informations d'un système recherché par son nom
        /// </summary>
        /// <param name="systemName">Nom de système recherché</param>
        /// <returns></returns>
        [HttpGet("{systemName}")]
        public ActionResult<StarSystemDto> GetSystemByName(string systemName)
        {
            StarSystemDto system = _sectorService.GetSystemByName(systemName);
            var systemList = new List<StarSystemDto>() { system };
            return RetrievalResponse(systemList, RetrievalContext.Single);
        }

        /// <summary>
        /// Récupère les informations des planètes d'un système recherché par son nom
        /// </summary>
        /// <param name="systemName">Nom de système recherché</param>
        /// <returns></returns>
        [HttpGet("{systemName}/planets")]
        public ActionResult<List<PlanetDto>> GetAllPlanetsFromNamedSystem(string systemName)
        {
            List<PlanetDto> planets = _sectorService.GetAllPlanetsFromSystem(systemName);
            return RetrievalResponse<StarSystemDto, PlanetDto>(planets, RetrievalContext.Several);
        }

        /// <summary>
        /// Récupère les informations d'une planète recherchée par son nom dans un système spécifique recherché par son nom
        /// </summary>
        /// <param name="systemName">Nom de système recherché</param>
        /// <param name="planetName">Nom de planète recherchée</param>
        /// <returns></returns>
        [HttpGet("{systemName}/planets/{planetName}")]
        public ActionResult<PlanetDto> GetSinglePlanetFromNamedSystem(string systemName, string planetName)
        {
            PlanetDto planet = _sectorService.GetSinglePlanetFromSystem(systemName, planetName);
            List<PlanetDto> planetList = GetSinglePlanetList(planet);
            return RetrievalResponse<StarSystemDto, PlanetDto>(planetList, RetrievalContext.Single);
        }

        /// <summary>
        /// Retourne null si la planète est null, une nouvelle liste contenant une planète null si le nom de la planète est null,
        /// sinon retourne une liste contenant la planète
        /// </summary>
        /// <param name="planet">Planète récupérée</param>
        /// <returns></returns>
        private List<PlanetDto> GetSinglePlanetList(PlanetDto planet)
        {
            if (planet == null) return null;

            return planet.Name == null 
                ? new List<PlanetDto>() { null } 
                : new List<PlanetDto>() { planet };
        }
    }
}
