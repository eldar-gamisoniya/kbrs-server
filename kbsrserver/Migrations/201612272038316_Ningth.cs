namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ningth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Attempts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Attempts");
        }
    }
}
