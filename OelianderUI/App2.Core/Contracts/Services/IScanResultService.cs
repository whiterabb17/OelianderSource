using OelianderUI.Core.Models;

namespace OelianderUI.Core.Contracts.Services;

public interface IScanResultService
{
    Task<IEnumerable<ScanResult>> GetGridDataAsync();
}
