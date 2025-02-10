using System;

namespace PueblaApi.Settings;

public class ImageServiceConfiguration
{
    public string CloudName { set; get; }
    public string ApiKey { set; get; }
    public string ApiSecret { set; get; }

    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public string AssetFolderName { get; set; }
    public long MaxFileSize { get; set; } // in bytes.

}
