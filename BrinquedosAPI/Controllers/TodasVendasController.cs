using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrinquedosAPI.Models;
using Newtonsoft.Json;
using System.Text;
using PetaPoco;
using System.Data;
using MySql.Data.MySqlClient;
using Humanizer;

namespace BrinquedosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodasVendasController : ControllerBase
{
    private readonly BrinquedoContext _contexto;

    public TodasVendasController(BrinquedoContext contexto)
    {
        _contexto = contexto;
    }

    string conexaodb = "Server=localhost;Port=3306;Database=maquinadevendas;Uid=root;";

    // GET: api/TodosBrinquedos
    [HttpGet("ListaDeVendas")]
    public async Task<ActionResult<IEnumerable<TodasVendasDTO>>> GetTodasVendas()
    {
        
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
            {
                var todosVendas = await db.FetchAsync<TodasVendas>("SELECT * FROM vendas");

                var responseItems = todosVendas.Select(p => new TodasVendasDTO
                {
                    Id_venda = p.Id_venda,
                    Data = p.Data,
                    Id_produto = p.Id_produto,
                    Quantidade_Vendida = p.Quantidade_Vendida,
                    Preco = p.Preco,
                }).ToList();

                return Ok(responseItems);
            }        
    }

    // GET {id}: Vai buscar os itens da API por ID
    [HttpGet("ListaDeVendasPor/{id}")]
    public async Task<ActionResult<TodasVendasDTO>> GetTodasVendas(long id)
    {
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
        {
            var vendas = await db.FirstOrDefaultAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_produto = @0", id);

            if (vendas == null)
            {
                return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
            }

            var vendasDTO = new TodasVendasDTO
            {
                Id_venda = vendas.Id_venda,
                Data = vendas.Data,
                Id_produto = vendas.Id_produto,
                Quantidade_Vendida = vendas.Quantidade_Vendida,
                Preco = vendas.Preco,
            };

            return Ok(vendasDTO);
        }
    }
    // Método post que faz o eliminar
    [HttpPost("DeleteVendas")]
    public async Task<ActionResult> DeleteTodasVendas([FromBody] List<long> ids)
    {
        try
        {
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                foreach (var id in ids)
                {
                    var todasVendas = await db.SingleOrDefaultAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_venda = @0", id);

                    if (todasVendas == null)
                    {
                        return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                    }
                    else
                    {
                        await db.DeleteAsync("vendas", "Id_venda", todasVendas);
                    }
                }
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao excluir brinquedo(s)");
        }
    }
    // Método post que faz o inserir
    [HttpPost("AddVendas")]
    public async Task<ActionResult> AddVendas([FromBody] List<TodasVendasDTO> TodasVendasDTO)
    {
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            foreach (var todasVendasDTO in TodasVendasDTO)
            {
                    // O produto não existe no banco de dados, então vamos adicioná-lo
                    var novaVenda = new TodasVendas
                    {
                        Data = todasVendasDTO.Data,
                        Id_produto = todasVendasDTO.Id_produto,
                        Quantidade_Vendida = todasVendasDTO.Quantidade_Vendida,
                        Preco = todasVendasDTO.Preco
                    };

                    await db.InsertAsync("vendas", "Id_venda", true, novaVenda);
            }
        }

        return Ok();
    }
    private bool TodasVendasExist(long id)
    {
       return _contexto.TodasVendas.Any(e => e.Id_venda == id);
    }

    private static TodasVendasDTO VendasToDTO(TodasVendas TodaVenda) =>
       new TodasVendasDTO
       {
           Id_venda = TodaVenda.Id_venda,
           Data = TodaVenda.Data,
           Id_produto = TodaVenda.Id_produto,
           Quantidade_Vendida = TodaVenda.Quantidade_Vendida,
           Preco = TodaVenda.Preco,
       };
}
