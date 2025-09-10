using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(9)]
    public class AddUserProfileOnAdd : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Ads")
                .AddColumn("UserProfile").AsString().Nullable();
        }
        public override void Down()
        {
        }

    }
}
