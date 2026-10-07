using Microsoft.AspNetCore.Mvc;
using UsersProxy.Models;

namespace Backend.Controllers
{
    public static class ClientAccessResultExtensions
    {
        // Ответ клиенту, если смежная система не разрешила обслуживание
        public static IActionResult ToDeniedResponse(this ClientAccessResult result, ControllerBase controller)
        {
            var body = new { served = false, message = result.Message };
            return result.Status switch
            {
                ClientAccessStatus.InvalidData => controller.BadRequest(body),
                ClientAccessStatus.ServiceUnavailable => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, body),
                _ => controller.StatusCode(StatusCodes.Status403Forbidden, body)
            };
        }
    }
}
