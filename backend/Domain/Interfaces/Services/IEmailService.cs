using System.Security.Cryptography.X509Certificates;

namespace Domain.Interfaces.Services { 
    public interface IEmailService {
        public void EnviarEmailRegistroDispositivo(DateTime dataHoraRegistro);
    }
}