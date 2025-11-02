namespace House.House.Services.Gooning.HTTP;

public class CoomerFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string URL => $"https://coomer.st/{Path.TrimStart('/')}";
}