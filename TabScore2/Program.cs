// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcServices;
using ProtoBuf.Grpc.ClientFactory;
using System.Diagnostics;
using System.Net;
using TabScore2.DataServices;
using TabScore2.Forms;
using TabScore2.SharedClasses;
using TabScore2.UtilityServices;

namespace TabScore2
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.CurrentCultureIgnoreCase);
            string workingDirectory;

            // ------------------
            // Load splash screen
            // ------------------
            if (Properties.Settings.Default.ShowSplashScreen)
            {
                Process splashScreen = new(); 
                if (isDevelopment)
                {
                    workingDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, @"SplashScreen\bin\x64\Debug\net8.0-windows");
                }
                else
                {
                    workingDirectory = Path.Combine(Application.StartupPath, "SplashScreen");
                }
                splashScreen.StartInfo.FileName = Path.Combine(workingDirectory, "SplashScreen.exe");
                splashScreen.Start();
            }

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
            Uri grpcAddress = new($"http://{ipAddress}:5119");

            // -------------------------
            // Start gRPC server process
            // -------------------------
            Process grpcServer = new();
            if(isDevelopment)
            {
                workingDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, @"GrpcBwsDatabaseServer\bin\x86\Debug\net8.0-windows");
                grpcServer.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }
            else
            {
                workingDirectory = Path.Combine(Application.StartupPath, "GrpcBwsDatabaseServer");
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
            desktopBuilder.Services.AddTransient<Func<Result, Point, EditResultForm>>(
                container =>
                    (result, location) =>
                    {
                        return new EditResultForm(container, result, location);
                    });
            IHost host = desktopBuilder.Build();
            IServiceProvider services = host.Services;

            // Close the splash screen (if it's open) and start the Windows Forms app
            Process[] processArray = Process.GetProcessesByName("SplashScreen");
            if (processArray.Length > 0) processArray[0].Kill();
            Application.Run(services.GetRequiredService<MainForm>());

            // Close gRPC server
            processArray = Process.GetProcessesByName("GrpcBwsDatabaseServer");
            if (processArray.Length > 0) processArray[0].Kill();
        }
    }
}