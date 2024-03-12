
using CleanUpFileHistory;

if (args.Length != 1)
{
    Console.WriteLine("Enter folder path to start");
    Console.ReadKey();
    return;
}

string folderName = args[0];
if (!Directory.Exists(folderName))
{
    Console.WriteLine($"Invalid folder path: {folderName}");
    Console.ReadKey();
    return;
}

Console.WriteLine("Start job");
try
{
    var cleaner = new Cleaner(folderName);
    cleaner.CleanUpFolder(true);
}
catch (Exception e)
{
    Console.WriteLine(e);
}
finally
{
    Console.WriteLine("End job");
    Console.ReadKey();
}



