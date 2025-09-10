using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(10)]
    public class AddUserProfileOnTables : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Offer")
                .AddColumn("UserProfile").AsString().Nullable();

            _ = Alter.Table("Chat")
                .AddColumn("CustomerUserProfile").AsString().Nullable();

            _ = Alter.Table("Chat")
                .AddColumn("UberUserProfile").AsString().Nullable();

            _ = Alter.Table("ChatMessage")
                .AddColumn("UserProfile").AsString().Nullable();

            _ = Alter.Table("FavoriteAds")
                .AddColumn("UserProfile").AsString().Nullable();

            _ = Alter.Table("EmailMessage")
                .AddColumn("UserProfile").AsString().Nullable();

            _ = Alter.Table("Comment")
                .AddColumn("UserProfile").AsString().Nullable();
        }
        public override void Down()
        {
        }

    }
}
