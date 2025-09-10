using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(11)]
    public class AddUserProfileOnNotification : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Notification")
                .AddColumn("UserProfile").AsString().Nullable();
        }
        public override void Down()
        {
        }

    }
}
