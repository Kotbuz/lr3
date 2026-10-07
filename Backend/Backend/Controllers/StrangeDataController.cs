using System.Net;
using System.Text;
using Backend.Data;
using Backend.Logging;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using UsersProxy;

namespace Backend.Controllers
{
    [ApiController]
    [Route("strangeData")]
    public class StrangeDataController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<StrangeDataController> _logger;
        private readonly IUserAccessLogic _userAccess;

        public StrangeDataController(AppDbContext db, ILogger<StrangeDataController> logger, IUserAccessLogic userAccess)
        {
            _db = db;
            _logger = logger;
            _userAccess = userAccess;
        }

        #region Методы для фронта

        // Заказ колы: проверяем клиента в системе пользователей, логируем и сохраняем в базу
        [HttpPost("SendCola")]
        public async Task<IActionResult> SendCola([FromBody] TConnectDialogCola dto)
        {
            _logger.LogInformation("Заказ колы: {Data}", LogJson.Serialize(dto));

            var access = await _userAccess.CheckClientAsync(dto.Name, dto.Phone);
            if (!access.IsAllowed)
            {
                _logger.LogWarning("Заказ колы отклонён: {Message}", access.Message);
                return access.ToDeniedResponse(this);
            }

            return await SaveColaAsync(dto);
        }

        private async Task<IActionResult> SaveColaAsync(TConnectDialogCola dto)
        {
            var order = new ColaOrder { Tasty = dto.Tasty, Volume = dto.Volume, Name = dto.Name, Phone = dto.Phone };
            _db.ColaOrders.Add(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ колы сохранён в БД, id = {Id}", order.Id);
            return Ok(order);
        }

        // Вызов полиции: помогаем только пользователям Google Chrome
        [HttpPost("HelpPolice")]
        public IActionResult HelpPolice([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] TConnectDialogPolice? dto)
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            var browser = DetectBrowser(userAgent);
            _logger.LogInformation("Вызов полиции. Адрес: {Address}, браузер: {Browser}, User-Agent: {UserAgent}",
                dto?.Address ?? "не указан", browser, userAgent);

            if (browser != "Chrome")
            {
                _logger.LogWarning("Полиция не приедет: браузер {Browser} не Chrome", browser);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { help = false, message = $"Полиция помогает только пользователям Chrome. Ваш браузер: {browser}" });
            }

