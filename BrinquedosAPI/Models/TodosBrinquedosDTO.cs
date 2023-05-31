namespace BrinquedosAPI.Models
{
    public class TodosBrinquedosDTO
    {
        public long Id { get; set; }
        public string? Brinquedo { get; set; }
        public double Preco { get; set; }
        public int Quantidade { get; set; }
        public int VendasTotais { get; set; }
    }
}
