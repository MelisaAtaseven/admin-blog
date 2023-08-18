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
            _logger = logger; // ILogger �rne�i eklenir
            _context = context; //// BlogContext �rne�i eklenir, veritaban�
        }

        // Blog yaz�lar�n�n listesini g�r�nt�ler
        public IActionResult Index()
        {
            var list = _context.Blog.ToList(); // T�m blog yaz�lar� veritaban�ndan �ekilir
            return View(list); // Blog yaz�lar�n�n listesi 'Index' view'�na g�nderilir
        }

        // Blog yaz�s�n� yay�nlar
        public IActionResult Publish(int Id)
        {
            var blog = _context.Blog.Find(Id); // Bir ID'ye sahip blog yaz�s� �ekilir
            blog.IsPublish = true; // Yaz� yay�nland� olarak i�aretlenir
            _context.Update(blog); // Veritaban� g�ncelleme 
            _context.SaveChanges(); 
            return RedirectToAction(nameof(Index)); 
        }

        // Blog yaz�s� d�zenleme sayfas�n� g�r�nt�ler
        public IActionResult Edit(int id)
        {
            var blog = _context.Blog.Find(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // Blog yaz�s�n�n g�ncelleme i�lemini ger�ekle�tirir
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

        // Blog yaz�s� silme sayfas�n� g�r�nt�ler
        public IActionResult Delete(int id)
        {
            var blog = _context.Blog.Find(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        // Blog yaz�s� ekleme sayfas�n� g�r�nt�ler
        public IActionResult Blog()
        {
            ViewBag.Categories = _context.Category.Select(w =>
                new SelectListItem
                {
                    Text = w.Name,
                    Value = w.Id.ToString()
                }
            ).ToList();// Kategori se�enekleri haz�rlan�p ViewBag �zerinden view'a iletilir
            return View();
        }

        // Yeni blog yaz�s� ekler
        [HttpPost]
        public async Task<IActionResult> Save(Blog model)
        {
            if (model != null)
            {
                var file = Request.Form.Files.First(); // Formdan dosya �ekilir

                // Dosya kaydetme yolu olu�turulur
                string savePath = Path.Combine("C:", "Users", "atase", "Desktop", "my-blog-master", "wwwroot", "img");

                // �u anki tarih ve saat ile dosya ad� olu�turulur
                var fileName = $"{DateTime.Now:MMddHHmmss}.{file.FileName.Split(".").Last()}";
                var fileUrl = Path.Combine(savePath, fileName);

                // Kaydetme
                using (var fileStream = new FileStream(fileUrl, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Modelde dosya yolu ve yazar kimli�i g�ncelleme
                model.ImagePath = fileName;
                model.AuthorId = (int)HttpContext.Session.GetInt32("id");

                // Model veritaban�na eklenir ve de�i�iklikler kaydetme
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();

                return Json(true); 
            }

            return Json(false); // Ba�ar�s�z sonu�
        }

        // Blog yaz�s� silme i�lemini onaylar
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

        // Hata sayfas�n� g�r�nt�ler
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
