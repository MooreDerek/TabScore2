// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using System.Net;

namespace GrpcServices
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