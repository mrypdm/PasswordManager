using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Abstractions.Contexts;

/// <summary>
/// Data context
/// </summary>
public interface IDataContext
{
    /// <summary>
    /// Saves data changes
    /// </summary>
    Task SaveChangesAsync(CancellationToken token);
}
