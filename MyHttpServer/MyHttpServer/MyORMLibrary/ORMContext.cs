using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using MyORMLibrary;
using System.Reflection;
using System.Data.Common;

namespace MyORMLibrary;
public class ORMContext<T> where T : class, new()
{
    private readonly IDbConnection _dbconnection;

    public ORMContext(IDbConnection dbconnection)
    {
        _dbconnection = dbconnection;
    }

    public void Create(T entity)
    {
        var properties = entity.GetType().GetProperties()
            .Where(p => p.Name != "Id")
            .ToList();

        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => "@" + p.Name));

        var query = $"INSERT INTO {typeof(T).Name} ({columns}) VALUES ({values})";

        using (var command = _dbconnection.CreateCommand())
        {
            command.CommandText = query;

            foreach (var property in properties)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@" + property.Name;
                parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            if (_dbconnection.State == ConnectionState.Open)
            {
                _dbconnection.Open();
            }
            command.ExecuteNonQuery();
            if (_dbconnection.State == ConnectionState.Closed)
            {
                _dbconnection.Close();
            }
        }
    }

    public T ReadById(int id)
    {
        using (var connection = _dbconnection)
        {
            connection.Open();
            string tableName = typeof(T).Name;
            string queryRequest = $"SELECT * FROM {tableName} WHERE id = @id";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryRequest;

                var parametr = command.CreateParameter();
                parametr.ParameterName = "@id";
                parametr.Value = id;
                command.Parameters.Add(parametr);


                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }

        return null;
    }

    private T Map(IDataReader reader)
    {
        var entity = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
            {
                property.SetValue(entity, reader[property.Name]);
            }
        }
        return entity;
    }


    public T CheckUserByData(string email)
    {
        var tableName = typeof(T).Name;
        using (var connection = _dbconnection)
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName} WHERE email = @email ";
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryRequest;
                var parametr = command.CreateParameter();
                parametr.ParameterName = "@email";
                parametr.Value = email;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }
        return null;
    }


    public T ReadByAll<T>() where T : class, new()
    {
        var tableName = typeof(T).Name;
        using (var connection = _dbconnection)
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName}";
            using (var command = connection.CreateCommand())
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToEntity<T>(reader);
                    }
                }
            }
        }

        return null;
    }

    public T ReadByName<T>(string Name) where T : class, new()
    {
        var tableName = typeof(T).Name;
        using (var connection = _dbconnection)
        {
            connection.Open();
            string queryRequest = $"SELECT * FROM {tableName} WHERE name = @Name ";
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryRequest;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@name";
                parameter.Value = Name;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToEntity<T>(reader);
                    }
                }
            }
        }

        return null;
    }

    public void Update(int id, T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id")
            .ToList();

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var query = $"UPDATE {typeof(T).Name}s SET {setClause} WHERE Id = @id";

        using (var command = _dbconnection.CreateCommand())
        {
            command.CommandText = query;

            foreach (var property in properties)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@" + property.Name;
                parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            _dbconnection.Open();
            command.ExecuteNonQuery();
            _dbconnection.Close();
        }
    }


    public void Delete(int id)
    {
        var query = $"DELETE FROM {typeof(T).Name}s WHERE Id = @id";
        using (var command = _dbconnection.CreateCommand())
        {
            command.CommandText = query;
            var parametr = command.CreateParameter();
            parametr.ParameterName = "@id";
            parametr.Value = id;
            command.Parameters.Add(parametr);

            _dbconnection.Open();
            command.ExecuteNonQuery();
            _dbconnection.Close();
        }
    }

    private T MapToEntity<T>(IDataReader reader) where T : class, new()
    {
        T entity = new T();
        Type entityType = typeof(T);
        PropertyInfo[] properties = entityType.GetProperties();

        DataTable schemaTable = reader.GetSchemaTable();

        foreach (PropertyInfo property in properties)
        {
            string columnName = property.Name;
            DataRow[] rows = schemaTable.Select($"ColumnName = '" + columnName + "'");
            if (rows.Length > 0)
            {
                try
                {
                    int ordinal = reader.GetOrdinal(columnName);
                    object value = reader.GetValue(ordinal);
                    if (value != DBNull.Value)
                    {
                        property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine($"Column '{columnName}' not found in result set.");
                }
            }
        }
        return entity;
    }

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var sqlQuery = BuildSqlQuery(predicate, singleResult: true);
        return ExecuteQuerySingle(sqlQuery);
    }

    public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
    {
        var sqlQuery = BuildSqlQuery(predicate, singleResult: false);
        return ExecuteQueryMultiple(sqlQuery);
    }

    private T ExecuteQuerySingle(string query)
    {
        using (var command = _dbconnection.CreateCommand())
        {
            command.CommandText = query;
            _dbconnection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Map(reader);
                }
            }
            _dbconnection.Close();
        }
        return null;
    }

    private IEnumerable<T> ExecuteQueryMultiple(string query)
    {
        var results = new List<T>();
        using (var command = _dbconnection.CreateCommand())
        {
            command.CommandText = query;
            _dbconnection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(Map(reader));
                }
            }
            _dbconnection.Close();
        }
        return results;
    }

    private Tuple<string, Dictionary<string, object>> BuildQuery<T>(Expression<Func<T, bool>> predicate)
    {
        var tableName = typeof(T).Name + "s";
        var parameters = new Dictionary<string, object>();
        var whereClause = BuildWhereClause(predicate.Body, parameters);

        var query = $"SELECT TOP 1 * FROM {tableName} {whereClause}";
        return new Tuple<string, Dictionary<string, object>>(query, parameters);
    }

    private string BuildWhereClause(Expression expression, Dictionary<string, object> parameters)
    {
        if (expression is BinaryExpression binaryExpression)
        {
            var left = BuildWhereClause(binaryExpression.Left, parameters);
            var right = BuildWhereClause(binaryExpression.Right, parameters);
            var operatorString = binaryExpression.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Operator {binaryExpression.NodeType} is not supported")
            };
            return $"{left} {operatorString} {right}";
        }
        else if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression is ConstantExpression constantExpression)
        {
            var parameterName = $"@p{parameters.Count}";
            parameters.Add(parameterName, constantExpression.Value);
            return parameterName;
        }
        else
        {
            throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported");
        }
    }

    private string ParseExpression(Expression expression)
    {
        if (expression is BinaryExpression binary)
        {
            // разбираем выражение на составляющие
            var left = ParseExpression(binary.Left);  // Левая часть выражения
            var right = ParseExpression(binary.Right); // Правая часть выражения
            var op = GetSqlOperator(binary.NodeType);  // Оператор (например, > или =)
            return $"({left} {op} {right})";
        }
        else if (expression is MemberExpression member)
        {
            return member.Member.Name; // Название свойства
        }
        else if (expression is ConstantExpression constant)
        {
            return FormatConstant(constant.Value); // Значение константы
        }
        throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
    }

    private string GetSqlOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported node type: {nodeType}")
        };
    }

    private string FormatConstant(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    private string BuildSqlQuery(Expression<Func<T, bool>> predicate, bool singleResult)
    {
        var tableName = typeof(T).Name + "s"; // Имя таблицы, основанное на имени класса
        var whereClause = ParseExpression(predicate.Body);
        var limitClause = singleResult ? "LIMIT 1" : string.Empty;

        return $"SELECT * FROM {tableName} WHERE {whereClause}".Trim();
    }
}