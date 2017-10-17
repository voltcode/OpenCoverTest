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

namespace Profiler
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ProfilerAttribute : NUnitAttribute, IWrapTestMethod, ICommandWrapper
    {
        private readonly ulong threshold;
        private readonly string openCoverConsolePath;
        private readonly string nunit3ConsolePath;

        public ProfilerAttribute(ulong threshold, string openCoverConsolePath = null, string nunit3ConsolePath = null)
        {
            this.threshold = threshold;
            this.openCoverConsolePath = openCoverConsolePath;
            this.nunit3ConsolePath = nunit3ConsolePath;
        }        

        public TestCommand Wrap(TestCommand command)
        {
            return new ProfilerCommand(command, this.threshold, this.openCoverConsolePath, this.nunit3ConsolePath);
        }

        private class ProfilerCommand : DelegatingTestCommand
        {
            private readonly ulong threshold;
            private readonly string openCoverConsolePath;
            private readonly string nunit3ConsolePath;

            public ProfilerCommand(TestCommand innerCommand, ulong threshold, string openCoverConsolePath, string nunit3ConsolePath) : base(innerCommand)
            {
                this.threshold = threshold;
                this.openCoverConsolePath = openCoverConsolePath;
                this.nunit3ConsolePath = nunit3ConsolePath;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var covParam = TestContext.Parameters.Get("COV");

                if (string.IsNullOrEmpty(covParam))
                {                    
                    //obtain assembly and test information
                    //obtain opencover & nunit console path
                    string testName = context.CurrentTest.FullName;
                    string assemblyName = Assembly.GetAssembly(context.TestObject.GetType()).Location;
                    string opencoverPath = this.openCoverConsolePath ?? @"C:\src\opencover\main\bin\Debug\OpenCover.Console.exe";
                    string nunitPath = this.nunit3ConsolePath ?? @"C:\src\opencover2\ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe";
                    string openCoverOutputFile = System.AppDomain.CurrentDomain.BaseDirectory + "test_output.xml";

                    StringBuilder arguments = new StringBuilder();
                    arguments.Append(" -target:\"").Append(nunitPath).Append("\"");
                    arguments.Append(" -register:user -filter:\"+[*]* -[Profiler*]* -[NUnit*]*\" -output:\"").Append(openCoverOutputFile).Append("\"");
                    arguments.Append(" -targetargs:\"").Append(assemblyName).Append(" --params=COV=TRUE").Append(" --test=").Append(testName).Append("\"");

                    Console.WriteLine("arguments");
                    Console.WriteLine(arguments);

                    ProcessStartInfo psi = new ProcessStartInfo(opencoverPath, arguments.ToString());
                    var openCoverProcess = Process.Start(psi);
                    openCoverProcess.WaitForExit();

                    XmlDocument doc = new XmlDocument();
                    doc.Load(openCoverOutputFile);

                    XPathDocument document = new XPathDocument(new XmlNodeReader(doc));
                    XPathNavigator navigator = document.CreateNavigator();

                    XPathExpression query = navigator.Compile("sum(//MethodPoint/@vc)");

                    double total = (double)navigator.Evaluate(query);
                    Console.WriteLine("Total: " + total);
                    if (total > threshold)
                    {
                        context.CurrentResult.SetResult(ResultState.Failure, string.Format("Expected max {0} function calls but were {1}", threshold, total));
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
        }
    }
}
