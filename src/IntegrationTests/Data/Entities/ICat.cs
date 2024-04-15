namespace CursedQueryable.IntegrationTests.Data.Entities;

public interface ICat
{
    Sex? Sex { get; set; }
    string Name { get; set; }
    string Eman { get; set; }
    string Random { get; set; }
    bool HasExpensiveTastes { get; set; }
    DateTime? DateOfBirth { get; set; }
}