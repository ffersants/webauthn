using System.Security.Cryptography.X509Certificates;
using System.Text;
using Domain.Entities;
using Domain.Interfaces.Repository;
using Domain.Repository;

namespace Data.Repository {
    public class DispositivoRepository : IDispositivoRepository
    {
        readonly IFido2Repository _fido2Store;

        public DispositivoRepository(IFido2Repository fido2Store) {
            _fido2Store = fido2Store;
        }

        public async Task<List<Dispositivo>> ObterDispositivosDoUsuario(string matricula)
        {
            var devices = await _fido2Store.ListCredentialDetailsByUser(matricula);
            return devices.Select(p => new Dispositivo{
                Id = p.Id,
                DataHoraCadastro = p.RegDate,
                DadosDispositivo = p.SecurityKeyName,
                 ChavePublicaId = Convert.ToBase64String(p.PublicKeyId)
            }).ToList();
        }

        public async Task RemoverDispositivo(string chavePublicaId) 
        {
            var result = await _fido2Store.RemoveByPublicKeyId(Convert.FromBase64String(chavePublicaId));
            System.Console.WriteLine(result);
        }
    }
}