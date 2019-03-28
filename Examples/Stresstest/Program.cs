using System;
using System.IO;
using System.Threading.Tasks;
using Leaf.xNet;

namespace Stresstest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const int threads = 100;
            const string resultFile = "result.txt";
            const string sitesFile = "sites.txt";

            var results = TextWriter.Synchronized(new StreamWriter(resultFile, false));
            var sites = File.ReadAllText(sitesFile).Split(new [] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            
            Parallel.ForEach(sites, new ParallelOptions { MaxDegreeOfParallelism = threads }, siteUrl => {
                HttpRequest req = null;

                try
                {
                    req = new HttpRequest();
                    string resp = req.Get(siteUrl).ToString();
                }
                catch (Exception ex)
                {
                    string err = $"[ERROR]: {siteUrl}{Environment.NewLine}{ex.Message}";
                    Console.WriteLine(err);
                    Console.WriteLine();

                    results.WriteLine(err);
                    results.WriteLine();
                    results.Flush();
                }
                finally
                {
                    req?.Dispose();
                }
            });

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

    }
}
