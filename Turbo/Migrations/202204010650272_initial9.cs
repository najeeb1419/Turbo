namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial9 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TradingSignals", "Buy", c => c.String());
            AlterColumn("dbo.Notifications", "Price", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.TradingSignals", "Buy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
