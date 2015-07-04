using Common.IO.File;
using F4Utils.Campaign.F4Structs;
using NUnit.Framework;
using System.IO;

namespace F4Utils.Campaign.Tests
{
    [TestFixture]
    public class ClassTableTests
    {
        private string _loadedClassTablePath;
        private Falcon4EntityClassType[] _classTable;
        private string _savedClassTablePath;
        [Test]
        public void ShouldLoadClassTableFileWithoutThrowingExceptions()
        {
            GivenClassTableFilePath(TestDatalLocations.ClassTable);
            WhenClassTableIsLoaded();
            ThenNoExceptionsShouldBeThrown();
        }
        [Test]
        public void ShouldSaveClassTableFileWithoutThrowingExceptions()
        {
            GivenClassTableFilePath(TestDatalLocations.ClassTable);
            WhenClassTableIsLoaded();
            WhenClassTableIsWritten(TestDatalLocations.TestDataOutputFolder + "FALCON4.CT");
            ThenNoExceptionsShouldBeThrown();
        }
        [Test]
        public void ShouldSaveIdenticalFileAsFileThatWasLoaded()
        {
            GivenClassTableFilePath(TestDatalLocations.ClassTable);
            WhenClassTableIsLoaded();
            WhenClassTableIsWritten(TestDatalLocations.TestDataOutputFolder + "FALCON4.CT");
            ThenSavedFileShouldMatchOriginal();
        }
        private void GivenClassTableFilePath(string classTablePath)
        {
            _loadedClassTablePath = classTablePath;
        }

        private void WhenClassTableIsWritten(string fileName)
        {
            _savedClassTablePath = fileName;
            ClassTable.WriteClassTable(_savedClassTablePath, _classTable);
        }

        private void WhenClassTableIsLoaded()
        {
            _classTable = ClassTable.ReadClassTable(_loadedClassTablePath);
        }
        private void ThenNoExceptionsShouldBeThrown() { }
        private void ThenSavedFileShouldMatchOriginal()
        {
            var filesAreEqual = new FileComparer().Compare(_loadedClassTablePath, _savedClassTablePath);
            Assert.That(filesAreEqual);
        }
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (!Directory.Exists(TestDatalLocations.TestDataOutputFolder))
            {
                Directory.CreateDirectory(TestDatalLocations.TestDataOutputFolder);
            }
        }
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (Directory.Exists(TestDatalLocations.TestDataOutputFolder))
            {
                Directory.Delete(TestDatalLocations.TestDataOutputFolder, true);
            }
        }
    }
}
