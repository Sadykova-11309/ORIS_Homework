using System.Data;


namespace MyORMLibrary
{
    public class Test1Context<T> where T : class, new()
    {
        private readonly IDbConnection _dbConnection;

        public Test1Context(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public T GetById(int id)
        {
            string query = $"SELECT * FROM {typeof(T).Name}s WHERE Id = @Id";

            _dbConnection.Open();

            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }

            _dbConnection.Close();
            return null;
        }

        public IEnumerable<T> GetAll()
        {
            var entities = new List<T>();
            string query = $"SELECT * FROM {typeof(T).Name}s";

            _dbConnection.Open();

            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entities.Add(Map(reader));
                    }
                }
            }

            _dbConnection.Close();
            return entities;
        }

        public bool Create(T entity)
        {
            var properties = typeof(T).GetProperties();
            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            string query = $"INSERT INTO {typeof(T).Name}s ({columns}) VALUES ({values})";

            _dbConnection.Open();

            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;

                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }

                int result = command.ExecuteNonQuery();
                _dbConnection.Close();

                return result > 0;
            }
        }

        public bool Update(T entity)
        {
            var properties = typeof(T).GetProperties();
            var updates = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            string query = $"UPDATE {typeof(T).Name}s SET {updates} WHERE Id = @Id";

            _dbConnection.Open();

            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;

                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }

                int result = command.ExecuteNonQuery();
                _dbConnection.Close();

                return result > 0;
            }
        }

        public bool Delete(int id)
        {
            string query = $"DELETE FROM {typeof(T).Name}s WHERE Id = @Id";

            _dbConnection.Open();

            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                int result = command.ExecuteNonQuery();
                _dbConnection.Close();

                return result > 0;
            }
        }

        private T Map(IDataReader reader)
        {
            var entity = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (reader[property.Name] != DBNull.Value)
                {
                    property.SetValue(entity, reader[property.Name]);
                }
            }

            return entity;
        }
    }
}
