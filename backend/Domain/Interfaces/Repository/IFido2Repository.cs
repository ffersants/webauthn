using Domain.Entities;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Fido2NetLib.Development;

namespace Domain.Interfaces.Repository;

public interface IFido2Repository {
    Task<IEnumerable<StoredCredential>> ListCredentialsByUser(string username);
    Task<IEnumerable<PublicKeyCredentialDescriptor>> ListPublicKeysByUser(string username);
    Task<IEnumerable<StoredCredential>> ListCredentialsByPublicKeyIdAsync(byte[] credentialId);
    Task<StoredCredential> GetCredentialByPublicKeyIdAsync(byte[] credentialId);
    void Store(string securityKeyAlias, Fido2User user, StoredCredential storedCredential);
    //void Store(Model.FidoInfo info, StoredCredential storedCredential);
    Task<IEnumerable<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
    Task UpdateCounter(byte[] credentialId, uint counter);
    Task<string?> GetUsernameByIdAsync(byte[] userId);
    Task<List<Domain.Entities.StoredCredentialDetail>> ListCredentialDetailsByUser(byte[] userId);
    Task<List<Domain.Entities.StoredCredentialDetail>> ListCredentialDetailsByUser(string username);
    Task<bool> RemoveByPublicKeyId(byte[] publicKeyId);
    Task<bool> RemoveBySecretName(string name);
    Task<bool> HasSecurityKey(string name);
}