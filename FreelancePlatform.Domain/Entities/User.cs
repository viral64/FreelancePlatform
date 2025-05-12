using System;
using System.Collections.Generic;

namespace FreelancePlatform.Persistence;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> PaymentClients { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentFreelancers { get; set; } = new List<Payment>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserTypeMapping> UserTypeMappings { get; set; } = new List<UserTypeMapping>();
}
