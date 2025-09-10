using FluentMigrator;

namespace PassingCarApis.SQL.MyMigrations
{
    [Migration(1)]
    public class _1_InitDatabase : Migration
    {
        public override void Up()
        {
            _ = Create.Table("User")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("FacebookId").AsString().Nullable()
                .WithColumn("GoogleId").AsString().Nullable()
                .WithColumn("RegistrationType").AsString().Nullable()
                .WithColumn("Name").AsString().Nullable()
                .WithColumn("Surname").AsString().Nullable()
                .WithColumn("PhoneNumber").AsString().Nullable()
                .WithColumn("Email").AsString().Nullable()
                .WithColumn("EmailVerified").AsString().Nullable().WithDefaultValue(false)
                .WithColumn("Photo").AsBinary(int.MaxValue).Nullable()
                .WithColumn("HashedPassword").AsString().Nullable()
                .WithColumn("Uber").AsString().Nullable().WithDefaultValue(false)
                .WithColumn("Customer").AsString().Nullable().WithDefaultValue(false)
                .WithColumn("JuridicPerson").AsString().Nullable().WithDefaultValue(false)
                .WithColumn("JuridicDetails").AsString().Nullable()
                .WithColumn("Range").AsInt32().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();

            _ = Create.Table("Ads")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("Photo1").AsBinary(int.MaxValue).Nullable()
                .WithColumn("Photo2").AsBinary(int.MaxValue).Nullable()
                .WithColumn("Photo3").AsBinary(int.MaxValue).Nullable()
                .WithColumn("Photo4").AsBinary(int.MaxValue).Nullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("From").AsString().NotNullable()
                .WithColumn("To").AsString().NotNullable()
                .WithColumn("Price").AsCurrency().NotNullable()
                .WithColumn("ViewsCounter").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Size").AsString().Nullable()
                .WithColumn("Weight").AsString().Nullable()
                .WithColumn("Fragile").AsString().Nullable().WithDefaultValue(false)
                .WithColumn("State").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Ads").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("Offer")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("AdId").AsInt32().NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Offer").ForeignColumn("AdId")
                 .ToTable("Ads").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Offer").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("Chat")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("AdId").AsInt32().NotNullable()
                .WithColumn("CustomerUserId").AsInt32().NotNullable()
                .WithColumn("UberUserId").AsInt32().NotNullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Chat").ForeignColumn("AdId")
                 .ToTable("Ads").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Chat").ForeignColumn("CustomerUserId")
                 .ToTable("User").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Chat").ForeignColumn("UberUserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("ChatMessage")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("ChatId").AsInt32().NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("ChatMessage").ForeignColumn("ChatId")
                 .ToTable("Chat").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("ChatMessage").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("FavoriteAds")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("AdId").AsInt32().NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            _ = Create.ForeignKey()
                 .FromTable("FavoriteAds").ForeignColumn("AdId")
                 .ToTable("Ads").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("FavoriteAds").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("Shipping")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("OfferId").AsInt32().NotNullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("CurrentLocation").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Shipping").ForeignColumn("OfferId")
                 .ToTable("Offer").PrimaryColumn("Id");

            _ = Create.Table("Review")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("ReviewedUserId").AsInt32().NotNullable()
                .WithColumn("ReviewerUserId").AsInt32().NotNullable()
                .WithColumn("Rating").AsInt32().NotNullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Review").ForeignColumn("ReviewedUserId")
                 .ToTable("User").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Review").ForeignColumn("ReviewerUserId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("ValidationCode")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("PhoneNumber").AsString().Nullable()
                .WithColumn("Email").AsString().Nullable()
                .WithColumn("Code").AsString().Nullable()
                .WithColumn("ExpiresAt").AsDateTime().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            _ = Create.Table("PhoneMessage")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("PhoneNumber").AsString().NotNullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("SentAt").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            _ = Create.Table("EmailMessage")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserToSendId").AsInt32().NotNullable()
                .WithColumn("Content").AsString().Nullable()
                .WithColumn("State").AsString().Nullable()
                .WithColumn("SentAt").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
            _ = Create.ForeignKey()
                 .FromTable("EmailMessage").ForeignColumn("UserToSendId")
                 .ToTable("User").PrimaryColumn("Id");

            _ = Create.Table("Comment")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("AdId").AsInt32().NotNullable()
                .WithColumn("ParentId").AsInt32().Nullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("ModifiedAt").AsDateTime().Nullable();
            _ = Create.ForeignKey()
                 .FromTable("Comment").ForeignColumn("UserId")
                 .ToTable("User").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Comment").ForeignColumn("AdId")
                 .ToTable("Ads").PrimaryColumn("Id");
            _ = Create.ForeignKey()
                 .FromTable("Comment").ForeignColumn("ParentId")
                 .ToTable("Comment").PrimaryColumn("Id");
        }

        public override void Down()
        {
        }
    }
}