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

        [Test]
        public void OldestMember_WhenPersonsExist_ReturnsOldestMemberInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1950, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1940, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jim", LastName = "Smith", DateOfBirth = new DateTime(1960, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.OldestMember() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count());
            Assert.AreEqual("Jane", model.First().FirstName);
            Assert.AreEqual("Doe", model.First().LastName);
        }

        [Test]
        public void OldestMember_WhenNoPersonsExist_ReturnsEmptyListInView()
        {
            // Arrange
            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = 0,
                Persons = new List<Person>()
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.OldestMember() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model);
        }

        [Test]
        public void Older_WhenPersonsExist_ReturnsPersonsBornAfter2000()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2001, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1999, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jim", LastName = "Smith", DateOfBirth = new DateTime(2002, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Older() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count());
            Assert.IsTrue(model.Any(p => p.FirstName == "John" && p.LastName == "Doe"));
            Assert.IsTrue(model.Any(p => p.FirstName == "Jim" && p.LastName == "Smith"));
        }

        [Test]
        public void Older_WhenNoPersonsBornAfter2000_ReturnsEmptyListInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1999, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null, null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Older() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model);
        }

        [Test]
        public void Younger_WhenPersonsExist_ReturnsPersonsBornBefore2000()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(2001, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jim", LastName = "Smith", DateOfBirth = new DateTime(1999, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Younger() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count());
            Assert.IsTrue(model.Any(p => p.FirstName == "John" && p.LastName == "Doe"));
            Assert.IsTrue(model.Any(p => p.FirstName == "Jim" && p.LastName == "Smith"));
        }

        [Test]
        public void Younger_WhenNoPersonsBornBefore2000_ReturnsEmptyListInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2001, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(2002, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null, null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Younger() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model);
        }

        [Test]
        public void Edit_WhenPersonExists_ReturnsPersonInView()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var persons = new List<Person>
            {
                new Person { Id = personId, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(2001, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null, null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Edit(personId) as ViewResult;
            var model = result.Model as Person;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(personId, model.Id);
            Assert.AreEqual("John", model.FirstName);
            Assert.AreEqual("Doe", model.LastName);
        }

        [Test]
        public void Edit_WhenPersonDoesNotExist_ReturnsNullInView()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(2001, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Edit(personId) as ViewResult;
            var model = result.Model as Person;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(model);
        }

        [Test]
        public void Equal_WhenPersonsExist_ReturnsPersonsBornIn2000()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2000, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1999, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jim", LastName = "Smith", DateOfBirth = new DateTime(2000, 6, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null, null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Equal() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count());
            Assert.IsTrue(model.Any(p => p.FirstName == "John" && p.LastName == "Doe"));
            Assert.IsTrue(model.Any(p => p.FirstName == "Jim" && p.LastName == "Smith"));
        }

        [Test]
        public void Equal_WhenNoPersonsBornIn2000_ReturnsEmptyListInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1999, 1, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null, null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.Equal() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model);
        }

        [Test]
        public void Create_WhenCalled_ReturnView()
        {
            // Act
            var result = _controller.Create() as ViewResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Model);
        }

        [Test]
        public void ConfirmDelete_WhenCalled_ReturnView()
        {
            // Act
            var result = _controller.ConfirmDelete() as ViewResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Model);
        }

        [Test]
        public void MaleMembers_WhenMalePersonsExist_ReturnsMalePersonsInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Gender = "Female", DateOfBirth = new DateTime(1999, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Jim", LastName = "Smith", Gender = "Male", DateOfBirth = new DateTime(2000, 6, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.MaleMembers() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count());
            Assert.IsTrue(model.Any(p => p.FirstName == "John" && p.LastName == "Doe"));
            Assert.IsTrue(model.Any(p => p.FirstName == "Jim" && p.LastName == "Smith"));
        }

        [Test]
        public void MaleMembers_WhenNoMalePersonsExist_ReturnsEmptyListInView()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Gender = "Female", DateOfBirth = new DateTime(1995, 1, 1) },
                new Person { Id = Guid.NewGuid(), FirstName = "Janet", LastName = "Smith", Gender = "Female", DateOfBirth = new DateTime(2000, 6, 1) }
            };

            var paginatedPersonList = new PaginatedPersonList
            {
                CountTotal = persons.Count,
                Persons = persons
            };

            _mockPersonService.Setup(service => service.GetAll(null,null)).Returns(paginatedPersonList);

            // Act
            var result = _controller.MaleMembers() as ViewResult;
            var model = result.Model as IEnumerable<Person>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model);
        }
    }
}

