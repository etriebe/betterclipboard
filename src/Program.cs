using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using TextCopy;

namespace betterclipboard
{
    public class Program
    {
        static string logFile;
        static string clipboard;

        static Dictionary<string, string> clipboardRegex;

        public static void Main(string[] args)
        {
            try
            {
                logFile = $"bc_{DateTime.Now.ToString("yyyyMMdd")}.log";
                clipboard = ClipboardService.GetText();
                clipboardRegex = new Dictionary<string, string>();
                clipboardRegex.Add(@"https:\/\/www\.tiktok\.com\/(.+)?@(?<username>[\w\d]+)\/video\/(?<videoID>\d+)", "https://www.tiktok.com/@##username##/video/##videoID##");
                while (true)
                {
                    WriteMessage($"Worker running at: {DateTimeOffset.Now}");
                    CheckChangedClipboard();
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                WriteMessage($"Exception: {e.ToString()}");
            }
        }

        private static void CheckChangedClipboard()
        {
            string currentClipboard = ClipboardService.GetText();
            if (currentClipboard != clipboard && currentClipboard != null)
            {
                WriteMessage($"Clipboard changed! {currentClipboard}");
                try
                {
                    CleanupClipboard(currentClipboard);
                }
                catch (Exception e)
                {
                    WriteMessage($"Exception in cleaning clipboard: {e.ToString()}");
                }
                clipboard = currentClipboard;
            }
        }
        private static void CleanupClipboard(string newClipboard)
        {
            foreach (string key in clipboardRegex.Keys)
            {
                Match match = Regex.Match(newClipboard, key);

                if (match.Success)
                {
                    string cleanFormat = clipboardRegex[key];
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

                    WriteMessage($"Cleaning clipboard entry from '{newClipboard}' to '{cleanFormat}'");
                    clipboard = cleanFormat;
                    ClipboardService.SetText(clipboard);
                    break;
                }
            }
        }

        private static void WriteMessage(string message)
        {
            string textToLog = $"{DateTime.Now.ToString()}: {message}\r\n";
            Console.WriteLine(textToLog);
            File.AppendAllText(logFile, textToLog);
        }
    }
}
