# Changelog

All notable changes to the Code Coverage package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-01-22

### Fixes

- Fixed the filtering and reporting of package's paths that were reported relative to the current project instead of absolute.
- Fixed a NullReferenceException that could occur while computing cyclomatic complexity.
- Fixed references to the deprecated EditorAnalytics methods for release streams from 6000.0 or newer (case [COV-44](https://issuetracker.unity3d.com/issues/sample-editoranalytics-warning-when-enabilg-code-coverage-in-the-sample-project))
- Fixed playmode tests failing in the _Asteroids sample project_ (case [COV-54](https://issuetracker.unity3d.com/issues/multiple-tests-are-failing-in-the-code-coverage-sample-asteroids-project))
- Fixed pink materials to be rendered for URP in _Asteroids sample project_
- Updated the Input System in _Asteroids sample project_ to use the new Input System package instead of the legacy one (case [COV-52](https://issuetracker.unity3d.com/issues/asteroids-sample-project-is-not-updated-for-the-new-unity-streams))
- Fixed _assembliesInclude_/_assembliesExclude_ parsing in `filtersFromFile` when running in the command line in `non-batchmode`

### Changes

- Raised the minimum supported Unity version for the Code Coverage package to 2021.3 and updated the codebase to align with this new dependency.
- Updated dependency packages versions: `com.unity.test-framework` to v1.4.5 and `com.unity.settings-manager` to v2.0.0.
- The entries in the `pathFilters` option in _-coverageOptions_ are now processed in order so a subset path can be included even if the parent path is excluded. Previously all exclusions were processed before any inclusions.
- Removed the `CoverageFormat` class, as there are currently no plans to support any other raw coverage formats other than OpenCover.

### Improvements

- Reduced the needed time to apply path filtering during the report generation.
- Migrated Mono APIs to CoreCLR-compatible APIs.
- Documentation: Added a note about the Input System package dependency in the [Code Coverage tutorial](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.3/manual/Quickstart.html) page.
- Documentation: Fixed broken links in the [Code Coverage tutorial](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.3/manual/Quickstart.html) page.

## [1.2.7] - 2025-09-12

### Changes

- Added new package signature.

## [1.2.6] - 2024-07-26

### Fixes

- Documentation: Fixed formatting in [Using Code Coverage in batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html) page (case [COV-40](https://issuetracker.unity3d.com/issues/docs-formatting-in-using-code-coverage-in-batchmode-docs-page-is-incorrect)).
- Removed the references to the deprecated FindObjectOfType method in the _Asteroids sample project_ (case [COV-42](https://issuetracker.unity3d.com/issues/sample-project-is-using-obsolete-findobjectoftype-method-which-causes-multiple-warnings-in-console-when-it-is-imported)).
- Added missing logs for the ReportGenerator (case [COV-46](https://issuetracker.unity3d.com/issues/code-coverage-package-does-not-report-some-of-the-internal-reportgenerator-errors)).

## [1.2.5] - 2023-12-20

### Fixes

- Fixed failing to generate code coverage for assemblies whose name starts with 'system' (case [COV-38](https://issuetracker.unity3d.com/issues/code-coverage-fails-to-generate-a-report-when-the-assembly-name-begins-with-the-word-system)).
- Fixed results xml file not been correctly stored under the EditMode folder if it is written after a domain reload (case [COV-36](https://issuetracker.unity3d.com/issues/coverage-results-xml-file-is-not-categorized-as-editmode-if-it-is-written-after-domain-reload)).

## [1.2.4] - 2023-06-02

### Fixes

- Fixed failing to gather code coverage for normal methods in generic classes (case [COV-27](https://issuetracker.unity3d.com/issues/non-generic-methods-in-generic-classes-always-show-no-coverage)).
- Documentation: Corrected _Settings.json_ path in `useProjectSettings` section in _-coverageOptions_ (case [COV-26](https://issuetracker.unity3d.com/issues/documentation-for-useprojectsettings-references-incorrect-settings-dot-json-path)).
- Make sure _Auto Generate Report_ defaults to true when running from the command line (case [COV-25](https://issuetracker.unity3d.com/issues/useprojectsettings-does-not-generate-report-until-auto-generate-report-is-toggled-off-and-on-again)).

## [1.2.3] - 2023-04-14

### Fixes

- Fixed failing to gather code coverage for generic methods (case [COV-17](https://issuetracker.unity3d.com/issues/coverage-package-fails-to-gather-any-coverage-for-generic-methods)).

### Improvements

- Added `filtersFromFile` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html). This allows specifying an external Json file which contains path and assembly filtering rules. When this file contains relative paths, the `sourcePaths` option can be used to specify the source directories.
  <br>**Note:** The `pathFiltersFromFile` option will be deprecated in the next package major release. Please use the new `filtersFromFile` option instead.
- Make sure `--burst-disable-compilation` is [expected](https://docs.unity3d.com/Packages/com.unity.burst@latest/index.html?subfolder=/manual/getting-started.html#command-line-options) to be passed with two dashes, unlike other editor command line options.

## [1.2.2] - 2022-11-18

### Fixes

- Temporary fix for the [Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html) 1.3 issue where the _RunFinished_ callback is not called when running from the command line and there is a domain reload (case [DSTR-692](https://issuetracker.unity3d.com/issues/registered-callbacks-dont-work-after-domain-reload)).

### Improvements

- Reduced the number of logs for the default _Verbosity:Info_.
- Added _Uncoverable lines_ definition in [How to interpret the results](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/HowToInterpretResults.html#summary) page in the documentation.
- Updated documentation to match version 1.2.2.

## [1.2.1] - 2022-10-27

### Fixes

- Fixed compatibility with [Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html) package version 1.3.

### Improvements

- A single file summary version of the report is now generated in Json format, in addition to the XML and Markdown formats.
- Added a warning when an invalid coverage option is passed in _-coverageOptions_ in [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html).

## [1.2.0] - 2022-08-01

### Fixes

- Ensure assemblies are removed from the Included Assemblies field if they no longer exist (case [1318668](https://issuetracker.unity3d.com/issues/code-coverage-the-included-assemblies-field-shows-assemblies-that-no-longer-exist)).
- Ensure hidden sequence points are ignored (case [1372305](https://issuetracker.unity3d.com/issues/class-which-derives-from-methodbase-causes-incorrect-sequence-points-to-be-generated-by-coverage-api)).

### Changes

- Updated Report Generator to version 5.0.4.
- Updated the UI of the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) moving the action buttons into a toolbar at the top.
- Renamed _assemblyFilters_ aliases in [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html); `<user>` was renamed to `<assets>` and `<project>` was renamed to `<all>`.
- Replaced `pathStrippingPatterns` with `pathReplacePatterns` in [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html). The `pathReplacePatterns` option allows stripping and replacing specific sections from the paths that are stored in the coverage results xml files.

See the [Upgrade guide](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/upgrade-guide.html) if upgrading to Code Coverage package version 1.2.

### Improvements

- The size of the coverage result files and the Code Coverage session duration have been optimized. At the start of the session a coverage xml result file is generated which includes all the lines but with zero coverage. The following coverage xml result files that are generated within a Code Coverage session include only the coverage data of the visited lines.
- Added Help IconButton in the toolbar in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html).
- Updated the mechanic for opening the containing folder, change the location or reset to the default location for _Results Location_ and _Report History Location_.
- Refactored the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) UI to include a new _Report Options_ section and removing the word 'Generate' from the options.
- Introduced new selection buttons under the _Included Assemblies_ dropdown in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html); use the _All_ button to select all the assemblies in the project. Use the _Assets_ button to select only the assemblies under the `Assets` folder. Use the _Packages_ button to select only the Packages' assemblies. If searching, the buttons will apply only to the assemblies visible in the list.
- Updated [What's new](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/whats-new.html) and [Upgrade guide](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/upgrade-guide.html) pages in the documentation.
- Added [Using relative paths in path filters](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html#using-relative-paths-in-path-filters) section in documentation.
- Updated the editor and console logs; added information about the assembly and path filters, improved coverage session logs.
- Improved the progress bars for `Writing coverage results` and `Generating the report`.
- Added an icon for the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html).
- Updated documentation to match version 1.2.0.

### Features

- Added `Pause Recording` and `Resume Recording` buttons in the toolbar in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html).
- Added `Log Verbosity Level` setting in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) which allows setting the verbosity level for the editor and console logs.
- Added `Additional Reports` option in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) which if checked [SonarQube](https://docs.sonarqube.org/latest/analysis/generic-test), [Cobertura](https://cobertura.github.io/cobertura) and [LCOV](https://github.com/linux-test-project/lcov) reports will be generated. Added `generateAdditionalReports` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html).
- Added `Test Runner References` report option in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) which if checked includes test references to the generated coverage results and enables the [Coverage by test methods](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/HowToInterpretResults.html#coverage-by-test-methods) section in the HTML report, allowing you to see how each test contributes to the overall coverage. Added `generateTestReferences` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html).
- Added `Auto Open Report` option in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html) which if checked the coverage report will open automatically after it has been generated.
- Added `pathFiltersFromFile` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html) which allows specifying an external file which contains a list of path filters. When this file contains relative paths, the `sourcePaths` option can be used to specify the source directories.
- Added `dontClear` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html) which allows coverage results to be accumulated after every code coverage session. If not passed the results are cleared before a new session. For more information see [Generate combined report from EditMode and PlayMode tests](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html#generate-combined-report-from-editmode-and-playmode-tests).
- When the `pathFilters` option or the `pathFiltersFromFile` option in _-coverageOptions_ contains relative paths, the `sourcePaths` option can be used to specify the source directories.

## [1.1.1] - 2021-12-17

### Fixes

- Ensure assemblies are removed from the Included Assemblies field if they no longer exist (case [1318668](https://issuetracker.unity3d.com/issues/code-coverage-the-included-assemblies-field-shows-assemblies-that-no-longer-exist))

### Changes

- Updated Report Generator to version 4.8.13

### Improvements

- Added Help IconButton in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CodeCoverageWindow.html) for Unity versions 2021.2.2f1 and above
- Added [What's new](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/whats-new.html) and [Upgrade guide](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/upgrade-guide.html) pages in the documentation
- Updated documentation to match version 1.1.1

## [1.1.0] - 2021-06-09

### Fixes

- Ensure Results and History folders are created if they do not exist (case [1334551](https://issuetracker.unity3d.com/issues/code-coverage-results-slash-history-location-path-is-reset-to-default-if-set-path-no-longer-exists))
- Added support for [ExcludeFromCoverage/ExcludeFromCodeCoverage](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/UsingCodeCoverage.html#excluding-code-from-code-coverage) for lambda expressions and yield statements (case [1338636](https://issuetracker.unity3d.com/issues/code-coverage-excludefromcoverage-attribute-doesnt-exclude-lambda-expressions-and-yield-statements-from-coverage))
- Added support for [ExcludeFromCodeCoverage](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/UsingCodeCoverage.html#excluding-code-from-code-coverage) for getter/setter properties (case [1338665](https://issuetracker.unity3d.com/issues/code-coverage-get-and-set-accessors-are-still-marked-as-coverable-when-property-has-excludefromcodecoverage-attribute))
- _-coverageOptions_ are only parsed when running from the command line ([feedback](https://forum.unity.com/threads/code-coverage-slowing-editor-on-enter-playmode-and-assembly-reload.1121566))

### Changes

- Updated Report Generator to version 4.8.9

### Improvements

- Implemented changes to support [Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html) package version 1.2
- Logs while the Report is generated are output per message rather than at the end of the generation
- Do not log burst warning when `--burst-disable-compilation` is passed in the command line
- Added [Ignoring tests for Code Coverage](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CoverageBatchmode.html#ignoring-tests-for-code-coverage) section in documentation
- Updated the [Generate combined report from separate projects](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CoverageBatchmode.html#generate-combined-report-from-separate-projects) section in documentation
- Updated documentation to match version 1.1.0

### Features

- Added `Code Coverage session Events` [API](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/api/UnityEditor.TestTools.CodeCoverage.Events.html) to subscribe to events invoked during a Code Coverage session
- Added `useProjectSettings` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CoverageBatchmode.html) which allows using the settings specified in `ProjectSettings/Settings.json`
- Added `pathStrippingPatterns` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CoverageBatchmode.html) which allows stripping specific sections from the paths that are stored in the coverage results xml files
- Added `sourcePaths` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.1/manual/CoverageBatchmode.html) which allows specifying the source directories which contain the corresponding source code

## [1.0.0] - 2021-03-09

### Fixes

- Fixed issues with Path Filtering (cases [1318896](https://issuetracker.unity3d.com/issues/code-coverage-typing-comma-into-the-included-or-excluded-paths-list-will-start-adding-row-for-each-letter-you-type-afterwards), [1318897](https://issuetracker.unity3d.com/issues/code-coverage-clearing-last-included-paths-row-immediatly-jumps-to-the-first-excluded-paths-row-and-starts-editing-it))

### Improvements

- Selection/focus is cleared when mouse is clicked outside of the individual settings' areas
- Added [Quickstart guide](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/Quickstart.html) in documentation
- Renamed the _Code Coverage Workshop_ sample to _Code Coverage Tutorial_
- Updated documentation and workshop to match version 1.0.0

**Note:** In Unity 2019 and 2020 you can enable Code Coverage in [General Preferences](https://docs.unity3d.com/Manual/Preferences.html). This was removed in Unity 2021; the user interface for managing Code Coverage is now entirely inside the Code Coverage package.

## [1.0.0-pre.4] - 2021-02-26

### Fixes

- Fixed assembly version validation error due to internal libraries included in the ReportGeneratorMerged.dll (case [1312121](https://issuetracker.unity3d.com/issues/code-coverage-reportgeneratormerged-cant-be-loaded-due-to-assembly-version-validation-failure))

### Changes

- Added _Enable Code Coverage_ checkbox under Settings in [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/CodeCoverageWindow.html).<br>**Note:** In Unity 2019 and 2020 you can enable Code Coverage in [General Preferences](https://docs.unity3d.com/Manual/Preferences.html). This was removed in Unity 2021; the user interface for managing Code Coverage is now entirely inside the Code Coverage package.
- The settings and options passed in the command line override/disable the settings in the Code Coverage window and relevant warnings display to indicate this
- Updated Report Generator to version 4.8.5
- Updated documentation and workshop to match version 1.0.0-pre.4

### Improvements

- Added `verbosity` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/CoverageBatchmode.html)
- Added _Generate combined report from separate projects_ section in documentation, under [Using Code Coverage in batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/CoverageBatchmode.html#generate-combined-report-from-separate-projects)

## [1.0.0-pre.3] - 2021-01-21

### Fixes

- Updated Include Platforms to Editor only in the ReportGeneratorMerged.dll settings. Fixes an Android build error introduced in 1.0.0-pre.2 (case 1306557)

## [1.0.0-pre.2] - 2021-01-13

### Fixes

- Fixed multiple reports generated in batchmode when passing `generateHtmlReport` in _-coverageOptions_ without passing `-runTests`

### Changes

- All project assemblies are included when there are included paths specified in _pathFilters_ but no included assemblies in _assemblyFilters_, when running in batchmode
- Updated Report Generator to version 4.8.4
- Updated documentation to match version 1.0.0-pre.2

### Improvements

- Introduced new _assemblyFilters_ aliases in [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.0/manual/CoverageBatchmode.html), used for referencing a group of assemblies to include or exclude. These are `<user>`, `<project>` and `<packages>`

## [1.0.0-pre.1] - 2020-11-12

- _1.0.0-pre.1_ matches _0.4.0-preview_

## [0.4.0-preview] - 2020-11-11

### Changes

- Moved Code Coverage window under _Window_ > _Analysis_
- _Included Assemblies_ now use a single dropdown instead of an editable text field which acted as a dropdown
- Added CommandLineParser and removed dependency to internals in Test Framework
- Removed the old _EditorPref_ workflow from CoveragePreferences
- Moved _Generate History_ outside of _Generate HTML Report_. It is now disabled only if both _Generate HTML Report_ and _Generate Badges_ are not selected
- Updated Report Generator to version 4.7.1
- Updated documentation and workshop to match version 0.4.0-preview

### Improvements

- Implemented `{ProjectPath}` alias in `Settings.json`
- Added a console warning when _Burst Compilation_ is enabled and an info HelpBox with a button to disable
- Added Analytics to help improve the user experience
- Disabled _Generate from Last_ button when there are no assemblies selected
- Display an info HelpBox when there are no assemblies selected
- Paths are now stored with forward slashes on Windows
- Added warning about Code Coverage not being supported currently when running PlayMode tests in standalone player
- Refactored code; in Utils, Filtering, ResultWriter, Window and API classes
- Added _CoverageWindow_ and _Filtering_ folders

### Features

- Added _Included Paths_ and _Excluded Paths_ as ReorderableLists in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.4/manual/CodeCoverageWindow.html)
- Added [support](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.4/manual/UsingCodeCoverage.html#excluding-code-from-code-coverage) for `ExcludeFromCoverage` and `ExcludeFromCodeCoverage` attributes
- Added `CodeCoverage.VerbosityLevel` [API](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.4/api/UnityEditor.TestTools.CodeCoverage.CodeCoverage.html) to set the verbosity level used in editor and console logs

## [0.3.1-preview] - 2020-08-03

### Fixes

- Fixed issue where CRAP calculation was incorrect when generic methods were parsed
- Corrected Six Labors License copyright in Third Party Notices

### Changes

- If `assemblyFilters` is not specified in _-coverageOptions_ in batchmode, include only the assemblies found under the _Assets_ folder
- Updated Report Generator to version 4.6.4

## [0.3.0-preview] - 2020-05-20

### Fixes

- Make sure _operator_ and _anonymous function_ names are generated correctly

### Changes

- Added _Generate Additional Metrics_ setting in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/CodeCoverageWindow.html) and removed _Cyclomatic Complexity_ (it is now included in Additional Metrics)
- Updated Report Generator to version 4.5.8
- Updated documentation to match version 0.3.0-preview

### Improvements

- Added [Code Coverage Workshop](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/CodeCoverageWorkshop.html) sample project
- Using the [Settings Manager](https://docs.unity3d.com/Manual/com.unity.settings-manager.html) package to handle the serialization of project settings
- Added an info HelpBox when _Code Optimization_ is set to Release mode with a button to switch to Debug mode
- Execute _Stop Recording_ on the update loop, instead of the OnGUI (removes an _EndLayoutGroup_ error)
- Refactored code; in OpenCoverReporter class (to reduce Cyclomatic Complexity), in CodeCoverageWindow class and others

### Features

- Added _History Location_ and _Generate History_ settings in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/CodeCoverageWindow.html)
- Added `coverageHistoryPath` and `generateHtmlReportHistory` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/UsingCodeCoverage.html#using-code-coverage-in-batchmode)
- Added `generateAdditionalMetrics` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/UsingCodeCoverage.html#using-code-coverage-in-batchmode) and removed _enableCyclomaticComplexity_ (it is now included in Additional Metrics)
- Added _Crap Score_ in Additional Metrics. See [How to interpret the results](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.3/manual/HowToInterpretResults.html).

## [0.2.3-preview] - 2020-02-18

### Fixes

- _Included Assemblies_ dropdown is now resizing to the longest assembly name ([1215600](https://issuetracker.unity3d.com/issues/there-is-no-way-to-view-the-full-name-of-an-assembly-when-selecting-it-in-a-small-code-coverage-window))
- When closing (selecting outside of) the _Included Assemblies_ dropdown, input is not accidentally propagated to the Code Coverage window

### Improvements

- If more than one instance of the _-coverageOptions_ command-line argument is specified, they will now be merged into a single instance
- If more than one instance of the _-coverageResultsPath_ command-line argument is specified, only the first instance will be accepted
- Added _Generate combined report from EditMode and PlayMode tests_ section in documentation, under [Using Code Coverage in batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.2/manual/UsingCodeCoverage.html#using-code-coverage-in-batchmode)

## [0.2.2-preview] - 2019-12-11

### Fixes

- Fixed unassigned _CodeCoverageWindow.m_IncludeWarnings_ warning in 2019.3

### Changes

- The default _Included Assemblies_ are now only the assemblies found under the project's _Assets_ folder, instead of all project assemblies

### Improvements

- After the report is generated, the file viewer window highlights the `index.htm` file, if _Generate HTML Report_ is selected

## [0.2.1-preview] - 2019-12-04

### Improvements

- Improved globbing for `pathFilters` and `assemblyFilters`
- Added new sections and examples in documentation
- Added confirmation dialogs when selecting _Clear Data_ and _Clear History_ buttons
- Added warning and button to switch to debug mode, when using Code Optimization in release mode in 2020.1 and above

### Features

- Added `pathFilters` in _-coverageOptions_ for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.2/manual/UsingCodeCoverage.html#using-code-coverage-in-batchmode)

## [0.2.0-preview] - 2019-11-13

### Fixes

- Make sure recording coverage results are saved in the _Recording_ folder, and starting a new recording session does not affect existing non-recording data

### Changes

- Updated Report Generator to version 4.3.6
- Split documentation into separate pages

### Improvements

- Updated UX design of the Code Coverage window
- Make sure settings and Record button are disabled when coverage is running
- Make sure coverage window is disabled before unity is restarted when _Enabling Code Coverage_ in Preferences
- Only parse xml files with the correct filename format when generating the report
- Implemented try/catch when deleting files/folders when selecting _Clear Data_ or _Clear History_
- Handle nested classes, nested generic classes and anonymous functions

### Features

- Exposed `CodeCoverage.StartRecording()`, `CodeCoverage.StopRecording()`, `CodeCoverage.PauseRecording()` and `CodeCoverage.UnpauseRecording()` [API](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.2/api/UnityEditor.TestTools.CodeCoverage.CodeCoverage.html)

## [0.1.0-preview.3] - 2019-09-27

### Improvements

- Passing `-coverageOptions generateHtmlReport` on the command line now creates a report if `-runTests` is not passed

## [0.1.0-preview.2] - 2019-09-23

### Changes

- Updated Report Generator to version 4.2.20

### Improvements

- Added support for correct naming of c# operators
- Added support for correct naming of constructors
- Added declaring type name as a prefix
- Added support for return types in method names

### Features

- Added [Coverage Recording](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@0.2/manual/CoverageRecording.html) feature

## [0.1.0-preview.0] - 2019-03-18

### This is the first release of _Code Coverage Package_
