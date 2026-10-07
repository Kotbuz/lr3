using funny.Logic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace funny.Controllers
{
    public class JokeController : Controller
    {
        ILogger<JokeController> _logger;
        IOldSexLogic _oldSexLogic;


   

        public JokeController(ILogger<JokeController> logger, IOldSexLogic oldSexLogic)
        {
            _oldSexLogic = oldSexLogic;
            _logger = logger;
        }
        [HttpGet("GetJoke")]
        public async Task GetJoke([FromQuery] int index, [FromQuery] int old, [FromQuery] bool sex)
        {
            // Загружаем анекдот
            var joke = await _oldSexLogic.GetJoke(sex,old);

            Response.ContentType = "text/html;charset=utf-8";

            // Заполняем таблицу заголовками http запроса
            System.Text.StringBuilder table = new System.Text.StringBuilder("<h2>Request Headers</h2>");
            foreach(var header in Request.Headers)
            {
                table.Append($"<div style=\"display: flex; flex-direction: row;\"><div>{header.Key}</div><div style=\"margin-left: 30px\">{header.Value}</div></div>");
            }
            table.Append("<div style=\"height:100px;\"></div>");

            //Заполняем рекламными данными
            var userAgent = Request.Headers.FirstOrDefault(e => e.Key == "User-Agent");

            if (userAgent.Value.Any(a => a.Contains("Mozilla")))
            {
                joke += " .\r\n Купите новый сяоми!"; 
            }

            if (userAgent.Value.Any(a => a.Contains("Chrome")))
            {
                joke += " .\r\n Купите новый iphone!";
            }

            table.Append(joke);


            await Response.WriteAsync(table.ToString());
        }



    }
}
