using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FishingIndustry.Data;
using FishingIndustry.Models;
using FishingIndustry.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace FishingIndustry.Controllers
{
    public class FishTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FishTypesController> _logger;

        public FishTypesController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<FishTypesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var fishWithZones = await _context.FishTypes
                .Include(f => f.FishingZones)
                .ToListAsync();

            return View(fishWithZones);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fishType = await _context.FishTypes
                .Include(f => f.FishingZones)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fishType == null)
            {
                return NotFound();
            }

            return View(fishType);
        }

        [Authorize]
        public IActionResult Create()
        {
            var viewModel = new FishTypeViewModel
            {
                AllFishingZones = _context.FishingZones
            .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
            .ToList()
            };
            
            return View(viewModel);
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FishTypeViewModel fishType)
        {
            _logger.LogInformation("Начало обработки создания новой рыбы");

            fishType.AllFishingZones = await _context.FishingZones
                .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректные данные модели: {@ModelErrors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return View(fishType);
            }

            // Проверка размера файла должна быть после проверки на null
            if (fishType.ImageUpload == null)
            {
                ModelState.AddModelError("ImageUpload", "Необходимо загрузить изображение");
                return View(fishType);
            }

            if (fishType.ImageUpload.Length == 0)
            {
                ModelState.AddModelError("ImageUpload", "Файл пустой");
                return View(fishType);
            }

            if (fishType.ImageUpload.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ImageUpload", "Файл слишком большой (макс. 5MB)");
                return View(fishType);
            }

            // Проверка расширения файла
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(fishType.ImageUpload.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ImageUpload", "Допустимы только JPG, PNG или GIF");
                return View(fishType);
            }

            var fish = new FishType
            {
                Name = fishType.Name,
                Description = fishType.Description,
                AveragePricePerKg = fishType.AveragePricePerKg,
                Family = fishType.Family,
                HabitatType = fishType.HabitatType
            };

            // Найти выбранные зоны
            var selectedZones = _context.FishingZones
                .Where(z => fishType.SelectedZoneIds.Contains(z.Id))
                .ToList();

            // Присвоить в навигационное свойство
            fish.FishingZones = selectedZones;

            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "fish");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Безопасное имя файла
                string safeFileName = Path.GetFileNameWithoutExtension(fishType.ImageUpload.FileName)
                    .Replace(" ", "_", StringComparison.Ordinal);
                string fileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fishType.ImageUpload.CopyToAsync(fileStream);
                }

                fish.ImagePath = $"/images/fish/{fileName}";

                _context.Add(fish);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Успешно создана новая рыба: {FishName} (ID: {FishId})",
                    fish.Name, fish.Id);

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
                _logger.LogError(ex, "Ошибка базы данных при сохранении рыбы");
                ModelState.AddModelError("", "Ошибка при сохранении в базу данных");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при создании рыбы");
                ModelState.AddModelError("", "Произошла непредвиденная ошибка");
            }

            return View(fishType);
        }

  
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fishType = await _context.FishTypes
            .Include(f => f.FishingZones)
            .FirstOrDefaultAsync(f => f.Id == id);

            if (fishType == null)
            {
                return NotFound();
            }

            var viewModel = new FishTypeViewModel
            {
                Id = fishType.Id,
                Name = fishType.Name,
                Description = fishType.Description,
                AveragePricePerKg = fishType.AveragePricePerKg,
                Family = fishType.Family,
                HabitatType = fishType.HabitatType,
                SelectedZoneIds = fishType.FishingZones.Select(z => z.Id).ToList(),
                AllFishingZones = await _context.FishingZones
                    .Select(z => new SelectListItem
                   {
                       Value = z.Id.ToString(),
                       Text = z.Name
                   }).ToListAsync()
            };

            return View(viewModel);
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FishTypeViewModel fishType)
        {
            if (id != fishType.Id)
            {
                return NotFound();
            }

            fishType.AllFishingZones = await _context.FishingZones
                .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                fishType.AllFishingZones = await _context.FishingZones
                    .Select(z => new SelectListItem
                    {
                        Value = z.Id.ToString(),
                        Text = z.Name
                    }).ToListAsync();
                return View(fishType);
            }

            var fish = await _context.FishTypes
                .Include(f => f.FishingZones)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fish == null)
                return NotFound();

            fish.Name = fishType.Name;
            fish.Description = fishType.Description;
            fish.AveragePricePerKg = fishType.AveragePricePerKg;
            fish.Family = fishType.Family;
            fish.HabitatType = fishType.HabitatType;

            // Обновление изображения, если новое загружено
            if (fishType.ImageUpload != null && fishType.ImageUpload.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fishType.ImageUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageUpload", "Допустимы только JPG, PNG или GIF");
                    fishType.AllFishingZones = await _context.FishingZones
                        .Select(z => new SelectListItem
                        {
                            Value = z.Id.ToString(),
                            Text = z.Name
                        }).ToListAsync();
                    return View(fishType);
                }

                // Удаляем старое изображение
                if (!string.IsNullOrEmpty(fish.ImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, fish.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Сохраняем новое изображение
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "fish");
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fishType.ImageUpload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fishType.ImageUpload.CopyToAsync(fileStream);
                }

                fish.ImagePath = $"/images/fish/{uniqueFileName}";
            }

            // Обновляем связи с зонами
            var selectedZoneIds = fishType.SelectedZoneIds ?? new List<int>();
            var selectedZones = await _context.FishingZones.Where(z => selectedZoneIds.Contains(z.Id)).ToListAsync();

            fish.FishingZones.Clear();
            foreach (var zone in selectedZones)
            {
                fish.FishingZones.Add(zone);
            }

            try
            {
                _context.Update(fish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении рыбы");
                ModelState.AddModelError("", "Произошла ошибка при сохранении данных");
            }

            fishType.AllFishingZones = await _context.FishingZones
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                }).ToListAsync();

            return View(fishType);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fishType = await _context.FishTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fishType == null)
            {
                return NotFound();
            }

            return View(fishType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fishType = await _context.FishTypes.FindAsync(id);
            if (fishType != null)
            {
                _context.FishTypes.Remove(fishType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
