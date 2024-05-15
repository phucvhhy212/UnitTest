using MVC.Models;

namespace MVC
{
    public class PersonServiceTests
    {
        private IPersonService personService;
        [SetUp]
        public void Setup()
        {
            personService = new PersonService();
            PersonService.data = new List<Person>
            {
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Huy1",
                    LastName = "Phuc1",
                    BirthPlace = "Ha Noi",
                    DateOfBirth = DateTime.Today,
                    Gender = "Female",
                    IsGraduated = false,
                    PhoneNumber = "7329074222"
                }
            };
        }

        [Test]
        public void GetAll_WithValidPageAndRecordsPerPage_ReturnsPaginatedResult()
        {
            var result = personService.GetAll(1, 1);

            Assert.AreEqual(1, result.Persons.Count());
            Assert.AreEqual(1, result.CountTotal);
        }

        [Test]
        public void GetAll_WithNullPageAndRecordsPerPage_ReturnsAllRecords()
        {
            var result = personService.GetAll(null, null);

            Assert.AreEqual(PersonService.data.Count, result.Persons.Count());
            Assert.AreEqual(PersonService.data.Count, result.CountTotal);
        }

        [Test]
        public void GetAll_WithZeroOrNegativePage_ReturnsEmptyList()
        {
            var result = personService.GetAll(0, 1);
            Assert.AreEqual(0, result.Persons.Count());
        }

        [Test]
        public void GetAll_WithZeroOrNegativeRecordsPerPage_ReturnsEmptyList()
        {
            var result = personService.GetAll(1, 0);

            Assert.AreEqual(0, result.Persons.Count());
            Assert.AreEqual(PersonService.data.Count, result.CountTotal);
        }

        [Test]
        public void GetAll_WithPageGreaterThanTotalPages_ReturnsEmptyList()
        {
            var result = personService.GetAll(10, 1);

            Assert.AreEqual(0, result.Persons.Count());
            Assert.AreEqual(PersonService.data.Count, result.CountTotal);
        }

        [Test]
        public void Create_WhenPersonDoesNotExist_AddsPersonToList()
        {
            var newPerson = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Huy2",
                LastName = "Phuc2",
                BirthPlace = "Ho Chi Minh",
                DateOfBirth = DateTime.Today.AddYears(-25),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "1234567890"
            };

            personService.Create(newPerson);

            Assert.Contains(newPerson, PersonService.data);
        }

        [Test]
        public void Create_WhenPersonExists_DoesNotAddPersonToList()
        {
            var existingPerson = PersonService.data[0];

            personService.Create(existingPerson);

            var count = PersonService.data.FindAll(p => p.Id == existingPerson.Id).Count;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Edit_WhenPersonExists_UpdatesPersonDetails()
        {
            var existingPerson = PersonService.data[0];
            var updatedPerson = new Person
            {
                Id = existingPerson.Id,
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                BirthPlace = "UpdatedBirthPlace",
                DateOfBirth = existingPerson.DateOfBirth.AddYears(-1),
                Gender = "Male",
                IsGraduated = true,
                PhoneNumber = "9876543210"
            };

            personService.Edit(updatedPerson);

            Assert.AreEqual(updatedPerson.FirstName, existingPerson.FirstName);
            Assert.AreEqual(updatedPerson.LastName, existingPerson.LastName);
            Assert.AreEqual(updatedPerson.BirthPlace, existingPerson.BirthPlace);
            Assert.AreEqual(updatedPerson.Gender, existingPerson.Gender);
            Assert.AreEqual(updatedPerson.DateOfBirth, existingPerson.DateOfBirth);
            Assert.AreEqual(updatedPerson.IsGraduated, existingPerson.IsGraduated);
            Assert.AreEqual(updatedPerson.PhoneNumber, existingPerson.PhoneNumber);
        }

        [Test]
        public void Edit_WhenPersonDoesNotExist_DoesNotUpdateAnyPerson()
        {
            var nonExistentPerson = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "NonExistent",
                LastName = "Person",
                BirthPlace = "Nowhere",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Female",
                IsGraduated = false,
                PhoneNumber = "0000000000"
            };
            personService.Edit(nonExistentPerson);

            var foundPerson = PersonService.data.FirstOrDefault(p => p.Id == nonExistentPerson.Id);
            Assert.IsNull(foundPerson);
        }

        [Test]
        public void Delete_WhenPersonExists_RemovesPersonFromList()
        {
            var personToRemove = PersonService.data[0];
            personService.Delete(personToRemove);

            var foundPerson = PersonService.data.FirstOrDefault(p => p.Id == personToRemove.Id);
            Assert.IsNull(foundPerson);
        }

        [Test]
        public void Delete_WhenPersonDoesNotExist_DoesNotThrowException()
        {
            var nonExistentPerson = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "NonExistent",
                LastName = "Person",
                BirthPlace = "Nowhere",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Female",
                IsGraduated = false,
                PhoneNumber = "0000000000"
            };

            Assert.DoesNotThrow(() => personService.Delete(nonExistentPerson));
        }

        [Test]
        public void Find_WhenPersonExists_ReturnsPerson()
        {
            var existingPerson = PersonService.data[0];

            var result = personService.Find(existingPerson.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(existingPerson.Id, result.Id);
        }

        [Test]
        public void Find_WhenPersonDoesNotExist_ReturnsNull()
        {
            var nonExistentPersonId = Guid.NewGuid();

            var result = personService.Find(nonExistentPersonId);

            Assert.IsNull(result);
        }
    }
}
