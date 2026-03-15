using PharmacyManagement.CSharp.Models;

namespace PharmacyManagement.CSharp.Data;

internal static class ModuleCatalog
{
    internal static readonly IReadOnlyList<ModuleDefinition> Modules =
    [
        new("Customer", "cust"),
        new("Doctor", "doc"),
        new("Medicine", "med"),
        new("Supplier", "supplier"),
        new("Stock", "stock"),
        new("Order", "order1"),
        new("Bill", "bill"),
        new("Supplier Invoice", "sinvoice")
    ];
}
