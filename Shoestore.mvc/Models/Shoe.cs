namespace Shoestore.mvc.Models;

public class Shoe {
    public int ID { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public DateTime CreatedDate { get; set; }
}