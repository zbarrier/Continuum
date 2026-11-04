# Roadmap

Remaining work, in priority order. Items marked *Not required for 1.0.0* can follow the first release.

- [ ] **KurrentDB Orleans stream provider: catch-up subscriptions to a single stream.** Replace `UseCatchUpSubscription` with `UseAllStreamCatchUpSubscription` (queue is `$all`, optional `StreamFilterPrefix`) and `UseStreamCatchUpSubscription` (one queue naming the stream, such as `$ce-{Category}` or a custom projection stream; `$all` is rejected; no prefix filter). Single-stream receivers use `SubscribeToStream` with `ResolveLinkTos`, position by link revision, skip deleted link targets while advancing, and do not wait for transaction contiguity. Document that a subscription name is bound to its source and checkpoint; replay uses a new name (for example `Projections-V2`). Add the either/or validation to the self-hosted catch-up subscription.
- [ ] **KurrentDB subscription hosted in an Orleans grain.** Like the self-hosted subscriptions, but run inside a subscription grain instead of through Orleans Streams. *Not required for 1.0.0.*
- [ ] **Catch-up handoff to the Orleans stream provider.** A non-Orleans KurrentDB subscription catches up first, batching writes so the database scales better. When it catches up, it produces an event and deactivates, and an Orleans stream provider subscription continues from where it left off. *Not required for 1.0.0.*
- [ ] **API projects: Result to HTTP response.** Add extensions that convert a `Result` into an HTTP response, starting from existing code, then clean up and add tests.
