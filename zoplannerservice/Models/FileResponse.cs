namespace zoplannerservice.Models;
public class FileResponse
{
    public int Id { get; set; }          // DB id or storage id
    public string Url { get; set; } = ""; // Public URL to use in your app
    public string FileName { get; set; } = "";
    public long Size { get; set; }
}