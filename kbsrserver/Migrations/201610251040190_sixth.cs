namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sixth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserKeys", "PublicSignKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserKeys", "PublicSignKey");
        }
    }
}
