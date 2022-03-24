using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Universe
{
    public class StarSystem
    {
        public StarSystem(List<Planet> planets, string name)
        {
            Planets = planets;
            Name = name;
        }
        
        [BsonId]
        public string Name { get; set; }

        public List<Planet> Planets { get; set; }
    }
}