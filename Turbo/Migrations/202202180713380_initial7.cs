namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial7 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TradingSignals", "CurrentPrice", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TradingSignals", "CurrentPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
