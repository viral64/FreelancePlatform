using System;
using System.Collections.Generic;

namespace FreelancePlatform.Persistence;

public partial class Bid
{
    public int BidId { get; set; }

    public int TaskId { get; set; }

    public int FreelancerId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime BidTime { get; set; }

    public bool IsAccepted { get; set; }

    public virtual User Freelancer { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
