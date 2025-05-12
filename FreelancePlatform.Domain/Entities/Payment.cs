using System;
using System.Collections.Generic;

namespace FreelancePlatform.Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int TaskId { get; set; }

    public int ClientId { get; set; }

    public int FreelancerId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime PaymentDate { get; set; }

    public virtual User Client { get; set; } = null!;

    public virtual User Freelancer { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
