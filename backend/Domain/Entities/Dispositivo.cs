namespace Domain.Entities
{
    public class Dispositivo {
        public int Id {get; set;}
        public string ChavePublicaId {get; set;}
        public DateTime DataHoraCadastro {get; set;}
        public string? DadosDispositivo {get; set;}
    }
}