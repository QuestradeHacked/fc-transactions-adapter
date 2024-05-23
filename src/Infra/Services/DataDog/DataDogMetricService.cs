using Domain.Services;
using Environment = Infra.Utils.Environment;
using StatsdClient;

namespace Infra.Services.DataDog;

public class DataDogMetricService : IMetricService
{
    private readonly IDogStatsd _dogStatsd;

    public DataDogMetricService(IDogStatsd dogStatsd)
    {
        _dogStatsd = dogStatsd;
    }

    public void Distribution(string metricName, double value, IList<string>? metricTags = null)
    {
        metricTags ??= new List<string>();

        AddEnvironment(metricTags);

        _dogStatsd.Distribution(metricName, value, tags: ToArray(metricTags));
    }

    public void Increment(string statName)
    {
        var metricTags = new List<string>();

        AddEnvironment(metricTags);

        _dogStatsd.Increment(statName, tags: metricTags.ToArray());
    }


    public void Increment(string metricName, IList<string> metricTags)
    {
        AddEnvironment(metricTags);

        _dogStatsd.Increment(metricName, tags: ToArray(metricTags));
    }

    private static string[] ToArray(IList<string> metricTags)
    {
        var array = new string[metricTags.Count];

        metricTags.CopyTo(array, 0);

        return array;
    }

    private static void AddEnvironment(ICollection<string> metricTags)
    {
        metricTags.Add($"env:{Environment.Name.ToUpperInvariant()}");
    }
}
