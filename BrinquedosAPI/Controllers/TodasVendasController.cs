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
using AutoMapper;


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

    // GET: Vai buscar á base de dados todas as vendas feitas na máquina de vendas
    [HttpGet("ListaDeVendas")]
    public async Task<ActionResult<IEnumerable<TodasVendasDTO>>> GetTodasVendas()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendas, TodasVendasDTO>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            var todosVendas = await db.FetchAsync<TodasVendas>("SELECT * FROM vendas");
            var responseItems = mapper.Map<List<TodasVendasDTO>>(todosVendas);

            return Ok(responseItems);
        }        
    }

    // GET {id}: Vai buscar á base de dados a venda do id inserido
    [HttpGet("ListaDeVendasPor/{id}")]
    public async Task<ActionResult<TodasVendasDTO>> GetTodasVendas(long id)
    {

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendas, TodasVendasDTO>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            var vendas = await db.FirstOrDefaultAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_produto = @0", id);

            if (vendas == null)
            {
                return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
            }
            var vendasDTO = mapper.Map<TodasVendasDTO>(vendas);
            return Ok(vendasDTO);
        }
    }
    // Método post que elemina da base de dados as vendas por id inserido
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
    // Método post que insere na base de dados as vendas
    [HttpPost("AddVendas")]
    public async Task<ActionResult> AddVendas([FromBody] List<TodasVendasDTO> TodasVendasDTO)
    {

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendasDTO, TodasVendas>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            foreach (var todasVendasDTO in TodasVendasDTO)
            {
                var novaVenda = mapper.Map<TodasVendas>(todasVendasDTO);
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
           Troco = TodaVenda.Troco,
       };
}
