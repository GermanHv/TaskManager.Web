using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    public class TaskApiClient : ITaskApiClient
    {
        private readonly HttpClient _httpClient;

        public TaskApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<PagedResultViewModel<TaskViewModel>> GetTasksAsync(int page = 1, int pageSize = 10)
        {
            var url = $"/api/tasks/advanced-search?page={page}&pageSize={pageSize}";

            var result = await _httpClient.GetFromJsonAsync<PagedResultViewModel<TaskViewModel>>(url);

            return result!;
        }

        public async Task<PagedResultViewModel<TaskViewModel>> SearchTasksAsync(TaskSearchViewModel filters)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(filters.Text))
                query.Add($"text={filters.Text}");

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
                query.Add($"categoryName={filters.CategoryName}");

            if (filters.IsCompleted.HasValue)
                query.Add($"isCompleted={filters.IsCompleted.Value}");

            if (filters.Step.HasValue)
                query.Add($"step={filters.Step.Value}");

            query.Add($"page={filters.Page}");
            query.Add($"pageSize={filters.PageSize}");

            var finalQueryString = string.Join("&", query);

            var url = $"/api/tasks/advanced-search?{finalQueryString}";

            return await _httpClient.GetFromJsonAsync<PagedResultViewModel<TaskViewModel>>(url);
        }

        public async Task CreateTaskAsync(CreateTaskViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/tasks",
                model
            );

            response.EnsureSuccessStatusCode();
        }

        public async Task<EditTaskViewModel> GetTaskByIdAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<TaskViewModel>($"/api/tasks/{id}");

            return new EditTaskViewModel
            {
                Id = response.Id,
                Title = response.Title,
                CategoryId = response.CategoryId,
                Step = response.Step,
                IsCompleted = response.IsCompleted
            };
        }

        //public async Task<EditTaskViewModel> GetTaskById2Async(int id)
        //{
        //    var response = await _httpClient.GetFromJsonAsync<TaskViewModel>($"/api/tasks/{id}");

            //var model = new UpdateTaskViewModel
            //{
            //    Id = task.Id,
            //    Title = task.Title,
            //    CategoryId = task.CategoryId,
            //    Step = task.Step,
            //    IsCompleted = task.IsCompleted
            //};

        //    return new EditTaskViewModel
        //    {
        //        Id = response.Id,
        //        Title = response.Title,
        //        CategoryId = response.CategoryId,
        //        Step = response.Step,
        //        IsCompleted = response.IsCompleted
        //    };
        //}

        public async Task UpdateTaskAsync(EditTaskViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"/api/tasks/{model.Id}",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                // Leer mensaje de la API
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/tasks/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return true;
        }

        public async Task<TaskViewModel>GetTaskDetailAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TaskViewModel>($"/api/tasks/{id}");
        }

        public async Task<PagedResultViewModel<TaskViewModel>> AdvancedSearchAsync(TaskSearchViewModel filters)
        {
            var query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filters.Text))
                query["text"] = filters.Text;

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
                query["categoryName"] = filters.CategoryName;

            if (filters.CategoryId.HasValue)
                query["categoryId"] = filters.CategoryId.Value.ToString();

            if (filters.Step.HasValue)
                query["step"] = filters.Step.Value.ToString();

            if (filters.IsCompleted.HasValue)
                query["isCompleted"] = filters.IsCompleted.Value.ToString().ToLower();

            query["page"] = filters.Page.ToString();
            query["pageSize"] = filters.PageSize.ToString();

            // Construir una URL con QueryString dinámico
            var queryString = string.Join("&",
                query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            var url = $"/api/tasks/advanced-search?{queryString}";

            return await _httpClient.GetFromJsonAsync<PagedResultViewModel<TaskViewModel>>(url)
                   ?? new PagedResultViewModel<TaskViewModel>
                   {
                       Items = new List<TaskViewModel>(),
                       Page = filters.Page,
                       PageSize = filters.PageSize,
                       TotalCount = 0
                   };
        }
    }
}
