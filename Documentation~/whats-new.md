# What's new in version 1.2

Summary of changes in Code Coverage package version 1.2

The main updates in this release include:

## Added

- Added `Test Runner References` coverage report option in the [Code Coverage window](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CodeCoverageWindow.html). When you check this option, the generated coverage data includes references to the triggering tests and allows per-test coverage in the report. Note that this option only affects Test Runner Coverage sessions. In [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html), you can generate test references by adding the `generateTestReferences` option in *-coverageOptions*.
- Added `pathFiltersFromFile` in *-coverageOptions* for [batchmode](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html). This allows you to specify an external file which contains a list of path filtering rules.
- When the `pathFilters` option in *-coverageOptions* contains relative paths, the `sourcePaths` option can be used to specify the source directories.

For a full list of changes and updates in this version, see the [Code Coverage package changelog](../changelog/CHANGELOG.html).
