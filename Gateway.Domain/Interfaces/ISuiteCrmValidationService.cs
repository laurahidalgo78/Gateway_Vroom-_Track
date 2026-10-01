using Gateway.Domain.Models;

namespace Gateway.Domain.Interfaces;

public interface ISuiteCrmValidationService
{
    Task<SuiteCrmConnectionResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
}
