namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StopLoses", "SL", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.StopLoses", "PIPS", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.TakeProfits", "TP", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.TakeProfits", "PIPS", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TakeProfits", "PIPS", c => c.Long(nullable: false));
            AlterColumn("dbo.TakeProfits", "TP", c => c.Long(nullable: false));
            AlterColumn("dbo.StopLoses", "PIPS", c => c.Long(nullable: false));
            AlterColumn("dbo.StopLoses", "SL", c => c.Long(nullable: false));
        }
    }
}
