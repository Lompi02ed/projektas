
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;

class Master
{
    private static Dictionary<string, Dictionary<string, int>> globalWordCounts = new();

    static void Main(string[] args)
    {
        Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << 0);

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Master.exe <pipe1> <pipe2>");
            return;
        }

        string pipe1 = args[0];
        string pipe2 = args[1];

        Thread t1 = new Thread(() => HandlePipe(pipe1));
        Thread t2 = new Thread(() => HandlePipe(pipe2));

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        PrintResults();
    }

    static void HandlePipe(string pipeName)
    {
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
        server.WaitForConnection();

        using var ms = new MemoryStream();
        server.CopyTo(ms);

        string json = Encoding.UTF8.GetString(ms.ToArray());
        var indexList = JsonSerializer.Deserialize<List<WordIndex>>(json);

        lock (globalWordCounts)
        {
            foreach (var fileIndex in indexList)
            {
                if (!globalWordCounts.ContainsKey(fileIndex.FileName))
                    globalWordCounts[fileIndex.FileName] = new Dictionary<string, int>();

                foreach (var pair in fileIndex.WordCounts)
                {
                    if (!globalWordCounts[fileIndex.FileName].ContainsKey(pair.Key))
                        globalWordCounts[fileIndex.FileName][pair.Key] = 0;
                    globalWordCounts[fileIndex.FileName][pair.Key] += pair.Value;
                }
            }
        }
    }

    static void PrintResults()
    {
        foreach (var file in globalWordCounts)
        {
            foreach (var word in file.Value)
            {
                Console.WriteLine($"{file.Key}:{word.Key}:{word.Value}");
            }
        }
    }
}
