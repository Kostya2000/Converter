namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            var driveConverter = new OneDriveConverter();
            IFile file = new FileFromDisk();
            file.Read("КОСТЯ.docx");
            driveConverter.SendFile(((FileFromDisk)file).Stream);
            ((FileFromDisk)file).Stream = driveConverter.GetFile();
            driveConverter.DeleteFile();
            file.Write();
        }
    }
}
