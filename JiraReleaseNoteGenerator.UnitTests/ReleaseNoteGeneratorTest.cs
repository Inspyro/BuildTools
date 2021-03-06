// Copyright (c) 2009 rubicon informationstechnologie gmbh
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using NUnit.Framework;
using Remotion.BuildTools.JiraReleaseNoteGenerator.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.BuildTools.JiraReleaseNoteGenerator.UnitTests
{
  [TestFixture]
  public class ReleaseNoteGeneratorTest
  {
    private ReleaseNoteGenerator _releaseNoteGenerator;
    private IJiraIssueAggregator _jiraIssueAggregatorStub;
    private IXmlTransformer _xmlTransformerStub;
    private readonly Configuration _configuration = Configuration.Current;

    [SetUp]
    public void SetUp ()
    {
      _jiraIssueAggregatorStub = MockRepository.GenerateStub<IJiraIssueAggregator>();
      _xmlTransformerStub = MockRepository.GenerateMock<IXmlTransformer>();
      _releaseNoteGenerator = new ReleaseNoteGenerator (_configuration, _jiraIssueAggregatorStub, _xmlTransformerStub);
    }

    [Test]
    public void GenerateReleaseNotes ()
    {
      const string outputFile = @".\ReleaseNoteGenerator-UnitTest\output.html";
      var constraints = new CustomConstraints ("2.0.2", null);

      using (var reader = new StreamReader (ResourceManager.GetResourceStream ("Issues_v2.0.2_complete.xml")))
      {
        _jiraIssueAggregatorStub.Stub (stub => stub.GetXml (constraints)).Return (XDocument.Load (reader));
      }

      using (var reader = new StreamReader (ResourceManager.GetResourceStream ("Issues_v2.0.2_complete.xml")))
      {
        var issues = XDocument.Load (reader);
        var config = XDocument.Load (_configuration.ConfigFile);
        config.Root.Add (new XElement ("generatedForVersion", "2.0.2"));
        issues.Root.AddFirst (config.Elements());
        _xmlTransformerStub.Expect (mock => mock.GenerateResultFromXml (
          Arg<XDocument>.Matches (d => d.ToString() == issues.ToString()), Arg.Is (outputFile))).Return (0);
      }

      var exitCode = _releaseNoteGenerator.GenerateReleaseNotes (constraints, outputFile, null);

      Assert.That (exitCode, Is.EqualTo (0));

      _xmlTransformerStub.VerifyAllExpectations();
    }

    [Test]
    public void GenerateReleaseNotes_InvalidVersion ()
    {
      const string outputFile = @".\ReleaseNoteGenerator-UnitTest\output.html";
      var constraints = new CustomConstraints ("unknownVersion", null);

      _jiraIssueAggregatorStub.Stub (stub => stub.GetXml (constraints)).Throw (new WebException ("The remote server returned an error: (400) Bad Request."));

      var exitCode = _releaseNoteGenerator.GenerateReleaseNotes (constraints, outputFile, null);
      
      Assert.That (exitCode, Is.EqualTo (1));
    }

  }
}