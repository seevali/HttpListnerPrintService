using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpListenerPrintService
{
    public class HttpListenerService : IHostedService
    {
        private readonly ILogger<HttpListenerService> _logger;
        private HttpListener _listener;

        public HttpListenerService(ILogger<HttpListenerService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("https://localhost:5001/");
            _listener.Start();
            _logger.LogInformation("HTTP Listener started at: {time}", DateTimeOffset.Now);

            Task.Run(() => ListenForRequests(cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task ListenForRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/print")
                {
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        var requestBody = await reader.ReadToEndAsync();
                        var pdfBase64 = ExtractPdfBase64(requestBody);
                        var pdfBytes = Convert.FromBase64String(pdfBase64);
                        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
                        await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

                        PrintPdf(tempFilePath);
                        File.Delete(tempFilePath);
                    }
                }
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();
            }
        }

        private string ExtractPdfBase64(string requestBody)
        {
            var startIndex = requestBody.IndexOf("\"pdfBase64\":\"") + 12;
            var endIndex = requestBody.IndexOf("\"", startIndex);
            return requestBody.Substring(startIndex, endIndex - startIndex);
        }

        private void PrintPdf(string filePath)
        {
            // Implement the logic to print the PDF file to the default printer
            // and block the use of file printers
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _logger.LogInformation("HTTP Listener stopped at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
