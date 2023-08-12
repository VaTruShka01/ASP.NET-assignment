using Assignment.Data;
using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment.Controllers
{
    public class ShopController : Controller
    {

        private ApplicationDbContext _context;


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

        public async Task<IActionResult> BagDetails(int? id)
        {

            var bag = await _context.Bags
                .FirstOrDefaultAsync(bag => bag.Id == id);


            return View(bag);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int bagId, int quantity)
        {
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            
            var cart = await _context.Carts
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            
            if (cart == null)
            {
                cart = new Models.Cart { UserId = userId };
                await _context.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            
            var bag = await _context.Bags
                .FirstOrDefaultAsync(bag => bag.Id == bagId);


            if (bag == null)
            {
                return NotFound();
            }

           
            var cartItem = new CartItem
            {
                Cart = cart,
                Bag = bag,
                Quantity = quantity,
                Price = (decimal)bag.Price,
            };

            
            if (ModelState.IsValid)
            {
                await _context.AddAsync(cartItem);
                await _context.SaveChangesAsync();

                return RedirectToAction("ViewMyCart");
            }

            
            return NotFound();
        }

        [Authorize]
        public async Task<IActionResult> ViewMyCart()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = await _context.Carts
                .Include(cart => cart.User)
                .Include (cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.Bag)
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCartItem (int cartId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = await _context.Carts
                
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            if (cart == null) { 
            return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(cartItem => cartItem.Bag)
                .FirstOrDefaultAsync(cartItem => cartItem.Id == cartId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return RedirectToAction("ViewMyCart");

            }

            return RedirectToAction("ViewMyCart");
            
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = await _context.Carts
                .Include(cart => cart.User)
                .Include(cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.Bag)
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            var order = new Order
            {
                UserId = userId,
                Cart = cart,
                Total = ((decimal)(cart.CartItems.Sum(cartItem => (cartItem.Price * cartItem.Quantity)))),
                ShippingAddress = "",
                PaymentMethods = PaymentMethods.VISA,
            };

            ViewData["PaymentMethods"] = new SelectList(Enum.GetValues(typeof(PaymentMethods)));

            return View(order);
        }

    }

}