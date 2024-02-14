// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using ElmahCore;
using ElmahCore.Mvc;
using TabScore2.Classes;
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
            // ----------------------------------------
            // Configure, build and run web application
            // ----------------------------------------
            WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder();
            webAppBuilder.Services.AddLocalization();
            webAppBuilder.Services.AddControllersWithViews();
            webAppBuilder.Services.AddWebOptimizer();
            webAppBuilder.Services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.LogPath = "~/log";
            }); 
            webAppBuilder.Services.AddSingleton<IUtilities, Utilities>();
            webAppBuilder.Services.AddSingleton<IDatabase, BwsDatabase>();
            webAppBuilder.Services.AddSingleton<IExternalNamesDatabase, ExternalNamesDatabase>();
            webAppBuilder.Services.AddSingleton<ISettings, Settings>();
            webAppBuilder.Services.AddSingleton<IAppData, AppData>();
            webAppBuilder.Services.AddHttpContextAccessor();
            WebApplication webApp = webAppBuilder.Build();
            webApp.UseExceptionHandler("/ErrorScreen/Index");
            webApp.UseElmah();
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
            desktopBuilder.Services.AddSingleton<MainForm>();
            desktopBuilder.Services.AddSingleton<IDatabase, BwsDatabase>();
            desktopBuilder.Services.AddSingleton<ISettings, Settings>();
            desktopBuilder.Services.AddSingleton<IAppData, AppData>();

            // Create services for forms with free parameters
            desktopBuilder.Services.AddTransient<Func<Point, SettingsForm>>(
                container =>
                    location =>
                    {
                        ISettings iSettings = container.GetRequiredService<ISettings>();
                        return new SettingsForm(iSettings, location);
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
            Application.Run(services.GetRequiredService<MainForm>());
        }
    }
}