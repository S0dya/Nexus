using Nexus.Features.Registration.Dto;

namespace Nexus.Features.Registration.Services;

public interface IAccountRegistrationService
{ 
    Task<AccountRegistrationResult> CreateAccount(AccountRegistrationRequest request);
}