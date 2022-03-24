using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shard.WiemEtBrunelle.Web.Database.Options;

namespace Shard.WiemEtBrunelle.Web.Database.Services
{
    public class MongoDbConnection
    {
        public IMongoDatabase Database { get; }

        public MongoDbConnection(IOptions<MongoDbConnectionOptions> mongoOptions)
        {
            var client = new MongoClient(new MongoUrl(mongoOptions.Value.ConnectionString));
            Database = client.GetDatabase(mongoOptions.Value.DatabaseName);
        }
    }
}
