namespace CursedQueryable.IntegrationTests.Data.Entities;

public class Cat : ICat
{
    public int Id { get; set; }
    public ICollection<Kitten> Kittens { get; set; } = new HashSet<Kitten>();
    public Sex? Sex { get; set; }
    public string Name { get; set; } = default!;
    public string Eman { get; set; } = default!;
    public string Random { get; set; } = default!;
    public DateTime? DateOfBirth { get; set; }
    public bool HasExpensiveTastes { get; set; }
}