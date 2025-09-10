using FluentMigrator.Runner;
using PassingCarApis.Configuration;
using PassingCarApis.SQL.MyMigrations;

namespace PassingCarApis.SQL
{
    public class MyMigration
    {
        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        public IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SqlServer
                    .AddSqlServer()
                    .WithVersionTable(new VersionTable())
                    // Set the connection string
                    .WithGlobalConnectionString(AppConfiguration.MyConnectionString)
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(_1_InitDatabase).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </summary>
        public void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            IMigrationRunner runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            // Execute the migrations
            runner.MigrateUp();
        }

        /// <summary>
        /// Update the database
        /// </summary>
        public void DowngradeDatabase(IServiceProvider serviceProvider, long version)
        {
            // Instantiate the runner
            IMigrationRunner runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            // Execute the migrations
            runner.MigrateDown(version);
        }
    }
}