            return Ok(new { help = true, message = "Полиция выехала!" });
        }

        // Заказ пиццы: проверяем клиента в системе пользователей, логируем и сохраняем в базу
        [HttpPost("SendPizza")]
        public async Task<IActionResult> SendPizza([FromBody] TConnectDialogPizza dto)
        {
            _logger.LogInformation("Заказ пиццы: {Data}", LogJson.Serialize(dto));

            var access = await _userAccess.CheckClientAsync(dto.Name, dto.Phone);
            if (!access.IsAllowed)
            {
                _logger.LogWarning("Заказ пиццы отклонён: {Message}", access.Message);
                return access.ToDeniedResponse(this);
            }

            return await SavePizzaAsync(dto);
        }

        private async Task<IActionResult> SavePizzaAsync(TConnectDialogPizza dto)
        {
            var order = new PizzaOrder
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Size = dto.Size,
                Options = dto.Options ?? [],
                Thickness = dto.Thickness
            };
            _db.PizzaOrders.Add(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ пиццы сохранён в БД, id = {Id}", order.Id);
            return Ok(order);
        }

        // Список услуг из БД в виде html (фильтр по названию/описанию, сортировка по названию)
        [HttpGet("GetDigitalList")]
        [Produces("text/html")]
        public async Task<ContentResult> GetDigitalList([FromQuery] TDigitalList query)
        {
            _logger.LogInformation("Запрос списка услуг: filter = {Filter}, sorted = {Sorted}", query.Filter, query.Sorted);

            var services = _db.DigitalServices.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var pattern = $"%{query.Filter.Trim()}%";
                services = services.Where(s => EF.Functions.ILike(s.Name, pattern)
                                            || (s.Description != null && EF.Functions.ILike(s.Description, pattern)));
            }

            var descending = query.Sorted?.StartsWith("desc", StringComparison.OrdinalIgnoreCase) == true;
            services = descending ? services.OrderByDescending(s => s.Name) : services.OrderBy(s => s.Name);

            var list = await services.ToListAsync();

            var html = new StringBuilder();
            html.Append("<div style=\"padding: 12px; width: 100%;\">");
            if (list.Count == 0)
            {
                html.Append("<p>Услуги не найдены</p>");
            }
            else
            {
                html.Append("<table style=\"width: 100%; border-collapse: collapse;\">");
                html.Append("<tr><th style=\"text-align:left; border-bottom:1px solid #ccc; padding:6px;\">Услуга</th>");
                html.Append("<th style=\"text-align:left; border-bottom:1px solid #ccc; padding:6px;\">Описание</th>");
                html.Append("<th style=\"text-align:right; border-bottom:1px solid #ccc; padding:6px;\">Цена, руб.</th></tr>");
                foreach (var s in list)
                {
                    html.Append("<tr>");
                    html.Append($"<td style=\"padding:6px;\">{WebUtility.HtmlEncode(s.Name)}</td>");
                    html.Append($"<td style=\"padding:6px;\">{WebUtility.HtmlEncode(s.Description ?? "")}</td>");
                    html.Append($"<td style=\"padding:6px; text-align:right;\">{s.Price:0.00}</td>");
                    html.Append("</tr>");
                }
                html.Append("</table>");
            }
            html.Append("</div>");

            return Content(html.ToString(), "text/html; charset=utf-8");
        }

        #endregion

        #region CRUD: заказы колы

        [HttpGet("cola")]
        public async Task<List<ColaOrder>> GetColaOrders() =>
            await _db.ColaOrders.OrderBy(e => e.Id).ToListAsync();

        [HttpGet("cola/{id:int}")]
        public async Task<ActionResult<ColaOrder>> GetColaOrder(int id) =>
            await _db.ColaOrders.FindAsync(id) is { } order ? order : NotFound();

        [HttpPost("cola")]
        public Task<IActionResult> CreateColaOrder([FromBody] TConnectDialogCola dto) => SaveColaAsync(dto);

        [HttpPut("cola/{id:int}")]
        public async Task<IActionResult> UpdateColaOrder(int id, [FromBody] TConnectDialogCola dto)
        {
            var order = await _db.ColaOrders.FindAsync(id);
            if (order == null) return NotFound();

            order.Tasty = dto.Tasty;
            order.Volume = dto.Volume;
            order.Name = dto.Name;
            order.Phone = dto.Phone;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ колы {Id} изменён: {Data}", id, LogJson.Serialize(dto));
            return Ok(order);
        }

        [HttpDelete("cola/{id:int}")]
        public async Task<IActionResult> DeleteColaOrder(int id)
        {
            var order = await _db.ColaOrders.FindAsync(id);
            if (order == null) return NotFound();

            _db.ColaOrders.Remove(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ колы {Id} удалён", id);
            return NoContent();
        }

        #endregion

        #region CRUD: заказы пиццы

        [HttpGet("pizza")]
        public async Task<List<PizzaOrder>> GetPizzaOrders() =>
            await _db.PizzaOrders.OrderBy(e => e.Id).ToListAsync();

        [HttpGet("pizza/{id:int}")]
        public async Task<ActionResult<PizzaOrder>> GetPizzaOrder(int id) =>
            await _db.PizzaOrders.FindAsync(id) is { } order ? order : NotFound();

        [HttpPost("pizza")]
        public Task<IActionResult> CreatePizzaOrder([FromBody] TConnectDialogPizza dto) => SavePizzaAsync(dto);

        [HttpPut("pizza/{id:int}")]
        public async Task<IActionResult> UpdatePizzaOrder(int id, [FromBody] TConnectDialogPizza dto)
        {
            var order = await _db.PizzaOrders.FindAsync(id);
            if (order == null) return NotFound();

            order.Name = dto.Name;
            order.Phone = dto.Phone;
            order.Size = dto.Size;
            order.Options = dto.Options ?? [];
            order.Thickness = dto.Thickness;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ пиццы {Id} изменён: {Data}", id, LogJson.Serialize(dto));
            return Ok(order);
        }

        [HttpDelete("pizza/{id:int}")]
        public async Task<IActionResult> DeletePizzaOrder(int id)
        {
            var order = await _db.PizzaOrders.FindAsync(id);
            if (order == null) return NotFound();

            _db.PizzaOrders.Remove(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Заказ пиццы {Id} удалён", id);
            return NoContent();
        }

        #endregion

        #region CRUD: список услуг

        [HttpGet("digital")]
        public async Task<List<DigitalService>> GetDigitalServices() =>
            await _db.DigitalServices.OrderBy(e => e.Id).ToListAsync();

        [HttpGet("digital/{id:int}")]
        public async Task<ActionResult<DigitalService>> GetDigitalService(int id) =>
            await _db.DigitalServices.FindAsync(id) is { } service ? service : NotFound();

        [HttpPost("digital")]
        public async Task<IActionResult> CreateDigitalService([FromBody] DigitalServiceDto dto)
        {
            var service = new DigitalService { Name = dto.Name, Description = dto.Description, Price = dto.Price };
            _db.DigitalServices.Add(service);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Добавлена услуга {Id}: {Data}", service.Id, LogJson.Serialize(dto));
            return CreatedAtAction(nameof(GetDigitalService), new { id = service.Id }, service);
        }

        [HttpPut("digital/{id:int}")]
        public async Task<IActionResult> UpdateDigitalService(int id, [FromBody] DigitalServiceDto dto)
        {
            var service = await _db.DigitalServices.FindAsync(id);
            if (service == null) return NotFound();

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Услуга {Id} изменена: {Data}", id, LogJson.Serialize(dto));
            return Ok(service);
        }

        [HttpDelete("digital/{id:int}")]
        public async Task<IActionResult> DeleteDigitalService(int id)
        {
            var service = await _db.DigitalServices.FindAsync(id);
            if (service == null) return NotFound();

            _db.DigitalServices.Remove(service);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Услуга {Id} удалена", id);
            return NoContent();
        }

        #endregion

        // Определяем браузер по User-Agent. Edge, Opera и Яндекс тоже пишут "Chrome", поэтому проверяем их раньше
        private static string DetectBrowser(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            if (userAgent.Contains("Edg/") || userAgent.Contains("Edge/")) return "Edge";
            if (userAgent.Contains("OPR/") || userAgent.Contains("Opera")) return "Opera";
            if (userAgent.Contains("YaBrowser/")) return "Yandex";
            if (userAgent.Contains("Firefox/")) return "Firefox";
            if (userAgent.Contains("Chrome/") || userAgent.Contains("CriOS/")) return "Chrome";
            if (userAgent.Contains("Safari/")) return "Safari";
            return "Unknown";
        }
    }
}
