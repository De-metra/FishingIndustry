using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FishingIndustry.Data;
using FishingIndustry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using FishingIndustry.ViewModels;
using System.Security.Policy;

namespace FishingIndustry.Controllers
{
    public class VesselsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FishTypesController> _logger;

        public VesselsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<FishTypesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Vessels.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vessel = await _context.Vessels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vessel == null)
            {
                return NotFound();
            }

            return View(vessel);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VesselViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректные данные модели: {@ModelErrors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return View(viewModel);
            }

            if (viewModel.ImageUpload == null)
            {
                ModelState.AddModelError("ImageUpload", "Необходимо загрузить изображение");
                return View(viewModel);
            }

            if (viewModel.ImageUpload.Length == 0)
            {
                ModelState.AddModelError("ImageUpload", "Файл пустой");
                return View(viewModel);
            }

            if (viewModel.ImageUpload.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ImageUpload", "Файл слишком большой (макс. 5MB)");
                return View(viewModel);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(viewModel.ImageUpload.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ImageUpload", "Допустимы только JPG, PNG или GIF");
                return View(viewModel);
            }

            var vessel = new Vessel
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                RegistrationNumber = viewModel.RegistrationNumber,
                TypeVessel = viewModel.TypeVessel,
                Capacity = viewModel.Capacity,
                YearBuilt = viewModel.YearBuilt
            };

            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "vessel");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                
                string safeFileName = Path.GetFileNameWithoutExtension(viewModel.ImageUpload.FileName)
                    .Replace(" ", "_", StringComparison.Ordinal);
                string fileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageUpload.CopyToAsync(fileStream);
                }

                vessel.ImagePath = $"/images/vessel/{fileName}";

                _context.Add(vessel);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Успешно создано новое судно: {VesselName} (ID: {Id})",
                vessel.Name, vessel.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Ошибка доступа к файловой системе при сохранении изображения");
                ModelState.AddModelError("", "Ошибка доступа к файловой системе");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Ошибка ввода-вывода при сохранении изображения");
                ModelState.AddModelError("", "Ошибка при сохранении файла");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при сохранении");
                ModelState.AddModelError("", "Ошибка при сохранении в базу данных");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при создании");
                ModelState.AddModelError("", "Произошла непредвиденная ошибка");
            }

            return View(viewModel);
          
        }

      
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel == null)
            {
                return NotFound();
            }

            var viewModel = new VesselViewModel
            {
                Id = vessel.Id,
                Name = vessel.Name,
                Description = vessel.Description,
                RegistrationNumber = vessel.RegistrationNumber,
                TypeVessel = vessel.TypeVessel,
                Capacity = vessel.Capacity,
                YearBuilt = vessel.YearBuilt
            };

            return View(viewModel);

        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VesselViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var vessel = await _context.Vessels
                .FirstOrDefaultAsync(f => f.Id == id);

                if (vessel == null) return NotFound();

                vessel.Name = viewModel.Name;
                vessel.Description = viewModel.Description;
                vessel.TypeVessel = viewModel.TypeVessel;
                vessel.Capacity = viewModel.Capacity;
                vessel.RegistrationNumber = viewModel.RegistrationNumber;
                vessel.YearBuilt = viewModel.YearBuilt;

                if (viewModel.ImageUpload != null && viewModel.ImageUpload.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(viewModel.ImageUpload.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageUpload", "Допустимы только JPG, PNG или GIF");

                        return View(viewModel);
                    }

                    // Удаляем старое изображение
                    if (!string.IsNullOrEmpty(vessel.ImagePath))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, vessel.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    // Сохраняем новое изображение
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "vessel");
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.ImageUpload.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImageUpload.CopyToAsync(fileStream);
                    }

                    vessel.ImagePath = $"/images/vessel/{uniqueFileName}";
                }


                try
                {
                    _context.Update(vessel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении судов");
                    ModelState.AddModelError("", "Произошла ошибка при сохранении данных");
                }

               

            }
            return View(viewModel);

        }

       
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vessel = await _context.Vessels
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vessel == null)
            {
                return NotFound();
            }

            return View(vessel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vessel = await _context.Vessels.FindAsync(id);
            if (vessel != null)
            {
                _context.Vessels.Remove(vessel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
