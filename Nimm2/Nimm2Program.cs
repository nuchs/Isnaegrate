using Nimm2;

try
{
    if (args.Length < 1)
    {
        throw new ArgumentException("You provide at least one argument");
    }

    ITest test = args[0] switch
    {
        "sub" => new SubscribeTest(),
        "read" => new ReadTest(),
        "write" => new WriteTest(),
        "users" => new UsersTest(args),
        _ => throw new ArgumentException($"Unrecognised command {args[0]} - must be one of 'read', 'sub', 'users' or 'write'")
    };

    await test.Run();

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    return 0;
}
catch (ArgumentException ex)
{
    Usage(ex.Message);
    return -1;
}
catch(Exception ex)
{
    Console.WriteLine("An error occurred");
    Console.WriteLine(ex.Message);
    return -2;
}


void Usage(string error)
{
    Console.WriteLine(error);
    Console.WriteLine(@"
Usage:
    nimm2.exe <command> [options]

Commands
    read        Run the read test against resin
    sub         Run the subscription test against epoxy
    write       Run the write test against epoxy
    user [id]   Run the users tests against the Jaundiced Sage
                    - if no id is specifed all uses will be returned
                    - if an id is returned then the user with that id will be 
                      returned (if they exist)
");
}
