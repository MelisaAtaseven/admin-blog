using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AdminBlog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using AdminBlog.Filter;

namespace AdminBlog.Controllers
{
    [UserFilter]
    public class BlogController : Controller
    {
        private readonly ILogger<BlogController> _logger;
        private readonly BlogContext _context;

        public BlogController(ILogger<BlogController> logger, BlogContext context)
        {
            _logger = logger; // ILogger örneði eklenir
            _context = context; //// BlogContext örneði eklenir, veritabaný
        }

        // Blog yazýlarýnýn listesini görüntüler
        public IActionResult Index()
        {
            var list = _context.Blog.ToList(); // Tüm blog yazýlarý veritabanýndan çekilir
            return View(list); // Blog yazýlarýnýn listesi 'Index' view'ýna gönderilir
        }

        // Blog yazýsýný yayýnlar
        public IActionResult Publish(int Id)
        {
            var blog = _context.Blog.Find(Id); // Bir ID'ye sahip blog yazýsý çekilir
            blog.IsPublish = true; // Yazý yayýnlandý olarak iþaretlenir
            _context.Update(blog); // Veritabaný güncelleme 
            _context.SaveChanges(); 
            return RedirectToAction(nameof(Index)); 
        }

        // Blog yazýsý düzenleme sayfasýný görüntüler
        public IActionResult Edit(int id)
        {
            var blog = _context.Blog.Find(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // Blog yazýsýnýn güncelleme iþlemini gerçekleþtirir
        [HttpPost]
        public IActionResult Update(int id, Blog updatedBlog)
        {
            var existingBlog = _context.Blog.Find(id);
            if (existingBlog == null)
            {
                return NotFound();
            }

            existingBlog.Title = updatedBlog.Title;
            existingBlog.Subtitle = updatedBlog.Subtitle;
            existingBlog.Content = updatedBlog.Content;
          

            _context.Update(existingBlog);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Blog yazýsý silme sayfasýný görüntüler
        public IActionResult Delete(int id)
        {
            var blog = _context.Blog.Find(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        // Blog yazýsý ekleme sayfasýný görüntüler
        public IActionResult Blog()
        {
            ViewBag.Categories = _context.Category.Select(w =>
                new SelectListItem
                {
                    Text = w.Name,
                    Value = w.Id.ToString()
                }
            ).ToList();// Kategori seçenekleri hazýrlanýp ViewBag üzerinden view'a iletilir
            return View();
        }

        // Yeni blog yazýsý ekler
        [HttpPost]
        public async Task<IActionResult> Save(Blog model)
        {
            if (model != null)
            {
                var file = Request.Form.Files.First(); // Formdan dosya çekilir

                // Dosya kaydetme yolu oluþturulur
                string savePath = Path.Combine("C:", "Users", "atase", "Desktop", "my-blog-master", "wwwroot", "img");

                // þu anki tarih ve saat ile dosya adý oluþturulur
                var fileName = $"{DateTime.Now:MMddHHmmss}.{file.FileName.Split(".").Last()}";
                var fileUrl = Path.Combine(savePath, fileName);

                // Kaydetme
                using (var fileStream = new FileStream(fileUrl, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Modelde dosya yolu ve yazar kimliði güncelleme
                model.ImagePath = fileName;
                model.AuthorId = (int)HttpContext.Session.GetInt32("id");

                // Model veritabanýna eklenir ve deðiþiklikler kaydetme
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();

                return Json(true); 
            }

            return Json(false); // Baþarýsýz sonuç
        }

        // Blog yazýsý silme iþlemini onaylar
        [HttpPost, ActionName("Delete")]
        public IActionResult ConfirmDelete(int id)
        {
            var blog = _context.Blog.Find(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blog.Remove(blog);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Hata sayfasýný görüntüler
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
