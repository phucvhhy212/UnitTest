using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Person = MVC.Models.Person;

namespace MVC.Areas.NashTech.Controllers
{
    [Area("NashTech")]
    public class RookiesController : Controller
    {
        private readonly IPersonService _personService;
        public RookiesController(IPersonService personService)
        {
            _personService = personService;
        }
        public IActionResult Index(int page = 1,int recordPerPage = 2)
        {
            var response = _personService.GetAll(page, recordPerPage);
            var countPage = Math.Ceiling((double)response.CountTotal / recordPerPage);
            if (response.Persons.Count() != 0)
            {
                TempData["countPage"] = (int)countPage;
            }
            return View(response.Persons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Person p)
        {
            if (ModelState.IsValid)
            {
                p.Id = Guid.NewGuid();
                _personService.Create(p);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(Guid id)
        {
            var response = _personService.GetAll();
            return View(response.Persons.FirstOrDefault(p => p.Id == id));
        }

        [HttpPost]
        public IActionResult Edit(Person p)
        {
            if (ModelState.IsValid)
            {
                _personService.Edit(p);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Details(Guid id)
        {
            return View(_personService.Find(id));
        }

        public IActionResult OldestMember()
        {
            var response = _personService.GetAll();
            return View("Index",response.Persons.Where(m => DateTime.Now.Year - m.DateOfBirth.Year == response.Persons.Max(x => DateTime.Now.Year - x.DateOfBirth.Year)));
        }

        public IActionResult MaleMembers()
        {
            var response = _personService.GetAll();
            return View("Index",response.Persons.Where(x => x.Gender == "Male"));
        }

        public IActionResult FullName()
        {
            var response = _personService.GetAll();
            return View(response.Persons.Select(x => x.FullName));
        }

        public IActionResult BirthYear(string option)
        {
            switch (option)
            {
                case "older":
                    return RedirectToAction("Older");
                case "younger":
                    return RedirectToAction("Younger");
                default:
                    return RedirectToAction("Equal");
            }
        }

        public IActionResult Delete(Guid id)
        {
            var response = _personService.GetAll();
            return View(response.Persons.FirstOrDefault(p => p.Id == id));
        }

        [HttpPost]
        public IActionResult DeletePerson(Guid id)
        {
            var personToRemove = _personService.Find(id);
            if (personToRemove != null)
            {
                _personService.Delete(personToRemove);
                TempData["personName"] = personToRemove.FullName;
                return RedirectToAction("ConfirmDelete");
            }
            return RedirectToAction("Index");
        }

        public IActionResult ConfirmDelete()
        {
            return View();
        }
        public IActionResult Older()
        {
            var response = _personService.GetAll();
            return View("Index",response.Persons.Where(x => x.DateOfBirth.Year > 2000));
        }

        public IActionResult Younger()
        {
            var response = _personService.GetAll();
            return View("Index",response.Persons.Where(x => x.DateOfBirth.Year < 2000));
        }

        public IActionResult Equal()
        {
            var response = _personService.GetAll();
            return View("Index",response.Persons.Where(x => x.DateOfBirth.Year == 2000));
        }

        public IActionResult Export()
        {
            var response = _personService.GetAll();
            var stream = ExportData(response.Persons);
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PersonRecords.xlsx");
        }

        public MemoryStream ExportData(IEnumerable<Person> persons)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells.LoadFromCollection(persons, true);

                // AutoFit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream(excelPackage.GetAsByteArray());
                return stream;
            }
        }
    }
}
