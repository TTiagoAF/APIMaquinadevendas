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

    // String de conexão com o banco de dados
    string conexaodb = "Server=localhost;Port=3306;Database=maquinadevendas;Uid=root;";

    // GET: /api/TodasVendas/ListaDeVendas
    // Obtém todas as vendas da máquina de vendas a partir da base de dados
    [HttpGet("ListaDeVendas")]
    public async Task<ActionResult<IEnumerable<TodasVendasDTO>>> GetTodasVendas()
    {
        // Configuração do AutoMapper para mapear a classe TodasVendas para TodasVendasDTO
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendas, TodasVendasDTO>();
        });
        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Consulta todas as vendas na tabela vendas
            var todasVendas = await db.FetchAsync<TodasVendas>("SELECT * FROM vendas");

            // Mapeia as vendas para a lista de DTOs
            var responseItems = mapper.Map<List<TodasVendasDTO>>(todasVendas);

            // Retorna a lista de vendas como resposta
            return Ok(responseItems);
        }
    }

    // GET: /api/TodasVendas/ListaDeVendasPor/{id}
    // Obtém uma venda específica da máquina de vendas a partir do ID fornecido
    [HttpGet("TodasAsVendasDeCadaProduto/{id}")]
    public async Task<ActionResult<IEnumerable<TodasVendasDTO>>> GetTodasVendas(long id)
    {
        // Configuração do AutoMapper para mapear a classe TodasVendas para TodasVendasDTO
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendas, TodasVendasDTO>();
        });
        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Consulta a venda pelo ID
            var venda = await db.FetchAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_produto = @0", id);

            // Verifica se a venda foi encontrada
            if (venda == null)
            {
                return NotFound($"Não foi encontrada nenhuma Venda com o Id: {id}. Insira outro Id.");
            }

            // Mapeia a venda para o DTO
            var vendaDTO = mapper.Map<List<TodasVendasDTO>>(venda);

            // Retorna a venda como resposta
            return Ok(vendaDTO);
        }
    }

    // GET: /api/TodasVendas/ListaDeVendasPor/{id}
    // Obtém uma venda específica da máquina de vendas a partir do ID fornecido
    [HttpGet("VendasPorId/{id}")]
    public async Task<ActionResult<TodasVendasDTO>> GetVendas(long id)
    {
        // Configuração do AutoMapper para mapear a classe TodasVendas para TodasVendasDTO
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendas, TodasVendasDTO>();
        });
        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Consulta a venda pelo ID
            var venda = await db.FirstOrDefaultAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_venda = @0", id);

            // Verifica se a venda foi encontrada
            if (venda == null)
            {
                return NotFound($"Não foi encontrada nenhuma Venda com o Id: {id}. Insira outro Id.");
            }

            // Mapeia a venda para o DTO
            var vendaDTO = mapper.Map<TodasVendasDTO>(venda);

            // Retorna a venda como resposta
            return Ok(vendaDTO);
        }
    }

    // POST: /api/TodasVendas/DeleteVendas
    // Exclui vendas da base de dados com base nos IDs fornecidos
    [HttpPost("DeleteVendas")]
    public async Task<ActionResult> DeleteTodasVendas([FromBody] List<long> ids)
    {
        try
        {
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                foreach (var id in ids)
                {
                    // Consulta a venda pelo ID
                    var venda = await db.SingleOrDefaultAsync<TodasVendas>("SELECT * FROM vendas WHERE Id_venda = @0", id);

                    // Verifica se a venda foi encontrada
                    if (venda == null)
                    {
                        return NotFound($"Não foi encontrada nenhuma Venda com o Id: {id}. Insira outro Id.");
                    }
                    else
                    {
                        // Exclui a venda da tabela vendas
                        await db.DeleteAsync("vendas", "Id_venda", venda);
                    }
                }
            }

            // Retorna uma resposta sem conteúdo
            return NoContent();
        }
        catch (Exception ex)
        {
            // Retorna uma resposta de erro interno do servidor
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao excluir venda(s)");
        }
    }

    // POST: /api/TodasVendas/AddVendas
    // Insere vendas na base de dados
    [HttpPost("AddVendas")]
    public async Task<ActionResult> AddVendas([FromBody] List<TodasVendasDTO> TodasVendasDTO)
    {
        // Configuração do AutoMapper para mapear a classe TodasVendasDTO para TodasVendas
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodasVendasDTO, TodasVendas>();
        });
        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            foreach (var todasVendasDTO in TodasVendasDTO)
            {
                // Mapeia o DTO para a classe TodasVendas
                var novaVenda = mapper.Map<TodasVendas>(todasVendasDTO);

                // Insere a venda na tabela vendas
                await db.InsertAsync("vendas", "Id_venda", true, novaVenda);
            }
        }

        // Retorna uma resposta de sucesso
        return Ok();
    }

    // Método auxiliar para verificar se uma venda com o ID especificado existe
    private bool TodasVendasExist(long id)
    {
        return _contexto.TodasVendas.Any(e => e.Id_venda == id);
    }
}