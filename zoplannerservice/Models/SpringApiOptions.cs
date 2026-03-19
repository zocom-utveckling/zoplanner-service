namespace zoplannerservice.Models;

public class SpringApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 200;
}