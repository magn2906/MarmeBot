namespace MarmeBot.Utilities;

public static class EnvironmentHandler
{
    private static IConfiguration Configuration { get; set; }

    static EnvironmentHandler()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

#if DEBUG
        builder.AddJsonFile("appsettings.Development.json", optional: true);
#endif

        Configuration = builder.Build();
    }

    public static string GetVariable(string variableName)
    {
        var value = Configuration.GetSection("Variables")[variableName];

        if (value == null)
        {
            throw new Exception($"Configuration variable {variableName} not found");
        }

        return value;
    }
}