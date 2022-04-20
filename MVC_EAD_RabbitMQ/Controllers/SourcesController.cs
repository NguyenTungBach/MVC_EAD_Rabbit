using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using MVC_EAD_RabbitMQ.Data;
using MVC_EAD_RabbitMQ.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MVC_EAD_RabbitMQ.Controllers
{
    public class SourcesController : Controller
    {
        public static HashSet<Content> setLink;
        private RabbitContext db = new RabbitContext();

        // GET: Sources
        public ActionResult Index()
        {
            return View(db.Source.ToList());
        }

        // GET: Sources/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Source.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }
            return View(source);
        }

        // GET: Sources/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sources/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Url,LinkSelector")] Source source)
        {
            if (ModelState.IsValid)
            {
                db.Source.Add(source);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(source);
        }

        // GET: Sources/Create
        public ActionResult GetLink()
        {
            return View();
        }

        public ActionResult Fail()
        {
            return View();
        }

        public ActionResult Success()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckLink(Content content)
        {
            if (content.UrlSource != "" && content.LinkSelector != "")
            {
                try
                {
                    var web = new HtmlWeb();
                    HtmlDocument doc = web.Load(content.UrlSource);
                    var nodeList = doc.QuerySelectorAll(content.LinkSelector); // tìm đến những thẻ a nằm trong h3 có class= title-news
                    setLink = new HashSet<Content>(); // Đảm bảo link không giống nhau, nếu có sẽ bị ghi đè ở phần tử trước

                    foreach (var node in nodeList)
                    {
                        var link = node.Attributes["href"].Value;
                        if (string.IsNullOrEmpty(link)) // nếu link này null
                        {
                            continue;
                        }
                        Content sourceLink = new Content()
                        {
                            UrlSource = link,
                            LinkSelector = content.LinkSelector
                        };
                        if (sourceLink == null) // nếu link này null
                        {
                            continue;
                        }

                        setLink.Add(sourceLink);
                    }
                    
                    return PartialView("ListLink", setLink);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    return PartialView("Fail");
                }
            }
            return PartialView("Fail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Preview(Content content)
        {
            if (content.UrlArticle != "" && content.Title != "" && content.Image != "" && content.Description != "")
            {
                try
                {
                    var web = new HtmlWeb();
                    HtmlDocument doc = web.Load(content.UrlArticle); // Lấy nội dung bên trong link đó
                    var title = doc.QuerySelector(content.Title).InnerHtml; // tìm đến những h1 có class= title-detail
                    var description = doc.QuerySelector(content.Description).InnerHtml;
                    var image = doc.QuerySelector(content.Image).Attributes["src"].Value;


                    var article = new Article()
                    {
                        Url = content.UrlArticle,
                        Title = title,
                        Description = description,
                        Image = image,
                    };

                    return PartialView("Preview", article);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    return PartialView("Fail");
                }
            }
            return PartialView("Fail");
        }

        public ActionResult SaveLink()
        {
            QueueSend();
            QueueReceived();

            return View("Index", db.Source.ToList());
        }

        public void QueueSend()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue ",
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);


                foreach (var link in setLink)
                {
                    var source = new Source()
                    {
                        Url = link.UrlSource,
                        LinkSelector = link.LinkSelector,
                    };

                    try
                    {
                        db.Source.Add(source);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error: " + e.Message);
                    }
                    var body = Encoding.UTF8.GetBytes(link.UrlSource);

                    channel.BasicPublish(exchange: "",
                                            routingKey: "task_queue",
                                            basicProperties: null,
                                            body: body);
                }
            }
        }

        public void QueueReceived()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: "task_queue ",
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var link = Encoding.UTF8.GetString(body);
                        var web = new HtmlWeb();
                        HtmlDocument doc = web.Load(link); // Lấy nội dung bên trong link đó
                        string title = doc.QuerySelector("h1.title-detail").InnerHtml ?? null; // tìm đến những h1 có class= title-detail
                        string description = doc.QuerySelector("p.description").InnerHtml ?? null;
                        string image = doc.QuerySelector("img").Attributes["src"].Value ?? null;

                        var article = new Article()
                        {
                            Url = link,
                            Title = title,
                            Description = description,
                            Image = image,
                        };
                        db.Article.Add(article);
                        db.SaveChanges();
                    };
                    channel.BasicConsume(queue: "task_queue",
                                            autoAck: true,
                                            consumer: consumer);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                }
            }
        }

        // GET: Sources/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Source.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }

            return View(source);
        }

        // POST: Sources/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Url,LinkSelector")] Source source)
        {
            if (ModelState.IsValid)
            {
                db.Entry(source).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(source);
        }

        // GET: Sources/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Source.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }
            return View(source);
        }

        // POST: Sources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Source source = db.Source.Find(id);
            db.Source.Remove(source);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
