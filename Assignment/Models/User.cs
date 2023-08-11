using Microsoft.AspNetCore.Identity;

namespace Assignment.Models


{
    public class User : IdentityUser
    {

        public List<Cart>? Cart { get; set; }

        public List<Order>? Order { get; set;}




    }
}
