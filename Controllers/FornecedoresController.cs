using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CadastroFornecedores.Models;

namespace CadastroFornecedores.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<FornecedoresController> _logger;

        public FornecedoresController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<FornecedoresController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // GET: Fornecedores
        public async Task<IActionResult> Index()
        {
            var fornecedores = await _context.Fornecedores.ToListAsync();
            return View(fornecedores);
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SaveFotoToServer(fornecedor);

                    _context.Add(fornecedor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Erro ao criar fornecedor");
                }
            }

            LogModelStateErrors();
            return View(fornecedor);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fornecedor fornecedor)
        {
            if (id != fornecedor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica se um novo arquivo de imagem foi enviado
                    if (fornecedor.FotoArquivo != null && fornecedor.FotoArquivo.Length > 0)
                    {
                        // Salva o novo arquivo de imagem no servidor
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fornecedor.FotoArquivo.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fornecedor.FotoArquivo.CopyToAsync(fileStream);
                        }

                        fornecedor.Foto = "/images/" + uniqueFileName; // Atualiza o caminho da foto no banco de dados
                    }
                    else
                    {
                        // Se nenhum novo arquivo de imagem foi enviado, mantém a foto existente
                        var existingFornecedor = await _context.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.ID == fornecedor.ID);
                        fornecedor.Foto = existingFornecedor.Foto;
                    }

                    _context.Update(fornecedor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(fornecedor.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Se chegou aqui, há erros de validação no ModelState
            // Garantir que a validação obrigatória para FotoArquivo seja restaurada se necessário
            if (fornecedor.FotoArquivo == null || fornecedor.FotoArquivo.Length == 0)
            {
                ModelState.AddModelError("FotoArquivo", "Selecione uma imagem.");
            }

            return View(fornecedor);
        }

        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(m => m.ID == id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // POST: Fornecedores/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            _context.Fornecedores.Remove(fornecedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(m => m.ID == id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // Método auxiliar para salvar foto no servidor
        private async Task SaveFotoToServer(Fornecedor fornecedor)
        {
            if (fornecedor.FotoArquivo != null && fornecedor.FotoArquivo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fornecedor.FotoArquivo.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fornecedor.FotoArquivo.CopyToAsync(fileStream);
                }

                fornecedor.Foto = "/images/" + uniqueFileName;
            }
        }

        // Método auxiliar para registrar erros de modelo no log
        private void LogModelStateErrors()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                _logger.LogWarning("ModelState Error: {ErrorMessage}", error.ErrorMessage);
                if (error.Exception != null)
                {
                    _logger.LogWarning("Exception: {Exception}", error.Exception);
                }
            }
        }

        // Método auxiliar para lidar com erros gerais
        private void HandleError(Exception ex, string errorMessage)
        {
            _logger.LogError(ex, errorMessage);
            ModelState.AddModelError("", $"{errorMessage}: {ex.Message}");
        }

        // Verifica se um fornecedor existe
        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores.Any(e => e.ID == id);
        }
    }
}
