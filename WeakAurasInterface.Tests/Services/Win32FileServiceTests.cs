using WeakAurasInterface.WPF.Services;

namespace WeakAurasInterface.Tests.Services;

public class Win32FileServiceTests
{
    private Win32FileService _fileService = null!;

    [SetUp]
    public void SetUp()
    {
        _fileService = new Win32FileService();
    }

    [Test]
    public void SanitizeFileNameTest()
    {
        const string fileName = "Test:File\\Name";
        const string expected = "Test_File_Name";
        string actual = _fileService.SanitizeFilename(fileName);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildFilenameTest()
    {
        const string fileName = "Test:File\\Name";
        const string directory = "C:\\Test\\Directory";
        var extension = "txt";
        const string expected = "C:\\Test\\Directory\\Test_File_Name.txt";
        string actual = _fileService.BuildFilename(directory, fileName, extension);
        Assert.That(actual, Is.EqualTo(expected));

        extension = ".txt.";
        actual = _fileService.BuildFilename(directory, fileName, extension);
        Assert.That(actual, Is.EqualTo(expected));
    }
}