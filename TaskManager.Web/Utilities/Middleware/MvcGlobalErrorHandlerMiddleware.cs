using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using TaskManager.Web.Utilities.Exceptions;

namespace TaskManager.Web.Middleware
{
    public class MvcGlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MvcGlobalErrorHandlerMiddleware> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public MvcGlobalErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<MvcGlobalErrorHandlerMiddleware> logger,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _next = next;
            _logger = logger;
            _tempDataFactory = tempDataFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex) // Excepcion personalilzada
            {
                // Errores que vienen de la API (400, 404, etc.)
                _logger.LogWarning(ex, "Error de API capturado en MVC. StatusCode: {StatusCode}", ex.StatusCode);

                var TempData = _tempDataFactory.GetTempData(context); // Se integra el mensaje que manda el API
                TempData["Error"] = ex.Message;

                // Redirigimos a una página amigable (por ejemplo, el listado)
                context.Response.Clear();
                context.Response.Redirect("/Tasks/Index2");
            }
            catch (Exception ex)
            {
                // Errores inesperados del propio MVC
                _logger.LogError(ex, "Error inesperado en MVC.");

                var TempData = _tempDataFactory.GetTempData(context);
                TempData["Error"] = "Ocurrió un error inesperado. Intente de nuevo más tarde.";

                // Aquí puedes redirigir a una vista de error genérica
                context.Response.Clear();
                context.Response.Redirect("/Tasks/Index2");
                //context.Response.Redirect("/Home/Error");
            }
        }
    }
}