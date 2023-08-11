using Assignment.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class ShopController : Controller
    {

        private  ApplicationDbContext _context;


        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            var categories = await _context.Categories
                .OrderBy(category => category.Name)
                .ToListAsync();


            return View(categories);
        }

        public async Task<IActionResult> Details(int? id)
        { 
            var categoryWithBags = await _context.Categories
                .Include(category => category.Bags)
                .FirstOrDefaultAsync(category => category.Id == id);


            return View(categoryWithBags);
        }
    }
}
