using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services
{
    public class CategoryApiClient : ICategoryApiClient
    {
        private readonly HttpClient _httpClient;

        public CategoryApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> ImportCategoriesFromExcelAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent(); 

            // Convertimos el IFormFile en StreamContent
            using var stream = file.OpenReadStream(); // crea un espacio virtual
            var fileContent = new StreamContent(stream); // accesos para manipular el archivo

            // Tipo MIME típico para Excel .xlsx (no es obligatorio pero está bien ponerlo)
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            // El "file" aquí debe coincidir con el nombre del parámetro en el endpoint de la API
            content.Add(fileContent, "file", file.FileName);

            // Llamamos a la API
            var response = await _httpClient.PostAsync("/api/categories/import-excel", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al importar categorías. Respuesta API: {errorBody}");
            }

            // Leemos el JSON que envía la API
            var result = await response.Content.ReadFromJsonAsync<ImportCategoriesResult>();

            // Si por alguna razón no se pudo deserializar
            if (result == null || string.IsNullOrWhiteSpace(result.Message))
            {
                return "Importación realizada correctamente.";
            }

            // Devolvemos el mensaje que vino de la API
            // Ej: "Se importaron 5 categorías nuevas."
            return result.Message +
                   (result.Duplicadas > 0
                        ? $" ({result.Duplicadas} filas duplicadas no se importaron.)"
                        : string.Empty);
        }

        public async Task<List<CategoryOptionViewModel>> GetSimpleListAsync()
        {
            // Llamamos al endpoint de la API: /api/categories/simple-list
            var response = await _httpClient.GetAsync("/api/categories");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"Error al obtener categorías. Código: {(int)response.StatusCode}"
                    : body;

                throw new ApiException(message, (int)response.StatusCode);
            }

            var categories =
                await response.Content.ReadFromJsonAsync<List<CategoryOptionViewModel>>()
                ?? new List<CategoryOptionViewModel>();

            return categories;
        }
    }
}