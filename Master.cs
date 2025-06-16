
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;

class Scanner
{
    static void Main(string[] args)
    {
        Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << 1);

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Scanner.exe <directoryPath> <pipeName>");
            return;
        }

        string directory = args[0];
        string pipeName = args[1];

        Thread readThread = new Thread(() => ReadAndSendFiles(directory, pipeName));
        readThread.Start();
    }

    static void ReadAndSendFiles(string directory, string pipeName)
    {
        var wordIndexList = new List<WordIndex>();

        foreach (string file in Directory.GetFiles(directory, "*.txt"))
        {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var words = line.Split(new[] { ' ', '.', ',', ';', '-', '_', ':', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var w = word.Trim().ToLowerInvariant();
                    if (!wordCounts.ContainsKey(w))
                        wordCounts[w] = 0;
                    wordCounts[w]++;
                }
            }

            wordIndexList.Add(new WordIndex
            {
                FileName = Path.GetFileName(file),
                WordCounts = wordCounts
            });
        }

        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        pipe.Connect();

        string json = JsonSerializer.Serialize(wordIndexList);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        pipe.Write(buffer, 0, buffer.Length);
    }
}
