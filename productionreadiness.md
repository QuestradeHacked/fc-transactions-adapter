# Production readiness checklist

This checklist is to be used to gauge if a microservice is considered to be ready for production release. It is an opinionated collection of things we wanna embrace in Questrade engineering culture that should allow us to ship services not only in a safer way but also faster.

The checklist is meant for the engineering teams that are responsible for creating the application. There are other aspects that might also covered before production release (eg. pen testing) but these are the things that the developing team is directly responsible for.

## Engineering practice

-   [x] Application repository has to have a README.md
    -   [x] Readme should describe purpose of the service
    -   [ ] Readme should describe usage of datastores
    -   [ ] Readme should describe any other resiliency patterns/degradation features
    -   [x] Anything else that is critical to understanding the service (eg. external partner reliance, caching mechanisms)
    -   [x] Readme should contain diagram of service and dependencies
    -   [x] Teams Slack Channel Link
    -   [x] Teams Email group
-   [ ] Exposed APIs must be published on [API portal](https://app-backstage.uat.questrade.com/api-docs).
-   [x] Code is reviewed by another engineer
    -   [x] Usually another team member but enterprise architects can also be engaged for this
    -   [x] Code reviews done on a MR basis. Smaller the better
-   [x] Project must implement unit tests
    -   [x] Quality/coverage of unit tests should be part of code review (per feature/MR)
    -   Coverage % should be defined by each team, for each project
-   [x] Project must be using supported languages and libraries
    -   [x] Nothing should be EOL at time of shipping
    -   [x] Nothing should be in preview or early access stages at time of shipping
-   [x] All secrets in secure storage. All non-secret environment variables must be committed to Git.
    -   Details [here](https://app-backstage.uat.questrade.com/docs/default/resource/cloud-native-application-platform-docs/pass-config-references/google-secrets)

## Operational Readiness

-   [ ] Service level indicators (SLI) and Service level objectives (SLO) need to be identified
    -   [ ] SLOs should be set by the business with the end user experience being the focus
    -   [ ] Start with something realistic and simple (eg. closer to 3 than 10 SLOs, easily identifiable things)
    -   [ ] Production support team can help with this and see this [doc](https://landing.google.com/sre/sre-book/chapters/service-level-objectives/) for basics
-   [x] Application must have been deployed successfully to SIT before moving to higher environments
-   [x] Idempotent for restarts (a service can be killed and started multiple times)
-   [x] Idempotent for scaling up/down (a service can be autoscaled to multiple instances)
-   [x] Application should not have any background job running in the same process as the normal runtime (eg: timed cache clean or DB record purge)
    -   Exception: Jobs like message subscribers, which should also gracefully handle and understand app healthiness state and lifecycle
-   [x] Capacity planning must be done before moving to production
    -   See [docs](https://app-backstage.uat.questrade.com/docs/default/resource/cloud-native-application-platform-docs/PaaS-Config-References)
-   [ ] Alerts must be created (by the developing team) for production incidents. Examples:
    -   [ ] Service is down
    -   [ ] Amount of 5xx s is high
    -   [ ] Service is throwing exceptions

## Observability

-   [ ] Service needs to have a dashboard in Datadog ([this dashboard template](https://app.datadoghq.com/dashboard/5j4-v2u-mwc) is not mandatory to follow but might help you get started - remember to configure for all environments!)
    -   [ ] Showing health of the service (error rates, response times, usage)
    -   [ ] Showing resource usage (CPU/MEM metrics etc)
    -   [ ] Datastore (Redis, MySQL etc.) usage
-   Tracing and/or custom metrics are not a must-have currently, but to have good SLIs (and dashboard) most likely they need to be implemented for those purposes
    -   See [docs](https://docs.google.com/document/d/1IhxFN2zo3wmYv1dHfSEP1ZSLCKmHKOdVcOUka2TIf8k/edit#)

## Logging

-   [x] Must log in JSON format and have at least the following as fields:
    -   [x] Level (eg. Fatal, Error, Warning, Information, Debug, Verbose)
    -   [x] Message
    -   [x] Timestamp
-   [x] LogLevel must tunable via environment variable
-   [x] Logs must exclude PII/PCI data
    -   [x] Passwords, names, phone numbers, emails, PAN numbers, CVV and user logins
    -   See [data dictionary](https://questrade.collibra.com/domain/ff251c03-a2d5-42f7-b655-6750012aced1?view=7e197f26-27ce-4691-be6b-e00fb666c438)
-   [x] Must have ability to log every request
    -   But should not have that enabled with Information or higher loglevels as Istio does raw request logging
-   [x] All errors/exceptions must be caught and logged
-   [x] Logs must follow [best practices](https://docs.google.com/document/d/1IhxFN2zo3wmYv1dHfSEP1ZSLCKmHKOdVcOUka2TIf8k/edit#heading=h.ewbkjuohju4w) and formatting standards
-   [x] Team must have reviewed logging output in SIT to make sure its at the correct level
    -   [x] Logging enough so that errors are caught
    -   [x] Logging only things that have value
    -   [x] Logging in a structure that is easy for both creating alerts and for humans to understand

## Application Security

-   [x] Application must have been scanned via SonarQube and must have the SonarQube Dashboard link for the app available in the readme file. Any major findings must have been fixed or determined to be false positives.
-   [x] Application must have been scanned via relevant DAST tool (InsighAppSec for NowSecure) and must have the DAST tool link for the app available in the readme file. Any major findings must have been fixed or determined to be false positives.
-   [x] Application have been Pen Tested to all of its public endpoints with no major findings or with ETAs provided for each finding according to the current security posture. To find if a Pen Test is necessary, please reach out to Cybersecurity.
