using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UenodaiAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// DBContextの設定
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// CORS 設定　
// AddCors()は Action<CorsOptions> を受け取り、IServiceCollection に CORS 設定を追加。
// AddPolicy()は、ポリシー名（string 型）と 設定用のアクション（Action<CorsPolicyBuilder> 型）を引数として受け取る。

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // 全てのオリジンを許可
              .AllowAnyMethod()  // 全ての HTTP メソッドを許可
              .AllowAnyHeader(); // 全てのヘッダーを許可
    });
});

var app = builder.Build();

// DB接続確認
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Database connection successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
    }
}

// 開発環境でSwaggerを有効に
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/uploads"
});

app.Run();
