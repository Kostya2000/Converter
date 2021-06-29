namespace Converter
{
    class OneDriveConverterConsole
    {
        static void Main(string[] args)
        {
            Init();
            ConvertFileFromDiskSource(args[0]);
        }

        /// <summary>
        /// Загрузка данных из конфигурации
        /// </summary>
        static void Init()
        {
            IOneDriveConnection connection = new OneDriveConnection();
            connection.Init();
        }

        /// <summary>
        /// Конвертируем файл из диска
        /// </summary>
        /// <param name="fileName"></param>
        static void ConvertFileFromDiskSource(string fileName)
        {
            var driveConverter = new OneDriveConverter();
            IFile file = new DiskSource();
            file.Read(fileName);
            driveConverter.SendFile(((DiskSource)file).Stream);
            ((DiskSource)file).Stream = driveConverter.GetFile();
            file.Write();
            driveConverter.DeleteFile();
        }
    }
}
