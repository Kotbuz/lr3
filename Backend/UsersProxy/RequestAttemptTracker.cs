using System.Collections.Concurrent;

namespace UsersProxy
{
    // Считает заявки каждого клиента за последние WindowSeconds секунд (хранится в памяти, регистрируется как Singleton)
    public class RequestAttemptTracker
    {
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _attempts = new();

        // Регистрирует новую заявку и возвращает число заявок клиента в окне, включая текущую
        public int Register(string userId, TimeSpan window)
        {
            var now = DateTime.UtcNow;
            var queue = _attempts.GetOrAdd(userId, _ => new Queue<DateTime>());
            lock (queue)
            {
                while (queue.Count > 0 && now - queue.Peek() > window)
                    queue.Dequeue();

                queue.Enqueue(now);
                return queue.Count;
            }
        }

        public void Reset(string userId) => _attempts.TryRemove(userId, out _);
    }
}
