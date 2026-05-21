# Grpc_migration_to_Linux.md
# TabScore2 Linux Migration Plan - Option 2: .NET Framework 4.8 + Wine64

**Status:** READY FOR EXECUTION  
**Last Updated:** 2026-04-21  
**Strategy:** Migrate GrpcBwsDatabaseServer to .NET Framework 4.8 for proven Wine64 compatibility

---

## Executive Summary

**Problem:** 
- Windows 11 TCP connection limit (20 max) blocks tablet clients during scoring
- Cloud deployment on Linux T3.micro is cost-effective vs Windows servers
- .bws file format must be preserved for Bridge Movement Creation app compatibility

**Solution:** 
- Downgrade GrpcBwsDatabaseServer to .NET Framework 4.8 (proven Wine compatibility)
- Run database server under Wine64 on Linux
- Keep main TabScore2 app on .NET 8 (native Linux web layer)

**Why .NET Framework 4.8:**
- Excellent Wine compatibility (15+ years of testing)
- Windows ODBC Access driver works perfectly
- gRPC support via Grpc.Core package
- No commercial ODBC driver costs

**Trade-offs:**
- 150+ syntax downgrades (C# 12 → C# 7.3)
- ASP.NET Core → Grpc.Core rewrite (~30 lines)
- Minor performance reduction (~5-10%)
- Maintains .NET Framework dependency

---

## Phase 1: Code Migration to .NET Framework 4.8

### 1.1 Project File Changes

**File:** `GrpcBwsDatabaseServer/GrpcBwsDatabaseServer.csproj`

**Changes:**
```xml
<!-- BEFORE -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="protobuf-net.Grpc.AspNetCore" Version="1.1.1" />
    <PackageReference Include="System.Data.Odbc" Version="8.0.0" />
  </ItemGroup>
</Project>

<!-- AFTER -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <OutputType>Exe</OutputType>
    <Platforms>AnyCPU</Platforms>
    <LangVersion>7.3</LangVersion>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Grpc.Core" Version="2.46.6" />
    <PackageReference Include="protobuf-net.Grpc" Version="1.1.1" />
    <PackageReference Include="System.Data.Odbc" Version="6.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\GrpcSharedContracts\GrpcSharedContracts.csproj" />
  </ItemGroup>
</Project>
```

**Testing:**
```bash
# Verify project loads without errors
dotnet build GrpcBwsDatabaseServer/GrpcBwsDatabaseServer.csproj --framework net48
# Expected: Build should fail with C# syntax errors (expected at this stage)
```

---

### 1.2 Program.cs Rewrite (ASP.NET Core → Grpc.Core)

**File:** `GrpcBwsDatabaseServer/Program.cs`

**Changes:**
```csharp
// BEFORE (ASP.NET Core)
using GrpcBwsDatabaseServer.GrpcServices;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using System.Net;

namespace GrpcBwsDatabaseServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCodeFirstGrpc();
            builder.WebHost.ConfigureKestrel((context, serverOptions) =>
            {
                serverOptions.Listen(IPAddress.Loopback, 5119, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            WebApplication app = builder.Build();
            app.MapGrpcService<BwsDatabaseService>();
            app.MapGrpcService<ExternalNamesDatabaseService>();

            app.Run();
        }
    }
}

// AFTER (Grpc.Core)
using System;
using Grpc.Core;
using GrpcBwsDatabaseServer.GrpcServices;
using ProtoBuf.Grpc.Server;

namespace GrpcBwsDatabaseServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = 
                { 
                    GrpcService.BindService(new BwsDatabaseService()),
                    GrpcService.BindService(new ExternalNamesDatabaseService())
                },
                Ports = { new ServerPort("127.0.0.1", 5119, ServerCredentials.Insecure) }
            };

            server.Start();
            Console.WriteLine("GrpcBwsDatabaseServer listening on port 5119");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadLine();
            
            server.ShutdownAsync().Wait();
        }
    }
}
```

**Testing:**
```bash
# Build and verify server starts
dotnet build GrpcBwsDatabaseServer --framework net48
dotnet run --project GrpcBwsDatabaseServer --framework net48

# Expected output:
# "GrpcBwsDatabaseServer listening on port 5119"
# "Press any key to stop the server..."

# In another terminal:
netstat -an | grep 5119
# Expected: Port 5119 should be LISTENING
```

---

### 1.3 C# Syntax Downgrade (C# 12 → C# 7.3)

**Files to modify:**
- `GrpcBwsDatabaseServer/GrpcServices/BwsDatabaseService.cs` (1962 lines)
- `GrpcBwsDatabaseServer/GrpcServices/ExternalNamesDatabaseService.cs` (46 lines)
- `GrpcBwsDatabaseServer/GrpcServices/ODBCRetryHelper.cs` (32 lines)

**Changes Required (150+ instances):**

#### 1.3.1 Using Declarations → Using Statements (~50 instances)

**Pattern:**
```csharp
// BEFORE (C# 8)
using OdbcConnection connection = new(connectionString);
// code...

// AFTER (C# 7.3)
using (OdbcConnection connection = new OdbcConnection(connectionString))
{
    // code...
}
```

**BwsDatabaseService.cs affected lines:** 33, 46, 74, 88, 106, 659, 752, 794, 832, 875, 923, 1013, 1052, 1092, 1133, 1174, 1213, 1253, 1293, 1332, 1371, 1410, 1449, 1488, 1527, 1566, 1605, 1644, 1683, 1722, 1761, 1800, 1839, 1878, 1917, 1956

**ExternalNamesDatabaseService.cs affected lines:** 17

#### 1.3.2 Target-Typed New → Explicit Types (~100 instances)

**Pattern:**
```csharp
// BEFORE (C# 9)
OdbcConnectionStringBuilder cs = new() { Driver = "..." };
OdbcCommand cmd = new(SQLString, connection);
return new InitializeReturnMessage() { ReturnMessage = "..." };

// AFTER (C# 7.3)
OdbcConnectionStringBuilder cs = new OdbcConnectionStringBuilder() { Driver = "..." };
OdbcCommand cmd = new OdbcCommand(SQLString, connection);
return new InitializeReturnMessage() { ReturnMessage = "..." };
```

**All new() expressions must specify the type explicitly**

#### 1.3.3 Collection Expressions → Traditional Init (~10 instances)

**Pattern:**
```csharp
// BEFORE (C# 12)
private static readonly List<Section> sectionsList = [];
private static readonly List<Hand> handsList = [];

// AFTER (C# 7.3)
private static readonly List<Section> sectionsList = new List<Section>();
private static readonly List<Hand> handsList = new List<Hand>();
```

**BwsDatabaseService.cs affected lines:** 16, 17, 694, 885

#### 1.3.4 Nullable Annotations → Remove (~30 instances)

**Pattern:**
```csharp
// BEFORE (C# 8)
object? queryResult = cmd.ExecuteScalar();
string? pairNo = queryResult.ToString();

// AFTER (C# 7.3)
object queryResult = cmd.ExecuteScalar();
string pairNo = queryResult.ToString();
```

**Note:** Runtime behavior unchanged; just remove `?` from reference types

#### 1.3.5 Null-Coalescing Assignment → If Statement (1 instance)

**Pattern:**
```csharp
// BEFORE (C# 8)
section ??= sectionsList[0];

// AFTER (C# 7.3)
if (section == null)
{
    section = sectionsList[0];
}
```

**BwsDatabaseService.cs line:** 785

**Testing Strategy for Syntax Changes:**
```bash
# After each file is modified, build incrementally
dotnet build GrpcBwsDatabaseServer --framework net48

# Expected: Zero errors when all syntax is C# 7.3 compliant
# Run after every 20-30 changes to catch errors early

# Verify no logic changes with diff review
git diff GrpcBwsDatabaseServer/GrpcServices/
# Review: Ensure only syntax changed, no logic altered
```

---

## Phase 2: Wine64 PoC Validation

### 2.1 Build Self-Contained .NET Framework 4.8 Executable

**Create new workflow:** `.github/workflows/build-grpc-net48-poc.yml`

```yaml
name: Build GrpcBwsDatabaseServer (.NET Framework 4.8 PoC)

on:
  workflow_dispatch:

jobs:
  build:
    runs-on: windows-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v1.1

      - name: Setup NuGet
        uses: NuGet/setup-nuget@v1

      - name: Restore NuGet packages
        run: nuget restore GrpcBwsDatabaseServer/GrpcBwsDatabaseServer.csproj

      - name: Build with MSBuild
        run: msbuild GrpcBwsDatabaseServer/GrpcBwsDatabaseServer.csproj /p:Configuration=Release /p:Platform=AnyCPU

      - name: Copy dependencies
        run: |
          mkdir PoC/publish-net48
          copy GrpcBwsDatabaseServer\bin\Release\net48\*.exe PoC\publish-net48\
          copy GrpcBwsDatabaseServer\bin\Release\net48\*.dll PoC\publish-net48\
          copy GrpcBwsDatabaseServer\bin\Release\net48\*.config PoC\publish-net48\

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: GrpcBwsDatabaseServer-net48
          path: PoC/publish-net48/
          retention-days: 7
```

**Testing:**
```bash
# Trigger workflow
gh workflow run build-grpc-net48-poc.yml

# Download artifact
gh run download <run-id> -n GrpcBwsDatabaseServer-net48 -D PoC/publish-net48/

# Verify files
ls -lh PoC/publish-net48/
# Expected: GrpcBwsDatabaseServer.exe + all DLL dependencies
```

---

### 2.2 Update Wine PoC Docker Configuration

**File:** `PoC/Dockerfile.wine-poc-net48`

```dockerfile
FROM ubuntu:22.04

# ─── Environment ────────────────────────────────────────────────────────────
ENV DEBIAN_FRONTEND=noninteractive
ENV WINEARCH=win64
ENV WINEPREFIX=/root/.wine
ENV WINEDEBUG=-all
ENV HOME=/root
ENV DISPLAY=:99

# ─── System dependencies ─────────────────────────────────────────────────────
RUN apt-get update && apt-get install -y \
    wine64 \
    winetricks \
    xvfb \
    wget \
    ca-certificates \
    cabextract \
    && rm -rf /var/lib/apt/lists/*

# ─── Wine prefix initialization ──────────────────────────────────────────────
RUN xvfb-run -a wineboot --init

# ─── Install .NET Framework 4.8 ──────────────────────────────────────────────
# .NET Framework 4.8 is well-supported by Wine and has excellent compatibility
RUN xvfb-run -a winetricks -q dotnet48

# ─── Verify .NET Framework installation ──────────────────────────────────────
RUN wine64 reg query "HKLM\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full" /v Version

# ─── Install ODBC components ─────────────────────────────────────────────────
# Wine includes ODBC32.DLL; we need to register the Access driver
RUN xvfb-run -a winetricks -q mdac28

# ─── Application ─────────────────────────────────────────────────────────────
WORKDIR /app

# Copy .NET Framework 4.8 build output
COPY publish-net48/ .

# Mount point for .bws database
VOLUME ["/data"]

# ─── gRPC port ───────────────────────────────────────────────────────────────
EXPOSE 5119

# ─── Entrypoint ──────────────────────────────────────────────────────────────
CMD ["xvfb-run", "-a", "wine64", "/app/GrpcBwsDatabaseServer.exe"]
```

**File:** `PoC/docker-compose.wine-poc-net48.yml`

```yaml
version: '3.8'

services:
  grpc-database-server:
    build:
      context: .
      dockerfile: Dockerfile.wine-poc-net48
    container_name: tabscore2-grpc-wine-net48
    ports:
      - "5119:5119"
    volumes:
      - ./test-data:/data:ro
    environment:
      - WINEDEBUG=-all
    networks:
      - tabscore2-net

networks:
  tabscore2-net:
    driver: bridge
```

**Testing:**
```bash
cd PoC

# Build Docker image
docker-compose -f docker-compose.wine-poc-net48.yml build

# Expected: Build completes without errors
# Expected: "dotnet48" installation succeeds (may take 5-10 minutes)

# Start container
docker-compose -f docker-compose.wine-poc-net48.yml up -d

# Monitor logs
docker logs -f tabscore2-grpc-wine-net48

# Expected output:
# "GrpcBwsDatabaseServer listening on port 5119"
# "Press any key to stop the server..."

# If successful, Gate 1 is PASSED!
```

---

### 2.3 Gate 1 Test: .NET Framework 4.8 Runs Under Wine

**Objective:** Verify GrpcBwsDatabaseServer.exe starts successfully under Wine64

**Test Commands:**
```bash
# Check if process is running
docker exec tabscore2-grpc-wine-net48 ps aux | grep GrpcBws

# Verify port is listening
docker exec tabscore2-grpc-wine-net48 netstat -tuln | grep 5119

# Check for Wine errors
docker logs tabscore2-grpc-wine-net48 2>&1 | grep -i "error\|exception\|failed"

# Success criteria:
# ✓ No illegal instruction errors
# ✓ No .NET runtime errors
# ✓ Port 5119 is listening
# ✓ Server startup message appears in logs
```

**If Gate 1 Fails:**
- Review Wine version: `docker exec tabscore2-grpc-wine-net48 wine64 --version`
- Check .NET Framework install: `docker exec tabscore2-grpc-wine-net48 wine64 reg query "HKLM\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full"`
- Review full logs: `docker logs tabscore2-grpc-wine-net48 > gate1-failure.log`
- **Decision:** If Gate 1 fails, .NET Framework 4.8 + Wine is not viable; pivot to hybrid deployment

---

### 2.4 Gate 2 Test: Access ODBC Driver Works in Wine

**Objective:** Verify Wine's ODBC32 can connect to .bws file with read/write operations

**Prerequisites:**
```bash
# Place a test .bws file
cp /path/to/test-database.bws PoC/test-data/TestScoring.bws
```

**Test Setup - Create minimal test client:**

**File:** `PoC/test-client/TestOdbcConnection.cs`

```csharp
using System;
using System.Data.Odbc;

class TestOdbcConnection
{
    static void Main(string[] args)
    {
        string dbPath = args.Length > 0 ? args[0] : "/data/TestScoring.bws";
        
        OdbcConnectionStringBuilder cs = new OdbcConnectionStringBuilder();
        cs.Driver = "Microsoft Access Driver (*.mdb)";
        cs.Add("Dbq", dbPath);
        cs.Add("Uid", "Admin");
        cs.Add("Pwd", string.Empty);
        
        Console.WriteLine($"Testing connection to: {dbPath}");
        Console.WriteLine($"Connection string: {cs.ToString()}");
        
        try
        {
            using (OdbcConnection connection = new OdbcConnection(cs.ToString()))
            {
                connection.Open();
                Console.WriteLine("✓ Connection successful");
                
                // Test read
                using (OdbcCommand cmd = new OdbcCommand("SELECT TOP 1 * FROM Section", connection))
                {
                    using (OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("✓ Read operation successful");
                        }
                    }
                }
                
                // Test write (add a test field)
                using (OdbcCommand cmd = new OdbcCommand("ALTER TABLE Section ADD TestField SHORT", connection))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("✓ Write operation successful");
                    }
                    catch (OdbcException ex)
                    {
                        if (ex.Errors[0].SQLState == "HYS21") // Field already exists
                        {
                            Console.WriteLine("✓ Write operation successful (field exists)");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                
                connection.Close();
                Console.WriteLine("✓ All tests passed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
```

**Build and run test:**
```bash
# On Windows (or in Windows Docker container)
csc /target:exe /out:PoC/publish-net48/TestOdbcConnection.exe PoC/test-client/TestOdbcConnection.cs

# Copy to Docker publish directory and rebuild image
docker-compose -f PoC/docker-compose.wine-poc-net48.yml build

# Run ODBC test inside container
docker exec tabscore2-grpc-wine-net48 wine64 /app/TestOdbcConnection.exe /data/TestScoring.bws

# Success criteria:
# ✓ Connection successful
# ✓ Read operation successful
# ✓ Write operation successful
# ✓ All tests passed!
```

**If Gate 2 Fails:**
- Check ODBC driver install: `docker exec tabscore2-grpc-wine-net48 wine64 odbcinst -q -d`
- Verify .bws file is readable: `docker exec tabscore2-grpc-wine-net48 ls -lh /data/`
- Test with mdbtools: `docker exec tabscore2-grpc-wine-net48 mdb-tables /data/TestScoring.bws`
- **Decision:** If Gate 2 fails, Wine ODBC is not viable; evaluate alternative ODBC solutions

---

### 2.5 Gate 3 Test: gRPC Endpoint Reachable from Native Linux

**Objective:** Verify gRPC calls from native Linux .NET 8 client to Wine-hosted server

**Test Client Setup:**

**File:** `PoC/test-client/GrpcTestClient.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Grpc.Net.Client" Version="2.63.0" />
    <PackageReference Include="protobuf-net.Grpc" Version="1.1.1" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\GrpcSharedContracts\GrpcSharedContracts.csproj" />
  </ItemGroup>
</Project>
```

**File:** `PoC/test-client/Program.cs`

```csharp
using Grpc.Net.Client;
using GrpcSharedContracts;
using ProtoBuf.Grpc.Client;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string serverAddress = args.Length > 0 ? args[0] : "http://localhost:5119";
        
        Console.WriteLine($"Connecting to gRPC server at: {serverAddress}");
        
        using var channel = GrpcChannel.ForAddress(serverAddress);
        var client = channel.CreateGrpcService<IBwsDatabaseService>();
        
        try
        {
            // Test Initialize call
            var initMessage = new InitializeMessage { PathToDatabase = "/data/TestScoring.bws" };
            var response = await Task.Run(() => client.Initialize(initMessage));
            
            Console.WriteLine($"✓ gRPC call successful");
            Console.WriteLine($"  Response: {response.ReturnMessage}");
            
            // Test GetSectionsList
            var sections = await Task.Run(() => client.GetSectionsList());
            Console.WriteLine($"✓ GetSectionsList returned {sections.Count} sections");
            
            Console.WriteLine("✓ All gRPC tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ gRPC test failed: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
```

**Run test:**
```bash
# Build test client
dotnet build PoC/test-client/GrpcTestClient.csproj

# Run against Wine-hosted server
dotnet run --project PoC/test-client/GrpcTestClient.csproj -- http://localhost:5119

# Success criteria:
# ✓ gRPC call successful
# ✓ GetSectionsList returned N sections
# ✓ All gRPC tests passed!
```

**Alternative: Test from macOS/Linux host:**
```bash
# If running on macOS with Docker
dotnet run --project PoC/test-client/GrpcTestClient.csproj -- http://localhost:5119

# Expected: Same successful output as above
```

---

## Phase 3: Integration with Main TabScore2 App

### 3.1 Update GrpcSharedContracts for Multi-Targeting

**Objective:** Ensure GrpcSharedContracts works with both .NET 8 and .NET Framework 4.8

**File:** `GrpcSharedContracts/GrpcSharedContracts.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net48</TargetFrameworks>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="protobuf-net.Grpc" Version="1.1.1" />
  </ItemGroup>
</Project>
```

**Testing:**
```bash
# Build for both targets
dotnet build GrpcSharedContracts

# Expected output:
# Build succeeded for net8.0
# Build succeeded for net48

# Verify DLLs
ls -lh GrpcSharedContracts/bin/Release/net8.0/
ls -lh GrpcSharedContracts/bin/Release/net48/
```

---

### 3.2 Test Full Stack: Native Linux Web App → Wine DB Server

**Objective:** Verify complete application flow with native Linux web + Wine database server

**Docker Compose Full Stack:**

**File:** `PoC/docker-compose.full-stack.yml`

```yaml
version: '3.8'

services:
  grpc-database-server:
    build:
      context: .
      dockerfile: Dockerfile.wine-poc-net48
    container_name: tabscore2-grpc-wine-net48
    volumes:
      - ./test-data:/data:ro
    environment:
      - WINEDEBUG=-all
    networks:
      - tabscore2-net

  web-app:
    build:
      context: ..
      dockerfile: PoC/Dockerfile.webapp-linux
    container_name: tabscore2-webapp-linux
    ports:
      - "5213:5213"
    depends_on:
      - grpc-database-server
    environment:
      - GRPC_SERVER=grpc-database-server:5119
      - DATABASE_PATH=/data/TestScoring.bws
    networks:
      - tabscore2-net

networks:
  tabscore2-net:
    driver: bridge
```

**File:** `PoC/Dockerfile.webapp-linux`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy published web app (build separately)
COPY TabScore2/bin/Release/net8.0/linux-x64/publish/ .

EXPOSE 5213

ENTRYPOINT ["dotnet", "TabScore2.dll"]
```

**Testing:**
```bash
# Build TabScore2 web app for Linux
dotnet publish TabScore2 -c Release -r linux-x64 --self-contained false

# Start full stack
cd PoC
docker-compose -f docker-compose.full-stack.yml up -d

# Monitor both containers
docker logs -f tabscore2-grpc-wine-net48 &
docker logs -f tabscore2-webapp-linux &

# Test web endpoint
curl http://localhost:5213/StartScreen

# Success criteria:
# ✓ Both containers running
# ✓ No gRPC connection errors in webapp logs
# ✓ Database operations succeed
# ✓ Web pages load correctly
```

**Functional Test Checklist:**
```bash
# Open in browser
open http://localhost:5213/StartScreen

# Test workflow:
# 1. [ ] Load start screen
# 2. [ ] Select section
# 3. [ ] Enter table number
# 4. [ ] View hand records (tests database read)
# 5. [ ] Enter a result (tests database write)
# 6. [ ] View rankings (tests database query)
# 7. [ ] Confirm no errors in Docker logs
```

---

## Phase 4: Performance & Stability Testing

### 4.1 Load Testing: Concurrent Connections

**Objective:** Verify Wine-hosted server handles >20 concurrent connections (solves Windows limit)

**Test Script:** `PoC/test-client/LoadTest.sh`

```bash
#!/bin/bash

# Load test: Spawn 50 concurrent gRPC clients
CONCURRENT_CLIENTS=50
SERVER_URL="http://localhost:5119"

echo "Starting load test with $CONCURRENT_CLIENTS concurrent clients..."

for i in $(seq 1 $CONCURRENT_CLIENTS); do
    dotnet run --project PoC/test-client/GrpcTestClient.csproj -- $SERVER_URL &
done

wait

echo "Load test complete!"
```

**Run test:**
```bash
chmod +x PoC/test-client/LoadTest.sh
./PoC/test-client/LoadTest.sh

# Monitor container resources
docker stats tabscore2-grpc-wine-net48

# Success criteria:
# ✓ All 50 clients connect successfully
# ✓ No connection refused errors
# ✓ CPU usage <80%
# ✓ Memory usage stable
# ✓ No crashes or hangs
```

---

### 4.2 Data Integrity Testing

**Objective:** Verify Wine ODBC writes are committed correctly to .bws file

**Test Script:** `PoC/test-client/DataIntegrityTest.cs`

```csharp
using System;
using System.Data.Odbc;

class DataIntegrityTest
{
    static void Main(string[] args)
    {
        string dbPath = "/data/TestScoring.bws";
        string testValue = $"TestData_{DateTime.Now.Ticks}";
        
        // Write test data via Wine ODBC
        WriteData(dbPath, testValue);
        
        // Wait for commit
        System.Threading.Thread.Sleep(1000);
        
        // Read back and verify
        string readValue = ReadData(dbPath);
        
        if (readValue == testValue)
        {
            Console.WriteLine("✓ Data integrity verified!");
        }
        else
        {
            Console.WriteLine($"✗ Data mismatch! Expected: {testValue}, Got: {readValue}");
            Environment.Exit(1);
        }
    }
    
    static void WriteData(string dbPath, string value)
    {
        // Connection string setup...
        // INSERT INTO test table...
    }
    
    static string ReadData(string dbPath)
    {
        // Connection string setup...
        // SELECT FROM test table...
    }
}
```

**Run test:**
```bash
# Run inside Wine container
docker exec tabscore2-grpc-wine-net48 wine64 /app/DataIntegrityTest.exe

# Verify .bws file is modified
docker exec tabscore2-grpc-wine-net48 ls -lh /data/TestScoring.bws

# Compare file checksum before/after
docker exec tabscore2-grpc-wine-net48 md5sum /data/TestScoring.bws
```

---

### 4.3 Memory Leak & Stability Testing

**Objective:** Verify server runs stably for extended periods

**Test:**
```bash
# Start server
docker-compose -f PoC/docker-compose.wine-poc-net48.yml up -d

# Monitor memory over 8 hours
for i in {1..480}; do
    docker stats --no-stream tabscore2-grpc-wine-net48 >> memory-test.log
    sleep 60
done

# Analyze memory trends
grep Memory memory-test.log | awk '{print $4}' | sort -n

# Success criteria:
# ✓ Memory usage does not grow unbounded
# ✓ Server remains responsive after 8+ hours
# ✓ No crashes or freezes
```

---

## Phase 5: Main Application Migration (Deferred Windows Forms)

### 5.1 Target Framework Change

**File:** `TabScore2/TabScore2.csproj`

**Changes:**
```xml
<!-- BEFORE -->
<TargetFramework>net8.0-windows</TargetFramework>
<UseWindowsForms>true</UseWindowsForms>

<!-- AFTER -->
<TargetFramework>net8.0</TargetFramework>
<!-- Remove UseWindowsForms for Linux build -->
```

**Conditional Compilation for Windows Forms:**
```xml
<PropertyGroup Condition="'$(OS)' == 'Windows_NT'">
  <DefineConstants>$(DefineConstants);WINDOWS_DESKTOP</DefineConstants>
</PropertyGroup>

<ItemGroup Condition="'$(OS)' == 'Windows_NT'">
  <UseWindowsForms>true</UseWindowsForms>
</ItemGroup>
```

**Testing:**
```bash
# Build for Linux
dotnet build TabScore2 -c Release -r linux-x64

# Expected: Build succeeds with warnings about WinForms
# Windows Forms features will be unavailable on Linux (expected)
```

---

### 5.2 App.config → appsettings.json Migration

**Create:** `TabScore2/appsettings.json`

```json
{
  "AppSettings": {
    "TabletsMove": false,
    "ShowTimer": true,
    "SecondsPerBoard": 390,
    "AdditionalSecondsPerRound": 60,
    "ShowHandRecordFromDirection": "South",
    "DoubleDummy": true,
    "SuppressRankingListForLastXRounds": 0,
    "SuppressRankingListForFirstXRounds": 2,
    "DatabaseReady": false,
    "IsIndividual": false,
    "SessionStarted": false,
    "ShowSplashScreen": true,
    "DefaultShowTraveller": true,
    "DefaultShowPercentage": true,
    "DefaultEnterLeadCard": true,
    "DefaultValidateLeadCard": true,
    "DefaultShowRanking": 1,
    "DefaultEnterResultsMethod": 1,
    "DefaultShowHandRecord": true,
    "DefaultNumberEntryEachRound": false,
    "DefaultNameSource": 0,
    "DefaultManualHandRecordEntry": false
  },
  "GrpcServer": {
    "Host": "localhost",
    "Port": 5119
  }
}
```

**Update:** `TabScore2/DataServices/Settings.cs`

```csharp
// Add IConfiguration dependency
private readonly IConfiguration _configuration;

public Settings(IConfiguration configuration)
{
    _configuration = configuration;
}

// Replace ConfigurationManager with IConfiguration
public bool TabletsMove 
{ 
    get => _configuration.GetValue<bool>("AppSettings:TabletsMove");
    set => _configuration["AppSettings:TabletsMove"] = value.ToString();
}
```

**Testing:**
```bash
# Verify settings load correctly
dotnet run --project TabScore2

# Check logs for configuration errors
# Expected: No "configuration not found" errors
```

---

### 5.3 Conditional Windows Forms Compilation

**File:** `TabScore2/Program.cs`

```csharp
#if WINDOWS_DESKTOP
using System.Windows.Forms;
#endif

public static void Main(string[] args)
{
#if WINDOWS_DESKTOP
    // Windows Forms startup (existing code)
    Application.Run(services.GetRequiredService<MainForm>());
#else
    // Linux: Start web server only
    var builder = WebApplication.CreateBuilder(args);
    // ... existing web setup ...
    app.Run();
#endif
}
```

**Testing:**
```bash
# Build for Windows (WinForms enabled)
dotnet build TabScore2 -c Release -r win-x64

# Build for Linux (WinForms disabled)
dotnet build TabScore2 -c Release -r linux-x64

# Expected: Both builds succeed
```

---

## Phase 6: Cloud Deployment Configuration

### 6.1 AWS T3.micro Docker Deployment

**File:** `docker-compose.production.yml`

```yaml
version: '3.8'

services:
  grpc-database-server:
    image: tabscore2-grpc-net48:latest
    container_name: tabscore2-grpc
    restart: unless-stopped
    volumes:
      - ./data:/data
      - ./logs-grpc:/logs
    environment:
      - WINEDEBUG=-all
    networks:
      - tabscore2-net

  web-app:
    image: tabscore2-webapp:latest
    container_name: tabscore2-web
    restart: unless-stopped
    ports:
      - "80:5213"
    depends_on:
      - grpc-database-server
    environment:
      - ASPNETCORE_URLS=http://+:5213
      - GRPC_SERVER=grpc-database-server:5119
    volumes:
      - ./logs-web:/logs
    networks:
      - tabscore2-net

networks:
  tabscore2-net:
    driver: bridge
```

**Deployment Script:** `deploy-aws.sh`

```bash
#!/bin/bash

# Deploy to AWS T3.micro (Amazon Linux 2)

# Install Docker
sudo yum update -y
sudo yum install docker -y
sudo service docker start
sudo usermod -a -G docker ec2-user

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Pull images
docker pull tabscore2-grpc-net48:latest
docker pull tabscore2-webapp:latest

# Create data directories
mkdir -p /opt/tabscore2/data
mkdir -p /opt/tabscore2/logs-grpc
mkdir -p /opt/tabscore2/logs-web

# Copy .bws file
scp user@laptop:/path/to/scoring.bws /opt/tabscore2/data/

# Start services
cd /opt/tabscore2
docker-compose -f docker-compose.production.yml up -d

# Verify
docker ps
curl http://localhost/StartScreen
```

**Testing:**
```bash
# Launch T3.micro instance
aws ec2 run-instances --image-id ami-xxx --instance-type t3.micro

# SSH and deploy
ssh ec2-user@<instance-ip>
bash deploy-aws.sh

# Test from browser
open http://<instance-ip>/StartScreen

# Load test >20 connections
./PoC/test-client/LoadTest.sh http://<instance-ip>:80

# Success criteria:
# ✓ 50+ concurrent connections succeed
# ✓ No connection limits
# ✓ Instance cost <$10/month
```

---

### 6.2 Systemd Service Configuration (Alternative to Docker)

**File:** `/etc/systemd/system/tabscore2-grpc.service`

```ini
[Unit]
Description=TabScore2 gRPC Database Server (Wine/.NET Framework 4.8)
After=network.target

[Service]
Type=simple
User=tabscore2
WorkingDirectory=/opt/tabscore2
Environment="WINEPREFIX=/opt/tabscore2/.wine"
Environment="WINEARCH=win64"
Environment="DISPLAY=:99"
ExecStartPre=/usr/bin/Xvfb :99 -screen 0 1024x768x16 &
ExecStart=/usr/bin/wine64 /opt/tabscore2/GrpcBwsDatabaseServer.exe
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**File:** `/etc/systemd/system/tabscore2-web.service`

```ini
[Unit]
Description=TabScore2 Web Application (.NET 8)
After=network.target tabscore2-grpc.service

[Service]
Type=simple
User=tabscore2
WorkingDirectory=/opt/tabscore2/web
Environment="ASPNETCORE_URLS=http://+:5213"
Environment="GRPC_SERVER=localhost:5119"
ExecStart=/usr/bin/dotnet /opt/tabscore2/web/TabScore2.dll
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**Testing:**
```bash
# Enable and start services
sudo systemctl daemon-reload
sudo systemctl enable tabscore2-grpc
sudo systemctl enable tabscore2-web
sudo systemctl start tabscore2-grpc
sudo systemctl start tabscore2-web

# Check status
sudo systemctl status tabscore2-grpc
sudo systemctl status tabscore2-web

# View logs
sudo journalctl -u tabscore2-grpc -f
sudo journalctl -u tabscore2-web -f

# Success criteria:
# ✓ Both services running
# ✓ Auto-restart on failure
# ✓ Logs show no errors
```

---

## Phase 7: .bws File Workflow Integration

### 7.1 Pre-Event: Import .bws from Bridge Movement Creation

**Workflow:**
```bash
# On Windows laptop: Generate .bws with Bridge Movement Creation app
# (Manual process - no changes needed)

# Transfer .bws to Linux server
scp scoring-event-2026-04-21.bws ec2-user@<server>:/opt/tabscore2/data/

# Or: Mount shared volume if using Docker
docker cp scoring-event-2026-04-21.bws tabscore2-grpc:/data/
```

**Testing:**
```bash
# Verify file is readable
docker exec tabscore2-grpc ls -lh /data/

# Test initialization
curl -X POST http://<server>/StartScreen

# Expected: Event data loads correctly
```

---

### 7.2 Post-Event: Export .bws for Processing

**Workflow:**
```bash
# After event: Stop services to ensure .bws is flushed
docker-compose -f docker-compose.production.yml stop

# Download .bws file
scp ec2-user@<server>:/opt/tabscore2/data/scoring-event-2026-04-21.bws ./

# Process with Bridge Movement Creation app on Windows
# (Manual process - no changes needed)
```

**Testing:**
```bash
# Verify file integrity
md5sum scoring-event-2026-04-21.bws

# Open in Bridge Movement Creation app on Windows
# Expected: All results are present and correct
```

---

### 7.3 Data Integrity Validation Script

**File:** `validate-bws.sh`

```bash
#!/bin/bash

BWS_FILE=$1

echo "Validating .bws file: $BWS_FILE"

# Check file size
SIZE=$(stat -f%z "$BWS_FILE" 2>/dev/null || stat -c%s "$BWS_FILE" 2>/dev/null)
if [ $SIZE -lt 1000 ]; then
    echo "✗ File too small ($SIZE bytes) - possibly corrupt"
    exit 1
fi

# Check file type (should be Microsoft Access database)
FILE_TYPE=$(file "$BWS_FILE")
if [[ $FILE_TYPE != *"Microsoft Access Database"* ]]; then
    echo "✗ Not a valid Access database"
    exit 1
fi

# Use mdbtools to verify structure (if available)
if command -v mdb-tables &> /dev/null; then
    TABLES=$(mdb-tables "$BWS_FILE")
    if [[ $TABLES == *"Section"* ]] && [[ $TABLES == *"RoundData"* ]]; then
        echo "✓ Database structure valid"
    else
        echo "✗ Missing required tables"
        exit 1
    fi
fi

echo "✓ Validation complete"
```

**Testing:**
```bash
chmod +x validate-bws.sh

# Test with known good file
./validate-bws.sh test-data/TestScoring.bws

# Test with corrupted file
./validate-bws.sh test-data/corrupted.bws
# Expected: Error detection
```

---

## Phase 8: Monitoring & Maintenance

### 8.1 Health Check Endpoint

**Add to TabScore2 web app:** `Controllers/HealthController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using GrpcSharedContracts;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IBwsDatabaseService _dbService;
    
    public HealthController(IBwsDatabaseService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            // Test gRPC connectivity
            var sections = _dbService.GetSectionsList();
            
            return Ok(new 
            { 
                status = "healthy",
                grpc = "connected",
                sections = sections.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
```

**Testing:**
```bash
# Check health
curl http://localhost:5213/health

# Expected output:
# {"status":"healthy","grpc":"connected","sections":2,"timestamp":"2026-04-21T19:00:00Z"}

# Set up monitoring
watch -n 10 curl -s http://localhost:5213/health
```

---

### 8.2 Log Aggregation

**File:** `docker-compose.production.yml` (updated with logging)

```yaml
services:
  grpc-database-server:
    # ... existing config ...
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  web-app:
    # ... existing config ...
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

**Log analysis script:** `analyze-logs.sh`

```bash
#!/bin/bash

echo "=== Recent Errors ==="
docker logs tabscore2-grpc 2>&1 | grep -i "error\|exception" | tail -20

echo "=== gRPC Connection Stats ==="
docker logs tabscore2-web | grep -c "gRPC call"

echo "=== Wine Warnings ==="
docker logs tabscore2-grpc | grep -i "wine:" | tail -10

echo "=== Container Uptime ==="
docker ps --format "{{.Names}}: {{.Status}}"
```

**Testing:**
```bash
chmod +x analyze-logs.sh
./analyze-logs.sh

# Schedule daily reports
crontab -e
# Add: 0 2 * * * /opt/tabscore2/analyze-logs.sh > /opt/tabscore2/daily-report.txt
```

---

## Phase 9: Rollback Plan

### 9.1 If Wine Approach Fails

**Fallback to Hybrid Deployment:**

1. Keep GrpcBwsDatabaseServer on Windows 11 laptop (.NET 8, no migration needed)
2. Deploy TabScore2 web app to Linux cloud
3. Use secure tunnel (SSH, WireGuard, or Tailscale) for gRPC communication

**File:** `docker-compose.hybrid.yml`

```yaml
version: '3.8'

services:
  web-app:
    image: tabscore2-webapp:latest
    container_name: tabscore2-web
    restart: unless-stopped
    ports:
      - "80:5213"
    environment:
      - ASPNETCORE_URLS=http://+:5213
      - GRPC_SERVER=<laptop-tailscale-ip>:5119  # Laptop's Tailscale IP
```

**Laptop Setup (Windows 11):**
```powershell
# Install Tailscale
winget install Tailscale.Tailscale

# Start GrpcBwsDatabaseServer
cd C:\Projects\TabScore2
dotnet run --project GrpcBwsDatabaseServer

# Allow firewall
netsh advfirewall firewall add rule name="TabScore2 gRPC" dir=in action=allow protocol=TCP localport=5119
```

**Testing:**
```bash
# From Linux server
tailscale ping <laptop-hostname>

# Test gRPC connectivity
dotnet run --project PoC/test-client/GrpcTestClient.csproj -- http://<laptop-tailscale-ip>:5119

# Deploy web app
docker-compose -f docker-compose.hybrid.yml up -d
```

---

### 9.2 Revert to Full Windows Deployment

**If all Linux approaches fail:**

```bash
# Build Windows deployments
dotnet publish GrpcBwsDatabaseServer -c Release -r win-x64 --self-contained
dotnet publish TabScore2 -c Release -r win-x64 --self-contained

# Deploy to Windows Server (Azure/AWS)
# AWS EC2 Windows: ~$50-70/month (vs T3.micro Linux $8/month)
```

---

## Success Criteria

### Phase 1 (Code Migration)
- ✓ GrpcBwsDatabaseServer builds successfully targeting net48
- ✓ Zero C# compilation errors
- ✓ Grpc.Core server starts on Windows

### Phase 2 (Wine PoC)
- ✓ Gate 1: .NET Framework 4.8 exe runs under Wine64
- ✓ Gate 2: ODBC connects to .bws with read/write
- ✓ Gate 3: gRPC calls from native Linux client succeed

### Phase 3 (Integration)
- ✓ Full stack (web + db) runs in Docker
- ✓ Web pages load without errors
- ✓ Results can be entered and retrieved

### Phase 4 (Performance)
- ✓ 50+ concurrent connections succeed
- ✓ Response time <500ms for typical queries
- ✓ Memory usage stable over 8 hours

### Phase 5 (Main App Migration)
- ✓ TabScore2 builds for linux-x64
- ✓ Settings load from appsettings.json
- ✓ Web layer functional without WinForms

### Phase 6 (Deployment)
- ✓ AWS T3.micro deployment successful
- ✓ Monthly cost <$10
- ✓ 99%+ uptime over 1 week test

### Phase 7 (.bws Workflow)
- ✓ .bws file imports correctly
- ✓ Post-event export preserves all data
- ✓ Bridge Movement Creation app processes file

### Phase 8 (Monitoring)
- ✓ Health checks pass consistently
- ✓ Logs show no recurring errors
- ✓ Alerts configured for failures

---

## Timeline Estimate

| Phase | Estimated Time | Dependencies |
|-------|----------------|--------------|
| Phase 1: Code Migration | 8-12 hours | None |
| Phase 2: Wine PoC | 4-6 hours | Phase 1 |
| Phase 3: Integration | 2-4 hours | Phase 2 |
| Phase 4: Performance Testing | 12+ hours (mostly waiting) | Phase 3 |
| Phase 5: Main App Migration | 4-6 hours | Phase 2 |
| Phase 6: Deployment Setup | 2-4 hours | Phase 5 |
| Phase 7: Workflow Testing | 2-3 hours | Phase 6 |
| Phase 8: Monitoring | 2-3 hours | Phase 6 |
| **Total** | **36-50 hours** | Sequential |

**Critical Path:** Phase 1 → Phase 2 Gate Tests → Phase 3 → Production

---

## Risk Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Wine Gate 1 failure | Low | High | Proven .NET Framework 4.8 compatibility |
| Wine Gate 2 failure | Medium | High | Fallback: Hybrid deployment (laptop + cloud) |
| Performance degradation | Low | Medium | Extensive load testing in Phase 4 |
| Data corruption | Low | Critical | Validation scripts + backup procedures |
| Cloud deployment issues | Low | Low | Docker ensures portability |

---

## Next Steps

1. **Review and approve this plan**
2. **Begin Phase 1: Code Migration** to .NET Framework 4.8
3. **Execute Wine PoC** (Phase 2) to validate approach
4. **Decide on fallback** if Wine fails (hybrid vs full Windows)

---

**END OF PLAN**
