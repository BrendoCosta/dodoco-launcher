using System.IO.Compression;

namespace Dodoco.Core.Extension {

    public static class ZipArchiveExtension {

        public static long GetFullLength(this ZipArchive archive) {

            return archive.Entries.Select(e => e.Length).Sum();

        }

        public static void ExtractToDirectory(this ZipArchive archive, string directory, bool overwrite, ProgressReporter<ProgressReport>? reporter = null) {

            for (int i = 0; i < archive.Entries.Count; i++) {

                ZipArchiveEntry entry = archive.Entries[i];
                string path = Path.Join(directory, entry.FullName);

                if (entry.FullName.EndsWith("/") && string.IsNullOrWhiteSpace(entry.Name)) {

                    /* The ZipArchiveEntry.ExtractToFile method doesn't creates zip archive's internal
                     * directories due some unknown reason, so we need to create them otherwise an
                     * UnauthorizedAccessException will be raised up.
                    */

                    Directory.CreateDirectory(path);
                    continue;

                }

                entry.ExtractToFile(path, overwrite);

                ProgressReport report = new ProgressReport {
                    Done = i + 1,
                    Total = archive.Entries.Count,
                    Rate = null,
                    EstimatedRemainingTime = null,
                    Message = path
                };

                reporter?.Report(report);

            }

        }

    }

}