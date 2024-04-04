// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using ProtoBuf.Grpc.ClientFactory;
using SharedContracts;
using System.Diagnostics;
using System.Net;
using TabScore2.DataServices;
using TabScore2.Forms;
using TabScore2.UtilityServices;

namespace TabScore2
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.CurrentCultureIgnoreCase);

            // -----------------------------------------------------------------------------------
            // Get local IP address (there must be one for TabScore2 to work) and set gRCP address
            // -----------------------------------------------------------------------------------
            string ipAddress = "";
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in entry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
            if (ipAddress == "")
            {
                throw new Exception("No network connection");
            }
            Uri grpcAddress = new($"http://{ipAddress}:5119");

            // -------------------------
            // Start gRPC server process
            // -------------------------
            Process grpcServer = new();
            string workingDirectory;
            if(isDevelopment)
            {
                string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory)!.FullName;
                workingDirectory = Path.Combine(solutionDirectory, @"GrpcBwsDatabaseServer\bin\x86\Debug\net8.0");
                grpcServer.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }
            else
            {
                string startupFolder = Application.StartupPath;
                workingDirectory = Path.Combine(startupFolder, @"GrpcBwsDatabaseServer");
                grpcServer.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            grpcServer.StartInfo.WorkingDirectory = workingDirectory;
            grpcServer.StartInfo.FileName = Path.Combine(workingDirectory, "GrpcBwsDatabaseServer.exe");
            grpcServer.Start();

            // ----------------------------------------
            // Configure, build and run web application
            // ----------------------------------------
            WebApplicationOptions webApplicationOptions = new();
            if (!isDevelopment) {
                // In the production environment, the scoring program may start TabScore2 using a different current working directory
                webApplicationOptions = new() { ContentRootPath = Application.StartupPath };
            }

            WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(webApplicationOptions);
            webAppBuilder.Services.AddLocalization();
            webAppBuilder.Services.AddControllersWithViews();
            webAppBuilder.Services.AddCodeFirstGrpcClient<IBwsDatabaseService>(option => { option.Address = new Uri($"http://{ipAddress}:5119"); });
            webAppBuilder.Services.AddCodeFirstGrpcClient<IExternalNamesDatabaseService>(option => { option.Address = new Uri($"http://{ipAddress}:5119"); });
            webAppBuilder.Services.AddWebOptimizer(option => { option.EnableDiskCache = false; });
            webAppBuilder.Services.AddSingleton<IUtilities, Utilities>();
            webAppBuilder.Services.AddSingleton<IDatabase, Database>();
            webAppBuilder.Services.AddSingleton<IExternalNamesDatabase, ExternalNamesDatabase>();
            webAppBuilder.Services.AddSingleton<ISettings, Settings>();
            webAppBuilder.Services.AddSingleton<IAppData, AppData>();
            webAppBuilder.Services.AddHttpContextAccessor();
            WebApplication webApp = webAppBuilder.Build();
            webApp.UseExceptionHandler("/ErrorScreen/Index");
            webApp.UseWebOptimizer();
            webApp.UseStaticFiles();
            webApp.UseRouting();
            webApp.MapControllerRoute(name: "default", pattern: "{controller=StartScreen}/{action=Index}");
            webApp.RunAsync();

            // --------------------------------------------
            // Configure, build and run desktop application
            // --------------------------------------------
            HostApplicationBuilder desktopBuilder = Host.CreateApplicationBuilder(args);
            desktopBuilder.Services.AddLocalization();
            desktopBuilder.Services.AddCodeFirstGrpcClient<IBwsDatabaseService>(option => { option.Address = new Uri($"http://{ipAddress}:5119"); });
            desktopBuilder.Services.AddCodeFirstGrpcClient<IExternalNamesDatabaseService>(option => { option.Address = new Uri($"http://{ipAddress}:5119"); });
            desktopBuilder.Services.AddSingleton<MainForm>();
            desktopBuilder.Services.AddSingleton<IDatabase, Database>();
            desktopBuilder.Services.AddSingleton<ISettings, Settings>();
            desktopBuilder.Services.AddSingleton<IAppData, AppData>();

            // Create services for forms with free parameters
            desktopBuilder.Services.AddTransient<Func<Point, SettingsForm>>(
                container =>
                    location =>
                    {
                        IDatabase iDatabase = container.GetRequiredService<IDatabase>();
                        ISettings iSettings = container.GetRequiredService<ISettings>();
                        return new SettingsForm(iDatabase, iSettings, location);
                    });
            desktopBuilder.Services.AddTransient<Func<Point, ViewResultsForm>>(
                container =>
                    location =>
                    {
                        return new ViewResultsForm(container, location);
                    });
            desktopBuilder.Services.AddTransient<Func<Classes.Result, Point, EditResultForm>>(
                container =>
                    (result, location) =>
                    {
                        return new EditResultForm(container, result, location);
                    });
            IHost host = desktopBuilder.Build();
            IServiceProvider services = host.Services;
            Application.Run(services.GetRequiredService<MainForm>());

            // Close gRPC server
            grpcServer.Close();
        }
    }
}