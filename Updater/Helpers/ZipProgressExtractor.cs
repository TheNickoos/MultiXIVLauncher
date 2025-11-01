using System;
using System.IO;
using System.IO.Compression;

namespace MultiXIVLauncher.Updater.Helpers
{
    internal static class ZipProgressExtractor
    {
        /// <summary>
        /// Computes the total uncompressed size (in bytes) of all file entries in the ZIP
        /// to enable a byte-accurate progress indicator.
        /// </summary>
        /// <param name="zipPath">Absolute path to the ZIP archive.</param>
        /// <returns>Total uncompressed byte size of all files (directories excluded).</returns>
        public static long ComputeUncompressedSize(string zipPath)
        {
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                long total = 0;
                foreach (var entry in zip.Entries)
                {
                    // Ignore directories (they have empty Name and size 0)
                    if (!string.IsNullOrEmpty(entry.Name))
                        total += entry.Length;
                }
                return total;
            }
        }

        /// <summary>
        /// Extracts a ZIP archive while reporting progress based on bytes written,
        /// replacing existing files if requested.
        /// </summary>
        /// <param name="zipPath">Absolute path to the ZIP archive.</param>
        /// <param name="destinationDirectoryName">Destination folder for extraction.</param>
        /// <param name="overwrite">If true, existing files are overwritten.</param>
        /// <param name="progress">Progress callback (0.0–1.0). Can be null.</param>
        /// <param name="onEntry">Callback invoked for each entry (e.g., to log file names). Can be null.</param>
        public static void ExtractZipToDirectoryWithProgress(
            string zipPath,
            string destinationDirectoryName,
            bool overwrite,
            IProgress<double> progress,
            Action<string> onEntry)
        {
            Directory.CreateDirectory(destinationDirectoryName);

            long totalBytes = ComputeUncompressedSize(zipPath);
            long doneBytes = 0;
            const int BufferSize = 1024 * 128; // 128 KB

            using (var zip = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in zip.Entries)
                {
                    // Handle directories
                    var fullPath = Path.Combine(destinationDirectoryName, entry.FullName);
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    onEntry?.Invoke("• " + entry.FullName);

                    // Write with optional overwrite
                    using (var entryStream = entry.Open())
                    using (var outStream = new FileStream(
                        fullPath,
                        overwrite ? FileMode.Create : FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        var buffer = new byte[BufferSize];
                        int read;
                        while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, read);
                            doneBytes += read;
                            if (totalBytes > 0)
                                progress?.Report((double)doneBytes / totalBytes);
                        }
                    }

                    // Preserve last write time (best-effort, optional)
                    try { File.SetLastWriteTime(fullPath, entry.LastWriteTime.DateTime); } catch { }
                }
            }

            progress?.Report(1.0);
        }
    }
}
