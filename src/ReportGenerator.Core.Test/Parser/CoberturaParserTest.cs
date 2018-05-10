﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Xunit;

namespace Palmmedia.ReportGeneratorTest.Parser
{
    /// <summary>
    /// This is a test class for CoberturaParser and is intended
    /// to contain all CoberturaParser Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CoberturaParserTest
    {
        private static readonly string FilePath1 = Path.Combine(FileManager.GetJavaReportDirectory(), "Cobertura2.1.1.xml");

        private static IEnumerable<Assembly> assemblies;

        public CoberturaParserTest()
        {
            var report = XDocument.Load(FilePath1);
            new CoberturaReportPreprocessor(report).Execute();
            assemblies = new CoberturaParser(report).Assemblies;
        }

        /// <summary>
        /// A test for SupportsBranchCoverage
        /// </summary>
        [Fact]
        public void SupportsBranchCoverage()
        {
            Assert.True(new CoberturaParser(XDocument.Load(FilePath1)).SupportsBranchCoverage);
        }

        /// <summary>
        /// A test for NumberOfLineVisits
        /// </summary>
        [Fact]
        public void NumberOfLineVisitsTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 15).LineVisits);
            Assert.Equal(1, fileAnalysis.Lines.Single(l => l.LineNumber == 17).LineVisits);
            Assert.Equal(0, fileAnalysis.Lines.Single(l => l.LineNumber == 20).LineVisits);
            Assert.Equal(-1, fileAnalysis.Lines.Single(l => l.LineNumber == 1).LineVisits);
        }

        /// <summary>
        /// A test for LineVisitStatus
        /// </summary>
        [Fact]
        public void LineVisitStatusTest()
        {
            var fileAnalysis = GetFileAnalysis(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java");

            var line = fileAnalysis.Lines.Single(l => l.LineNumber == 1);
            Assert.Equal(LineVisitStatus.NotCoverable, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 12);
            Assert.Equal(LineVisitStatus.Covered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 15);
            Assert.Equal(LineVisitStatus.PartiallyCovered, line.LineVisitStatus);

            line = fileAnalysis.Lines.Single(l => l.LineNumber == 20);
            Assert.Equal(LineVisitStatus.NotCovered, line.LineVisitStatus);
        }

        /// <summary>
        /// A test for NumberOfFiles
        /// </summary>
        [Fact]
        public void NumberOfFilesTest()
        {
            Assert.Equal(7, assemblies.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count());
        }

        /// <summary>
        /// A test for FilesOfClass
        /// </summary>
        [Fact]
        public void FilesOfClassTest()
        {
            Assert.Single(assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").Files);
            Assert.Single(assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.GenericClass").Files);
        }

        /// <summary>
        /// A test for ClassesInAssembly
        /// </summary>
        [Fact]
        public void ClassesInAssemblyTest()
        {
            Assert.Equal(7, assemblies.SelectMany(a => a.Classes).Count());
        }

        /// <summary>
        /// A test for Assemblies
        /// </summary>
        [Fact]
        public void AssembliesTest()
        {
            Assert.Equal(2, assemblies.Count());
        }

        /// <summary>
        /// A test for GetCoverageQuotaOfClass.
        /// </summary>
        [Fact]
        public void GetCoverableLinesOfClassTest()
        {
            Assert.Equal(3, assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.AbstractClass").CoverableLines);
        }

        /// <summary>
        /// A test for MethodMetrics
        /// </summary>
        [Fact]
        public void MethodMetricsTest()
        {
            var metrics = assemblies.Single(a => a.Name == "test").Classes.Single(c => c.Name == "test.TestClass").MethodMetrics;

            Assert.Equal(4, metrics.Count());
            Assert.Equal("<init>()V", metrics.First().Name);
            Assert.Equal(3, metrics.First().Metrics.Count());

            Assert.Equal("Cyclomatic complexity", metrics.First().Metrics.ElementAt(0).Name);
            Assert.Equal(0, metrics.First().Metrics.ElementAt(0).Value);
            Assert.Equal("Line coverage", metrics.First().Metrics.ElementAt(1).Name);
            Assert.Equal(1.0M, metrics.First().Metrics.ElementAt(1).Value);
            Assert.Equal("Branch coverage", metrics.First().Metrics.ElementAt(2).Name);
            Assert.Equal(1.0M, metrics.First().Metrics.ElementAt(2).Value);
        }

        /// <summary>
        /// A test for CodeElements
        /// </summary>
        [Fact]
        public void CodeElementsTest()
        {
            var codeElements = GetFile(assemblies, "test.TestClass", "C:\\temp\\test\\TestClass.java").CodeElements;
            Assert.Equal(4, codeElements.Count());
        }

        private static CodeFile GetFile(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName);

        private static FileAnalysis GetFileAnalysis(IEnumerable<Assembly> assemblies, string className, string fileName) => assemblies
                .Single(a => a.Name == "test").Classes
                .Single(c => c.Name == className).Files
                .Single(f => f.Path == fileName)
                .AnalyzeFile();
    }
}