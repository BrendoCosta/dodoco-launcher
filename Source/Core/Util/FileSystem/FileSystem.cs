using Dodoco.Core.Util.Log;

namespace Dodoco.Core.Util.FileSystem {

    public class FileSystem {

        public static long GetAvaliableStorageSpace(string directory) {

            /*
             * If the directory actually doesn't exist, the
             * "new DriveInfo(directory)" will fail, so we
             * create the directory, get the free space avaliable
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