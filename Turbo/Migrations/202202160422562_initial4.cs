namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradingSignals", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TradingSignals", "Type");
        }
    }
}
