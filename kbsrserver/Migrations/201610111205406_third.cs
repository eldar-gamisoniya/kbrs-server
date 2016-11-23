namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class third : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserKeys",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Imei = c.String(maxLength: 100),
                        PublicKey = c.String(),
                        SessionKey = c.String(),
                        SessionKeyGenerated = c.DateTime(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.Imei, unique: true)
                .Index(t => t.User_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserKeys", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UserKeys", new[] { "User_Id" });
            DropIndex("dbo.UserKeys", new[] { "Imei" });
            DropTable("dbo.UserKeys");
        }
    }
}
