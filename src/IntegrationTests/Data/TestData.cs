using CursedQueryable.IntegrationTests.Data.Entities;

namespace CursedQueryable.IntegrationTests.Data;

public static class TestData
{
    public static IEnumerable<Cat> GenerateCats(int max = 1000)
    {
        var rand = new Random(0);
        var kId = 0;

        return Enumerable.Range(1, max).Select(i =>
        {
            var kittens = GetKittens(rand, kId, i);
            kId += kittens.Count;

            return new Cat
            {
                Id = i,
                Name = $"Moggy {i:00000}",
                Eman = $"Yggom {max - i:00000}",
                Random = $"Random {rand.Next(max):00000}",
                Sex = rand.Next(10) != 0 ? (Sex)rand.Next(2) : null,
                DateOfBirth = rand.Next(10) != 0
                    ? DateTime.SpecifyKind(new DateTime(rand.Next(2010, 2021), rand.Next(1, 13), rand.Next(1, 29)),
                        DateTimeKind.Utc)
                    : null,
                HasExpensiveTastes = rand.Next(5) == 0,
                Kittens = kittens
            };
        });
    }

    public static IEnumerable<ManyKeyedCat> GenerateManyKeyedCats(int max = 1000)
    {
        var rand = new Random(0);

        return Enumerable.Range(1, max).Select(i => new ManyKeyedCat
        {
            Id1 = i < max / 2 ? 1 : 2,
            Id2 = Guid.Parse($"00000000-0000-0000-0000-{(max - i) / 100 + 1:000000000000}"),
            Id3 = $"Id3_{i:00000}",
            Name = $"Moggy {i:00000}",
            Eman = $"Yggom {max - i:00000}",
            Random = $"Random {rand.Next(max):00000}",
            Sex = rand.Next(10) != 0 ? (Sex)rand.Next(2) : null,
            DateOfBirth = rand.Next(10) != 0
                ? DateTime.SpecifyKind(new DateTime(rand.Next(2010, 2021), rand.Next(1, 13), rand.Next(1, 29)),
                    DateTimeKind.Utc)
                : null,
            HasExpensiveTastes = rand.Next(5) == 0
        });
    }

    private static List<Kitten> GetKittens(Random rand, int kId, int cId)
    {
        var kittens = new List<Kitten>();

        if (rand.Next(5) == 0)
        {
            kittens.Add(new Kitten
            {
                Id = kId,
                Name = $"Kitten {cId:00000}_1",
                Sex = rand.Next(10) != 0 ? (Sex)rand.Next(2) : null
            });

            if (rand.Next(5) == 0)
            {
                kittens.Add(new Kitten
                {
                    Id = kId + 1,
                    Name = $"Kitten {cId:00000}_2",
                    Sex = rand.Next(10) != 0 ? (Sex)rand.Next(2) : null
                });
            }
        }

        return kittens;
    }
}