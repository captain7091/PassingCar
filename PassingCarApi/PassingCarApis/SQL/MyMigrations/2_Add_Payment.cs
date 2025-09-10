using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(3)]
    public class _2_Add_Payment : Migration
    {
        public override void Up()
        {
            _ = Create.Table("Payment")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("OfferId").AsInt32().NotNullable()
                .WithColumn("ChargeRequestJson").AsString(int.MaxValue).Nullable()
                .WithColumn("ChargeResponseJson").AsString(int.MaxValue).Nullable()
                .WithColumn("RefundRequestJson").AsString(int.MaxValue).Nullable()
                .WithColumn("RefundResponseJson").AsString(int.MaxValue).Nullable()
                .WithColumn("PaymentProofModelJson").AsString(int.MaxValue).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Payment").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Payment").ForeignColumn("OfferId")
                 .ToTable("Offer").PrimaryColumn("Id");
        }

        public override void Down()
        {
        }
    }
}