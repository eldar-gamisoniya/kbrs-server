namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eighth : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UserKeys", new[] { "Imei" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.UserKeys", "Imei", unique: true);
        }
    }
}
