using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Domain.Entities;

namespace Domain.Interfaces.Services { 
    public interface IDispositivosService {
        Task<List<Dispositivo>> Get(string matricula);
        Task Delete (string chavePublicaId);
    }
}