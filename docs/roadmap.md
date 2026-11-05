# Roadmap

Remaining work, in priority order. Items marked *Not required for 1.0.0* can follow the first release.

- [x] **KurrentDB Orleans stream provider: catch-up subscriptions to a single stream.**
- [ ] **KurrentDB subscription hosted in an Orleans grain.** Like the self-hosted subscriptions, but run inside a subscription grain instead of through Orleans Streams. *Not required for 1.0.0.*
- [ ] **Catch-up handoff to the Orleans stream provider.** A non-Orleans KurrentDB subscription catches up first, batching writes so the database scales better. When it catches up, it produces an event and deactivates, and an Orleans stream provider subscription continues from where it left off. *Not required for 1.0.0.*
- [ ] **API projects: Result to HTTP response.** Add extensions that convert a `Result` into an HTTP response, starting from existing code, then clean up and add tests.
