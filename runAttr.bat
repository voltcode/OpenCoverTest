echo %time%
ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe Test\bin\Debug\Test.dll --shadowcopy
echo %time%

REM ClassLibrary1\packages\ReportGenerator.3.0.1\tools\ReportGenerator.exe -reports:output3.xml -targetdir:coveragereport -reporttypes:"Html;HtmlSummary;CsvSummary;Badges;PngChart;TextSummary"