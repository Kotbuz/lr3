using funny.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace funny.Logic
{
    public struct Joke
    {
        public string text;
        public int[] sexRange;
        public int[] oldRange;
    }
    public class OldSexLogic : IOldSexLogic
    {

        private readonly ILogger<HomeController> _logger;


        public List<Joke> jokes = new List<Joke>()
    {
        // --- ДЕТИ (0-12) ---
        new Joke {
            text = "Почему компьютер чихнул? Потому что у него был вирус!",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 0, 12 }
        },
        new Joke {
            text = "Что говорит одна рыбка другой? Ничего, они же молчат!",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 0, 12 }
        },
        new Joke {
            text = "Почему мальчики любят играть в футбол? Потому что там можно поваляться в грязи и никто не наругает!",
            sexRange = new int[] { 0 },
            oldRange = new int[] { 0, 12 }
        },
        new Joke {
            text = "Почему куклы не любят есть кашу? Потому что у них нет зубов, но есть характер!",
            sexRange = new int[] { 1 },
            oldRange = new int[] { 0, 12 }
        },

        // --- ПОДРОСТКИ (13-19) ---
        new Joke {
            text = "Мама: 'Ты сделал уроки?'. Я: 'Да'. Мама: 'А почему дневник пустой?'. Я: 'Так я его не открывал, чтобы знания не выветрились'.",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 13, 19 }
        },
        new Joke {
            text = "Парень приходит к девушке и говорит: 'Я тебя люблю'. Она: 'Докажи'. Он: 'Ладно, но только один раз, я не хочу тратить весь день'.",
            sexRange = new int[] { 0 },
            oldRange = new int[] { 13, 19 }
        },
        new Joke {
            text = "Девушка говорит подруге: 'Он сказал, что я единственная'. Подруга: 'Ну да, единственная, кто не дал списать контрольную'.",
            sexRange = new int[] { 1 },
            oldRange = new int[] { 13, 19 }
        },

        // --- ВЗРОСЛЫЕ (20-59) ---
        new Joke {
            text = "Мужчина спрашивает у жены: 'Дорогая, где мой кошелек?'. Жена: 'В холодильнике'. Мужчина: 'Что он там делает?'. Жена: 'Он там лежит и думает о смысле жизни'.",
            sexRange = new int[] { 0 },
            oldRange = new int[] { 20, 59 }
        },
        new Joke {
            text = "Женщина говорит мужу: 'Ты меня не слушаешь!'. Муж: 'Прости, что ты сказала?'.",
            sexRange = new int[] { 1 },
            oldRange = new int[] { 20, 59 }
        },
        new Joke {
            text = "Идеальный муж — это тот, кто помнит день рождения жены, но забывает, сколько ей лет.",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 20, 59 }
        },
        new Joke {
            text = "Взрослая жизнь — это когда ты мечтаешь о том, чтобы поспать 8 часов, но просыпаешься в 6 утра, чтобы успеть на работу, которая нужна только для того, чтобы оплатить кредит на машину, на которой ты ездишь на эту работу.",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 20, 59 }
        },

        // --- ПОЖИЛЫЕ (60+) ---
        new Joke {
            text = "Бабушка говорит деду: 'Ты помнишь, как мы познакомились?'. Дед: 'Конечно, помню. Ты сказала, что я выгляжу как мой отец'. Бабушка: 'А ты сказал, что твой отец умер'. Дед: 'Ну да, я просто пытался произвести впечатление'.",
            sexRange = new int[] { 0, 1 },
            oldRange = new int[] { 60, 120 }
        },
        new Joke {
            text = "Дед приходит к врачу и говорит: 'Доктор, у меня склероз'. Врач: 'И давно это у вас?'. Дед: 'Что именно?'.",
            sexRange = new int[] { 0 },
            oldRange = new int[] { 60, 120 }
        },
        new Joke {
            text = "Бабушка в аптеке: 'Дайте мне что-нибудь от старости'. Аптекарь: 'У нас нет такого лекарства'. Бабушка: 'Тогда дайте мне что-нибудь от того, что у меня нет такого лекарства'.",
            sexRange = new int[] { 1 },
            oldRange = new int[] { 60, 120 }
        }
    };
        public OldSexLogic(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public async Task <string> GetJoke(bool sex, int old)
        {
            var tsex = sex ? 0 : 1;
            var joke = jokes.Where(e => e.sexRange.Contains(tsex)).Where(e => e.oldRange.Min() <= old && e.oldRange.Max() >= old).FirstOrDefault().text;
       return joke ?? string.Empty;
        }
        
       
    }
    
}
