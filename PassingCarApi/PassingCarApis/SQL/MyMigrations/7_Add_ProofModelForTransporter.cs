using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(7)]
    public class Add_ProofModelForTransporter : Migration
    {
        public override void Up()
        {
            _ = Alter.Table("Payment")
                .AddColumn("PaymentProofFromTransporterModelJson").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
        }
    }
}