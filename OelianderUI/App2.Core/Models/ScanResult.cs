namespace OelianderUI.Core.Models;

// Remove this class once your pages/features are using your data.
// This is used by the SampleDataService.
// It is the model class we use to display data on pages like Grid, Chart, and List Details.
public class ScanResult
{
    public long Index { get; set; }

    public string IPAddress { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Status { get; set; }
}
