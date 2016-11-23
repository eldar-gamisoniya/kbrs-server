namespace kbsrserver.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fifth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserKeys", "SignHalfKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserKeys", "SignHalfKey");
        }
    }
}
