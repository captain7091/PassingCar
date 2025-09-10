using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(4)]
    public class _4_Add_UserColumns : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("User")
                    .AddColumn("PersonalInfo").AsString().Nullable()
                    .AddColumn("Address").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
        }
    }
}