using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.Extensions.Configuration;
using Kentico.Cli;

// ---------------------------------------------
// Target CMS API setup
// ---------------------------------------------
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Initialize target CMS API
Service.Use<IConfiguration>(() => config);
CMSApplication.Init();

using var context = new CMSActionContext();

// Turn off CMS logging/staging/etc for performance when we don't need to capture it.
context.DisableAll();

// Turn off CI for performance (not included above)
var origCiEnabled = SettingsKeyInfoProvider.GetBoolValue("CMSEnableCI");
SettingsKeyInfoProvider.SetGlobalValue("CMSEnableCI", false);
// Print setting because if the job is interrupted, it won't be reset and may be lost.
Console.WriteLine($"Original CI setting: {origCiEnabled}");

// Document sort order on insert
var origDocumentOrder = SettingsKeyInfoProvider.GetValue("CMSNewDocumentOrder");
SettingsKeyInfoProvider.SetGlobalValue("CMSNewDocumentOrder", "LAST");
// Print setting because if the job is interrupted, it won't be reset and may be lost.
Console.WriteLine($"Original Document sort order: {origDocumentOrder}");

// ---------------------------------------------
// Run jobs
// ---------------------------------------------
try
{
    Job1.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
// ---------------------------------------------
// App teardown
// ---------------------------------------------
finally
{
    SettingsKeyInfoProvider.SetGlobalValue("CMSNewDocumentOrder", origDocumentOrder);
    SettingsKeyInfoProvider.SetGlobalValue("CMSEnableCI", origCiEnabled);

    Console.WriteLine("Exiting.");
}
