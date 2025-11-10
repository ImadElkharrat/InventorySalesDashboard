using Microsoft.AspNetCore.Hosting;

namespace InventorySalesDashboard.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Validate file size
            if (imageFile.Length > _maxFileSize)
                throw new InvalidOperationException("File size exceeds 5MB limit.");

            // Validate file extension
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file type. Allowed: JPG, PNG, GIF, BMP.");

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/uploads/products/{uniqueFileName}";
        }

        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "products", fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}