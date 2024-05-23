# FinCrime Transactions Adapter

This repository is a microservice for Transactions Adapter.

## Purpose

This application receives alerts and send to Nice Actimize Xceed products to assign risk scores into ActOne for investigation.

## Datadog Dashboard

(Coming Soon)

### Monitors

(Coming Soon)

### Health Check

https://fc-transactions-adapter-default.{env}.q3.questech.io/healthz

## Team Contact Information

Slack Channel: #team-tmj

Alerts Channel: #fincrime-fraud-aml-alerts

Email group: questrade-scrumteam-tmj@questrade.com

### Running Unit Test

```
dotnet test src/Tests/Unit/Unit.csproj
```

### Running Integration Tests

```
dotnet test src/Tests/Integration/Integration.csproj
```

# More information

-   https://questrade.atlassian.net/wiki/spaces/FINCRIME/pages/321781884/FinCrime+Actimize+Xceed
