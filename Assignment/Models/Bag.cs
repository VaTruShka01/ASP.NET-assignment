using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class Bag
    {

        public int Id { get; set; }
        [Display(Name = "Category")]
        [Required()]
        public int CategoryId { get; set; }
        [Required()]
        public string Name { get; set; }
        [Required()]
        public string Description { get; set; }
        [Required()]
        public double Capacity { get; set; }
        [Required()]
        public string Color { get; set; }
        [Required()]
        public DateTime Published { get; set; }

        public Category ? Category { get; set; }



    }
}
