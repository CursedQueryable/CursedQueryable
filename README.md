[![CI](https://github.com/CursedQueryable/CursedQueryable/actions/workflows/ci.yml/badge.svg)](https://github.com/CursedQueryable/CursedQueryable/actions/workflows/ci.yml)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/FrontierFox/a66a7792632db41df60777de803b8548/raw/cq-coverage.json)
[![NuGet](http://img.shields.io/nuget/vpre/CursedQueryable.svg?label=NuGet)](https://www.nuget.org/packages/CursedQueryable/)
[![License](https://img.shields.io/github/license/CursedQueryable/CursedQueryable)](https://github.com/CursedQueryable/CursedQueryable/blob/main/LICENSE.md)

### What is CursedQueryable?

CursedQueryable is a library that aims to implement cursor-based keyset pagination (aka seek pagination) for IQueryable
with as close to zero boilerplate code as possible. This is achieved via examining the underlying expression tree for
any `IQueryable` instance and rewriting it as needed prior to any database calls being made.

This is the main repository for CursedQueryable, and houses the bulk of the functionality involved in extending the
`IQueryable` interface. However, it probably isn't that useful on its own - the main expectation is for it to be used
alongside Object Relation Mapping frameworks, specifically Entity Framework Core. You can grab the implementation of
CursedQueryable for EFCore from
the [CursedQueryable.EntityFrameworkCore repository](https://github.com/CursedQueryable/CursedQueryable.EntityFrameworkCore).

### What is cursor-based keyset pagination?

Put simply, it is a performant and scalable way to traverse sets of data. To help understand why it is sometimes needed,
it's best to contrast it with offset pagination.

You are most likely familiar with offset pagination as it is available out-of-the-box via the use of `.Skip()`. While
simple to understand and implement, offset pagination has a major flaw in that its performance continually degrades as
the underlying dataset gets larger. This is because the underlying database always requires an index scan to use an
offset; so skipping to the millionth+1 row in a data set still requires all the preceding million rows to be retrieved
(even if they are ultimately discarded).

Keyset pagination avoids slow index scans by instead leveraging index seek behaviour. The tradeoff is that it requires
specifically constructed `where` and `order by` clauses to work, as well as the persistence of a number of different
tokens in order to keep paging through the data set. This means there's a lot of additional complexity to manage when
utilizing such an approach.

CursedQueryable aims to eliminate that complexity. It automatically generates all of the required `where` and `order by`
clauses needed for the keyset to work, and encapsulates all tokens into a single string (aka cursor).

(You can read a bit more about this topic [here](https://learn.microsoft.com/en-us/ef/core/querying/pagination)).

### Getting started

Reference the [CursedQueryable.EntityFrameworkCore package](), then ensure it's [configured](#configuration) correctly.

Once done, switching to CursedQueryable is as simple as removing any calls to `.Skip()` and replacing
`.ToList()`/`.ToListAsync()` with the CursedQueryable extension methods `.ToPage()`/`.ToPageAsync()`:

```csharp
using CursedQueryable.Extensions;

// Before
public async Task<List<Cat>> GetOffsetCats(int offset)
{
    return dbContext
        .Cats
        .Skip(offset)
        .Take(10)
        .ToListAsync();
}

var offsetPage = await GetOffsetCats(0);

// After
public async Task<Page<Cat>> GetCursedCats(string? cursor)
{
    return await dbContext
        .Cats
        .Take(10)
        .ToPageAsync(cursor);
}

var cursedPage = await GetCursedCats(null);
```

Note that this results in a single `Page` object, rather collection. A `Page` contains navigation info and edges, with
an edge being a node (the entity, or a projection of it) and a cursor (a unique identifier for that node).

Serialized as JSON, a `Page` will look something like this:

```json
{
    "edges": [
        {"cursor": "WzEzMzcsWzFdXQ==", "node": {"id": 1, "name": "Jonesy", "sex": "Male"}},
        {"cursor": "WzEzMzcsWzJdXQ==", "node": {"id": 2, "name": "Greebo", "sex": "Male"}},
        {"cursor": "WzEzMzcsWzNdXQ==", "node": {"id": 3, "name": "Lying Cat", "sex": "Female"}}
    ],
    "info": {
        "hasNextPage": true,
        "startCursor": "WzEzMzcsWzFdXQ==",
        "endCursor": "WzEzMzcsWzNdXQ=="
    }
}
```

Cursors are simply passed as an argument to `.ToPage()`/`.ToPageAsync()` in order to continue advancing through the data
set:

```csharp
public async Task<Page<Cat>> GetCursedCats(string? cursor)
{
    return await dbContext
        .Cats
        .Take(10)
        .ToPageAsync(cursor);
}

var firstPage = await GetCursedCats(null);

// HasNextPage will be true if there were more than 10 cats.
if(firstPage.Info.HasNextPage)
{
    // Info.EndCursor is provided for convenience - it matches the cursor value for the
    // last entry in the Edges collection.
    var secondPage = await GetCursedCats(firstPage.Info.EndCursor);
}
```

You can also traverse the data set in reverse (i.e. retrieve the previous page) by changing the direction:

```csharp
public async Task<Page<Cat>> GetCursedCats(string? cursor, Direction direction)
{
    return await dbContext
        .Cats
        .Take(10)
        .ToPageAsync(cursor, direction);
}

// Gets the first and next pages, then uses the StartCursor from nextPage to go back to 
// the first page.
var firstPage = await GetCursedCats(null, Direction.Forwards);
var secondPage = await GetCursedCats(firstPage.Info.EndCursor, Direction.Forwards);
var firstPageAgain = await GetCursedCats(secondPage.Info.StartCursor, Direction.Backwards);
```

NOTE: when traversing backwards, `hasNextPage` becomes `hasPreviousPage`:

```json
{
    "info": {
        "hasPreviousPage": true,
        "startCursor": "WzEzMzcsWzFdXQ==",
        "endCursor": "WzEzMzcsWzNdXQ=="
    }
}
```

While there are some [limitations](#limitations-and-best-practices), you are able to chain the vast majority of
queryable methods as normal:

```csharp
public async Task<Page<Purrito>> GetCursedPurritos(string? cursor, Direction direction)
{
    return await dbContext
        .Cats
        .AsNoTrackingWithIdentityResolution() // EFCore specific
        .Include(cat => cat.Kittens) // EFCore specific
        .Where(cat => cat.Sex == Sex.Female)
        .OrderByDescending(cat => cat.DateOfBirth)
        .ThenBy(cat => cat.Name)
        .Take(10)
        .Select(cat => new Purrito {
            CatName = cat.Name,
            KittenNames = cat.Kittens.Select(kitten => kitten.Name),
            Material = cat.HasExpensiveTastes ? "Silk" : "Cotton"
        })
        .ToPageAsync(cursor, direction);
}

// Get a page of cats and their kittens projected into a Purrito DTO. 
// Note that despite the Purrito DTO lacking the Sex/DateOfBirth properties, the cursor
// still uniquely identifies each using the values associated with the underlying Cat 
// entity. This correctly persists how the cats were ordered prior to the projection. 
var pageOfPurritos = await GetCursedPurritos(null, Direction.Forwards);
```

### Limitations and Best Practices

❌ DO NOT call `.ToPage()`/`.ToPageAsync()` on a queryable that breaks any of these rules (
throws `NotSupportedException`):

* Queryable methods that completely alter the meaning of data set before it is returned from the database are not
  permitted. For example, `.GroupBy()` and `.SelectMany()`.
* `.Take()` is allowed, but may only be followed by `.Select()`
* `.Skip()` is also allowed, but may only be followed by `.Take()` and/or `.Select()`

❌ DO NOT share/use cursors between queries that reference the same entity but implement completely different ordering
(throws `BadCursorException`). This limitation excludes ASC/DESC differences. For example:

* A cursor for 'Cats sorted by Name' is **invalid** for 'Cats sorted by DateOfBirth' (columns are different).
* A cursor for 'Cats sorted by Name then DateOfBirth' is **invalid** for 'Cats sorted by DateOfBirth then Name' (columns
  are the same but referenced in a different order).
* A cursor for 'Cats sorted by Name ASC' is **valid** for 'Cats sorted by Name DESC' (columns are the same, ASC/DESC
  differences are irrelevant).

❌ AVOID ordering on your primary key column(s). Ordering for these is automatically added by CursedQueryable as it is
an essential component in establishing where each cursor located in the set. Doing so may manually may lead to
incorrectly ordered results.

✔️ **DO** check that the correct `NullBehaviour` is set for your database, especially if you're ordering on any nullable
columns. See the [configuration section](#configuration) for information on this.

✔️ **CONSIDER** creating indexes/composite indexes depending on your use of `.OrderBy()`,`.ThenBy()`, etc.
CursedQueryable may take much of the complexity out of using these with keyset pagination, but without appropriate
indexing in place performance issues are inevitable.

### Configuration

`.ToPage()`/`.ToPageAsync()` are extension methods and consequently their default behaviour must be managed via the
static global configuration class, `CursedExtensionsConfig`. This configuration class will attempt to configure the
correct behaviours for your solution automatically the first time it is accessed. Should you need to explicitly set
these to something else for your solution, you should do so in your app startup code:

```csharp
using CursedQueryable.Extensions;
using CursedQueryable.Options;

CursedExtensionsConfig.Configure(opts => {
    // The automatic behaviour is to scan loaded assemblies for either 
    // 'Oracle.EntityFrameworkCore' or 'Npgsql.EntityFrameworkCore' and set 
    // LargerThanNonNullable if either is found.
    // You can therefore omit this, but it's probably best to be explicit.
    opts.NullBehaviour = NullBehaviour.SmallerThanNonNullable; // for most databases
    opts.NullBehaviour = NullBehaviour.LargerThanNonNullable;  // for Postgres, Oracle
    
    // Bad cursors are discarded by default and consequently the user is given the first 
    // page of results again.
    // You may instead prefer BadCursorException to be thrown so you can catch it and 
    // provide feedback to the user.
    opts.BadCursorBehaviour = BadCursorBehaviour.ThrowException;
    
    // When referencing CursedQueryable.EntityFrameworkCore this should be assigned
    // automatically to use the EfCoreEntityDescriptorProvider implementation.
    // If you plan on using your own implementation instead you'll need to write it up
    // manually.
    opts.Provider = new YourCustomEntityDescriptorProvider();
});
```

Additionally, you can override the global behaviour on a case-by-case basis by calling the `.ToPage()`/`.ToPageAsync()`
configurable overload:

```csharp
public async Task<Page<Cat>> GetCursedCats(string? cursor, Direction direction)
{
    return await dbContext
        .Cats
        .Take(10)
        .ToPageAsync(opts => {
            opts.Cursor = cursor;
            opts.Direction = direction;
            
            // opts.FrameworkOptions is cloned from your global configuration
            opts.FrameworkOptions.BadCursorBehaviour = BadCursorBehaviour.ThrowException;
        });
}
```