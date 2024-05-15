using MVC.Models;
using MVC.NewFolder;

namespace MVC
{
    public class PersonService:IPersonService
    {
        public static List<Person> GetAllPersons()
        {
            return new List<Person>
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

        public static List<Person> data = GetAllPersons();
        
        public PaginatedPersonList GetAll(int? page,int? recordsPerPage)
        {
            if (page <= 0)
            {
                return new PaginatedPersonList
                {
                    CountTotal = 0,
                    Persons = new List<Person> { }
                };
            }
            return new PaginatedPersonList
            {
                CountTotal = data.Count,
                Persons = page == null && recordsPerPage == null ? data : data.Skip((page.Value-1) * recordsPerPage.Value).Take(recordsPerPage.Value)
            };
        }

        public void Create(Person person)
        {
            var findPerson = Find(person.Id);
            if (findPerson == null)
            {
                data.Add(person);
            }
        }

        public void Edit(Person person)
        {
            var findPerson = Find(person.Id);
            if (findPerson != null)
            {
                findPerson.FirstName = person.FirstName;
                findPerson.LastName = person.LastName;
                findPerson.BirthPlace = person.BirthPlace;
                findPerson.Gender = person.Gender;
                findPerson.DateOfBirth = person.DateOfBirth;
                findPerson.IsGraduated = person.IsGraduated;
                findPerson.PhoneNumber = person.PhoneNumber;
            }
        }

        public void Delete(Person person)
        {
            data.Remove(person);
        }

        public Person? Find(Guid id)
        {
            return data.FirstOrDefault(x=>x.Id == id);
        }

    }
}
