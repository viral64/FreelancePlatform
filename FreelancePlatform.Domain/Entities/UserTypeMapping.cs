using System;
using System.Collections.Generic;

namespace FreelancePlatform.Domain.Entities;

public partial class UserTypeMapping
{
    public int UserId { get; set; }

    public int UserTypeId { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual UserType UserType { get; set; } = null!;
}
