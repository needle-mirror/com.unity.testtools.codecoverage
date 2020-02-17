# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.3-preview] - 2020-02-18
- If more than one instance of the *-coverageOptions* command-line argument is specified, they will now be merged into a single instance
- If more than one instance of the *-coverageResultsPath* command-line argument is specified, only the first instance will be accepted
- Added *Generate combined report from EditMode and PlayMode tests* section in documentation, under *Using Code Coverage in batchmode*
- When closing (selecting outside of) the *Included Assemblies* dropdown, input is not accidentally propagated to the Code Coverage window
- *Included Assemblies* dropdown is now resizing to the longest assembly name (case 1215600)

## [0.2.2-preview] - 2019-12-11
- The default *Included Assemblies* are now only the assemblies found under the project's *Assets* folder, instead of all project assemblies
- After the report is generated, the file viewer window highlights the `index.htm` file, if *Generate HTML Report* is selected
- Fixed unassigned *CodeCoverageWindow.m_IncludeWarnings* warning in 2019.3

## [0.2.1-preview] - 2019-12-04
- Added `pathFilters` batchmode *coverageOption*
- Improved globbing for `pathFilters` and `assemblyFilters`
- Added new sections and examples in documentation
- Added warning and button to switch to debug mode, when using Code Optimization in release mode in 2020.1 and above
- Added confirmation dialogs when selecting *Clear Data* and *Clear History* buttons

## [0.2.0-preview] - 2019-11-13
- Updated Report Generator to version v4.3.6
- Split documentation into separate pages
- Updated UX design of the Code Coverage window
- Exposed `CodeCoverage.StartRecording()`, `CodeCoverage.StopRecording()`, `CodeCoverage.PauseRecording()` and `CodeCoverage.UnpauseRecording()` API
- Make sure settings and Record button are disabled when coverage is running
- Make sure coverage window is disabled before unity is restarted when *Enabling Code Coverage* in Preferences
- Only parse xml files with the correct filename format when generating the report
- Make sure recording coverage results are saved in the *Recording* folder, and starting a new recording session does not affect existing non-recording data
- Implemented try/catch when deleting files/folders when selecting *Clear Data* or *Clear History*
- Handle nested classes, nested generic classes and anonymous functions

## [0.1.0-preview.3] - 2019-09-27
- Passing `-coverageOptions generateHtmlReport` on the command line now creates a report if `-runTests` is not passed

## [0.1.0-preview.2] - 2019-09-23
- Updated Report Generator to version v4.2.20
- Added *Coverage Recording* feature
- Added support for correct naming of c# operators
- Added support for correct naming of constructors
- Added declaring type name as a prefix
- Added support for return types in method names

## [0.1.0-preview.0] - 2019-03-18

### This is the first release of *Code Coverage Package*.
