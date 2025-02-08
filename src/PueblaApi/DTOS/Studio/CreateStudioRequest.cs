using System;

namespace PueblaApi.DTOS.Studio;

public class CreateStudioRequest
{
    public string Name { set; get; }
    public string Country { set; get; }
    public int FoundationYear { set; get; }
}
