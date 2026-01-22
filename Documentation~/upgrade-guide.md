# Upgrading to Code Coverage package version 1.3

To upgrade to Code Coverage package version 1.3, you need to do the following:

- [Update path filtering order in batchmode](upgrade-guide.md#update-path-filtering-order-in-batchmode)

**Note**: If you're upgrading from a version older than 1.2, follow the [upgrade guide for version 1.2](upgrade-guide.md#upgrading-to-code-coverage-package-version-12) first.

## Update path filtering order in batchmode

- In version 1.3 the entries in the `pathFilters` [batchmode](CoverageBatchmode.md) command line option are processed in order, so a subset path can be included even if the parent path is excluded. Previously all exclusions were processed before any inclusions. Therefore, you should inspect the entries passed in the `pathFilters` option and update based on the new logic.<br><br>**Example:**<br><br>`pathFilters:+**/Packages/myPackage/**,-**/Packages/**` will include just the files that have `/Packages/myPackage/` in their path and exclude all other files that have `/Packages/` in their path, where before version 1.3 all files that had `/Packages/` in their path were excluded.

# Upgrading to Code Coverage package version 1.2

To upgrade to Code Coverage package version 1.2, you need to do the following:

- [Update assembly filtering aliases in batchmode](upgrade-guide.md#update-assembly-filtering-aliases-in-batchmode)
- [Rename `pathStrippingPatterns` to `pathReplacePatterns` in batchmode](upgrade-guide.md#rename-pathstrippingpatterns-to-pathreplacepatterns-in-batchmode)

**Note**: If you're upgrading from a version older than 1.1, follow the [upgrade guide for version 1.1](upgrade-guide.md#upgrading-to-code-coverage-package-version-11) first.

## Update assembly filtering aliases in batchmode

- Rename assembly filtering aliases when running in [batchmode](CoverageBatchmode.md). `<user>` alias was renamed to `<assets>` and `<project>` was renamed to `<all>`.

## Rename `pathStrippingPatterns` to `pathReplacePatterns` in batchmode

- Rename `pathStrippingPatterns` to `pathReplacePatterns` in [batchmode](CoverageBatchmode.md).<br><br>**Example:**<br><br>Change `pathStrippingPatterns:C:/MyProject/` to `pathReplacePatterns:C:/MyProject/,`.<br>This is equivalent to stripping `C:/MyProject/` by replacing `C:/MyProject/` with an empty string.

# Upgrading to Code Coverage package version 1.1

To upgrade to Code Coverage package version 1.1, you need to do the following:

- [Update path filtering globbing rules](upgrade-guide.md#update-path-filtering-globbing-rules)

## Update path filtering globbing rules

- Update the path filtering globbing rules in your batchmode commands and code coverage window. To keep the current behavior when using [globbing](https://en.wikipedia.org/wiki/Glob_%28programming%29) to match any number of folders, the `*` character should be replaced with `**`. A single `*` character can be used to specify a single folder layer.<br><br>**Examples:**<br><br>`pathFilters:+C:/MyProject/Assets/Scripts/*` will include all files in the `C:/MyProject/Assets/Scripts` folder. Files in subfolders will not be included.<br>`pathFilters:+C:/MyProject/Assets/Scripts/**` will include all files under the `C:/MyProject/Assets/Scripts` folder and any of its subfolders.

For a full list of changes and updates in this version, see the [Code Coverage package changelog](../changelog/CHANGELOG.html).
