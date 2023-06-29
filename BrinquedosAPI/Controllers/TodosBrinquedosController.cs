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

    // String de conexão com o banco de dados
    string conexaodb = "Server=localhost;Port=3306;Database=maquinadevendas;Uid=root;";

    // Método GET para obter todos os brinquedos disponíveis na máquina de vendas
    [HttpGet("ListaDeBrinquedos")]
    public async Task<ActionResult<IEnumerable<TodosBrinquedosDTO>>> GetTodosBrinquedos()
    {
        // Configuração do AutoMapper para mapear a classe TodosBrinquedos para TodosBrinquedosDTO
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedos, TodosBrinquedosDTO>();
        });

        // Criação do objeto IMapper com base na configuração do AutoMapper
        AutoMapper.IMapper mapper = config.CreateMapper();

        // Conexão com o banco de dados
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Consulta todos os brinquedos na tabela brinquedos
            var todosProdutos = await db.FetchAsync<TodosBrinquedos>("SELECT * FROM brinquedos");

            // Mapeia os objetos TodosBrinquedos para TodosBrinquedosDTO usando o AutoMapper
            var responseItems = mapper.Map<List<TodosBrinquedosDTO>>(todosProdutos);

            // Retorna a lista de brinquedos como resposta
            return Ok(responseItems);
        }
    }

    // Método GET para obter um brinquedo específico pelo ID
    [HttpGet("ListaDeBrinquedosPor/{id}")]
    public async Task<ActionResult<TodosBrinquedosDTO>> GetTodoBrinquedos(long id)
    {
        // Configuração do AutoMapper para mapear a classe TodosBrinquedos para TodosBrinquedosDTO
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedos, TodosBrinquedosDTO>();
        });

        // Criação do objeto IMapper com base na configuração do AutoMapper
        AutoMapper.IMapper mapper = config.CreateMapper();

        // Conexão com o banco de dados
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Consulta um brinquedo específico pelo ID na tabela brinquedos
            var brinquedo = await db.FirstOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE Id = @0", id);

            // Verifica se o brinquedo foi encontrado
            if (brinquedo == null)
            {
                return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
            }

            // Realiza o mapeamento do objeto brinquedo para um objeto TodosBrinquedosDTO usando o AutoMapper
            var brinquedoDTO = mapper.Map<TodosBrinquedosDTO>(brinquedo);

            // Retorna o brinquedo como resposta
            return Ok(brinquedoDTO);
        }
    }

    // Método POST para excluir brinquedos da base de dados pelo ID
    [HttpPost("DeleteBrinquedo")]
    public async Task<ActionResult> DeleteTodosBrinquedos([FromBody] List<long> ids)
    {
        try
        {
            // Conexão com o banco de dados
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                // Percorre a lista de IDs de brinquedos a serem excluídos
                foreach (var id in ids)
                {
                    // Consulta o brinquedo pelo ID
                    var todosBrinquedos = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", id);

                    // Verifica se o brinquedo foi encontrado
                    if (todosBrinquedos == null)
                    {
                        return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                    }
                    else
                    {
                        // Exclui o brinquedo da tabela brinquedos
                        await db.DeleteAsync("brinquedos", "id", todosBrinquedos);
                    }
                }
            }

            // Retorna uma resposta sem conteúdo
            return NoContent();
        }
        catch (Exception ex)
        {
            // Retorna uma resposta de erro interno do servidor
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao excluir brinquedo(s)");
        }
    }

    // Método POST para adicionar ou atualizar um brinquedo na base de dados
    [HttpPost("AddOrUpdateBrinquedo")]
    public async Task<ActionResult> AddOrUpdateBrinquedo([FromBody] List<TodosBrinquedosDTO> TodosBrinquedosDTO)
    {
        // Configuração do AutoMapper para mapear a classe TodosBrinquedosDTO para TodosBrinquedos
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosBrinquedosDTO, TodosBrinquedos>();
        });

        // Criação do objeto IMapper com base na configuração do AutoMapper
        AutoMapper.IMapper mapper = config.CreateMapper();

        // Conexão com o banco de dados
        using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
        {
            // Percorre a lista de brinquedos a serem adicionados ou atualizados
            foreach (var todosProdutosDTO in TodosBrinquedosDTO)
            {
                // Verifica se o brinquedo já existe na tabela brinquedos
                var produtoExistente = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", todosProdutosDTO.Id);

                if (produtoExistente == null)
                {
                    // Se o brinquedo não existe, cria um novo objeto TodosBrinquedos a partir do DTO e insere na tabela brinquedos
                    var novoProduto = mapper.Map<TodosBrinquedos>(todosProdutosDTO);
                    await db.InsertAsync("brinquedos", "id", true, novoProduto);
                }
                else
                {
                    // Se o brinquedo existe, atualiza os dados do objeto existente com base no DTO e atualiza na tabela brinquedos
                    produtoExistente = mapper.Map(todosProdutosDTO, produtoExistente);
                    await db.UpdateAsync("brinquedos", "id", produtoExistente);
                }
            }
        }

        // Retorna uma resposta de sucesso
        return Ok();
    }

    // Método POST para atualizar a quantidade e vendas totais de um brinquedo após uma venda
    [HttpPost("AtualizarQuantidadeEVendas/{id}")]
    public async Task<ActionResult> AtualizarQuantidadeEVendas(long id)
    {
        try
        {
            // Conexão com o banco de dados
            using (var db = new Database(conexaodb, "MySql.Data.MySqlClient"))
            {
                // Consulta o brinquedo pelo ID
                var brinquedo = await db.SingleOrDefaultAsync<TodosBrinquedos>("SELECT * FROM brinquedos WHERE id = @0", id);

                // Verifica se o brinquedo foi encontrado
                if (brinquedo == null)
                {
                    return NotFound($"Não foi encontrado nenhum Brinquedo com o Id: {id}. Insira outro Id.");
                }

                // Atualiza a quantidade e vendas totais do brinquedo
                brinquedo.quantidade -= 1;
                brinquedo.vendastotais += 1;

                // Atualiza o brinquedo na tabela brinquedos
                await db.UpdateAsync("brinquedos", "id", brinquedo);

                // Retorna uma resposta sem conteúdo
                return NoContent();
            }
        }
        catch (Exception ex)
        {
            // Retorna uma resposta de erro interno do servidor
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao atualizar a quantidade e vendas totais do brinquedo");
        }
    }

    // Método auxiliar para verificar se um brinquedo com o ID especificado existe
    private bool TodosBrinquedosExists(long id)
    {
        return _contexto.TodoBrinquedos.Any(e => e.Id == id);
    }
    
}