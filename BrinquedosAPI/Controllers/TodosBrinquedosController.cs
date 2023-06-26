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
using AutoMapper;

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

    // GET: Vai buscar á base de dados todos os brinquedos que estão á venda na máquina de vendas
    [HttpGet("ListaDeBrinquedos")]
    public async Task<ActionResult<IEnumerable<TodosBrinquedosDTO>>> GetTodosBrinquedos()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedos, TodosBrinquedosDTO>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            var todosProdutos = await db.FetchAsync<TodosBrinquedos>("SELECT * FROM brinquedos");

            var responseItems = mapper.Map<List<TodosBrinquedosDTO>>(todosProdutos);

            return Ok(responseItems);
        }
    }

    // GET {id}: Vai buscar á base de dados o brinquedo do id que foi inserido
    [HttpGet("ListaDeBrinquedosPor/{id}")]
    public async Task<ActionResult<TodosBrinquedosDTO>> GetTodoBrinquedos(long id)
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedos, TodosBrinquedosDTO>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            var brinquedo = await db.FirstOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE Id = @0", id);

            if (brinquedo == null)
            {
                return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
            }

            var brinquedoDTO = mapper.Map<TodosBrinquedosDTO>(brinquedo);

            return Ok(brinquedoDTO);
        }
    }
    // Método post que elemina da base de dados os brinquedos pelo o id que foi inserido
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
    // Método post que dependendo se o brinquedo ainda não existe ele insere na base de dados o brinquedo se o brinquedo já existir ele atualiza conforme os dados fornecidos
    [HttpPost("AddOrUpdateBrinquedo")]
    public async Task<ActionResult> AddOrUpdateBrinquedo([FromBody] List<TodosBrinquedosDTO> TodosBrinquedosDTO)
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedosDTO, TodosBrinquedos>();
        });

        AutoMapper.IMapper mapper = config.CreateMapper();

        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            foreach (var todosProdutosDTO in TodosBrinquedosDTO)
            {
                var produtoExistente = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", todosProdutosDTO.Id);

                if (produtoExistente == null)
                {
                    var novoProduto = mapper.Map<TodosBrinquedos>(todosProdutosDTO);
                    await db.InsertAsync("brinquedos", "id", true, novoProduto);
                }
                else
                {
                    produtoExistente = mapper.Map(todosProdutosDTO, produtoExistente);
                    await db.UpdateAsync("brinquedos", "id", produtoExistente);
                }
            }
        }

        return Ok();
    }
    // Quando é feita uma venda na máquina de vendas ele vai buscar o id do brinquedo que foi vendido e aumenta 1 ás vendas totais e diminui 1 ao stock
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
