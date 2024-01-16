using Domain.Entities;

namespace Domain.Repository {
    public interface IDispositivoRepository {
        Task<List<Dispositivo>> ObterDispositivosDoUsuario(string matricula);
        Task RemoverDispositivo(string chavePublicaId);
    }
}