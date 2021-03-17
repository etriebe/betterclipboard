using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TextCopy;

namespace betterclipboard
{
    public class Worker : BackgroundService
    {
        string clipboard;

        Dictionary<string, string> clipboardRegex;

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            clipboard = ClipboardService.GetText();
            this.clipboardRegex = new Dictionary<string, string>();
            this.clipboardRegex.Add(@"https:\/\/www\.tiktok\.com\/@(?<username>[\w\d]+)\/video\/(?<videoID>\d+)", "https://www.tiktok.com/@##username##/video/##videoID##");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                CheckChangedClipboard();
                await Task.Delay(5000, stoppingToken);
            }
        }

        private void CheckChangedClipboard()
        {
            string currentClipboard = ClipboardService.GetText();
            if (currentClipboard != this.clipboard)
            {
                _logger.LogInformation($"Clipboard changed! {currentClipboard}");
                CleanupClipboard(currentClipboard);
                this.clipboard = currentClipboard;
            }
        }

        private void CleanupClipboard(string newClipboard)
        {
            foreach (string key in this.clipboardRegex.Keys)
            {
                Match match = Regex.Match(newClipboard, key);

                if (match.Success)
                {
                    string cleanFormat = this.clipboardRegex[key];
                    string tokenRegexFormat = "##(?<tokenName>[\\w\\d]+)##";
                    MatchCollection tokens = Regex.Matches(cleanFormat, tokenRegexFormat);
                    foreach (Match token in tokens)
                    {
                        if (token.Success)
                        {
                            string tokenName = token.Groups["tokenName"].Value;
                            string tokenReplacementValue = match.Groups[tokenName].Value;
                            cleanFormat = cleanFormat.Replace(token.Value, tokenReplacementValue);
                        }
                    }

                    _logger.LogInformation($"Cleaning clipboard entry from '{newClipboard}' to '{cleanFormat}'");
                    this.clipboard = cleanFormat;
                    ClipboardService.SetText(this.clipboard);
                    break;
                }
            }

        }
    }
}
