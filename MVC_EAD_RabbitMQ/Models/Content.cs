﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_EAD_RabbitMQ.Models
{
    public class Content
    {
        public string Name { get; set; }
        public string UrlSource { get; set; }
        public string LinkSelector { get; set; }
        public string UrlArticle { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}