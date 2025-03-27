namespace IP_Address_Counter
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

            // Split the file into parts and sort them in parallel
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
                var sortedDict = new SortedDictionary<string, Queue<int>>();

                // Initialize dictionary with the first values from each file
                for (int i = 0; i < readers.Count; i++)
                {
                    if (!readers[i].EndOfStream)
                    {
                        string line = readers[i].ReadLine();
                        if (!sortedDict.ContainsKey(line))
                            sortedDict[line] = new Queue<int>();

                        sortedDict[line].Enqueue(i);
                    }
                }

                string lastWrittenLine = null;

                while (sortedDict.Count > 0)
                {
                    var minKey = sortedDict.Keys.First(); // Get the smallest key
                    int fileIndex = sortedDict[minKey].Dequeue(); // Retrieve file index

                    // Write only unique values to the output file
                    if (minKey != lastWrittenLine)
                    {
                        writer.WriteLine(minKey);
                        lastWrittenLine = minKey;
                    }

                    // Remove key if no more associated files
                    if (sortedDict[minKey].Count == 0)
                        sortedDict.Remove(minKey);

                    // Read the next line from the same file
                    if (!readers[fileIndex].EndOfStream)
                    {
                        string newLine = readers[fileIndex].ReadLine();
                        if (!sortedDict.ContainsKey(newLine))
                            sortedDict[newLine] = new Queue<int>();

                        sortedDict[newLine].Enqueue(fileIndex);
                    }
                }

                // Close all file readers
                foreach (var reader in readers)
                {
                    reader.Close();
                }
            }
        }
    }
}