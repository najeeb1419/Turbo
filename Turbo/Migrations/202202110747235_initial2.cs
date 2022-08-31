namespace Turbo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AllowNotifications",
                c => new
                    {
                        AllowNotificationId = c.Int(nullable: false, identity: true),
                        DeviceUserId = c.Int(nullable: false),
                        CompanyEmployeeID = c.Int(nullable: false),
                        RegisterComapanyID = c.Int(nullable: false),
                        status = c.Boolean(nullable: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AllowNotificationId)
                .ForeignKey("dbo.CompanyEmployees", t => t.CompanyEmployeeID, cascadeDelete: false)
                .ForeignKey("dbo.RegisterComapanies", t => t.RegisterComapanyID, cascadeDelete: false)
                .Index(t => t.CompanyEmployeeID)
                .Index(t => t.RegisterComapanyID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AllowNotifications", "RegisterComapanyID", "dbo.RegisterComapanies");
            DropForeignKey("dbo.AllowNotifications", "CompanyEmployeeID", "dbo.CompanyEmployees");
            DropIndex("dbo.AllowNotifications", new[] { "RegisterComapanyID" });
            DropIndex("dbo.AllowNotifications", new[] { "CompanyEmployeeID" });
            DropTable("dbo.AllowNotifications");
        }
    }
}
