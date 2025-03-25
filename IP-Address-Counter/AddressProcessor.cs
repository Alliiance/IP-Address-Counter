﻿namespace IP_Address_Counter
{
    public class AddressProcessor
    {
        private string _inputFilePath;
        private string _tempDirectory;
        private string _outputFilePath;

        public AddressProcessor(string inputFilePath, string tempDirectory)
        {
            _inputFilePath = inputFilePath;
            _tempDirectory = tempDirectory;
            _outputFilePath = "unique_ips.txt";
        }

        public async Task Process()
        {
            Directory.CreateDirectory(_tempDirectory);

            //Split the file into parts and sort them in parallel
            await SplitAndSortFileAsync();

            // Merge the sorted parts
            MergeSortedFiles();
        }

        public long GetUniqueIPCount()
        {
            return File.ReadLines(_outputFilePath).Count();
        }

        private async Task SplitAndSortFileAsync()
        {
            const int linesPerFile = 1000000; // 1 million lines per part
            var tasks = new List<Task>();

            using (StreamReader reader = new StreamReader(_inputFilePath))
            {
                string line;
                List<string> buffer = new List<string>();
                int fileIndex = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    buffer.Add(line);
                    if (buffer.Count >= linesPerFile)
                    {
                        var localBuffer = new List<string>(buffer);
                        tasks.Add(Task.Run(() =>
                        {
                            localBuffer.Sort();
                            WriteSortedFile(fileIndex++, localBuffer);
                        }));
                        buffer.Clear();
                    }
                }

                // Process remaining lines
                if (buffer.Count > 0)
                {
                    buffer.Sort();
                    WriteSortedFile(fileIndex, buffer);
                }
            }

            await Task.WhenAll(tasks);
        }

        private void WriteSortedFile(int fileIndex, List<string> lines)
        {
            string filePath = Path.Combine(_tempDirectory, $"sorted_part_{fileIndex}.txt");
            File.WriteAllLines(filePath, lines);
        }

        private void MergeSortedFiles()
        {
            var files = Directory.GetFiles(_tempDirectory, "sorted_part_*.txt");
            using (StreamWriter writer = new StreamWriter(_outputFilePath))
            {
                List<StreamReader> readers = files.Select(file => new StreamReader(file)).ToList();
                bool allFilesEnded = false;

                string lastWrittenLine = null;

                while (!allFilesEnded)
                {
                    string minLine = null;
                    int minFileIndex = -1;

                    for (int i = 0; i < readers.Count; i++)
                    {
                        if (readers[i].EndOfStream) continue;

                        string line = readers[i].ReadLine();
                        if (minLine == null || string.Compare(line, minLine) < 0)
                        {
                            minLine = line;
                            minFileIndex = i;
                        }
                    }

                    if (minLine != null && minLine != lastWrittenLine)
                    {
                        writer.WriteLine(minLine);
                        lastWrittenLine = minLine;
                    }
                    else
                    {
                        allFilesEnded = true;
                    }
                }
            }
        }
    }
}