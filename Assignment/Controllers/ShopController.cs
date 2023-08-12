using Assignment.Data;
using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Security.Claims;

namespace Assignment.Controllers
{
    public class ShopController : Controller
    {

        private ApplicationDbContext _context;
        private IConfiguration _configuration;


        public ShopController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Payment(string shippingAddress, PaymentMethod paymentMethod)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = await _context.Carts
                .Include(cart => cart.CartItems)
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            if (cart == null) return NotFound();

            // Add Order data to the session
            HttpContext.Session.SetString("ShippingAddress", shippingAddress);
            HttpContext.Session.SetString("PaymentMethod", paymentMethod.ToString());

            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.CartItems.Sum(cartItem => cartItem.Quantity * cartItem.Price) * 100),
                        Currency = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "PacketOff Purchase",
                        },
                    },
                    Quantity = 1,
                  },
                },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/ViewMyCart",
            };
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public async Task<IActionResult> SaveOrder()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = await _context.Carts
                .Include(cart => cart.User)
                .Include(cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.Bag)
                .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Active == true);

            var paymentMethod = HttpContext.Session.GetString("PaymentMethod");

            var order = new Order
            {
                UserId = userId,
                Cart = cart,
                Total = ((decimal)(cart.CartItems.Sum(cartItem => (cartItem.Price * cartItem.Quantity)))),
                ShippingAddress = HttpContext.Session.GetString("ShippingAddress"),
                PaymentMethods = PaymentMethods.VISA,
                PaymentReceived = true,
            };

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            cart.Active = false;
            _context.Update(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderDetails", new { id = order.Id });
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var order = await _context.Orders
                .Include(order => order.User)
                .Include(order => order.Cart)
                .ThenInclude(cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.Bag)
                .FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var orders = await _context.Orders
                .OrderBy(order => order.Id)
                .Where(order => order.UserId == userId)
                .ToListAsync();

            return View(orders);
        }

    }

}