namespace APIGateway.Presentation.Middleware
{
    public class AttachSignature(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers["Api-Gateway"] = "Signed";
            await next(context);
        }
    }
}
