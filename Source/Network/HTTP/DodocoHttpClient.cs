using Dodoco.Application;
using Dodoco.Util.Log;
using Dodoco.Util.Unit;

using System.Diagnostics;

namespace Dodoco.Network.HTTP {

    public class DodocoHttpClient: HttpClient {

        public async Task<HttpResponseMessage> FetchAsync(Uri requestUri) {

            this.WriteLogHeader(requestUri);
            HttpResponseMessage res = await this.TryToFetch(base.GetAsync(requestUri));
            this.WriteLogFooter(res);
            return res;

        }

        public async Task<HttpResponseMessage> FetchAsync(Uri requestUri, HttpCompletionOption option) {

            this.WriteLogHeader(requestUri);
            HttpResponseMessage res = await this.TryToFetch(base.GetAsync(requestUri, option));
            this.WriteLogFooter(res);
            return res;

        }

        private async Task<HttpResponseMessage> TryToFetch(Task<HttpResponseMessage> fetchTask) {

            for (int i = 1; i < 6; i++) {

                try {

                    return await fetchTask.WaitAsync(CancellationToken.None);

                } catch (HttpRequestException e) {

                    int timeOut = 5000 * i;
                    Logger.GetInstance().Error($"Failed to make a HTTP request to remote servers. Trying again in {timeOut/1000} seconds...", e);
                    await Task.Delay(timeOut);

                }

            }

            throw new NetworkException($"Failed to make a HTTP request to remote servers");

        }

        public async Task DownloadFileAsync(Uri url, string destinationPath, CancellationToken token = default) => await this.DownloadFileAsync(url, destinationPath, null, token);
        public async Task DownloadFileAsync(Uri url, string destinationPath, ApplicationProgress<DownloadProgressReport>? progress, CancellationToken token = default) {

            if (string.IsNullOrWhiteSpace(url.ToString())) throw new NetworkException($"The given URL is empty");
            if (string.IsNullOrWhiteSpace(destinationPath)) throw new NetworkException($"The given destination path is empty");

            Logger.GetInstance().Log($"Downloading the file \"{Path.GetFileName(destinationPath)}\" to directory \"{Path.GetDirectoryName(destinationPath)}\"...");
            
            double totalBytesTransferred = 0L;

            using (HttpResponseMessage response = await this.FetchAsync(url, HttpCompletionOption.ResponseHeadersRead)) {

                try { response.EnsureSuccessStatusCode(); }
                catch (HttpRequestException e) { throw new NetworkException($"Failed to download the requested file", e); }
                
                double contentLength = response.Content.Headers.ContentLength ?? 0.0D;
                
                using (Stream downloadStream = await response.Content.ReadAsStreamAsync()) {

                    using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create)) {

                        byte[] buffer = new byte[5 * ((int) DataUnit.MEGABYTE)];
                        bool thereIsBytesToRead = true;

                        Stopwatch stopWatch = new Stopwatch();
                        double calcCompletionPercentage = 0;
                        double calcBytesPerSecond = 0;
                        TimeSpan calcEstimatedRemainingTime = TimeSpan.FromSeconds(0);

                        while (thereIsBytesToRead && !token.IsCancellationRequested) {

                            stopWatch.Restart();
                            int bytesRead = await downloadStream.ReadAtLeastAsync(buffer, buffer.Length, false);
                            stopWatch.Stop();

                            if (bytesRead != 0) {

                                totalBytesTransferred += (double) bytesRead;

                                // Write buffer content to the output file

                                await fileStream.WriteAsync(buffer, 0, bytesRead);

                                // Report the download status

                                if (contentLength != 0) {

                                    calcCompletionPercentage = (totalBytesTransferred / contentLength) * 100.0D;
                                    calcBytesPerSecond = (calcBytesPerSecond + ((double) bytesRead / stopWatch.Elapsed.TotalSeconds)) / 2.0D;
                                    calcEstimatedRemainingTime = TimeSpan.FromSeconds((calcEstimatedRemainingTime.TotalSeconds + (contentLength / calcBytesPerSecond)) / 2.0D);

                                    DownloadProgressReport report = new DownloadProgressReport {

                                        completionPercentage = calcCompletionPercentage,
                                        bytesPerSecond = calcBytesPerSecond,
                                        estimatedRemainingTime = calcEstimatedRemainingTime

                                    };

                                    // Avoiding duplicated reports due to Math.Floor() precision loss
                                    
                                    progress?.Report(report);

                                    // Log the download status for each +10% downloaded

                                    if (Convert.ToUInt64(Math.Floor(calcCompletionPercentage)) % 10 == 0)
                                        Logger.GetInstance().Log($"Downloading the file \"{Path.GetFileName(destinationPath)}\" (Status: {report.completionPercentage}% / Speed: {DataUnitFormatter.Format(report.bytesPerSecond, DataUnitFormatterOption.USE_SYMBOL)}/s / ETA: {report.estimatedRemainingTime.ToString(@"hh\:mm\:ss")})");

                                }

                            } else {

                                thereIsBytesToRead = false;

                            }

                        }

                    }

                }

            }

            Logger.GetInstance().Log($"Successfully downloaded the file \"{Path.GetFileName(destinationPath)}\" to directory \"{Path.GetDirectoryName(destinationPath)}\" ({DataUnitFormatter.Format(totalBytesTransferred, DataUnitFormatterOption.USE_FULLNAME)} transferred)");

        }

        public async Task DownloadMultipleFilesAsync(List<Tuple<Uri, string>> targets, ApplicationProgress<DownloadProgressReport>? progress, CancellationToken token = default) {

            List<Task> tasks = new List<Task>();
            DownloadProgressReport generalReport = new DownloadProgressReport {

                completionPercentage = 0,
                bytesPerSecond = 0,
                estimatedRemainingTime = TimeSpan.FromSeconds(0)

            };

            foreach (var target in targets) {

                ApplicationProgress<DownloadProgressReport> targetProgress = new ApplicationProgress<DownloadProgressReport>();
                
                targetProgress.ProgressChanged += new EventHandler<DownloadProgressReport>((object? sender, DownloadProgressReport e) => {

                    generalReport.completionPercentage = Convert.ToInt32(Math.Floor(((double) generalReport.completionPercentage + (double) e.completionPercentage) / 2.0D));
                    generalReport.bytesPerSecond = Convert.ToUInt64(Math.Floor(((double) generalReport.bytesPerSecond + (double) e.bytesPerSecond) / 2.0D));
                    generalReport.estimatedRemainingTime = TimeSpan.FromSeconds((generalReport.estimatedRemainingTime.TotalSeconds + e.estimatedRemainingTime.TotalSeconds) / 2.0D);

                    progress?.Report(generalReport);

                });

                tasks.Add(this.DownloadFileAsync(target.Item1, target.Item2, targetProgress, token));

            }

            await Task.WhenAll(tasks);

        }

        public void WriteLogHeader(Uri url) {

            Logger.GetInstance().Debug($"================= HTTP GET BEGIN =================");
            Logger.GetInstance().Debug($"REQUEST URI: {url.ToString()}");

        }

        public void WriteLogFooter(HttpResponseMessage response) {

            Logger.GetInstance().Debug($"RESPONSE STATUS CODE: {response.StatusCode.ToString()}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT ENCODING: {response.Content.Headers.ContentEncoding}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT LENGTH: {response.Content.Headers.ContentLength}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT TYPE: {response.Content.Headers.ContentType}");
            Logger.GetInstance().Debug($"=================  HTTP GET END  =================");

        }

    }

}