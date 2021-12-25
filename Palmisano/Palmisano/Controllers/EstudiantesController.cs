using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Palmisano;
using Palmisano.Models;
using Palmisano.ViewModel;

namespace Palmisano.Controllers
{
    [Authorize(Roles = Roles.SuperAdminRole)]
    public class EstudiantesController : Controller 
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;

        public EstudiantesController(ApplicationDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }

        // GET: Estudiantes

        [AllowAnonymous]
        public async Task<IActionResult> Index(string textoBusqueda, int? CarreraId, int pagina = 1)

        {
            ShowRequest();
            int RegistrosPorPagina = 4;


            //Para generar la    consulta con los filtros  
            var applicationDbContext = _context.Estudiantes.Include(e => e.Carrera).Select(e => e);
            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                applicationDbContext = applicationDbContext.Where(e => e.Nombre.Contains(textoBusqueda));
            }

            if (CarreraId.HasValue)
            {
                applicationDbContext = applicationDbContext.Where(e => e.CarreraId == CarreraId.Value);
            }

            //ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Descripcion",CarreraId);
            //Generar pagina
            var registrosMostrar = applicationDbContext
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina);


            //Para crear el modelo para la vista
            EstudiantesViewModel modelo = new EstudiantesViewModel()
            {
                Estudiantes = await applicationDbContext.ToListAsync(),
                ListCarreras = new SelectList(_context.Carreras, "Id", "Descripcion", CarreraId),
                textoBusqueda = textoBusqueda,
                CarreraId = CarreraId
            };
            modelo.Paginador.PaginaActual = pagina;
            modelo.Paginador.RegistrosPorPagina = RegistrosPorPagina;
            modelo.Paginador.TotalRegistros = await applicationDbContext.CountAsync();
            if (!string.IsNullOrEmpty(textoBusqueda))
                modelo.Paginador.ValoresQueryString.Add("textoBusqueda", textoBusqueda);
            if (CarreraId.HasValue)
                modelo.Paginador.ValoresQueryString.Add("CarreraId", CarreraId.Value.ToString());


            // modelo.Paginador.ValoresQueryString.Add("CarreraId", CarreraId.HasValue ? CarreraId.Value.ToString() : "");


            return View(modelo);
        }
        private void ShowRequest()
        {
            Console.WriteLine("Query");

            Console.WriteLine(this.Request.QueryString);
            //textoBusqueda=&CarreraId=1&boton=Filtrar&Pagina=1
            foreach (var item in Request.Query)
            {
                Console.WriteLine($"   {item.Key}: {item.Value}");
            }

        }

        // GET: Estudiantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (estudiante == null)
            {
                return NotFound();
            }

            return View(estudiante);
        }

        // GET: Estudiantes/Create
        public IActionResult Create()
        {
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Descripcion");
            return View();
        }

        // POST: Estudiantes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Edad,Año,CarreraId,Localidad,Calle")] Estudiante estudiante)
        {
            if (ModelState.IsValid)
            {
                var archivos = HttpContext.Request.Form.Files;
                if(archivos!=null && archivos.Count >0)
                {
                    var archivoFoto = archivos[0];
                    var pathDestino =  Path.Combine(env.WebRootPath, "images\\estudiantes");
                    if (archivoFoto.Length >0)
                    {
                        // var archivoDestino = Guid.NewGuid().ToString().Replace("-", "");
                        var archivoDestino = Guid.NewGuid().ToString();
                        archivoDestino = archivoDestino.Replace("-", "");
                        archivoDestino += Path.GetExtension(archivoFoto.FileName);

                        using (var filestream = new FileStream(Path.Combine(pathDestino, archivoDestino), FileMode.Create))
                        {
                            archivoFoto.CopyTo(filestream);
                            estudiante.NombreFoto = archivoDestino;
                        };
                      
                    }
                    
                }
                _context.Add(estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Descripcion", estudiante.CarreraId);
            return View(estudiante);
        }

        // GET: Estudiantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var estudiante = await _context.Estudiantes.FindAsync(id);
            var estudiante = await _context.Estudiantes.Include(x => x.EstudianteMateria).ThenInclude(m => m.Materia).FirstOrDefaultAsync(e => e.Id == id);
            if (estudiante == null)
            {
                return NotFound();
            }
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Descripcion", estudiante.CarreraId);
            return View(estudiante);
        }

        // POST: Estudiantes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Edad,Año,CarreraId,NombreFoto,Localidad,Calle")] Estudiante estudiante)
        {
            if (id != estudiante.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var archivos = HttpContext.Request.Form.Files;
                if (archivos != null && archivos.Count > 0)
                {
                    var archivoFoto = archivos[0];
                    var pathDestino = Path.Combine(env.WebRootPath, "images\\estudiantes");
                    if (archivoFoto.Length > 0)
                    {
                        // var archivoDestino = Guid.NewGuid().ToString().Replace("-", "");
                        var archivoDestino = Guid.NewGuid().ToString();
                        archivoDestino = archivoDestino.Replace("-", "");
                        archivoDestino += Path.GetExtension(archivoFoto.FileName);

                        using (var filestream = new FileStream(Path.Combine(pathDestino, archivoDestino), FileMode.Create))
                        {
                            archivoFoto.CopyTo(filestream);
                            string viejoarchivo = Path.Combine(pathDestino, estudiante.NombreFoto ?? "");
                            if(System.IO.File.Exists(viejoarchivo))
                            System.IO.File.Delete(viejoarchivo);
                            estudiante.NombreFoto = archivoDestino;
                        };

                    }

                }
                try
                {
                    _context.Update(estudiante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstudianteExists(estudiante.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Descripcion", estudiante.CarreraId);
            return View(estudiante);
        }

        // GET: Estudiantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (estudiante == null)
            {
                return NotFound();
            }

            return View(estudiante);
        }

        // POST: Estudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.Id == id);
        }
    }
}
