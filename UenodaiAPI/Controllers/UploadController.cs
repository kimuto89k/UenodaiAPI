using Microsoft.AspNetCore.Mvc;
using UenodaiAPI.Data;
using UenodaiCommon.Models;
namespace UenodaiAPI.Controllers { 

    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UploadController(AppDbContext context)
        {
            _context = context;
        }
        public enum ImageCategory
        {
            Profile,
            Event,
            Feed
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile imageFile, [FromForm] string category)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // 文字列を ImageCategory に変換
            if (!Enum.TryParse<ImageCategory>(category, true, out var imageCategory))
            {
                return BadRequest("Invalid category.");
            }

            var categoryPath = imageCategory.ToString().ToLower();

            // 保存先のディレクトリを設定
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", categoryPath);

            // フォルダが存在しない場合は作成
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // ファイル名を生成
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            // 保存先の完全パスを生成
            var filePath = Path.Combine(uploadPath, fileName);

            try
            {
                // ファイルを保存
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            // 画像URLを生成
            var imageUrl = $"/Uploads/{category}/{fileName}";

            return Ok(new { FilePath = imageUrl });
        }
    }
}
