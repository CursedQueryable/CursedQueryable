namespace CursedQueryable.IntegrationTests.Data.Entities;

public class Kitten
{
    public int Id { get; set; }
    public Sex? Sex { get; set; }
    public string Name { get; set; } = default!;
    public Cat Parent { get; set; } = default!;
    public int ParentId { get; set; }
}