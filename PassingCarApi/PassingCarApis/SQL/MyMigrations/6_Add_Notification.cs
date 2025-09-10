using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(6)]
    public class _6_Add_Notification : Migration
    {
        public override void Up()
        {
            _ = Create.Table("Notification")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("ShellRoute").AsString(int.MaxValue).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            _ = Create.ForeignKey()
                 .FromTable("Notification").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Alter.Table("User")
                    .AddColumn("Sold").AsCurrency().Nullable();
        }

        public override void Down()
        {
        }
    }
}