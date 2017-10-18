using NUnit.Framework.Interfaces;
using System;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Internal;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using System.IO;
using System.Configuration;

namespace Profiler
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TotalMethodCallsAssertionAttribute : TestAttribute, IWrapTestMethod, ICommandWrapper
    {
        public TotalMethodCallsAssertionAttribute(ulong maximum)
        {
            this.maxMethodCalls = maximum;
        }        

        public TestCommand Wrap(TestCommand command)
        {
            return new ProfilerCommand(command, this.maxMethodCalls, this.MinMethodCalls, this.OpenCoverConsolePath, this.NUnit3ConsolePath, this.KeepCoverageResults);
        }

        private readonly ulong maxMethodCalls;

        public ulong MinMethodCalls { get; set; }

        public string OpenCoverConsolePath { get; set; }

        public string NUnit3ConsolePath { get; set; }

        public bool KeepCoverageResults { get; set; }

        private class ProfilerCommand : DelegatingTestCommand
        {
            private const string OpenCoverConsolePathKey = "OpenCoverConsolePath";
            private const string NUnit3ConsolePathKey = "NUnit3ConsolePath";
            private const string RecursiveTestRunDetectionParameter = "COV";
            private readonly ulong maxMethodCalls;
            private readonly ulong minMethodCalls;
            private readonly string openCoverConsolePath;
            private readonly string nunit3ConsolePath;
            private readonly bool keepCoverageResults;

            public ProfilerCommand(TestCommand innerCommand, ulong maxMethodCalls, ulong minMethodCalls, string openCoverConsolePath, string nunit3ConsolePath, bool keepCoverageResults) : base(innerCommand)
            {
                if (maxMethodCalls == 0)
                {
                    throw new ArgumentException("maximum method calls count must be specified for TotalMethodCalls Assertion");
                }
                this.keepCoverageResults = keepCoverageResults;
                this.maxMethodCalls = maxMethodCalls;
                this.minMethodCalls = minMethodCalls;
                this.openCoverConsolePath = openCoverConsolePath;
                this.nunit3ConsolePath = nunit3ConsolePath;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var covParam = TestContext.Parameters.Get(RecursiveTestRunDetectionParameter);                

                if (string.IsNullOrEmpty(covParam))
                {
                    string testName = context.CurrentTest.FullName;
                    string assemblyName = Assembly.GetAssembly(context.TestObject.GetType()).Location;
                    string opencoverPath = this.openCoverConsolePath ?? ConfigurationManager.AppSettings[OpenCoverConsolePathKey] ?? @"C:\src\opencover\main\bin\Debug\OpenCover.Console.exe";
                    string nunitPath = this.nunit3ConsolePath ?? ConfigurationManager.AppSettings[NUnit3ConsolePathKey] ?? @"C:\src\opencover2\ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe";

                    context.CurrentResult.OutWriter.WriteLine("Using OpenCover.Console.exe path" + opencoverPath);
                    if (!File.Exists(opencoverPath) || !opencoverPath.EndsWith("OpenCover.Console.exe"))
                    {
                        context.CurrentResult.SetResult(ResultState.Failure, "Invalid OpenCover.Console.exe path");
                        return context.CurrentResult;
                    }

                    context.CurrentResult.OutWriter.WriteLine("Using nunit3-console.exe console path " + nunitPath);
                    if (!File.Exists(nunitPath) || !nunitPath.EndsWith("nunit3-console.exe"))
                    {
                        context.CurrentResult.SetResult(ResultState.Failure, "Invalid nunit3-console.exe path");
                        return context.CurrentResult;
                    }

                    context.CurrentResult.OutWriter.WriteLine("To change these paths, use app.config for your test assembly or via attribute parameter");

                    string nunitOutputFile = Path.GetTempFileName();
                    string openCoverOutputFile = Path.GetTempFileName();

                    string arguments = PrepareOpenCoverCall(testName, assemblyName, nunitPath, nunitOutputFile, openCoverOutputFile);

                    // OpenCover invocation
                    ProcessStartInfo psi = new ProcessStartInfo(opencoverPath, arguments)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    var openCoverProcess = Process.Start(psi);
                    string openCoverStdOut = openCoverProcess.StandardOutput.ReadToEnd();                    
                    openCoverProcess.WaitForExit();

                    string tempOpenCoverStdOutPath = Path.GetTempFileName();
                    File.WriteAllText(tempOpenCoverStdOutPath, openCoverStdOut);

                    double total = CountMethodCalls(openCoverOutputFile);

                    if (!this.keepCoverageResults)
                    {
                        File.Delete(nunitOutputFile);
                        File.Delete(openCoverOutputFile);
                        File.Delete(tempOpenCoverStdOutPath);
                    }

                    context.CurrentResult.OutWriter.WriteLine("Counted method calls: " + total);
                    context.CurrentResult.OutWriter.WriteLine("OpenCover results written to " + openCoverOutputFile);

                    // setting assertion result, if ok then continue executing test
                    if (total == 0)
                    {
                        context.CurrentResult.SetResult(ResultState.Inconclusive, string.Format("Unable to process temp. results from {0} or call count was 0", openCoverOutputFile));
                    }
                    else if (total > maxMethodCalls)
                    {
                        context.CurrentResult.SetResult(ResultState.Failure, string.Format("Expected max {0} function calls but counted {1}", maxMethodCalls, total));
                    }
                    else if (total <= minMethodCalls)
                    {
                        context.CurrentResult.SetResult(ResultState.Failure, string.Format("Expected at least {0} function calls but counted {1}", minMethodCalls, total));
                    }
                    else
                    {
                        context.CurrentResult.SetResult(ResultState.Success);
                    }
                }
                else
                {
                    context.CurrentResult = innerCommand.Execute(context);
                }
                return context.CurrentResult;
            }

            private static string PrepareOpenCoverCall(string testName, string assemblyName, string nunitPath, string nunitOutputFile, string openCoverOutputFile)
            {
                // OpenCover call configuration
                StringBuilder arguments = new StringBuilder();
                arguments.Append(" -target:\"").Append(nunitPath).Append("\"");
                arguments.Append(" -register:user -filter:\"+[*]* -[Profiler*]* -[NUnit*]*\" -output:\"").Append(openCoverOutputFile).Append("\"");
                arguments.Append(" -targetargs:\"").Append(assemblyName).Append(" --params=").Append(RecursiveTestRunDetectionParameter).Append("=TRUE").Append(" --result=").Append(nunitOutputFile).Append(" --test=").Append(testName).Append("\"");
                return arguments.ToString();
            }

            // parsing OpenCover output
            private static double CountMethodCalls(string openCoverOutputFile)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openCoverOutputFile);

                XPathDocument document = new XPathDocument(new XmlNodeReader(doc));
                XPathNavigator navigator = document.CreateNavigator();
                XPathExpression query = navigator.Compile("sum(//MethodPoint/@vc)");

                double total = (double)navigator.Evaluate(query);
                return total;
            }
        }
    }
}
