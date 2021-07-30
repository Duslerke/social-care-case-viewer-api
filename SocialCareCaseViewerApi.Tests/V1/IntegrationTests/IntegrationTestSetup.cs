using System;
using System.Data;
using System.Data.Common;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public class IntegrationTestSetup<TStartup> where TStartup : class
    {
        private HttpClient _client;
        private DatabaseContext _databaseContext;
        private ISccvDbContext _mongoDbContext;

        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private DbContextOptionsBuilder _builder;


        protected HttpClient Client => _client;
        protected DatabaseContext DatabaseContext => _databaseContext;
        protected ISccvDbContext MongoDbTestContext => _mongoDbContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var serviceProvider = new ServiceCollection().AddDbContext<DatabaseContext>().AddEntityFrameworkNpgsql().BuildServiceProvider();

            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();

            var npgsqlCommand = _connection.CreateCommand();
            npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            npgsqlCommand.ExecuteNonQuery();

            _builder = new DbContextOptionsBuilder<DatabaseContext>();
            _builder.UseNpgsql(_connection).UseInternalServiceProvider(serviceProvider);
        }

        [SetUp]
        public void BaseSetup()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("SCCV_MONGO_CONN_STRING", "mongodb://localhost:1433/");
            Environment.SetEnvironmentVariable("SCCV_MONGO_DB_NAME", "social_care_db_test");
            Environment.SetEnvironmentVariable("SCCV_MONGO_COLLECTION_NAME", "form_data_test");
            Environment.SetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_URL", "https://mockBase");

            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            _client = _factory.CreateClient();

            _databaseContext = _factory.Server.Host.Services.GetRequiredService<DatabaseContext>();

            _databaseContext.ChangeTracker.LazyLoadingEnabled = false;

            _databaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_team;");
            _databaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_worker;");
            _databaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_workerteam;");

            _databaseContext.Database
            .ExecuteSqlRaw("insert into dbo.sccv_worker (id, email, first_name, last_name, role, context_flag) values (91, 'bhadfield5@example.com', 'Basilio', 'Hadfield', 'non', 'C');");

            _databaseContext.Database.ExecuteSqlRaw("UPDATE DBO.SCCV_WORKER SET is_active = true WHERE is_active isnull;");

            _databaseContext.Database
            .ExecuteSqlRaw("insert into dbo.sccv_team (id, name, context) values (35, 'Aenean', 'C');");

            _databaseContext.Database
            .ExecuteSqlRaw("insert into dbo.sccv_team (id, name, context) values (20, 'Tristique', 'C');");

            _databaseContext.Database
            .ExecuteSqlRaw("insert into dbo.sccv_workerteam (id, worker_id, team_id) values (29, 91, 35);");

            _mongoDbContext = new MongoDbTestContext();

            _transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            _databaseContext.Database.UseTransaction(_transaction);
            // _transaction.Commit();
            // var whatIsTheString = _databaseContext.Database.GetDbConnection();

        }

        [TearDown]
        public void BaseTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
            _transaction.Rollback();
            _transaction.Dispose();
            _mongoDbContext.getCollection().DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            _connection.Dispose();
        }
    }
}
