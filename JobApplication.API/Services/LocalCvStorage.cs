using JobApplication.Application.Interfaces;

namespace JobApplication.API.Services
{
    // CVs live in <content root>/Uploads/cvs - deliberately OUTSIDE wwwroot so they are never served as static files.
    public class LocalCvStorage : ICvStorage
    {
        private readonly string _folder;

        public LocalCvStorage(IWebHostEnvironment env)
        {
            _folder = Path.Combine(env.ContentRootPath, "Uploads", "cvs");
        }

        public async Task<string> SaveAsync(Stream content, string extension)
        {
            Directory.CreateDirectory(_folder);

            var fileName = $"{Guid.NewGuid():N}{extension}"; // random name: the client's file name is never used
            await using var target = new FileStream(Path.Combine(_folder, fileName), FileMode.CreateNew);
            await content.CopyToAsync(target);

            return $"cvs/{fileName}";
        }

        public void Delete(string cvUrl)
        {
            var path = Path.Combine(_folder, Path.GetFileName(cvUrl));
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
