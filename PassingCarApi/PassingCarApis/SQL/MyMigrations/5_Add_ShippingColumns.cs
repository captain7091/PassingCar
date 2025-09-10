using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(5)]
    public class Add_ShippingColumns : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Shipping")
                    .AddColumn("ConfirmationCode").AsString().Nullable()
                    .AddColumn("History").AsString(int.MaxValue).Nullable();
        }
        public override void Down()
        {
        }
    }
}