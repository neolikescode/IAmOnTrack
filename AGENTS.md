# Development Instructions

## Project overview

- This is a C# Windows Forms application targeting .NET Framework 4.8. Preserve compatibility with the existing project format and target framework.
- The active application entry point starts `DashBoardFormShow`. `DashBoardForm` is also present as a separate, older dashboard implementation; check which path is active before changing dashboard behavior.
- Core academic data is held in memory by `CMarkTracker`, which owns `CModules`; each module owns `CAssessments`. `CActivityLog` and `CDashboardStatistics` provide activity and derived dashboard information.
- The application currently has no general-purpose load/save persistence. Excel export is an output feature, not the application's persistence layer.

## Architecture and backend/UI separation

- Keep business rules and calculations in backend/model classes such as `CMarkTracker`, `CModules`, `CAssessments`, `CAcademicRecords`, and `CDashboardStatistics`.
- Keep forms responsible for collecting input, calling backend operations, and presenting results or errors. Do not put validation, academic calculations, or data mutation rules in UI event handlers when they belong in the backend.
- Reuse the shared `CMarkTracker` and `CActivityLog` instances when opening child forms so changes are visible throughout the current application session. Be aware that parameterless form constructors create fresh in-memory instances.
- Prefer existing services and custom controls in `UI/` over duplicating behavior. Avoid adding frameworks or dependencies unless a concrete requirement calls for them.
- Preserve working behavior and make focused changes. Do not rewrite classes for style alone.

## Validation and exception handling

- Enforce domain invariants in model or business-logic methods, not only in the form. Existing examples include module identity, valid semester, assessment weights, and marks within the maximum.
- Follow the existing exception-based validation approach: throw clear `ArgumentException`, `ArgumentOutOfRangeException`, `FormatException`, or `InvalidOperationException` errors from backend operations as appropriate.
- Catch expected validation and operational exceptions at the UI boundary and show a concise, actionable message. Avoid swallowing exceptions or showing UI dialogs from backend classes.
- Keep validation consistent across all entry points. Do not assume that UI validation makes backend validation unnecessary.

## Excel export

- Preserve the existing ClosedXML-based implementation in `CFileManager`; do not replace it with another Excel library or a different export mechanism unless explicitly required.
- Keep workbook construction and file operations in `CFileManager`, separate from form presentation. Keep export calls routed through the existing records/business-logic flow.
- Maintain the current workbook structure and safe temporary-file/replace behavior when making necessary export changes. Report any changes to output paths, worksheet naming, or workbook layout.

## Windows Forms and designer files

- Avoid editing `*.Designer.cs` or resource files unless a change cannot reasonably be made elsewhere. Prefer existing code-behind and programmatic layout files for behavior or layout updates.
- Preserve event wiring and shared UI conventions when changing a form. Check both the startup dashboard and Records navigation paths where relevant.

## Coding standards

- Follow the surrounding C# style and the project's .NET Framework 4.8 constraints. Use established naming conventions: PascalCase for public types and members, and the existing private-field convention for fields.
- Keep methods focused and use the existing collection and model APIs. Preserve read-only collection exposure and avoid bypassing model invariants.
- Use clear, specific exception messages and avoid introducing unrelated cleanup or formatting churn.

## Testing and build verification

- For changes, identify the affected behavior and use the narrowest relevant verification available; include a build check when the environment has the required .NET Framework/Visual Studio build tools.
- This is a classic .NET Framework WinForms project. A `dotnet build` failure in an environment without the appropriate .NET Framework reference assemblies or MSBuild task host does not by itself establish a source-code compile error; report the toolchain limitation and use the intended Windows/Visual Studio toolchain when available.
- Exercise relevant form flows and validation boundaries when practical, including shared data across dashboard/dialog navigation and Excel export behavior when those areas change.
- Do not add a testing framework or dependencies solely for convenience. Keep tests focused on domain rules and calculations when a test project or suitable test harness exists.

## Development workflow

- Explain the purpose of a proposed change before implementing significant features.
- Prefer incremental implementation over large, unrelated changes.
- Explain trade-offs before making architectural decisions.
- When asked to teach or guide the developer, provide step-by-step instructions instead of immediately writing the complete solution.
- For substantial changes, summarize modified files, implementation details, and testing results.

## Git and change management

- Do not commit, push, reset, rebase, or discard changes without explicit permission.
- Inspect the working tree before making changes.
- Preserve existing uncommitted user changes.
- Keep changes focused on the requested task.
