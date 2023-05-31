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

namespace BrinquedosAPI.Controllers
{
    [Route("api/TodosBrinquedos")]
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
            var filePath = "C:\\Users\\tiago\\OneDrive\\Documentos\\GitHub\\APIMaquinadevendas\\BrinquedosAPI\\brinquedos.json";

            var conteudoarquivo = await System.IO.File.ReadAllTextAsync(filePath);

            var brinquedosconvertidos = JsonConvert.DeserializeObject<TodosBrinquedosJSON>(conteudoarquivo);

            var brinquedoscarregados = brinquedosconvertidos?.Brinquedos ?? new List<TodosBrinquedosDTO>();

            return brinquedoscarregados;
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
        public async Task<IActionResult> DeleteTodosBrinquedos([FromBody] List<long> ids)
        {
            try
            {
                var filePath = "C:\\Users\\tiago\\OneDrive\\Documentos\\GitHub\\APIMaquinadevendas\\BrinquedosAPI\\brinquedos.json";

                // Lê o conteúdo atual do arquivo JSON
                var json = await System.IO.File.ReadAllTextAsync(filePath);

                // Desserializa o conteúdo JSON para um objeto
                var todosBrinquedosJSON = JsonConvert.DeserializeObject<TodosBrinquedosJSON>(json);

                // Remove os brinquedos correspondentes aos IDs fornecidos
                todosBrinquedosJSON.Brinquedos.RemoveAll(b => ids.Contains(b.Id));

                // Serializa o objeto atualizado de volta para JSON
                var updatedJson = JsonConvert.SerializeObject(todosBrinquedosJSON);

                // Salva o conteúdo atualizado de volta no arquivo
                await System.IO.File.WriteAllTextAsync(filePath, updatedJson);

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
        public async Task<IActionResult> AddOrUpdateBrinquedo([FromBody] List<TodosBrinquedosDTO> TodosBrinquedosDTO)
        {
            try
            {
                foreach (var brinquedoDTO in TodosBrinquedosDTO)
                {
                    // Verifica se o brinquedo já existe no banco de dados com base no ID
                    var brinquedoExistente = await _contexto.TodoBrinquedos.FindAsync(brinquedoDTO.Id);

                    if (brinquedoExistente != null)
                    {
                        // O brinquedo já existe, então atualiza os valores
                        brinquedoExistente.Brinquedo = brinquedoDTO.Brinquedo;
                        brinquedoExistente.Preco = brinquedoDTO.Preco;
                        brinquedoExistente.Quantidade = brinquedoDTO.Quantidade;
                        brinquedoExistente.VendasTotais = brinquedoDTO.VendasTotais;
                    }
                    else
                    {
                        // O brinquedo não existe, então cria um novo
                        var novoBrinquedo = new TodosBrinquedos
                        {
                            Brinquedo = brinquedoDTO.Brinquedo,
                            Preco = brinquedoDTO.Preco,
                            Quantidade = brinquedoDTO.Quantidade,
                            VendasTotais = brinquedoDTO.VendasTotais
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

                var json = JsonConvert.SerializeObject(TodosBrinquedosJSON);

                var filePath = "C:\\Users\\tiago\\OneDrive\\Documentos\\GitHub\\APIMaquinadevendas\\BrinquedosAPI\\brinquedos.json";

                await System.IO.File.WriteAllTextAsync(filePath, json);

                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound();
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
               Brinquedo = TodoBrinquedo.Brinquedo,
               Preco = TodoBrinquedo.Preco,
               Quantidade = TodoBrinquedo.Quantidade,
               VendasTotais = TodoBrinquedo.VendasTotais
           };
    }
}
