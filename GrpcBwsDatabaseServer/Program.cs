// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using ProtoBuf.Grpc.Server;

namespace GrpcServices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCodeFirstGrpc();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            WebApplication app = builder.Build();
            app.MapGrpcService<BwsDatabaseService>();
            app.MapGrpcService<ExternalNamesDatabaseService>();

            app.Run();
        }
    }
}