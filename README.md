# Hashgraph API Portal

The Hashgraph API Portal is a reference implementation example for the [.NET Core Client Library for Hedera Hashgraph](https://github.com/bugbytesinc/Hashgraph).  It is written in [.NET](https://dotnet.microsoft.com/download/dotnet/6.0) using the [Blazor framework](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor).  A running instance resides at [hashgraph.bugbytes.com](https://hashgraph.bugbytes.com).  This site is open to anyone who wishes to use it to interact with the Hedera Main and Test networks and adds no additional fees beyond what the hedera network itself charges for transactions and queries.

## Compiling and Running

This project is a [.NET Core 6.0 project](https://dotnet.microsoft.com/download/dotnet/6.0) leveraging the [Blazor framework](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor).  Running a local copy is relatively easy.  First clone the project:
```
git clone https://github.com/bugbytesinc/Hashgraph-Portal.git 
```

Next change to the `Hashgraph-Portal/Hashgraph.Portal` directory.  Issue the following command to compile and run the program in one step:
```bash
dotnet run --project Hashgraph.Portal.csproj 
```
Now the project should be available at https://localhost:7153.  (Note, the server port may vary, and your browser may have issues with the development certificate, please see the .net framework [documentation](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos).


You can also load the solution file, `Hashgraph.Portal.sln` file with [Visual Studio](https://visualstudio.microsoft.com/).
