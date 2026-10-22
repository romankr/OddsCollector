---
name: azure-functions-well-architected
description: Azure Well-Architected checklist (reliability, security, cost, operations, performance) for the OddsCollector function app. Use when making hosting, infrastructure, security, deployment, monitoring or scaling decisions, or reviewing changes that affect them.
---

# Azure Functions — Well-Architected checklist

Derived from Microsoft Learn, "Architecture best practices for Azure Functions" (Azure Well-Architected Framework)
(https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions),
applied to OddsCollector. Code-level rules are in the `azure-functions-best-practices` skill.

For Premium and Dedicated plans the App Service service guide also applies, since they run on App Service.

## Reliability

- Pick triggers for their delivery semantics; queue-driven work must survive reprocessing after a failure.
- Built-in retries for transient faults; Durable Functions only if a workflow genuinely needs orchestration/state.
- Fail gracefully: handle exceptions, log them, alert on critical failures.
- Design every function for idempotency — retries and redelivery must not duplicate side effects
  (Cosmos DB writes keyed by stable ids).
- Load-test for spikes and watch scaling in Application Insights.
- Prefer zone-redundant deployment where the plan supports it (Flex Consumption does).
- On Flex Consumption, rehearse deployments and rollback against in-flight messages and downstream dependencies.

## Security

- Managed identity for Service Bus, Cosmos DB and storage (identity-based connections) instead of connection strings.
- Secrets that can't use identity (The Odds API key) live in Key Vault via Key Vault references; rotate them.
- Protect HTTP functions (`PredictionsHttpFunction`) with Microsoft Entra ID authentication or at least
  function keys — never anonymous without a reason.
- Network isolation (VNet integration, private endpoints) when the plan allows it — not available on the
  classic Consumption plan; on Flex Consumption reach Key Vault/App Configuration privately.

## Cost optimization

- Choose the plan for the load profile: Consumption/Flex Consumption for spiky or infrequent work (this app is
  timer-driven), Premium/Dedicated with reservations only for steady load.
- Keep executions short and dependencies few; every run and every Odds API call has a cost (API quota is
  logged by `QuotaLoggingHandler`).
- Timer intervals and retry counts are cost levers — change them consciously.
- Set Cost Management budgets and anomaly alerts.

## Operational excellence

- Deploy only through CI/CD (GitHub Actions in `.github/workflows`); no manual portal changes.
- Safe deployment: slots (Consumption/Premium/Dedicated) or rolling updates (Flex Consumption) with rollback.
- Health monitoring with Application Insights / Azure Monitor: alerts on failures, dead-letter count,
  missing timer runs; dashboards and runbooks for operators.
- Plan disaster recovery for critical functions (cross-region, availability zones).
- Infrastructure and settings documented in README "Configuration"; keep it in sync with code.

## Performance efficiency

- Define a baseline (execution time, failures, queue length, CPU/memory) and alert on regressions.
- Reduce cold-start impact where latency matters (always-ready / prewarmed instances, warmup trigger).
- Efficient code: reuse connections, minimize external calls, async I/O.
- Scaling rules are tested, not guessed; concurrency settings in `host.json` are tuned with measurements.

## Governance

- Azure Policy can audit: managed identity enabled, private endpoints only, diagnostic logging enabled.
- Review Azure Advisor recommendations for the function app periodically.

## Review checklist

- [ ] Change keeps functions idempotent and retry-safe
- [ ] No new secrets outside Key Vault; identity-based connections preferred
- [ ] HTTP endpoints authenticated
- [ ] Cost impact considered (plan, interval, retries, API calls)
- [ ] Deployed via CI with a rollback path
- [ ] Monitoring/alerts cover the new behavior
- [ ] README configuration updated if settings changed
