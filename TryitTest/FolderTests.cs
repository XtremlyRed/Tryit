namespace TryitTest
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tryit;

    namespace TryitTest.Utils
    {
        [TestClass]
        public class FolderTests
        {
            private string _tempTestFolder = default!;

            [TestInitialize]
            public void TestInitialize()
            {
                // Reset static flag to default before each test
                Folder.AutoCreateFolder = true;
                _tempTestFolder = Path.Combine(Path.GetTempPath(), "Tryit_FolderTests", Guid.NewGuid().ToString());
            }

            [TestCleanup]
            public void TestCleanup()
            {
                // Clean up created directories
                if (Directory.Exists(_tempTestFolder))
                {
                    Directory.Delete(_tempTestFolder, true);
                }
            }

            [TestMethod]
            public void ConstructorAndImplicitConversion_FromString_ShouldSucceed()
            {
                // Arrange
                var path = @"C:\Test";

                // Act
                Folder folder1 = new Folder(path);
                Folder folder2 = path;

                // Assert
                Assert.AreEqual(path, folder1.ToString());
                Assert.AreEqual(path, folder2.ToString());
            }

            [TestMethod]
            public void Combine_ShouldCombinePathsCorrectly()
            {
                // Arrange
                var initialPath = @"C:\Users\Test";
                var folder = new Folder(initialPath);
                var segment1 = "Documents";
                var segment2 = "MyProject";
                var expected = Path.Combine(initialPath, segment1, segment2);

                // Act
                Folder result = folder.Combine(segment1, segment2);

                // Assert
                Assert.AreEqual(expected, result.ToString());
            }

            [TestMethod]
            public void CombinePaths_ShouldCombinePathsCorrectly()
            {
                // Arrange
                var segment1 = @"C:\Program Files";
                var segment2 = "MyApp";
                var segment3 = "Version1";
                var expected = Path.Combine(segment1, segment2, segment3);

                // Act
                Folder result = Folder.CombinePaths(segment1, segment2, segment3);

                // Assert
                Assert.AreEqual(expected, result.ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Combine_NullPaths_ThrowsArgumentNullException()
            {
                // Arrange
                var folder = new Folder(@"C:\Temp");

                // Act
                folder.Combine(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Combine_EmptyPaths_ThrowsArgumentNullException()
            {
                // Arrange
                var folder = new Folder(@"C:\Temp");

                // Act
                folder.Combine();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CombinePaths_NullPaths_ThrowsArgumentNullException()
            {
                // Act
                Folder.CombinePaths(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CombinePaths_EmptyPaths_ThrowsArgumentNullException()
            {
                // Act
                Folder.CombinePaths();
            }

            [TestMethod]
            public void ImplicitStringConversion_WithAutoCreate_CreatesDirectory()
            {
                // Arrange
                Folder folder = _tempTestFolder;
                Assert.IsFalse(Directory.Exists(_tempTestFolder), "Pre-condition: Directory should not exist.");

                // Act
                string path = folder; // Implicit conversion triggers creation

                // Assert
                Assert.AreEqual(_tempTestFolder, path);
                Assert.IsTrue(Directory.Exists(_tempTestFolder), "Directory should have been created.");
            }

            [TestMethod]
            public void ImplicitStringConversion_WithoutAutoCreate_DoesNotCreateDirectory()
            {
                // Arrange
                Folder.AutoCreateFolder = false;
                Folder folder = _tempTestFolder;
                Assert.IsFalse(Directory.Exists(_tempTestFolder), "Pre-condition: Directory should not exist.");

                // Act
                string path = folder; // Implicit conversion

                // Assert
                Assert.AreEqual(_tempTestFolder, path);
                Assert.IsFalse(Directory.Exists(_tempTestFolder), "Directory should not have been created.");
            }

            [TestMethod]
            public void TryCreateFolder_CreatesDirectoryWhenNotExists()
            {
                // Arrange
                var folder = new Folder(_tempTestFolder);
                Assert.IsFalse(Directory.Exists(_tempTestFolder), "Pre-condition: Directory should not exist.");

                // Act
                folder.TryCreateFolder();

                // Assert
                Assert.IsTrue(Directory.Exists(_tempTestFolder), "Directory should have been created.");
            }

            [TestMethod]
            public void Equality_ShouldBeCaseInsensitive()
            {
                // Arrange
                var path1 = @"C:\MyFolder\data";
                var path2 = @"c:\myfolder\DATA";
                Folder folder1 = path1;
                Folder folder2 = path2;

                // Act & Assert
                Assert.IsTrue(folder1.Equals(folder2));
                Assert.IsTrue(folder1.Equals((object)folder2));
                Assert.IsTrue(folder1 == folder2);
                Assert.IsFalse(folder1 != folder2);
            }

            [TestMethod]
            public void Equality_WithDifferentPaths_ShouldBeFalse()
            {
                // Arrange
                Folder folder1 = @"C:\Folder1";
                Folder folder2 = @"C:\Folder2";

                // Act & Assert
                Assert.IsFalse(folder1.Equals(folder2));
                Assert.IsFalse(folder1 == folder2);
                Assert.IsTrue(folder1 != folder2);
            }

            [TestMethod]
            public void GetHashCode_ShouldBeEqualForEqualObjects()
            {
                // Arrange
                var path1 = @"C:\MyFolder\data";
                var path2 = @"c:\myfolder\DATA";
                Folder folder1 = path1;
                Folder folder2 = path2;

                // Act & Assert
                Assert.AreEqual(folder1.GetHashCode(), folder2.GetHashCode());
            }

            [TestMethod]
            public void StaticFolderProperties_AreNotNullOrEmpty()
            {
                Assert.IsFalse(string.IsNullOrEmpty(Folder.Desktop));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.MyDocuments));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.ApplicationData));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.LocalApplicationData));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.CommonApplicationData));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.ProgramFiles));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.System));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.Current));
                // A few more checks for good measure
                Assert.IsFalse(string.IsNullOrEmpty(Folder.UserProfile));
                Assert.IsFalse(string.IsNullOrEmpty(Folder.Windows));
            }
        }
    }
}
