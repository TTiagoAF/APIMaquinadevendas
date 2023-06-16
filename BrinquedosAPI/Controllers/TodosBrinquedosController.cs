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

    // GET: api/TodosBrinquedos
    [HttpGet("ListaDeBrinquedos")]
    public async Task<ActionResult<IEnumerable<TodosBrinquedosDTO>>> GetTodosBrinquedos()
    {
        return await _contexto.TodoBrinquedos
               .Select(x => BrinquedosToDTO(x))
               .ToListAsync();
    }

    // GET {id}: Vai buscar os itens da API por ID
    [HttpGet("ListaDeBrinquedosPor{id}")]
    public async Task<ActionResult<TodosBrinquedosDTO>> GetTodoBrinquedos(long id)
    {
        var TodosBrinquedos = await _contexto.TodoBrinquedos.FindAsync(id);

        if (TodosBrinquedos == null)
        {
            return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
        }

        return BrinquedosToDTO(TodosBrinquedos);
    }
    // Método post que faz o eliminar
    [HttpPost("DeleteBrinquedo")]
    public async Task<ActionResult> DeleteTodosBrinquedos([FromBody] List<long> ids)
    {
        try
        {
            // Remove os brinquedos do banco de dados
            foreach (var id in ids)
            {
                var todosBrinquedos = await _contexto.TodoBrinquedos.FindAsync(id);

                if (todosBrinquedos == null)
                {
                    return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                }
                else
                {
                    _contexto.TodoBrinquedos.Remove(todosBrinquedos);
                }
            }

            // Salva as alterações no banco de dados
            await _contexto.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound();
        }
    }
    // Método post que faz o inserir
    [HttpPost("AddOrUpdateBrinquedo")]
    public async Task<ActionResult> AddOrUpdateBrinquedo([FromBody] List<TodosBrinquedosDTO> TodosBrinquedosDTO)
    {
        try
        {
            foreach (var brinquedoDTO in TodosBrinquedosDTO)
            {
                // Verifica se o brinquedo já existe no banco de dados com base no ID
                var brinquedoExistente = await _contexto.TodoBrinquedos.FindAsync(brinquedoDTO.id);

                if (brinquedoExistente != null)
                {
                    // O brinquedo já existe, então atualiza os valores
                    brinquedoExistente.brinquedo = brinquedoDTO.brinquedo;
                    brinquedoExistente.preco = brinquedoDTO.preco;
                    brinquedoExistente.quantidade = brinquedoDTO.quantidade;
                    brinquedoExistente.vendastotais = brinquedoDTO.vendastotais;
                }
                else
                {
                    // O brinquedo não existe, então cria um novo
                    var novoBrinquedo = new TodosBrinquedos
                    {
                        brinquedo = brinquedoDTO.brinquedo,
                        preco = brinquedoDTO.preco,
                        quantidade = brinquedoDTO.quantidade,
                        vendastotais = brinquedoDTO.vendastotais
                    };

                    _contexto.TodoBrinquedos.Add(novoBrinquedo);
                }
            }

            // Salva as alterações no banco de dados
            await _contexto.SaveChangesAsync();

            //Ler produtos que estão na base de dados
            var todosBrinquedos = await _contexto.TodoBrinquedos.ToListAsync();

            var TodosBrinquedosJSON = new TodosBrinquedosJSON
            {
                Brinquedos = todosBrinquedos.Select(BrinquedosToDTO).ToList()
            };

            return Ok();
        }
        catch (Exception ex)
        {
            return NotFound();
        }
    }

    [HttpPost("AtualizarQuantidadeEVendas/{id}")]
public async Task<ActionResult> AtualizarQuantidadeEVendas(long id)
{
    try
    {
        var brinquedo = await _contexto.TodoBrinquedos.FindAsync(id);

        if (brinquedo == null)
        {
            return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
        }

        brinquedo.quantidade -= 1;
        brinquedo.vendastotais += 1;

        await _contexto.SaveChangesAsync();

        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao atualizar a quantidade e vendas totais do brinquedo");
    }
}


    private bool TodosBrinquedosExists(long id)
    {
       return _contexto.TodoBrinquedos.Any(e => e.id == id);
    }

    private static TodosBrinquedosDTO BrinquedosToDTO(TodosBrinquedos TodoBrinquedo) =>
       new TodosBrinquedosDTO
       {
           id = TodoBrinquedo.id,
           brinquedo = TodoBrinquedo.brinquedo,
           preco = TodoBrinquedo.preco,
           quantidade = TodoBrinquedo.quantidade,
           vendastotais = TodoBrinquedo.vendastotais,
       };
}
