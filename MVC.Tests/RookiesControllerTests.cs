using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Areas.NashTech.Controllers;
using MVC.Models;
using MVC.NewFolder;

namespace MVC.Tests
{
    [TestFixture]
    public class RookiesControllerTests
    {
        private Mock<IPersonService> _mockPersonService;
        private RookiesController _controller;


        [SetUp]
        public void SetUp()
        {
            _mockPersonService = new Mock<IPersonService>();
            _controller = new RookiesController(_mockPersonService.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Test]
        public void Index_WhenPersonsExist_SetsTempDataAndReturnsView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe" }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = 2,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(1, 2)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Index(1, 2) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(persons, result.Model);
            Assert.AreEqual(1, _controller.TempData["countPage"]);
        }

        [Test]
        public void Index_WhenNoPersonsExist_SetsTempDataAndReturnsView()
        {
            // Arrange
            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = 0,
                Persons = new List<Person>()
            };

            _mockPersonService.Setup(service => service.GetAll(1, 2)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Index(1, 2) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, _controller.TempData.Count);
            Assert.IsEmpty(result.Model as IEnumerable<Person>);
        }

        [Test]
        public void Create_WhenModelStateIsValid_CreatesPersonAndRedirectsToIndex()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                BirthPlace = "New York",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };

            _controller.ModelState.Clear(); // Make sure the model state is valid

            // Act
            var result = _controller.Create(person) as RedirectToActionResult;

            // Assert
            _mockPersonService.Verify(service => service.Create(It.IsAny<Person>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void Create_WhenModelStateIsInvalid_DoesNotCreatePersonAndRedirectsToIndex()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                BirthPlace = "New York",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };

            _controller.ModelState.AddModelError("FirstName", "First name is required");

            // Act
            var result = _controller.Create(person) as RedirectToActionResult;

            // Assert
            _mockPersonService.Verify(service => service.Create(It.IsAny<Person>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void Edit_WhenModelStateIsValid_UpdatesPersonAndRedirectsToIndex()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                BirthPlace = "New York",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };

            _controller.ModelState.Clear(); // Make sure the model state is valid

            // Act
            var result = _controller.Edit(person) as RedirectToActionResult;

            // Assert
            _mockPersonService.Verify(service => service.Edit(It.IsAny<Person>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void Edit_WhenModelStateIsInvalid_DoesNotUpdatePersonAndRedirectsToIndex()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                BirthPlace = "New York",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };

            _controller.ModelState.AddModelError("FirstName", "First name is required");

            // Act
            var result = _controller.Edit(person) as RedirectToActionResult;

            // Assert
            _mockPersonService.Verify(service => service.Edit(It.IsAny<Person>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestCase("older", "Older")]
        [TestCase("abc", "Equal")]
        [TestCase("equal", "Equal")]
        [TestCase("younger", "Younger")]
        public void BirthYear_InputOption_ReturnCorespondingAction(string option, string expectedAction)
        {
            var result = _controller.BirthYear(option) as RedirectToActionResult;
            Assert.AreEqual(result.ActionName,expectedAction);
        }

        [Test]
        public void DeletePerson_WhenPersonExists_DeletesPersonAndRedirectsToConfirmDelete()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var person = new Person
            {
                Id = personId,
                FirstName = "John",
                LastName = "Doe",
                BirthPlace = "New York",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };
            

            _mockPersonService.Setup(service => service.Find(personId)).Returns(person);

            // Act
            var result = _controller.DeletePerson(personId) as RedirectToActionResult;

            // Assert
            _mockPersonService.Verify(service => service.Delete(It.IsAny<Person>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual("ConfirmDelete", result.ActionName);
            Assert.AreEqual(person.FullName, _controller.TempData["personName"]);
        }

        [Test]
        public void DeletePerson_WhenPersonDoesNotExist_DoesNotDeletePersonAndRedirectsToIndex()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _mockPersonService.Setup(service => service.Find(personId)).Returns((Person)null);

            // Act
            var result = _controller.DeletePerson(personId) as RedirectToActionResult;
            // Assert
            _mockPersonService.Verify(service => service.Delete(It.IsAny<Person>()), Times.Never);
            Assert.AreEqual("Index", result.ActionName);
        }
    }
}
