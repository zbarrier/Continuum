using System.Globalization;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Web.Components.Client.OrderEntryTabs;

/// <summary>
///     The save state of one order entry tab: whether a save is running, and the errors of the last save, each validation
///     error on its field and any other error above the tab.
/// </summary>
internal sealed class OrderEntryForm
{
    private readonly ILogger _logger;
    private readonly IReadOnlyDictionary<string, string> _fieldLabels;
    private readonly IReadOnlyDictionary<string, string> _fieldByTarget;
    private readonly ErrorMessageLocalizer _fieldNames;
    private readonly Dictionary<string, string> _fieldErrors = [];

    /// <param name="logger">Logs a save that throws.</param>
    /// <param name="fieldLabels">The label of each field, also used as the display name in its validation messages.</param>
    /// <param name="fieldByTarget">The field that shows a validation error, for the targets that are not a field of their own.</param>
    public OrderEntryForm(
        ILogger logger, IReadOnlyDictionary<string, string> fieldLabels, IReadOnlyDictionary<string, string>? fieldByTarget = null)
    {
        _logger = logger;
        _fieldLabels = fieldLabels;
        _fieldByTarget = fieldByTarget ?? new Dictionary<string, string>();
        _fieldNames = new ErrorMessageLocalizer
        {
            DisplayNameResolver = (target, _) => _fieldLabels.GetValueOrDefault(FieldOf(target))
        };
    }

    /// <summary>Whether a save is running.</summary>
    public bool Saving { get; private set; }

    /// <summary>The error of the last save that no field shows, if any.</summary>
    public string? Error { get; private set; }

    public bool HasError(string field) => _fieldErrors.ContainsKey(field);

    public string? ErrorText(string field) => _fieldErrors.GetValueOrDefault(field);

    /// <summary>Runs <paramref name="save"/> and shows its errors; returns its value, or <see langword="null"/> when it failed.</summary>
    public async Task<TValue?> SaveAsync<TValue>(Func<Task<Result<TValue>>> save) where TValue : class
    {
        Saving = true;
        Error = null;
        _fieldErrors.Clear();
        try
        {
            var result = await save();
            if (result.IsSuccess)
            {
                return result.Value;
            }

            ShowErrors(result.Error);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An order entry command failed.");
            Error = "The order could not be saved. Try again in a moment.";
            return null;
        }
        finally
        {
            Saving = false;
        }
    }

    /// <summary>
    ///     Runs <paramref name="save"/> on <paramref name="order"/> and shows its errors; returns the draft after the save, or
    ///     <see langword="null"/> when it failed. The draft is <paramref name="current"/> with the events the save returns
    ///     applied (or built from them, when the save starts the draft), so the order is not read again. When the order moved
    ///     past <paramref name="current"/> (another browser saved the draft), the draft is read from the order instead.
    /// </summary>
    public Task<OrderDraft?> SaveDraftAsync<TResponse>(
        IOrderGrain order, OrderDraft? current, Func<Task<Result<TResponse>>> save) where TResponse : ICommandResponseWithChanges
        => SaveAsync(() => save().Bind(async response =>
        {
            var events = response.Changes.Select(change => (OrderEvent)change.Event).ToList();
            if (response.Version != (current?.Version ?? 0) + events.Count)
            {
                return await order.GetOrderDraftAsync();
            }

            return Result.Success(current is null ? OrderDraft.Build(events) : current.Apply(events));
        }));

    private void ShowErrors(Error error)
    {
        if (error is not ValidationError validation)
        {
            Error = error.GetFormattedMessage();
            return;
        }

        var unplaced = new List<string>();
        foreach (var entry in validation.Entries)
        {
            var field = FieldOf(entry.Target);
            var message = entry.GetFormattedMessage(CultureInfo.CurrentUICulture, _fieldNames);
            if (_fieldLabels.ContainsKey(field))
            {
                _fieldErrors.TryAdd(field, message);
            }
            else
            {
                unplaced.Add(message);
            }
        }
        Error = unplaced.Count > 0 ? string.Join(" ", unplaced) : null;
    }

    private string FieldOf(string target) => _fieldByTarget.GetValueOrDefault(target, target);
}
