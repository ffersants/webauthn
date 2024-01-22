using System.ComponentModel.DataAnnotations;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Domain.Entities;


public class StoredCredentialDetail
{
    public StoredCredentialDetail(int id, string username, byte[]? userId, string? securityKeyName, byte[] publicKey, byte[] publicKeyId, 
        byte[] userHandle, uint signatureCounter, string? credType, DateTime regDate, Guid aaGuid)
    {
        Id = id;
        Username = username;
        UserId = userId;
        SecurityKeyName = securityKeyName;
        PublicKey = publicKey;
        PublicKeyId = publicKeyId;
        UserHandle = userHandle;
        SignatureCounter = signatureCounter;
        CredType = credType;
        RegDate = regDate;
        AaGuid = aaGuid;
    }

    [Key]
    public int Id { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public byte[]? UserId { get; set; }

    /// <summary>
    /// Friendly name for security key
    /// </summary>
    public string? SecurityKeyName { get; set; }


    [Required]
    public byte[] PublicKey { get; set; }

    [Required]
    public byte[] PublicKeyId { get; set; }

    [Required]
    public byte[] UserHandle { get; set; }

    public uint SignatureCounter { get; set; }
    public string? CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }


    public PublicKeyCredentialType? Type { get; set; }
    public string? Transports { get; set; }

    public StoredCredentialDetail UpdateUserDetails(Fido2User user)
    {
        Username = user.Name;
        UserId = user.Id;
        return this;
    }

    public StoredCredentialDetail SetSecurityKeyName(string securityKeyAlias)
    {
        securityKeyAlias ??= Username;
        SecurityKeyName = securityKeyAlias;
        return this;
    }
}