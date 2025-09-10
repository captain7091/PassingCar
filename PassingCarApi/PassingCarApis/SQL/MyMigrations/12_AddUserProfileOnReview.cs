using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(12)]
    public class AddUserProfileOnReview : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Review")
                .AddColumn("ReviewedUserProfile").AsString().Nullable()
                .AddColumn("ReviewerUserProfile").AsString().Nullable();
        }
        public override void Down()
        {
        }

    }
}
