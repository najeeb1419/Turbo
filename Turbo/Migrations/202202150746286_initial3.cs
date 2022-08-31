namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "CurrencyName", c => c.String());
            AddColumn("dbo.Notifications", "EmployeeName", c => c.String());
            AddColumn("dbo.Notifications", "Status", c => c.String());
            AddColumn("dbo.Notifications", "Type", c => c.String());
            AddColumn("dbo.Notifications", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Price");
            DropColumn("dbo.Notifications", "Type");
            DropColumn("dbo.Notifications", "Status");
            DropColumn("dbo.Notifications", "EmployeeName");
            DropColumn("dbo.Notifications", "CurrencyName");
        }
    }
}
