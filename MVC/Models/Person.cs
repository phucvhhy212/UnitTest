using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string FullName => LastName + " " + FirstName;
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }

        public string BirthPlace { get; set; }
        public bool IsGraduated { get; set; }
    }
}
