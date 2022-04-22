using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_EAD_RabbitMQ.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        //public virtual Source Source { get; set; } // 1 bài viết có 1 link
    }
}