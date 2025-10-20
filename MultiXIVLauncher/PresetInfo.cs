using System.Collections.Generic;

public class PresetInfo
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string DownloadUrl { get; set; }
    public List<string> Plugins { get; set; } = new List<string>();
}
