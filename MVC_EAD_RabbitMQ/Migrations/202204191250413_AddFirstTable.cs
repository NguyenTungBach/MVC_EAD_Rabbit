namespace MVC_EAD_RabbitMQ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFirstTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Url = c.String(),
                        LinkSelector = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Sources");
        }
    }
}
