using System;
using System.Collections.Generic;

namespace FreelancePlatform.Persistence;

public partial class UserType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserTypeMapping> UserTypeMappings { get; set; } = new List<UserTypeMapping>();
}
