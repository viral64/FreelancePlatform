using System;
using System.Collections.Generic;

namespace FreelancePlatform.Domain.Entities;

public partial class Task
{
    public int TaskId { get; set; }

    public int ClientId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Budget { get; set; }

    public DateTime? Deadline { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual User Client { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
