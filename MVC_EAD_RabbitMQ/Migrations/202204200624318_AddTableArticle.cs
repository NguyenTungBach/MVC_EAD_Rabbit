namespace MVC_EAD_RabbitMQ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableArticle : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        Url = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                        Image = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Url);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Articles");
        }
    }
}
