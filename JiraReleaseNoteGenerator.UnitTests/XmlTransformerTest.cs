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
using System.Xml.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.BuildTools.JiraReleaseNoteGenerator.UnitTests.TestDomain;
using Remotion.BuildTools.JiraReleaseNoteGenerator.Utilities;
using Rhino.Mocks;

namespace Remotion.BuildTools.JiraReleaseNoteGenerator.UnitTests
{
  [TestFixture]
  public class XmlTransformerTest
  {
    private IFileSystemHelper _fileSystemHelperMock;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemHelperMock = MockRepository.GenerateMock<IFileSystemHelper>();
    }

    [Test]
    public void GeneateHtmlFromXml_XmlInputSavedToXml ()
    {
      using (var reader = new StreamReader (ResourceManager.GetResourceStream ("Issues_v2.0.2.xml")))
      {
        var xmlInput = XDocument.Load (reader);
        _fileSystemHelperMock.Expect (mock => mock.SaveXml (xmlInput, @".\issuesForSaxon.xml"));
        var transformer = new XmlTransformer (_fileSystemHelperMock);

        transformer.GenerateHtmlFromXml (xmlInput, @".\output.html", @".\TestDomain\transform.xslt", @".\XmlUtilities\Saxon\Transform.exe");
      }

      _fileSystemHelperMock.VerifyAllExpectations();
    }

    [Test]
    public void GeneateHtmlFromXml ()
    {
      using (var reader = new StreamReader (ResourceManager.GetResourceStream ("Issues_v2.0.2.xml")))
      {
        var xmlInput = XDocument.Load (reader);
        
        var transformer = new XmlTransformer (new FileSystemHelper());

        Assert.That (transformer.GenerateHtmlFromXml (xmlInput, @".\output.html", @".\TestDomain\transform.xslt", @".\XmlUtilities\Saxon\Transform.exe"), Is.EqualTo (0));

        using (var resultReader = new StreamReader (ResourceManager.GetResourceStream ("XmlTransformerTest.html")))
        {
          Assert.That (File.ReadAllText (@".\output.html"), Is.EqualTo(resultReader.ReadToEnd()));
        }
      }
    }
  }
}