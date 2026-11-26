namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>How far a submitted order is, as the AMC's staff see it.</summary>
public enum StaffOrderStatus
{
    /// <summary>The client submitted the order and nobody is assigned yet.</summary>
    Submitted,

    /// <summary>The order is assigned to a staff member.</summary>
    Assigned,

    /// <summary>The staff member the order is assigned to is working on it.</summary>
    Acknowledged,

    /// <summary>The order's description of work waits for the client's approval.</summary>
    DescriptionOfWorkAwaitingApproval,

    /// <summary>The client approved the order's description of work.</summary>
    DescriptionOfWorkApproved,

    /// <summary>The order is assigned to a vendor.</summary>
    VendorAssigned
}
