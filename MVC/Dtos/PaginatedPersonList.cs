using MVC.Models;

namespace MVC.NewFolder
{
    public class PaginatedPersonList
    {
        public IEnumerable<Person> Persons { get; set; }
        public int CountTotal { get; set; }
    }
}
