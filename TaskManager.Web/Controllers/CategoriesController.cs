using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryApiClient _categoryApiClient;

        public CategoriesController(ICategoryApiClient categoryApiClient)
        {
            _categoryApiClient = categoryApiClient;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo Excel.";
                return View();
            }

            try
            {
                var resultMessage = await _categoryApiClient.ImportCategoriesFromExcelAsync(file);
                TempData["Success"] = resultMessage;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al importar el archivo: " + ex.Message;
            }

            return View();
        }

        public IActionResult Index()
        {
            return View(); // Queda pendiente para otra clase
        }

        // GET: /Categories/Options
        [HttpGet]
        public async Task<IActionResult> Options()
        {
            var categories = await _categoryApiClient.GetSimpleListAsync();
            return Json(categories); // Devuelve JSON al JS del front
        }
    }
}