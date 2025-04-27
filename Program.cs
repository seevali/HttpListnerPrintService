using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpListenerPrintService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpListener _listener;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("https://localhost:5001/");
            _listener.Start();
            _logger.LogInformation("Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
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

            _listener.Stop();
        }

        private string ExtractPdfBase64(string requestBody)
        {
            // Assuming the request body is a JSON object with a property named "pdfBase64"
            var startIndex = requestBody.IndexOf("\"pdfBase64\":\"") + 12;
            var endIndex = requestBody.IndexOf("\"", startIndex);
            return requestBody.Substring(startIndex, endIndex - startIndex);
        }

        private void PrintPdf(string filePath)
        {
            // Implement the logic to print the PDF file to the default printer
            // and block the use of file printers
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _listener.Stop();
            _logger.LogInformation("Service stopped at: {time}", DateTimeOffset.Now);
            await base.StopAsync(stoppingToken);
        }
    }
}
