﻿using System;
using System.Collections.Generic;

namespace CompleteEFCore.API.Domain.Entities.Northwind;

public partial class Region
{
    public short RegionId { get; set; }

    public string RegionDescription { get; set; } = null!;

    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}
