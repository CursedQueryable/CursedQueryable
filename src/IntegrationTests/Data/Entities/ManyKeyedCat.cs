namespace CursedQueryable.IntegrationTests.Data.Entities;

public class ManyKeyedCat : ICat
{
    public int Id1 { get; set; }
    public Guid Id2 { get; set; }
    public string Id3 { get; set; } = default!;
    public Sex? Sex { get; set; }
    public string Name { get; set; } = default!;
    public string Eman { get; set; } = default!;
    public string Random { get; set; } = default!;
    public bool HasExpensiveTastes { get; set; }
    public DateTime? DateOfBirth { get; set; }
}