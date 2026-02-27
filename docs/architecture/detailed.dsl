workspace "Workspace App (V1) - Detailed" "Detailed C4 view with module interfaces and key dependencies." {

  model {
    user = person "User" "Manages workspaces via the desktop UI."
    windowsOs = softwareSystem "Windows OS" "Provides Win32 windowing, monitor, and process APIs."

    workspaceApp = softwareSystem "Workspace App" "Restores predefined clean application contexts on Windows." {
      desktopApp = container "Desktop App" ".NET 8 / C# / WPF Windows desktop application" "Implements V1 capture, recapture, and restore workflows." {

        ui = component "UI" "Layer 5" "Workspace list and action controls; shows progress and errors."

        appController = component "IWorkspaceAppController" "Layer 4" "Facade: list/capture/recapture/restore/delete operations."
        progressSink = component "IProgressSink" "Layer 4" "Progress callback sink for operation events."

        captureService = component "ICaptureService" "Layer 3" "Create workspace from current visible window state."
        recaptureService = component "IRecaptureService" "Layer 3" "Overwrite existing workspace from current visible window state."
        restoreService = component "IRestoreService" "Layer 3" "Restore flow: global close -> launch -> layout -> z-order replay."

        workspaceRepository = component "IWorkspaceRepository" "Layer 2" "List/get/create/update/delete workspace JSON documents."
        schemaMigrator = component "ISchemaMigrator" "Layer 2" "Migrates persisted workspace schema to latest version."

        domainWorkspace = component "Domain: Workspace/ApplicationEntry/WindowLayout" "Layer 1" "Core persisted workspace model."
        domainResult = component "Domain: OperationResult/Event/Error" "Layer 1" "Operation status, events, and errors for UX/logging."

        windowEnumerator = component "IWindowEnumerator" "Layer 0" "Enumerates included top-level windows."
        windowCloser = component "IWindowCloser" "Layer 0" "Sends graceful WM_CLOSE requests."
        processLauncher = component "IProcessLauncher" "Layer 0" "Launches executables by path."
        windowMover = component "IWindowMover" "Layer 0" "Sets bounds, minimizes/restores, activates for z-order replay."
        monitorService = component "IMonitorService" "Layer 0" "Off-screen detection and clamping against visible work areas."
      }
    }

    user -> workspaceApp "Uses"
    user -> ui "Interacts with"

    ui -> appController "Invokes user actions"

    appController -> captureService "Coordinates capture"
    appController -> recaptureService "Coordinates recapture"
    appController -> restoreService "Coordinates restore + cancellation"
    appController -> workspaceRepository "Lists and deletes workspaces"

    restoreService -> progressSink "Reports operation progress"

    captureService -> windowEnumerator "Reads current visible windows"
    captureService -> domainWorkspace "Builds workspace entries/layouts"
    captureService -> workspaceRepository "Creates workspace"

    recaptureService -> windowEnumerator "Reads current visible windows"
    recaptureService -> domainWorkspace "Overwrites entries/layouts"
    recaptureService -> workspaceRepository "Updates workspace"

    restoreService -> workspaceRepository "Loads workspace by id"
    restoreService -> schemaMigrator "Migrates schema to latest"
    restoreService -> windowEnumerator "Enumerates windows for global close/matching"
    restoreService -> windowCloser "Requests graceful close"
    restoreService -> processLauncher "Launches application entries"
    restoreService -> windowMover "Applies bounds/minimize/activate"
    restoreService -> monitorService "Clamps off-screen layouts"
    restoreService -> domainResult "Emits operation result/events/errors"

    workspaceRepository -> domainWorkspace "Serializes/deserializes workspace model"

    windowEnumerator -> windowsOs "Uses Win32 enumeration APIs"
    windowCloser -> windowsOs "Uses WM_CLOSE/window APIs"
    processLauncher -> windowsOs "Uses process creation APIs"
    windowMover -> windowsOs "Uses window placement/z-order APIs"
    monitorService -> windowsOs "Uses monitor/work area APIs"
  }

  views {
    systemContext workspaceApp "detailed-c1" {
      include *
      autolayout lr
    }

    container workspaceApp "detailed-c2" {
      include *
      autolayout lr
    }

    component desktopApp "detailed-c3" {
      include *
      autolayout lr
    }

    styles {
      element "Person" {
        shape Person
        background #0b6fa4
        color #ffffff
      }

      element "Software System" {
        shape RoundedBox
        background #2f5d7c
        color #ffffff
      }

      element "Container" {
        shape RoundedBox
        background #4a7fa7
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
