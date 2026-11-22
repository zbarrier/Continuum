using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Gets the order entry form of the draft (the OrderDraft read model). The order builds it from its own events, so
    ///     the tabs always show the latest save. Only the user who started the draft can read it.
    /// </summary>
    [ReadOnly]
    Task<Result<OrderDraft>> GetOrderDraftAsync();
}

/// <summary>
///     The saved tabs of a draft, with the IDs and versions of the service, form type and intended use, so the Request
///     &amp; Property tab can show them for editing. The Borrower &amp; Access contact values are <see langword="null"/>
///     until that tab is saved. The draft applies the order's events itself, so the order can build it from its events and
///     the order entry tabs can apply the events a save returns without reading the draft again.
/// </summary>
/// <param name="OrderId">The order the draft belongs to.</param>
/// <param name="ServiceRequestTypeId">The service ordered.</param>
/// <param name="ServiceRequestTypeVersion">The version of the service ordered.</param>
/// <param name="ServiceRequestTypeName">The name of the service ordered.</param>
/// <param name="FormTypeId">The form type ordered.</param>
/// <param name="FormTypeVersion">The version of the form type ordered.</param>
/// <param name="FormTypeName">The name of the form type ordered.</param>
/// <param name="PropertyStreet">The street of the property.</param>
/// <param name="PropertyCity">The city of the property.</param>
/// <param name="PropertyState">The two-letter state of the property.</param>
/// <param name="PropertyZip">The ZIP code of the property.</param>
/// <param name="PropertyType">The type of the property.</param>
/// <param name="IntendedUseId">The intended use of the appraisal.</param>
/// <param name="IntendedUseVersion">The version of the intended use.</param>
/// <param name="IntendedUseName">The name of the intended use.</param>
/// <param name="LoanNumber">The client's loan number.</param>
/// <param name="DueDate">When the appraisal is due.</param>
/// <param name="BorrowerName">The borrower.</param>
/// <param name="AccessContactName">The person who gives access to the property, if any.</param>
/// <param name="AccessContactPhone">The access contact's 10-digit phone number, digits only.</param>
/// <param name="SupportingDocuments">The documents attached to the draft, in the order they were attached.</param>
/// <param name="LastSavedAt">When the draft was last saved.</param>
/// <param name="Version">The version of the order the draft shows: the number of the order's events it has applied.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDraft(
    string OrderId,
    string ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    string FormTypeId,
    int FormTypeVersion,
    string FormTypeName,
    string PropertyStreet,
    string PropertyCity,
    string PropertyState,
    string PropertyZip,
    string PropertyType,
    string IntendedUseId,
    int IntendedUseVersion,
    string IntendedUseName,
    string LoanNumber,
    DateOnly DueDate,
    string? BorrowerName,
    string? AccessContactName,
    string? AccessContactPhone,
    IReadOnlyList<SupportingDocument> SupportingDocuments,
    DateTimeOffset LastSavedAt,
    int Version)
{
    /// <summary>The property address on one line.</summary>
    public string PropertyAddress => $"{PropertyStreet}, {PropertyCity}, {PropertyState} {PropertyZip}";

    /// <summary>The draft <paramref name="started"/> starts, at version 1.</summary>
    public static OrderDraft Start(OrderDraftStarted started) => new(
        started.OrderId,
        started.ServiceRequestTypeId,
        started.ServiceRequestTypeVersion,
        started.ServiceRequestTypeName,
        started.FormTypeId,
        started.FormTypeVersion,
        started.FormTypeName,
        started.PropertyStreet,
        started.PropertyCity,
        started.PropertyState,
        started.PropertyZip,
        started.PropertyType,
        started.IntendedUseId,
        started.IntendedUseVersion,
        started.IntendedUseName,
        started.LoanNumber,
        started.DueDate,
        BorrowerName: null,
        AccessContactName: null,
        AccessContactPhone: null,
        SupportingDocuments: [],
        started.OccurredAt,
        Version: 1);

    /// <summary>Builds the draft from an order's events, from its first one.</summary>
    /// <exception cref="InvalidOperationException">The events do not start with <see cref="OrderDraftStarted"/>.</exception>
    public static OrderDraft Build(IEnumerable<OrderEvent> events)
    {
        using var enumerator = events.GetEnumerator();
        if (!enumerator.MoveNext() || enumerator.Current is not OrderDraftStarted started)
        {
            throw new InvalidOperationException($"An order's events must start with {nameof(OrderDraftStarted)}.");
        }

        var draft = Start(started);
        while (enumerator.MoveNext())
        {
            draft = draft.Apply(enumerator.Current);
        }
        return draft;
    }

    /// <summary>The draft after <paramref name="events"/>, the order's events that follow this version, in order.</summary>
    public OrderDraft Apply(IEnumerable<OrderEvent> events) => events.Aggregate(this, (draft, evt) => draft.Apply(evt));

    /// <summary>
    ///     The draft after <paramref name="evt"/>, the order's next event. Events that don't change the draft still move it
    ///     to the next version.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="evt"/> starts another draft.</exception>
    public OrderDraft Apply(OrderEvent evt) => evt switch
    {
        OrderDraftStarted => throw new InvalidOperationException($"The draft {OrderId} was already started."),
        RequestAndPropertySaved saved => this with
        {
            ServiceRequestTypeId = saved.ServiceRequestTypeId,
            ServiceRequestTypeVersion = saved.ServiceRequestTypeVersion,
            ServiceRequestTypeName = saved.ServiceRequestTypeName,
            FormTypeId = saved.FormTypeId,
            FormTypeVersion = saved.FormTypeVersion,
            FormTypeName = saved.FormTypeName,
            PropertyStreet = saved.PropertyStreet,
            PropertyCity = saved.PropertyCity,
            PropertyState = saved.PropertyState,
            PropertyZip = saved.PropertyZip,
            PropertyType = saved.PropertyType,
            IntendedUseId = saved.IntendedUseId,
            IntendedUseVersion = saved.IntendedUseVersion,
            IntendedUseName = saved.IntendedUseName,
            LoanNumber = saved.LoanNumber,
            DueDate = saved.DueDate,
            LastSavedAt = saved.OccurredAt,
            Version = Version + 1
        },
        BorrowerAndAccessContactSaved saved => this with
        {
            BorrowerName = saved.BorrowerName,
            AccessContactName = saved.AccessContactName,
            AccessContactPhone = saved.AccessContactPhone,
            LastSavedAt = saved.OccurredAt,
            Version = Version + 1
        },
        SupportingDocumentAttached attached => this with
        {
            SupportingDocuments =
            [
                .. SupportingDocuments,
                new SupportingDocument(
                    attached.DocumentId, attached.DocumentTypeId, attached.DocumentTypeVersion, attached.DocumentTypeName,
                    attached.FileName, attached.ContentType, attached.FileSize, attached.Description, attached.OccurredAt)
            ],
            LastSavedAt = attached.OccurredAt,
            Version = Version + 1
        },
        SupportingDocumentRemoved removed => this with
        {
            SupportingDocuments = [.. SupportingDocuments.Where(document => document.DocumentId != removed.DocumentId)],
            LastSavedAt = removed.OccurredAt,
            Version = Version + 1
        },
        _ => this with { Version = Version + 1 }
    };
}

/// <summary>A document attached to a draft, as the Supporting documents tab lists it.</summary>
/// <param name="DocumentId">The document.</param>
/// <param name="DocumentTypeId">The type of the document.</param>
/// <param name="DocumentTypeVersion">The version of the document type.</param>
/// <param name="DocumentTypeName">The name of the document type.</param>
/// <param name="FileName">The name of the uploaded file.</param>
/// <param name="ContentType">The media type of the file.</param>
/// <param name="FileSize">The size of the file in bytes.</param>
/// <param name="Description">What the document holds, if the user described it.</param>
/// <param name="AttachedAt">When the document was attached.</param>
[GenerateSerializer, Immutable]
public sealed record SupportingDocument(
    string DocumentId,
    string DocumentTypeId,
    int DocumentTypeVersion,
    string DocumentTypeName,
    string FileName,
    string ContentType,
    long FileSize,
    string? Description,
    DateTimeOffset AttachedAt);
