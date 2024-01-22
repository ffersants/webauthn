using Data.Context;
using Data.Models;
using Domain.Interfaces.Repository;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
namespace Data.Repository;

public class Fido2Repository : IFido2Repository
{
    private readonly AppDbContext _context;

    public Fido2Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StoredCredential>> ListCredentialsByUser(string username)
    {
        var credentials = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
        return credentials.Select(StoredCredentialMapper.ToDomain);
    }

    public async Task<IEnumerable<PublicKeyCredentialDescriptor>> ListPublicKeysByUser(string username)
    {
        var pks = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
        return pks.Select(PublicKeyCredentialDescriptorMapper.ToDomain);
    }

    public async Task<IEnumerable<StoredCredential>> ListCredentialsByPublicKeyIdAsync(byte[] credentialId)
    {
        var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.PublicKeyId == credentialId).ToListAsync();
        return users.Select(StoredCredentialMapper.ToDomain);
    }




    public async Task<StoredCredential> GetCredentialByPublicKeyIdAsync(byte[] credentialId)
    {
        var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(w => w.PublicKeyId == credentialId);
        if (users == null)
            return null;

        return StoredCredentialMapper.ToDomain(users);
    }

    public void Store(string securityKeyAlias, Fido2NetLib.Fido2User user, StoredCredential storedCredential)
    {
        var model = StoredCredentialMapper.ToModel(storedCredential).UpdateUserDetails(user).SetSecurityKeyName(securityKeyAlias);
        _context.Fido2StoredCredential.Add(model);
        _context.SaveChanges();
    }

    public async Task<IEnumerable<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
    {
        var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserHandle == userHandle).ToListAsync();
        return users.Select(StoredCredentialMapper.ToDomain);
    }


    public async Task UpdateCounter(byte[] credentialId, uint counter)
    {
        var cred = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.PublicKeyId == credentialId);
        if (cred != null)
        {
            cred.SignatureCounter = counter;
            _context.SaveChanges();
        }
    }

    public Task<string?> GetUsernameByIdAsync(byte[] userId)
    {
        return _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserId == userId).Select(s => s.Username).FirstOrDefaultAsync();
    }

    public async Task<List<Domain.Entities.StoredCredentialDetail>> ListCredentialDetailsByUser(byte[] userId)
    {
        var result = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserId == userId).ToListAsync();
        var a = result.Select(i => new Domain.Entities.StoredCredentialDetail(i.Id,
                                                                              i.Username,
                                                                              i.UserId,
                                                                              i.SecurityKeyName,
                                                                              i.PublicKey,
                                                                              i.PublicKeyId,
                                                                              i.UserHandle,
                                                                              i.SignatureCounter,
                                                                              i.CredType,
                                                                              i.RegDate,
                                                                              i.AaGuid)).ToList();
        return a;
    }

    public async Task<List<Domain.Entities.StoredCredentialDetail>> ListCredentialDetailsByUser(string username)
    {
        var result = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
        var a = result.Select(i => new Domain.Entities.StoredCredentialDetail(i.Id,
                                                                              i.Username,
                                                                              i.UserId,
                                                                              i.SecurityKeyName,
                                                                              i.PublicKey,
                                                                              i.PublicKeyId,
                                                                              i.UserHandle,
                                                                              i.SignatureCounter,
                                                                              i.CredType,
                                                                              i.RegDate,
                                                                              i.AaGuid)).ToList();
        return a;
    }
    public async Task<bool> RemoveByPublicKeyId(byte[] publicKeyId)
    {
        var key = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.PublicKeyId == publicKeyId);
        if (key is not null)
        {
            _context.Fido2StoredCredential.Remove(key);
            return _context.SaveChanges() > 0;
        }

        return false;
    }

    public async Task<bool> RemoveBySecretName(string name)
    {
        var key = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.SecurityKeyName == name);
        if (key is not null)
        {
            _context.Fido2StoredCredential.Remove(key);
            return _context.SaveChanges() > 0;
        }
        return false;
    }

    public async Task<bool> HasSecurityKey(string name)
    {
        return await _context.Fido2StoredCredential.AnyAsync(f => f.SecurityKeyName == name);
    }
}