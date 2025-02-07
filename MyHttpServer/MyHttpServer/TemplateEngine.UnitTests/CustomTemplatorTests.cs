using TemplateEngine.Core.Templaytor;
using TemplateEngine.UnitTests.Models;

namespace TemplateEngine.UnitTests.Core.Templaytor
{
    [TestClass]
    public class CustomTemplatorTests
    {
        [TestMethod]
        public void GetHtmlByTemplate_When_NameIsnotNull_ResultSuccess()
        {
            ICustomTemplator customTemplator = new CustomTemplator();
            string template = "<label>name</label><p>{name}</p>";
            string name = "Имя";

            var result = customTemplator.GetHtmlByTemplate(template, name);

            Assert.AreEqual("<label>name</label><p>Имя</p>", result);
        }

        [TestMethod]
        public void GetHtmlByTemplate_When_UserIsnotNull_ResultSuccess()
        {
            ICustomTemplator customTemplator = new CustomTemplator();
            string template = "<p>Ваш логин: {login}; Ваш пароль: {password};</p>";
            var user = new Person
            {
                Login = "test@test.ru",
                Password = "passWord",
                Name = "Имя"
            };

            var result = customTemplator.GetHtmlByTemplate(template, user);

            Assert.AreEqual("<p>Ваш логин: test@test.ru; Ваш пароль: passWord;</p>", result);
        }

        [TestMethod]
        public void GetHtmlByTemplate_When_ObjectIsnotNull_ResultSuccess()
        {
            var customTemplator = new CustomTemplator();
            string template = "Привет {name}! <p>Ваш логин: {login}; Ваш пароль: {password}; Мы очень рады с вами познакомится дорогой {name}!</p>";
            Person user = new Person() { Name = "Иван", Login = "test@test.ru", Password = "passWord" };
            var result = customTemplator.GetHtmlByTemplate(template, user);
            Assert.AreEqual("Привет Иван! <p>Ваш логин: test@test.ru; Ваш пароль: passWord; Мы очень рады с вами познакомится дорогой Иван!</p>", result);
        }
    }
}
