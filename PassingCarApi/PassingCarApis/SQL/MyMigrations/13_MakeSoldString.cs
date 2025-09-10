using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(13)]
    public class MakeSoldString : Migration
    {
        public override void Up()
        {
            Delete.Column("Sold").FromTable("User");

            _ = Alter.Table("User")
                    .AddColumn("Sold")
                    .AsString()
                    .Nullable();
        }
        public override void Down()
        {
        }

    }
}
