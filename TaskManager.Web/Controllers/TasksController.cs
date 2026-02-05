using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Models;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers
{
    public class TasksController : Controller
    {
        private readonly ITaskApiClient _client;

        public TasksController(ITaskApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _client.GetTasksAsync(page, pageSize);
            return View(result);
        }
        public async Task<IActionResult> Search(TaskSearchViewModel model)
        {
            // Si es la primera carga de la página
            if (model.Page == 0)
                model.Page = 1;

            model.Result = await _client.SearchTasksAsync(model);

            return View("Index1", model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTaskViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _client.CreateTaskAsync(model);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _client.GetTaskByIdAsync(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _client.UpdateTaskAsync(model);
                TempData["Success"] = "La tarea fue actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _client.DeleteTaskAsync(id);
                TempData["Success"] = "La tarea fue eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar la tarea: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _client.GetTaskDetailAsync(id);

            if (task == null)
            {
                TempData["Error"] = "La tarea no existe.";
                return RedirectToAction("Index");
            }

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Index2(TaskSearchViewModel filters)
        {
            var result = await _client.AdvancedSearchAsync(filters);
            filters.Result = result;
            return View(filters); // regresamos siempre el modelo completo
        }

        [HttpGet]
        public IActionResult AjaxDemo()
        {
            return View();
        }
    }
}