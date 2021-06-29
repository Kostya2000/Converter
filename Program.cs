namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            var driveConverter = new OneDriveConverter();
            var file = new FileFromDisk();
            file.Read("КОСТЯ.docx");
            driveConverter.SendFile(file.Stream);
            file.Stream = driveConverter.GetFile();
            driveConverter.DeleteFile();
            file.Write();
        }
    }
}
