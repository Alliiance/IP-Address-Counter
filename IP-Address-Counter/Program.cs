using IP_Address_Counter;

string inputFilePath = "C:\\Users\\maksi\\Desktop\\ip_addresses"; // Specify the path file
string tempDirectory = "C:\\Users\\maksi\\Desktop\\New folder"; // Directory for temporary files

AddressProcessor processor = new AddressProcessor(inputFilePath, tempDirectory);
processor.Process().Wait();

long uniqueIPCount = processor.GetUniqueIPCount();
Console.WriteLine($"Number of unique IP addresses: {uniqueIPCount}");