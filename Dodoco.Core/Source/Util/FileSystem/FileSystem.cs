using Dodoco.Core.Util.Log;

namespace Dodoco.Core.Util.FileSystem {

    public class FileSystem {

        public static void CopyDirectory(string source, string target, bool overwrite = false) {

            if (!Directory.Exists(source))
                throw new CoreException("The source directory doesn't exist");

            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            foreach (string filePath in Directory.GetFiles(source)) {

                File.Copy(filePath, Path.Join(target, Path.GetFileName(filePath)), overwrite);

            }

            foreach (string directoryPath in Directory.GetDirectories(source)) {

                CopyDirectory(directoryPath, Path.Join(target, Path.GetFileName(directoryPath)), overwrite);

            }

        }

        public static long GetAvailableStorageSpace(string directory) {

            /*
             * If the directory actually doesn't exist, the
             * "new DriveInfo(directory)" will fail, so we
             * create the directory, get the free space available
             * and then delete it.
            */

            bool directoryExists = Directory.Exists(directory);
            if (!directoryExists)
                Directory.CreateDirectory(directory);

            DriveInfo drive = new DriveInfo(directory);
            long storageFreeBytes = drive.TotalFreeSpace; 
            Logger.GetInstance().Log($"There is {DataUnitFormatter.Format(storageFreeBytes)} of space available in the drive {drive.VolumeLabel}");
            
            if (!directoryExists)
                Directory.Delete(directory);
            
            return storageFreeBytes;

        }

    }

}