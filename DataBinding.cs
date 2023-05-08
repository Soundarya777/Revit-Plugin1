using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3KeystoRVT
{
    class DataBinding
    {
        public static string CreateLatestCentralFile(string folderPath, string rvtFileName)
        {

            try
            {
                return CreateCentralFileFromBackUp(folderPath, rvtFileName);

            }
            catch (Exception ex)
            {
                return null;
            }

        }


        public static string CreateCentralFileFromBackUp(string folderPath, string rvtFileName)
        {
            string rvtFilePath = Path.Combine(Path.GetDirectoryName(folderPath), rvtFileName);
            try
            {
                long filesSizeinBytes = new DirectoryInfo(folderPath).EnumerateFiles("*", SearchOption.AllDirectories).Sum((FileInfo fi) => fi.Length);
                using (OpenMcdf.CompoundFile rvtCFFile = new OpenMcdf.CompoundFile(filesSizeinBytes))
                {
                    TraverseAndWrite(folderPath, rvtCFFile.RootStorage);
                    rvtCFFile.Save(rvtFilePath);
                    rvtCFFile.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            return rvtFilePath;
        }

        public static Dictionary<string, string> DicRvtFolderFormat
        {
            get
            {
                return new Dictionary<string, string>() {{"Others","preview/RevitPreview4.0|BasicFileInfo|Contents|ProjectInformation|TransmissionData"},{ "Formats", "formats/Latest" },{ "Global", "global/Latest|ContentDocuments|incrementtable/DocumentIncrementTable|ElemTable|History|PartitionTable" },
                    {"Partitions","*.rws"}};
            }
        }
        /// <summary>
        /// TO write content in respective folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="storage"></param>
        private static void TraverseAndWrite(string folderPath, OpenMcdf.CFStorage storage)
        {
            try
            {
                foreach (var rvtFolder in DicRvtFolderFormat)
                {
                    if (rvtFolder.Key != "Others")
                    {
                        OpenMcdf.CFStorage subStorage = storage.AddStorage(rvtFolder.Key);

                        if (rvtFolder.Key == "Partitions")
                        {
                            string[] rwsFiles = Directory.GetFiles(folderPath, rvtFolder.Value);
                            if (!rwsFiles.Any())
                            {
                                rwsFiles = Directory.GetFiles(folderPath).Where(e => Path.GetFileName(e).All(f => char.IsDigit(f))).ToArray();
                            }
                            //rwsFiles = rwsFiles.Reverse().ToArray();
                            foreach (string file in rwsFiles)
                            {
                                string streamName = Path.GetFileName(file);
                                //Console.WriteLine(streamName);
                                streamName = streamName.Contains('_') ? streamName.Substring(0, Path.GetFileName(file).IndexOf('_')) : streamName;
                                OpenMcdf.CFStream stream = subStorage.TryGetStream(streamName);
                                if (stream == null)
                                {
                                    OpenMcdf.CFStream storageStream = subStorage.AddStream(streamName);
                                    storageStream.CopyFrom(new FileStream(file, FileMode.Open, FileAccess.Read));
                                }
                            }
                        }
                        else
                        {
                            AddStream(folderPath, rvtFolder, subStorage);
                        }
                    }
                    else
                    {
                        AddStream(folderPath, rvtFolder, storage);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void AddStream(string folderPath, KeyValuePair<string, string> rvtFolder, OpenMcdf.CFStorage subStorage)
        {
            try
            {

                rvtFolder.Value.Split('|').ToList().ForEach((files) =>
                {
                    if (files.Contains('/'))
                    {
                        var fileMapping = files.Split('/');
                        string filePathWithLatestIndex = GetLatestIncrementalFile(folderPath, fileMapping[0] + ".*");
                        if (!string.IsNullOrWhiteSpace(filePathWithLatestIndex))
                            subStorage.AddStream(fileMapping[1]).CopyFrom(new FileStream(filePathWithLatestIndex, FileMode.Open, FileAccess.Read));
                        else
                        {
                            filePathWithLatestIndex = Path.Combine(folderPath, fileMapping[1]);
                            if (File.Exists(filePathWithLatestIndex))
                                subStorage.AddStream(fileMapping[1]).CopyFrom(new FileStream(filePathWithLatestIndex, FileMode.Open, FileAccess.Read));
                        }
                    }
                    else
                    {
                        string filePathWithLatestIndex = GetLatestIncrementalFile(folderPath, files + ".*");
                        subStorage.AddStream(files).CopyFrom(new FileStream(filePathWithLatestIndex, FileMode.Open, FileAccess.Read));
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        internal static string GetLatestIncrementalFile(string backupPath, string searchPattern)
        {
            List<string> lstFilePaths = Directory.GetFiles(backupPath, searchPattern, SearchOption.TopDirectoryOnly).ToList();
            if (lstFilePaths.Any())
            {
                lstFilePaths.Sort();
                return lstFilePaths.LastOrDefault();
            }
            else
                return string.Empty;
        }
    }
}
