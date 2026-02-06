using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TicTacToe.Util.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Criar uma instância do middleware com uma instância do delegate da 
        /// requisição e um objeto logger
        /// </summary>
        /// <param name="next">Instância do delagte da requisição</param>
        /// <param name="logger">Instância do logger</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        /// <summary>
        /// Executado sempre que uma requisição HTTP é recebida
        /// </summary>
        /// <param name="httpContext">Instância do contexto da requisição</param>
        public async Task InvokeAsync(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {

                HttpStatusCode statusCode;
                string message;

                if (ex is UnauthorizedAccessException)
                {
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Unauthorized -> " + ex.Message;
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "Internal Server Error -> " + ex.Message;
                }


                _logger.LogError(ex, "Erro ao processar requisição.");

                await HandleExceptionAsync(httpContext, ex, statusCode, message);
            }

        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
