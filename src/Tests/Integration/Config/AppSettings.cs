using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Integration.Config;

public class AppSettings
{
    private readonly IConfiguration _configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.Integration.json")
        .Build();

    private string GetStringConfiguration(string key, string value)
    {
        var configuration = _configurationBuilder[key];

        return configuration ?? value;
    }

    private int GetIntConfiguration(string key, int value)
    {
        var configuration = _configurationBuilder[key];

        return configuration == null ? value : int.Parse(configuration, new NumberFormatInfo());
    }

    internal string GetProcessPubSubHost() => GetStringConfiguration("PubSubConfig:Emulator:Host", "127.0.0.1");

    internal int GetProcessPubSubPort() => GetIntConfiguration("PubSubConfig:Emulator:Port", 8681);

    internal string GetPubSubProjectId() => GetStringConfiguration("PubSubConfig:ProjectId", "emulator");

    internal int GetPubSubSubscriberTimeout() => GetIntConfiguration("PubSubConfig:Subscriber:Timeout", 500);
}
