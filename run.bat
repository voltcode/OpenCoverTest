ClassLibrary1\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:"ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe" -targetargs:"Test\bin\Debug\Test.dll"  -register:user -filter:+[*]* -output:output3.xml

ClassLibrary1\packages\ReportGenerator.3.0.1\tools\ReportGenerator.exe -reports:output3.xml -targetdir:coveragereport