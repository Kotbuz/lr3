using Backend.Data;
using Backend.Logging;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersProxy;

namespace Backend.Controllers
{
    [ApiController]
    [Route("mail")]
    public class MailController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<MailController> _logger;
        private readonly IUserAccessLogic _userAccess;

        public MailController(AppDbContext db, ILogger<MailController> logger, IUserAccessLogic userAccess)
        {
            _db = db;
            _logger = logger;
            _userAccess = userAccess;
        }

        // Обратная связь: проверяем клиента в системе пользователей, логируем и сохраняем в базу
        // Использовать нейминг viewModel а не DTO
        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] TConnectDialog dto)
        {
            _logger.LogInformation("Обратная связь: {Data}", LogJson.Serialize(dto));

            var access = await _userAccess.CheckClientAsync(dto.Name, dto.Phone);
            if (!access.IsAllowed)
            {
                _logger.LogWarning("Обратная связь отклонена: {Message}", access.Message);
                return access.ToDeniedResponse(this);
            }

            return await SaveAsync(dto);
        }

        private async Task<IActionResult> SaveAsync(TConnectDialog dto)
        {
            var feedback = new Feedback { RequiredTask = dto.RequiredTask ?? [], Phone = dto.Phone, Name = dto.Name };
            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Обратная связь сохранена в БД, id = {Id}", feedback.Id);
            return Ok(feedback);
        }

        [HttpGet("feedback")]
        public async Task<List<Feedback>> GetAll() =>
            await _db.Feedbacks.OrderBy(e => e.Id).ToListAsync();

        [HttpGet("feedback/{id:int}")]
        public async Task<ActionResult<Feedback>> Get(int id) =>
            await _db.Feedbacks.FindAsync(id) is { } feedback ? feedback : NotFound();

        [HttpPost("feedback")]
        public Task<IActionResult> Create([FromBody] TConnectDialog dto) => SaveAsync(dto);

        [HttpPut("feedback/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TConnectDialog dto)
        {
            var feedback = await _db.Feedbacks.FindAsync(id);
            if (feedback == null) return NotFound();

            feedback.RequiredTask = dto.RequiredTask ?? [];
            feedback.Phone = dto.Phone;
            feedback.Name = dto.Name;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Обратная связь {Id} изменена: {Data}", id, LogJson.Serialize(dto));
            return Ok(feedback);
        }

        [HttpDelete("feedback/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _db.Feedbacks.FindAsync(id);
            if (feedback == null) return NotFound();

            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Обратная связь {Id} удалена", id);
            return NoContent();
        }
    }
}
