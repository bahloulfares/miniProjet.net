using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Models.Domain;
using BookStore.Repositories.Abstract;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IGenreService _genreService;
        private readonly IPublisherService _publisherService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BookController(
            IBookService bookService,
            IGenreService genreService,
            IPublisherService publisherService,
            IAuthorService authorService,
            IWebHostEnvironment hostingEnvironment)
        {
            _bookService = bookService;
            _genreService = genreService;
            _publisherService = publisherService;
            _authorService = authorService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Add()
        {
            var model = new Book();
            model.AuthorList = _authorService.GetAll().Select(a => new SelectListItem { Text = a.AuthorName, Value = a.Id.ToString() }).ToList();
            model.PublisherList = _publisherService.GetAll().Select(a => new SelectListItem { Text = a.PublisherName, Value = a.Id.ToString() }).ToList();
            model.GenreList = _genreService.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() }).ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(Book model, IFormFile? imageFile)
        {
            // Populate dropdown lists
            model.AuthorList = _authorService.GetAll()
                .Select(a => new SelectListItem { Text = a.AuthorName, Value = a.Id.ToString(), Selected = a.Id == model.AuthorId })
                .ToList();
            model.PublisherList = _publisherService.GetAll()
                .Select(a => new SelectListItem { Text = a.PublisherName, Value = a.Id.ToString(), Selected = a.Id == model.PublisherId })
                .ToList();
            model.GenreList = _genreService.GetAll()
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(), Selected = a.Id == model.GenreId })
                .ToList();

            if (!ModelState.IsValid)
            {
                // If an image is uploaded, save it to the server
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }
                    model.ImagePath = uniqueFileName;
                }
                return View(model);
            }

            // If the model state is valid, attempt to add the book
            var result = _bookService.Add(model);
            if (result)
            {
                TempData["msg"] = "Added Successfully";
                return RedirectToAction(nameof(Add));
            }

            TempData["msg"] = "Error has occurred on the server side";
            return View(model);
        }


        public IActionResult Update(int id)
        {
            var model = _bookService.FindById(id);
            model.AuthorList = _authorService.GetAll().Select(a => new SelectListItem { Text = a.AuthorName, Value = a.Id.ToString(), Selected = a.Id == model.AuthorId }).ToList();
            model.PublisherList = _publisherService.GetAll().Select(a => new SelectListItem { Text = a.PublisherName, Value = a.Id.ToString(), Selected = a.Id == model.PublisherId }).ToList();
            model.GenreList = _genreService.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(), Selected = a.Id == model.GenreId }).ToList();
            return View(model);
        }



        [HttpPost]
        public IActionResult Update(Book model, IFormFile? imageFile)
        {
            

            if (!ModelState.IsValid)
            {
                // If an image is uploaded, save it to the server
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }
                    model.ImagePath = uniqueFileName;
                }
                return View(model);
            }

            // Mettez à jour le livre
            var result = _bookService.Update(model);
            if (result)
            {
                TempData["msg"] = "Update Successful";
                return RedirectToAction("GetAll");
            }

            TempData["msg"] = "Update Failed";
            return View(model);
        }






        public IActionResult Delete(int id)
        {
            var result = _bookService.Delete(id);
            return RedirectToAction("GetAll");
        }

        public IActionResult GetAll(string searchTerm)
        {
            var data = _bookService.GetAll();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                data = data.Where(b =>
                    b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                    b.AuthorName.ToLower().Contains(searchTerm.ToLower()) ||
                    b.PublisherName.ToLower().Contains(searchTerm.ToLower()) ||
                    b.GenreName.ToLower().Contains(searchTerm.ToLower())
                ).ToList();
            }

            return View(data);
        }
    }
}
