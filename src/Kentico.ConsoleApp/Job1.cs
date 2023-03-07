using CMS.DocumentEngine;

namespace Kentico.ConsoleApp;

public static class Job1
{
    public static void Run()
    {
        var homePage = new DocumentQuery<TreeNode>()
            .Path("/home")
            .OnSite(1)
            .PublishedVersion()
            .Culture("en-US")
            .CombineWithDefaultCulture();

        Console.WriteLine(homePage.Name);
    }
}
