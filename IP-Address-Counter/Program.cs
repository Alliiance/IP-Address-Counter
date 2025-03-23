using IP_Address_Counter;

string inputFilePath = "C:\\Users\\maksi\\Desktop\\ip_addresses";
string tempDirectory = "C:\\Users\\maksi\\Desktop\\New folder";

AddressProcessor processor = new AddressProcessor(inputFilePath, tempDirectory);
processor.Process();

long uniqueIPCount = processor.GetUniqueIPCount();
Console.WriteLine("Number of unique IP addresses: " + uniqueIPCount);