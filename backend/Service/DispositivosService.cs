using Domain.Entities;
using Domain.Interfaces.Services;
using Domain.Repository;

namespace Service {
    public class DispositivosService : IDispositivosService
    {
        readonly IDispositivoRepository _repository;

        public DispositivosService(IDispositivoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Dispositivo>> Get(string matricula) => await _repository.ObterDispositivosDoUsuario(matricula);

        public async Task Delete(string chavePublicaId) => await _repository.RemoverDispositivo(chavePublicaId);
    }
}