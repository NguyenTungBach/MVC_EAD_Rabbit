namespace MVC_EAD_RabbitMQ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateArticleId : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Articles");
            AddColumn("dbo.Articles", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Articles", "Url", c => c.String());
            AddPrimaryKey("dbo.Articles", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Articles");
            AlterColumn("dbo.Articles", "Url", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Articles", "Id");
            AddPrimaryKey("dbo.Articles", "Url");
        }
    }
}
