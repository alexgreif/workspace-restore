workspace "Workspace App (V1) - High-Level Modules" "C4 component view of module-level relationships." {

  model {
    user = person "User" "Uses the workspace management UI."

    app = softwareSystem "Workspace App" "Windows desktop app for clean workspace restoration." {
      desktop = container "Desktop App" ".NET 8 / C# / WPF desktop application" "Implements V1 layered modules." {
        ui = component "UI" "Layer 5" "Presentation and user actions."
        orchestrator = component "AppOrchestrator" "Layer 4" "Application facade and operation coordination."
        engine = component "WorkspaceEngine" "Layer 3" "Capture, recapture, and restore workflows."
        persistence = component "Persistence" "Layer 2" "Workspace repository and schema migration."
        domain = component "Domain" "Layer 1" "Core models and operation result types."
        winapi = component "WinApi" "Layer 0" "Win32 adapters for windows, process, and monitor ops."
      }
    }

    user -> ui "Interacts with"
    ui -> orchestrator "Invokes application facade"
    orchestrator -> engine "Coordinates capture/recapture/restore"
    orchestrator -> persistence "Lists and deletes workspaces"

    engine -> persistence "Loads and stores workspaces"
    engine -> domain "Builds and returns models/results"
    engine -> winapi "Enumerates/closes/launches/repositions windows"

    persistence -> domain "Serializes/deserializes workspace models"
  }

  views {
    component desktop "overview-modules" {
      include *
      autolayout lr
    }

    styles {
      element "Person" {
        shape Person
        background #0b6fa4
        color #ffffff
      }
      element "Component" {
        shape Box
        background #d8ebff
        color #1b1f23
      }
    }
  }
}

