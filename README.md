# r47.Primitives.EnumT

Type-safe, extensible, enum-like values for .NET with rich metadata (display text, stable OID, UI sort index, visibility, default item) and convenient lookups and flag helpers.

## Why EnumT?
Classic C# `enum` is a simple integer with a name. Many real-world cases need more:
- Human-readable `Text` for UI
- A stable identifier (`Oid`) for persistence/migrations
- A separate `Index` to control UI ordering independent from the numeric value
- Visibility control and a default entry per type

`EnumT<T>` implements a “smart enum” pattern: each value is a singleton object with metadata, defined as a `public static readonly` field on a derived class.

## Features
- Type-safe enumeration pattern via `abstract partial class EnumT<T> where T : EnumT<T>`
- Rich metadata per entry: `Text`, `Value`, `Index`, `Oid`, `IsVisible`, and per-type `Default`
- Thread-safe registration and access; deterministic static initialization
- Uniqueness guaranteed for `Value` and `Index` within a type
- Lookups and parsing:
  - `FromValue(int)`, `TryFromValue(int, out T)`
  - `FromText(string, StringComparison)`, `TryFromText(...)`
  - `TryFind(Guid oid, out T)`
  - `TryParse(string input, out T)` (OID → int → text)
- Flags ergonomics: `&` (any common bit), `|` (mask), `~` (mask), `HasFlag(EnumT<T> flag)`
- Safe enumeration helpers: `Entries()`, `SortEntries()`, `SortVisibleEntries()`, `ClonedEntries()`

Supported TFMs (per repo setup): `netstandard2.0`, `netstandard2.1`, `net8.0`, `net10.0`.

## Quick start

1) Define your smart-enum type by deriving from `EnumT<T>` and declaring static instances:

```csharp
using r47.Primitives.EnumT;

public sealed class OrderStatus : EnumT<OrderStatus>
{
    // Private ctor funnels to base; you may pass explicit value/index if you need
    private OrderStatus(string text, int? value = null, int? index = null, bool isVisible = true)
        : base(text, value, index, isVisible) { }

    static OrderStatus()
    {
        Default = Pending;
    }

    public static readonly OrderStatus Pending   = new("Pending");
    public static readonly OrderStatus Confirmed = new("Confirmed");
    public static readonly OrderStatus Shipped   = new("Shipped");
    public static readonly OrderStatus Hidden    = new("Hidden", isVisible: false);
}
```

2) Use it in your code:

```csharp
// List all entries (registration order) as a read-only snapshot
var all = OrderStatus.Entries();

// UI-friendly list (visible only), sorted by Index
var visible = OrderStatus.SortVisibleEntries();

// Default value
var def = OrderStatus.Default; // Pending

// Lookups
var s1 = OrderStatus.FromValue(OrderStatus.Confirmed.Value);      // throws if missing
OrderStatus.TryFromValue(123, out var maybe);                      // false, maybe==null

var s2 = OrderStatus.FromText("Shipped");                         // case-sensitive by default
OrderStatus.TryFromText("shipped", out var ci);                   // true (ignore case default)

// Parse from string: OID → int → Text
OrderStatus.TryParse(OrderStatus.Confirmed.Oid.ToString(), out var byOid); // true
OrderStatus.TryParse(OrderStatus.Confirmed.Value.ToString(), out var byVal);// true
OrderStatus.TryParse("Confirmed", out var byText);                 // true
```

### Flags-style usage
If you use bit semantics for `Value`, you can combine and test flags:

```csharp
public sealed class FilePerm : EnumT<FilePerm>
{
    private FilePerm(string text, int value) : base(text, value) { }
    static FilePerm() { Default = Read; }

    public static readonly FilePerm Read  = new("Read",  0b0001);
    public static readonly FilePerm Write = new("Write", 0b0010);
    public static readonly FilePerm Exec  = new("Exec",  0b0100);
    public static readonly FilePerm ReadWrite  = 
        new(nameof(FilePerm.ReadWrite),  Read.Value | Write.Value);
}

int mask = FilePerm.Read | FilePerm.Write;      // 0b0011
bool anyCommon = FilePerm.Read & FilePerm.Exec;  // false (no common bit)
bool hasWrite = FilePerm.Read.HasFlag(FilePerm.Write); // false
```

Notes:
- `a & b` returns `false` if either is `null`; otherwise checks for any common bit.
- `a | b` and `~a` return integer masks (not new enum instances).
- `HasFlag` checks subset semantics: `(this.Value & flag.Value) == flag.Value`.

## Thread-safety and determinism
- Registration and auto-numbering are guarded by a per-type lock.
- `Value`/`Index` uniqueness is enforced; duplicates throw.
- Read APIs take snapshots under lock, so enumeration is safe.
- The base type forces deterministic initialization of `T` via `RuntimeHelpers.RunClassConstructor`.

## Memory retention and static lifetime
Smart-enum instances are usually defined as `public static readonly` fields on the derived type. This implies:
- Instances are strongly referenced for the lifetime of their declaring type within the current AppDomain/AssemblyLoadContext (ALC).
- The internal static `Items` list also holds references, but even if it didn’t, the derived type’s own static fields already keep instances alive.

Implications and guidance:
- If you repeatedly load different enum types or versions dynamically into the same, non-collectible ALC, memory will grow (each loaded type keeps its own statics).
- To reclaim memory for plugin scenarios, load the assembly into a collectible `AssemblyLoadContext`, drop all references from the default ALC, and call `Unload()`; only then can the enum instances (and the assembly) be GC’d.
- Tests that mutate or depend on global singletons should avoid cross-test interference. This library uses snapshots and locks, but if you create different enum types per test, prefer process isolation or collectible ALCs.

If you need values that are not process-lifetime singletons, consider a different pattern (e.g., data-driven descriptors plus ephemeral wrappers) since that changes semantics beyond `EnumT<T>`’s scope.

## Best practices
- Define all entries as `public static readonly` fields in the derived type’s body.
- Optionally set `Default` in a static constructor.
- Avoid mutating state – entries are immutable after construction.
- Use `Entries()`/`SortEntries()`/`SortVisibleEntries()` for safe enumeration.
- Prefer `Try*` methods for routine lookups to avoid exceptions.

## API highlights
- Construction: `EnumT(string text, int? value = null, int? index = null, bool isVisible = true)`
- Properties: `Text`, `Value`, `Index`, `Oid`, `IsVisible`, `IsDefault`, `Default`
- Lookups: `FromValue`, `TryFromValue`, `FromText`, `TryFromText`, `TryParse`, `TryFind`
- Enumeration: `Entries`, `SortEntries`, `SortVisibleEntries`, `ClonedEntries`
- Flags helpers: `&`, `|`, `~`, `HasFlag`

## Versioning and targets
This repository builds for multiple targets observed in CI and local builds:
- `netstandard2.0`, `netstandard2.1` for broad compatibility
- `net8.0`, `net10.0` for modern runtimes

## License
MIT (or the license in this repository). Update this section if a different license applies.

## Acknowledgments
This library follows the “smart enum” idea popularized by various community implementations, adapted with strong uniqueness, thread-safety, and richer metadata, 
see [this blog post](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types) for instance.