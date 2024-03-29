using Data.Models;
using Domain.Entities;

internal static class StoredCredentialMapper
{
    public static Data.Models.StoredCredentialDetail ToModel(Fido2NetLib.Development.StoredCredential domain)
    {
        string transports = null;
        if (domain.Descriptor.Transports != null)
            transports = string.Join(";", domain.Descriptor.Transports);
        return new Data.Models.StoredCredentialDetail
        {
            UserId = domain.UserId,
            PublicKey = domain.PublicKey,
            UserHandle = domain.UserHandle,
            SignatureCounter = domain.SignatureCounter,
            CredType = domain.CredType,
            RegDate = domain.RegDate,
            AaGuid = domain.AaGuid,

            PublicKeyId = domain.Descriptor.Id,
            Type = domain.Descriptor.Type,
            Transports = transports
        };
    }

    public static Fido2NetLib.Development.StoredCredential ToDomain(Data.Models.StoredCredentialDetail model)
    {
        return new Fido2NetLib.Development.StoredCredential
        {
            UserId = model.UserId,
            PublicKey = model.PublicKey,
            UserHandle = model.UserHandle,
            SignatureCounter = model.SignatureCounter,
            CredType = model.CredType,
            RegDate = model.RegDate,
            AaGuid = model.AaGuid,
            Descriptor = PublicKeyCredentialDescriptorMapper.ToDomain(model)
        };
    }
}