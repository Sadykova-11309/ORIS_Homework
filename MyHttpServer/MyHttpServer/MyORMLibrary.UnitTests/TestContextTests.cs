using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;

namespace MyORMLibrary.UnitTests
{
    [TestClass]
    public class TestsContextTest
    {
        [TestMethod]
        public void GetByIdTest()
        {
            // Arrange
            var dbConnection = new Mock<IDbConnection>();
            var dbCommand = new Mock<IDbCommand>();
            var dbDataReader = new Mock<IDataReader>();
            var person = new Person()
            {
                Id = 1,
                Name = "Chel",
                Email = "test@test.ru"
            };
            var context = new Test1Context<Person>(dbConnection.Object);

            // Создаем список для отслеживания добавленных параметров
            var parameterList = new List<IDbDataParameter>();

            // Настройка создания параметров с поддержкой записи/чтения свойств
            dbCommand.Setup(c => c.CreateParameter())
                .Returns(() =>
                {
                    var paramMock = new Mock<IDbDataParameter>();
                    paramMock.SetupProperty(p => p.ParameterName); // Поддержка свойства ParameterName
                    paramMock.SetupProperty(p => p.Value);         // Поддержка свойства Value
                    return paramMock.Object;
                });

            // Отслеживаем добавление параметров
            dbCommand.Setup(c => c.Parameters.Add(It.IsAny<object>())) // Принимаем object вместо IDbDataParameter
                .Callback<object>(param =>
                {
                    if (param is IDbDataParameter dataParam) // Проверяем, является ли параметр IDbDataParameter
                    {
                        parameterList.Add(dataParam);
                    }
                });

            dbCommand.Setup(c => c.ExecuteReader()).Returns(dbDataReader.Object);
            dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

            // Настройка DataReader
            dbDataReader.SetupSequence(r => r.Read())
                .Returns(true)  // Первый вызов возвращает true, чтобы симулировать наличие данных
                .Returns(false); // Второй вызов возвращает false, чтобы симулировать конец данных

            dbDataReader.Setup(r => r["Id"]).Returns(person.Id);
            dbDataReader.Setup(r => r["Name"]).Returns(person.Name);
            dbDataReader.Setup(r => r["Email"]).Returns(person.Email);

            // Act
            var result = context.GetById(person.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(person.Id, result.Id);
            Assert.AreEqual(person.Name, result.Name);
            Assert.AreEqual(person.Email, result.Email);

            // Проверяем, что параметр был добавлен с правильным значением
            Assert.AreEqual(1, parameterList.Count, "Неверное количество параметров.");
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Id" && p.Value.Equals(person.Id)), "Параметр Id не найден или значение неверное.");
        }

        [TestMethod]
        public void GetAllTest()
        {
            // Arrange
            var dbConnection = new Mock<IDbConnection>();
            var dbCommand = new Mock<IDbCommand>();
            var dbDataReader = new Mock<IDataReader>();

            var persons = new List<Person>
            {
                new Person { Id = 1, Name = "Chel", Email = "test@test.ru" },
                new Person { Id = 2, Name = "John", Email = "john@test.ru" }
            };

            var context = new Test1Context<Person>(dbConnection.Object);

            dbCommand.Setup(c => c.ExecuteReader()).Returns(dbDataReader.Object);
            dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

            dbDataReader.Setup(c => c.FieldCount).Returns(3); // Количество столбцов в таблице

            var readSequence = dbDataReader.SetupSequence(c => c.Read());
            foreach (var person in persons)
            {
                readSequence = readSequence.Returns(true);
            }
            readSequence.Returns(false); // После двух успешных вызовов Read(), возвращаем false

            // Настроим возврат значений **после** `SetupSequence`
            dbDataReader.Setup(r => r["Id"]).Returns(() => persons[0].Id);
            dbDataReader.Setup(r => r["Name"]).Returns(() => persons[0].Name);
            dbDataReader.Setup(r => r["Email"]).Returns(() => persons[0].Email);

            int index = -1;
            dbDataReader.Setup(c => c.Read()).Returns(() =>
            {
                index++;
                return index < persons.Count;
            });

            dbDataReader.Setup(r => r["Id"]).Returns(() => persons[index].Id);
            dbDataReader.Setup(r => r["Name"]).Returns(() => persons[index].Name);
            dbDataReader.Setup(r => r["Email"]).Returns(() => persons[index].Email);

            // Act
            var result = context.GetAll().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(persons.Count, result.Count);
            for (int i = 0; i < persons.Count; i++)
            {
                Assert.AreEqual(persons[i].Id, result[i].Id);
                Assert.AreEqual(persons[i].Name, result[i].Name);
                Assert.AreEqual(persons[i].Email, result[i].Email);
            }
        }

        [TestMethod]
        public void CreateTest()
        {
            // Arrange
            var dbConnection = new Mock<IDbConnection>();
            var dbCommand = new Mock<IDbCommand>();
            var dbParameterCollection = new Mock<IDataParameterCollection>();
            var person = new Person
            {
                Id = 1,
                Name = "Chel",
                Email = "test@test.ru"
            };
            var context = new Test1Context<Person>(dbConnection.Object);

            // Создаём список параметров для проверки
            var parameterList = new List<IDbDataParameter>();

            dbCommand.Setup(c => c.ExecuteNonQuery()).Returns(1); // Симулируем успешное выполнение команды

            // Настройка создания параметров с поддержкой записи/чтения свойств
            dbCommand.Setup(c => c.CreateParameter())
                .Returns(() =>
                {
                    var paramMock = new Mock<IDbDataParameter>();
                    paramMock.SetupProperty(p => p.ParameterName); // Поддержка свойства ParameterName
                    paramMock.SetupProperty(p => p.Value);         // Поддержка свойства Value
                    return paramMock.Object;
                });

            // Настроим поведение параметров
            dbParameterCollection.Setup(c => c.Add(It.IsAny<object>()))
                .Callback<object>(param =>
                {
                    if (param is IDbDataParameter dbParam)
                    {
                        parameterList.Add(dbParam);
                    }
                })
                .Returns(0);

            dbParameterCollection.Setup(c => c.Count).Returns(() => parameterList.Count);
            dbCommand.SetupGet(c => c.Parameters).Returns(dbParameterCollection.Object);

            dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

            // Act
            var result = context.Create(person);

            // Assert
            Assert.IsTrue(result);

            // Проверка, что параметры были добавлены с правильными значениями
            Assert.AreEqual(3, parameterList.Count);

            // Проверяем параметры, добавленные в коллекцию
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Id" && p.Value.Equals(person.Id)), "Параметр Id не найден или значение неверное.");
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Name" && p.Value.Equals(person.Name)), "Параметр Name не найден или значение неверное.");
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Email" && p.Value.Equals(person.Email)), "Параметр Email не найден или значение неверное.");
        }

        [TestMethod]
        public void UpdateTest()
        {
            // Arrange
            var dbConnection = new Mock<IDbConnection>();
            var dbCommand = new Mock<IDbCommand>();
            var person = new Person
            {
                Id = 1,
                Name = "Chel",
                Email = "test@test.ru"
            };
            var context = new Test1Context<Person>(dbConnection.Object);

            // Создаем список для отслеживания добавленных параметров
            var parameterList = new List<IDbDataParameter>();

            dbCommand.Setup(c => c.ExecuteNonQuery()).Returns(1); // Симулируем успешное выполнение команды

            // Настройка создания параметров с поддержкой записи/чтения свойств
            dbCommand.Setup(c => c.CreateParameter())
                .Returns(() =>
                {
                    var paramMock = new Mock<IDbDataParameter>();
                    paramMock.SetupProperty(p => p.ParameterName); // Поддержка свойства ParameterName
                    paramMock.SetupProperty(p => p.Value);         // Поддержка свойства Value
                    return paramMock.Object;
                });

            // Отслеживаем добавление параметров
            dbCommand.Setup(c => c.Parameters.Add(It.IsAny<object>())) // Принимаем object вместо IDbDataParameter
                .Callback<object>(param =>
                {
                    // Проверяем, является ли параметр IDbDataParameter
                    var dataParam = param as IDbDataParameter;
                    if (dataParam != null)
                    {
                        parameterList.Add(dataParam);
                    }
                });

            dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

            // Act
            var result = context.Update(person);

            // Assert
            Assert.IsTrue(result); // Проверяем, что метод вернул true (успешное обновление)

            // Проверяем, что параметры были добавлены с правильными значениями
            Assert.AreEqual(3, parameterList.Count, "Неверное количество параметров.");

            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Id" && p.Value.Equals(person.Id)), "Параметр Id не найден или значение неверное.");
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Name" && p.Value.Equals(person.Name)), "Параметр Name не найден или значение неверное.");
            Assert.IsTrue(parameterList.Any(p => p.ParameterName == "@Email" && p.Value.Equals(person.Email)), "Параметр Email не найден или значение неверное.");
        }

        [TestMethod]
        public void DeleteTest()
        {
            // Arrange
            var dbConnection = new Mock<IDbConnection>();
            var dbCommand = new Mock<IDbCommand>();
            var dbParameter = new Mock<IDbDataParameter>();
            var personId = 1;
            var context = new Test1Context<Person>(dbConnection.Object);

            // Симулируем успешное выполнение команды
            dbCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Настроим создание параметра и добавление в коллекцию
            dbCommand.Setup(c => c.CreateParameter()).Returns(dbParameter.Object);

            // Настройка мок-объекта для поддержки записи и чтения свойств ParameterName и Value
            dbParameter.SetupProperty(p => p.ParameterName); // Поддержка свойства ParameterName
            dbParameter.SetupProperty(p => p.Value);        // Поддержка свойства Value

            var parameterList = new List<IDbDataParameter>(); // Список для отслеживания параметров

            // Используем It.IsAny<object> для точной проверки типа
            dbCommand.Setup(c => c.Parameters.Add(It.IsAny<object>()))
                .Callback<object>(param =>
                {
                    if (param is IDbDataParameter dataParam)
                    {
                        parameterList.Add(dataParam);
                    }
                });

            dbConnection.Setup(c => c.CreateCommand()).Returns(dbCommand.Object);

            // Act
            var result = context.Delete(personId);

            // Assert
            Assert.IsTrue(result); // Проверяем, что метод вернул true (успешное удаление)

            // Проверяем, что параметр был добавлен с правильным значением
            Assert.AreEqual(1, parameterList.Count, "Параметр не был добавлен или количество параметров неверное.");

            var addedParameter = parameterList.FirstOrDefault();
            Assert.IsNotNull(addedParameter, "Параметр не был добавлен.");
            Assert.AreEqual("@Id", addedParameter.ParameterName, "Неверное имя параметра.");
            Assert.AreEqual(personId, addedParameter.Value, "Неверное значение параметра.");
        }
    }
}
