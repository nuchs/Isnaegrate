using Grpc.Net.Client;
using Nimm2;

ITest test;

if (args.Length < 1)
{
    Console.WriteLine("You must specify a command");
    Usage();
    return -1;
}

switch (args[0])
{
    case "read":
        test = new ReadTest();
        break;

    case "write":
        test = new WriteTest();
        break;

    default:
        Console.WriteLine("Unrecognised command {args[0]} - must be one of 'read' or 'write'");
        Usage();
        return -1;
}

await test.Run();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

return 0;

void Usage()
{
    Console.WriteLine(@"
Usage:
    nimm2.exe <command> [options]

Commands
    read    Run the read test against resin
    write   Run the write test against epoxy
");
}
