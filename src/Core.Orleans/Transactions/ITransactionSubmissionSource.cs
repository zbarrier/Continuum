namespace Continuum.Orleans.Transactions;

/// <summary>
///     Implemented by a journaled grain that coordinates multi-stream transactions. The log view adaptor asks it for
///     the transaction while the grain submits its own events.
/// </summary>
/// <remarks>
///     Only the grain's own events are applied to its state. The participants' events travel alongside them and are
///     appended in the same atomic write.
/// </remarks>
public interface ITransactionSubmissionSource
{
	/// <summary>
	///     Gets the transaction being submitted, or <see langword="null"/> for a normal submission. It is only set while
	///     the grain synchronously submits the transaction's events.
	/// </summary>
	TransactionSubmission? PendingTransaction { get; }
}

/// <summary>A transaction a coordinator is submitting with its own events.</summary>
/// <param name="transactionId">The transaction ID, recorded on every appended event.</param>
/// <param name="participants">The participants' logs.</param>
/// <param name="records">Every event of the transaction, in the order the business logic generated them.</param>
public sealed class TransactionSubmission(Guid transactionId, IReadOnlyList<TransactionParticipantLog> participants,
	IReadOnlyList<TransactionRecord> records)
{
	/// <summary>Gets the transaction ID, recorded on every appended event.</summary>
	public Guid TransactionId { get; } = transactionId;

	/// <summary>Gets the participants' logs.</summary>
	public IReadOnlyList<TransactionParticipantLog> Participants { get; } = participants;

	/// <summary>Gets every event of the transaction, in the order the business logic generated them.</summary>
	public IReadOnlyList<TransactionRecord> Records { get; } = records;

	/// <summary>
	///     Gets or sets why the transaction was rejected without being appended, for example because a participant's
	///     log changed after it prepared. Retrying cannot succeed, so the grain's events are dropped.
	/// </summary>
	public Exception? Failure { get; set; }

	/// <summary>Gets or sets whether <see cref="Failure"/> is a participant's log having changed after it prepared.</summary>
	public bool IsConflict { get; set; }
}

/// <summary>A participant's log in a transaction.</summary>
/// <param name="GrainId">The participant grain's ID.</param>
/// <param name="ExpectedVersion">The version its log must be at, or <see langword="null"/> for no version check.</param>
public sealed record TransactionParticipantLog(GrainId GrainId, int? ExpectedVersion);

/// <summary>One event of a transaction.</summary>
/// <param name="Participant">
///     The index of the participant in <see cref="TransactionSubmission.Participants"/>, or <see langword="null"/> for
///     the coordinator's next submitted event.
/// </param>
/// <param name="Event">The participant's event, or <see langword="null"/> for a coordinator event.</param>
public sealed record TransactionRecord(int? Participant, object? Event);
