using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class Category
    {
        public int Id {get;set;}
        [Required()]
        public string Name { get;set; }
        [Required()]
        public string Description { get; set; }
        [Required()]
        public DateTime Created { get;set;}

        public List<Bag>? Bags{ get; set;}

    }
}
