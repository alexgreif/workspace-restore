# Backlog --- External Workspace Trigger API (Post-V1)

## Title

Optional Local HTTP Trigger API for Workspace Restore

## Status

Backlog (Post-V1 / V1.5 or V2)

## Motivation

Enable external tools (e.g., Notion, task managers, automation systems,
scripts) to trigger workspace restores via HTTP requests.

This supports: - Deep linking from productivity tools - Integration into
existing workflows - Automation scenarios - URL-based workspace
invocation

The feature strengthens restoration and enforcement by making context
switching frictionless.

------------------------------------------------------------------------

## High-Level Concept

Expose an optional, local-only HTTP API that allows restoring workspaces
via REST calls.

Example:

POST http://localhost:`<port>`{=html}/restore/{workspaceId}

The API:

-   Runs on localhost only (127.0.0.1)
-   Is disabled by default
-   Forwards requests to AppOrchestrator → RestoreService
-   Returns structured OperationResult response

------------------------------------------------------------------------

## Security Requirements

-   Bind only to 127.0.0.1 (never 0.0.0.0)
-   No unauthenticated destructive GET endpoints
-   Use POST for restore actions
-   Support optional token-based authorization
-   Consider CSRF mitigation (header-based token validation)

------------------------------------------------------------------------

## Architecture Placement

Introduce a new adapter layer:

LocalHttpApiAdapter

This layer: - Depends on AppOrchestrator - Does not contain business
logic - Does not bypass WorkspaceEngine - Acts purely as a trigger
interface

Architecture remains:

UI ─┐ ├── AppOrchestrator ── WorkspaceEngine ── Persistence ── Domain
HTTP ─┘ │ └── WinApi

------------------------------------------------------------------------

## Explicit Non-Goals (Post-V1)

-   No browser-based primary UI
-   No full web application conversion
-   No multi-user server mode
-   No remote network access

The product remains a Windows-native desktop application.

------------------------------------------------------------------------

## Future Extensions

-   GET endpoint returning list of workspaces
-   Optional confirmation page served via local web UI
-   Custom URL scheme support (if platform allows)
-   Native integrations (e.g., Notion plugin, browser extension)

------------------------------------------------------------------------

## Rationale

This feature: - Expands integration capabilities - Preserves
Windows-first architecture - Keeps OS boundary intact - Avoids
unnecessary web-app complexity
