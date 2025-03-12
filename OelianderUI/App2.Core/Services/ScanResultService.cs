using OelianderUI.Core.Contracts.Services;
using OelianderUI.Core.Models;

namespace OelianderUI.Core.Services;

// This class holds sample data used by some generated pages to show how they can be used.
// TODO: The following classes have been created to display sample data. Delete these files once your app is using real data.
// 1. Contracts/Services/ISampleDataService.cs
// 2. Services/SampleDataService.cs
// 3. Models/SampleCompany.cs
// 4. Models/SampleOrder.cs
// 5. Models/SampleOrderDetail.cs
public class ScanResultService : IScanResultService
{
    public ScanResultService()
    {
    }

    private static IEnumerable<ScanResult> AllResults()
    {
        return new List<ScanResult>()
        {
            new ScanResult()
            {
                Index = 1,
                IPAddress = "127.0.0.1",
                Username = "admin",
                Password = "admin",
                Status = "Authenticated"
            }
        };
    }

    // Remove this once your DataGrid pages are displaying real data.
    public async Task<IEnumerable<ScanResult>> GetGridDataAsync()
    {
        await Task.CompletedTask;
        return AllResults();
    }
}
