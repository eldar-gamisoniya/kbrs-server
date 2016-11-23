namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fourth : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserKeys", "SessionKeyGenerated", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserKeys", "SessionKeyGenerated", c => c.DateTime(nullable: false));
        }
    }
}
