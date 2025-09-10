using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(8)]
    public class AddColumnsOnNotifications : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Notification")
                .AddColumn("Title").AsString().Nullable()
                .AddColumn("Seen").AsBoolean().Nullable()
                .AddColumn("Parameters").AsString().Nullable();
        }
        public override void Down()
        {
        }

    }
}
