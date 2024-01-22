using Data.Models;
using Fido2NetLib;
using Fido2NetLib.Objects;

internal static class PublicKeyCredentialDescriptorMapper
{
    public static PublicKeyCredentialDescriptor ToDomain(StoredCredentialDetail model)
    {

        return new PublicKeyCredentialDescriptor
        {
            Id = model.PublicKeyId,
            Transports = model.Transports?.Split(';').Select(s => s.ToEnum<AuthenticatorTransport>()).ToArray(),
            Type = model.Type
        };
    }
}