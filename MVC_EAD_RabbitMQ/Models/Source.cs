using System;
using System.Linq;
using System.Web;

namespace MVC_EAD_RabbitMQ.Models
{
    public class Source
    {
       
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string LinkSelector { get; set; }

        //public virtual Article Article { get; set; } // 1 link có 1 bài viết
    }
}