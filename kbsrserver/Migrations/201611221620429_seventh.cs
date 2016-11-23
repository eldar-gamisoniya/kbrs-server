namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seventh : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserKeys", "UserStorageKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserKeys", "UserStorageKey");
        }
    }
}
