using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Session
{
    internal class SessionStorage
    {
        private static readonly Dictionary<string, int> _sessions = new Dictionary<string, int>();

        // Сохранение токена и его соответствующего ID пользователя
        public static void SaveSession(string token, int userId)
        {
            _sessions[token] = userId;
        }

        // Проверка токена
        public static bool ValidateToken(string token)
        {
            return _sessions.ContainsKey(token);
        }

        // Получение ID пользователя по токену
        public static int? GetUserId(string token)
        {
            return _sessions.TryGetValue(token, out var userId) ? userId : null;
        }
    }
}
