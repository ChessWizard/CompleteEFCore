﻿using System;
using System.Collections.Generic;

namespace CompleteEFCore.API.Domain.Entities.Northwind;

public partial class UsState
{
    public short StateId { get; set; }

    public string? StateName { get; set; }

    public string? StateAbbr { get; set; }

    public string? StateRegion { get; set; }
}
