using System;
using System.Collections.Generic;

namespace CompleteEFCore.API.Domain.Entities.Northwind;

public partial class Territory
{
    public string TerritoryId { get; set; } = null!;

    public string TerritoryDescription { get; set; } = null!;

    public short RegionId { get; set; }

    public virtual Region Region { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
