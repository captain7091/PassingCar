using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(2)]
    public class AddLog : Migration
    {
        public override void Up()
        {
            _ = Create.Table("Log")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Message").AsString(int.MaxValue).Nullable()
                .WithColumn("UserId").AsInt32().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }
        public override void Down()
        {
        }

    }
}
