using Dodoco.Core.Util.Log;

namespace Dodoco.Core.Util.FileSystem {

    public class FileSystem {

        public static long GetAvaliableStorageSpace(string directory) {

            DriveInfo drive = new DriveInfo(directory);
            long storageFreeBytes = drive.AvailableFreeSpace; 
            Logger.GetInstance().Log($"There is {DataUnitFormatter.Format(storageFreeBytes)} of space available in the drive {drive.VolumeLabel}");
            return storageFreeBytes;

        }

    }

}