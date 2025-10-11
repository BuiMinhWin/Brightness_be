using BrightNess.Library.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrightNess.Library.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //khai bao default bien 
            string message = "Sorry, internal server error occured";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                // Kiểm tra xem response đã bắt đầu chưa
                if (!context.Response.HasStarted)
                {
                    //check if response is too many request :429 status code
                    if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                    {
                        title = "Warning";
                        message = "Too many request made.";
                        statusCode = (int)StatusCodes.Status429TooManyRequests;
                        await ModifyHeader(context, title, message, statusCode);
                    }

                    // if response is unauthorized// 401 status code
                    else if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                    {
                        title = "Alert";
                        message = "You are not authorized to access";
                        statusCode = (int)StatusCodes.Status401Unauthorized;
                        await ModifyHeader(context, title, message, statusCode);
                    }

                    //If response is forbidden //403 status code
                    else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        title = "Out of Access";
                        message = "You are not permission to access";
                        statusCode = (int)StatusCodes.Status403Forbidden;
                        await ModifyHeader(context, title, message, statusCode);
                    }
                }
            }
            catch (Exception e)
            {
                //Log original exceptions 
                LogException.LogExceptionx(e);

                //check if Exception is Timeout 
                if (e is TaskCanceledException || e is TimeoutException)
                {
                    title = "Out of time";
                    message = "Request timeout... try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                
                // Chỉ ghi response nếu chưa bắt đầu
                if (!context.Response.HasStarted)
                {
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // Kiểm tra lần nữa để đảm bảo an toàn
            if (context.Response.HasStarted)
            {
                return;
            }

            // Clear response trước khi ghi mới
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }));
        }
    }
}
