#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class UnityCommandLineTestRunner
{
    private const string ModeArgument = "-chuoTestMode";
    private const string ResultsArgument = "-chuoTestResults";
    private static TestRunnerApi activeApi;
    private static XmlResultCallbacks activeCallbacks;

    public static void Run()
    {
        string mode = GetArgumentValue(ModeArgument, "EditMode");
        string resultsPath = GetArgumentValue(ResultsArgument, string.Empty);

        if (string.IsNullOrWhiteSpace(resultsPath))
        {
            Debug.LogError($"{ResultsArgument} was not provided.");
            ExitEditor(1);
            return;
        }

        TestMode testMode = string.Equals(mode, "PlayMode", StringComparison.OrdinalIgnoreCase)
            ? TestMode.PlayMode
            : TestMode.EditMode;

        string resultsDirectory = Path.GetDirectoryName(resultsPath);
        if (!string.IsNullOrWhiteSpace(resultsDirectory))
        {
            Directory.CreateDirectory(resultsDirectory);
        }

        activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        activeCallbacks = ScriptableObject.CreateInstance<XmlResultCallbacks>();
        activeCallbacks.Initialize(resultsPath);
        activeApi.RegisterCallbacks(activeCallbacks);
        activeApi.Execute(new ExecutionSettings(new Filter
        {
            testMode = testMode
        }));
    }

    private static void ExitEditor(int exitCode)
    {
        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }

    private static string GetArgumentValue(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return fallback;
    }

    private class XmlResultCallbacks : ScriptableObject, ICallbacks
    {
        [SerializeField]
        private string resultsPath;

        public void Initialize(string path)
        {
            resultsPath = path;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log($"Unity command-line test run started: {testsToRun?.Name}");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            int passed = result != null ? result.PassCount : 0;
            int failed = result != null ? result.FailCount : 1;
            int skipped = result != null ? result.SkipCount : 0;
            int inconclusive = result != null ? result.InconclusiveCount : 0;
            int total = passed + failed + skipped + inconclusive;

            try
            {
                File.WriteAllText(resultsPath, BuildXml(result), new UTF8Encoding(false));
                Debug.Log($"Unity command-line test XML written to {resultsPath}");
                Debug.Log($"Unity command-line test summary: total={total} passed={passed} failed={failed} skipped={skipped} inconclusive={inconclusive}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not write Unity command-line test XML to {resultsPath}. {exception.Message}");
                ExitEditor(1);
                return;
            }

            ExitEditor(result != null && result.FailCount == 0 ? 0 : 2);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }

        private static string BuildXml(ITestResultAdaptor result)
        {
            var builder = new StringBuilder();
            int passed = result != null ? result.PassCount : 0;
            int failed = result != null ? result.FailCount : 1;
            int skipped = result != null ? result.SkipCount : 0;
            int inconclusive = result != null ? result.InconclusiveCount : 0;
            int total = passed + failed + skipped + inconclusive;
            string status = failed == 0 ? "Passed" : "Failed";

            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.AppendLine(
                $"<test-run result=\"{status}\" total=\"{total}\" passed=\"{passed}\" failed=\"{failed}\" skipped=\"{skipped}\" inconclusive=\"{inconclusive}\">");

            if (result != null)
            {
                AppendResult(builder, result, 1);
            }

            builder.AppendLine("</test-run>");
            return builder.ToString();
        }

        private static void AppendResult(StringBuilder builder, ITestResultAdaptor result, int indent)
        {
            string spaces = new string(' ', indent * 2);
            string name = Escape(result.Name);
            string fullName = Escape(result.FullName);
            string outcome = Escape(result.TestStatus.ToString());
            string message = Escape(result.Message);

            builder.AppendLine(
                $"{spaces}<test-suite name=\"{name}\" fullname=\"{fullName}\" result=\"{outcome}\" passed=\"{result.PassCount}\" failed=\"{result.FailCount}\" skipped=\"{result.SkipCount}\" inconclusive=\"{result.InconclusiveCount}\" duration=\"{result.Duration:0.###}\">");

            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.AppendLine($"{spaces}  <failure><message>{message}</message><stack-trace>{Escape(result.StackTrace)}</stack-trace></failure>");
            }

            if (result.HasChildren)
            {
                foreach (ITestResultAdaptor child in result.Children)
                {
                    AppendResult(builder, child, indent + 1);
                }
            }

            builder.AppendLine($"{spaces}</test-suite>");
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
#endif
