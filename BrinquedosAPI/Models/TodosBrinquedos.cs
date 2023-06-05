namespace BrinquedosAPI.Models
{
    public class TodosBrinquedos
    {
        public long id { get; set; }
        public string? brinquedo { get; set; }
        public double preco { get; set; }
        public int quantidade { get; set; }
        public int vendastotais { get; set; }
        public string? Secret { get; set; }
    }
}
