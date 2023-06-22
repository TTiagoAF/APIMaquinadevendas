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

namespace BrinquedosAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodosBrinquedosController : ControllerBase
{
    private readonly BrinquedoContext _contexto;

    public TodosBrinquedosController(BrinquedoContext contexto)
    {
        _contexto = contexto;
    }

    string conexaodb = "Server=localhost;Port=3306;Database=maquinadevendas;Uid=root;";

    // GET: api/TodosBrinquedos
    [HttpGet("ListaDeBrinquedos")]
    public async Task<ActionResult<IEnumerable<TodosBrinquedosDTO>>> GetTodosBrinquedos()
    {
        
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
            {
                var todosProdutos = await db.FetchAsync<TodosBrinquedos>("SELECT * FROM brinquedos");

                var responseItems = todosProdutos.Select(p => new TodosBrinquedosDTO
                {
                    Id = p.Id,
                    brinquedo = p.brinquedo,
                    quantidade = p.quantidade,
                    preco = p.preco,
                    vendastotais = p.vendastotais
                }).ToList();

                return Ok(responseItems);
            }        
    }

    // GET {id}: Vai buscar os itens da API por ID
    [HttpGet("ListaDeBrinquedosPor/{id}")]
    public async Task<ActionResult<TodosBrinquedosDTO>> GetTodoBrinquedos(long id)
    {
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
        {
            var brinquedo = await db.FirstOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE Id = @0", id);

            if (brinquedo == null)
            {
                return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
            }

            var brinquedoDTO = new TodosBrinquedosDTO
            {
                Id = brinquedo.Id,
                brinquedo = brinquedo.brinquedo,
                quantidade = brinquedo.quantidade,
                preco = brinquedo.preco,
                vendastotais = brinquedo.vendastotais
            };

            return Ok(brinquedoDTO);
        }
    }
    // Método post que faz o eliminar
    [HttpPost("DeleteBrinquedo")]
    public async Task<ActionResult> DeleteTodosBrinquedos([FromBody] List<long> ids)
    {
        try
        {
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                foreach (var id in ids)
                {
                    var todosBrinquedos = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", id);

                    if (todosBrinquedos == null)
                    {
                        return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                    }
                    else
                    {
                        await db.DeleteAsync("brinquedos", "id", todosBrinquedos);
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
    [HttpPost("AddOrUpdateBrinquedo")]
    public async Task<ActionResult> AddOrUpdateBrinquedo([FromBody] List<TodosBrinquedosDTO> TodosBrinquedosDTO)
    {
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            foreach (var todosProdutosDTO in TodosBrinquedosDTO)
            {
                var produtoExistente = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", todosProdutosDTO.Id);

                if (produtoExistente == null)
                {
                    // O produto não existe no banco de dados, então vamos adicioná-lo
                    var novoProduto = new TodosBrinquedos
                    {
                        brinquedo = todosProdutosDTO.brinquedo,
                        quantidade = todosProdutosDTO.quantidade,
                        preco = todosProdutosDTO.preco,
                        vendastotais = todosProdutosDTO.vendastotais
                    };

                    await db.InsertAsync("brinquedos", "id", true, novoProduto);
                }
                else
                {
                    // O produto já existe no banco de dados, então vamos atualizá-lo
                    produtoExistente.brinquedo = todosProdutosDTO.brinquedo;
                    produtoExistente.quantidade = todosProdutosDTO.quantidade;
                    produtoExistente.preco = todosProdutosDTO.preco;
                    produtoExistente.vendastotais = todosProdutosDTO.vendastotais;

                    await db.UpdateAsync("brinquedos", "id", produtoExistente);
                }
            }
        }

        return Ok();
    }

    [HttpPost("AtualizarQuantidadeEVendas/{id}")]
    public async Task<ActionResult> AtualizarQuantidadeEVendas(long id)
    {
        try
        {
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                var brinquedo = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", id);

                if (brinquedo == null)
                {
                    return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                }

                brinquedo.quantidade -= 1;
                brinquedo.vendastotais += 1;

                await db.UpdateAsync("brinquedos", "id", brinquedo);

                return NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao atualizar a quantidade e vendas totais do brinquedo");
        }
    }


    private bool TodosBrinquedosExists(long id)
    {
       return _contexto.TodoBrinquedos.Any(e => e.Id == id);
    }

    private static TodosBrinquedosDTO BrinquedosToDTO(TodosBrinquedos TodoBrinquedo) =>
       new TodosBrinquedosDTO
       {
           Id = TodoBrinquedo.Id,
           brinquedo = TodoBrinquedo.brinquedo,
           preco = TodoBrinquedo.preco,
           quantidade = TodoBrinquedo.quantidade,
           vendastotais = TodoBrinquedo.vendastotais,
       };
}
