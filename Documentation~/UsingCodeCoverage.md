# Using Code Coverage

## Enable Code Coverage

1. Go to **Edit** > **Preferences** > **General** and check **Enable Code Coverage**.<br/><br/>
![Enable Code Coverage](images/enable_coverage.png)

2. Restart Unity.

## Using Code Coverage in batchmode

There are 3 arguments that can be passed in batchmode:

**-enableCodeCoverage**, to enable code coverage.  
**-coverageResultsPath** (_optional_), to set the location where the coverage results and report will be saved to. The default location is the project's path.  
**-coverageOptions** (_optional_), to pass extra options. This is semicolon separated.   

|Coverage Option|Description|
|:---|:---|
|`enableCyclomaticComplexity`|Add this to enable the Cyclomatic Complexity calculation for each method. See the [Risk Hotspots](#risk-hotspots) section for more information about Cyclomatic Complexity.|
|`generateHtmlReport`|Add this to generate a coverage HTML report.|
|`generateBadgeReport`|Add this to generate a coverage summary badge.|
|`assemblyFilters`|Add this to specify the assemblies that should be included or excluded in the coverage calculation and/or report. This is a comma separated string. Prefix assemblies with `+` to include them and with `-` to exclude them. Globbing can be used to filter the assemblies. Example: `assemblyFilters:+UnityEngine.*,-UnityEditor.*`|

### Example

```
Unity.exe -projectPath <path-to-project> -batchmode -testPlatform editmode -runTests -testResults <path-to-results-xml> -enableCodeCoverage -coverageResultsPath <path-to-coverage-results> -coverageOptions enableCyclomaticComplexity;generateHtmlReport;generateBadgeReport;assemblyFilters:+UnityEngine.*,-UnityEditor.*
```

## Using Code Coverage with Burst compiler

If you use the Burst package and have jobs compiled with Burst, you will need to disable Burst compilation in order to get full coverage. To do this uncheck **Enable Compilation** under **Jobs** > **Burst** > **Enable Compilation** or pass the `--burst-disable-compilation` option to the command line.

## Using Code Coverage with Code Optimization

Code Optimization was introduced in 2020.1. Code Optimization mode defines whether Unity Editor compiles scripts in Debug or Release mode. Debug mode enables C# debugging and it is required in order to obtain accurate code coverage. To ensure Code optimization is set to Debug mode you can do one of the following:

- Switch to Debug mode in the Editor (bottom right corner, select the **Bug icon** > **Switch to debug mode**)
- Using the CompilationPipeline api, set `CompilationPipeline.codeOptimization = CodeOptimization.Debug`
- Pass `-debugCodeOptimization` to the command line