namespace BrinquedosAPI.Models
{
    public class TodasVendasDTO
    {
        public long Id_venda { get; set; }
        public string Data { get; set; }
        public long Id_produto { get; set; }
        public int Quantidade_Vendida { get; set; }
        public double Preco { get; set; }
    }
}
