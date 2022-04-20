using MVC_EAD_RabbitMQ.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MVC_EAD_RabbitMQ.Data
{
    public class RabbitContext: DbContext
    {
        public RabbitContext() : base("name=RabbitDBAssignment")
        {
        }

        public DbSet<Source> Source { get; set; }
        public DbSet<Article> Article { get; set; }
    }
}