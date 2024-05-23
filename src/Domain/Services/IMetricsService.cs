namespace Domain.Services;

public interface IMetricService
{
    void Increment(string metricName, IList<string> metricTags);

    void Distribution(string metricName, double value, IList<string>? metricTag = null);

    void Increment(string statName);
}
