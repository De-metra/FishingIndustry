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
using FishingIndustry.ViewModels;

namespace FishingIndustry.Controllers
{
    public class FishingZonesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FishTypesController> _logger;

        public FishingZonesController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<FishTypesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var fishingZones = await _context.FishingZones
               .Include(f => f.FishTypes)
               .ToListAsync();

            return View(fishingZones);
        }

      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fishingZone = await _context.FishingZones
                 .Include(f => f.FishTypes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fishingZone == null)
            {
                return NotFound();
            }

            return View(fishingZone);
        }

        [Authorize]
        public IActionResult Create()
        {
            var viewModel = new FishingZoneViewModel
            {
                AvailableFishTypes = _context.FishTypes
             .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
             .ToList()
            };

            return View(viewModel);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FishingZoneViewModel viewModel)
        {
            viewModel.AvailableFishTypes = await _context.FishTypes
                .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                .ToListAsync();

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


            var zone = new FishingZone
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Latitude = viewModel.Latitude,
                Longitude = viewModel.Longitude
            };

            // Найти выбранных рыб
            var selectedFishes = _context.FishTypes
                .Where(z => viewModel.SelectedFishTypeIds.Contains(z.Id))
                .ToList();
            zone.FishTypes = selectedFishes;

            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "zone");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Безопасное имя файла
                string safeFileName = Path.GetFileNameWithoutExtension(viewModel.ImageUpload.FileName)
                    .Replace(" ", "_", StringComparison.Ordinal);
                string fileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageUpload.CopyToAsync(fileStream);
                }

                zone.ImagePath = $"/images/zone/{fileName}";

                _context.Add(zone);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Успешно создан новый водоём: {ZoneName} (ID: {Id})",
                    zone.Name, zone.Id);

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

            var fishingZone = await _context.FishingZones
            .Include(f => f.FishTypes)
            .FirstOrDefaultAsync(f => f.Id == id);

            if (fishingZone == null)
            {
                return NotFound();
            }

            var viewModel = new FishingZoneViewModel
            {
                Id = fishingZone.Id,
                Name = fishingZone.Name,
                Description = fishingZone.Description,
                Latitude = fishingZone.Latitude,
                Longitude = fishingZone.Longitude,
                SelectedFishTypeIds = fishingZone.FishTypes.Select(z => z.Id).ToList(),
                AvailableFishTypes = await _context.FishTypes
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
        public async Task<IActionResult> Edit(int id, FishingZoneViewModel viewModel)
        {
             viewModel.AvailableFishTypes = await _context.FishTypes
            .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
            .ToListAsync();
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                viewModel.AvailableFishTypes = await _context.FishTypes
                    .Select(z => new SelectListItem
                    {
                        Value = z.Id.ToString(),
                        Text = z.Name
                    }).ToListAsync();
                return View(viewModel);
            }

            var zone = await _context.FishingZones
                .Include(f => f.FishTypes)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (zone == null)  return NotFound();

            zone.Name = viewModel.Name;
            zone.Description = viewModel.Description;
            zone.Latitude = viewModel.Latitude;
            zone.Longitude = viewModel.Longitude;

            // Обновление изображения, если новое загружено
            if (viewModel.ImageUpload != null && viewModel.ImageUpload.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(viewModel.ImageUpload.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageUpload", "Допустимы только JPG, PNG или GIF");
                    viewModel.AvailableFishTypes = await _context.FishTypes
                        .Select(z => new SelectListItem
                        {
                            Value = z.Id.ToString(),
                            Text = z.Name
                        }).ToListAsync();
                    return View(viewModel);
                }

                // Удаляем старое изображение
                if (!string.IsNullOrEmpty(zone.ImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, zone.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Сохраняем новое изображение
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "zone");
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.ImageUpload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageUpload.CopyToAsync(fileStream);
                }

                zone.ImagePath = $"/images/zone/{uniqueFileName}";
            }

            // Обновляем связи с рыбами
            var selectedFishIds = viewModel.SelectedFishTypeIds ?? new List<int>();
            var currentFishIds = zone.FishTypes.Select(f => f.Id).ToList();

            // Удаляем отсутствующие связи
            foreach (var fish in zone.FishTypes.ToList())
            {
                if (!selectedFishIds.Contains(fish.Id))
                {
                    zone.FishTypes.Remove(fish);
                }
            }

            // Добавляем новые связи
            var fishToAdd = await _context.FishTypes
                .Where(f => selectedFishIds.Contains(f.Id) && !currentFishIds.Contains(f.Id))
                .ToListAsync();

            foreach (var fish in fishToAdd)
            {
                zone.FishTypes.Add(fish);
            }

            try
            {
                _context.Update(zone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении водоёмов");
                ModelState.AddModelError("", "Произошла ошибка при сохранении данных");
            }

            viewModel.AvailableFishTypes = await _context.FishTypes
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                }).ToListAsync();

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fishingZone = await _context.FishingZones
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fishingZone == null)
            {
                return NotFound();
            }

            return View(fishingZone);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fishingZone = await _context.FishingZones.FindAsync(id);
            if (fishingZone != null)
            {
                _context.FishingZones.Remove(fishingZone);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
