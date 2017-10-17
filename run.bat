echo %time%
C:\src\opencover\main\bin\Debug\OpenCover.Console.exe -target:"ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe" -showvisited -targetargs:"Test\bin\Debug\Test.dll"  -register:user -filter:+[*]* -output:output3.xml
echo %time%

REM ClassLibrary1\packages\ReportGenerator.3.0.1\tools\ReportGenerator.exe -reports:output3.xml -targetdir:coveragereport -reporttypes:"Html;HtmlSummary;CsvSummary;Badges;PngChart;TextSummary"