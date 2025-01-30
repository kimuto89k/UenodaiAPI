using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UenodaiAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// DBContext�̐ݒ�
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// CORS �ݒ�@
// AddCors()�� Action<CorsOptions> ���󂯎��AIServiceCollection �� CORS �ݒ��ǉ��B
// AddPolicy()�́A�|���V�[���istring �^�j�� �ݒ�p�̃A�N�V�����iAction<CorsPolicyBuilder> �^�j�������Ƃ��Ď󂯎��B

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // �S�ẴI���W��������
              .AllowAnyMethod()  // �S�Ă� HTTP ���\�b�h������
              .AllowAnyHeader(); // �S�Ẵw�b�_�[������
    });
});

var app = builder.Build();

// DB�ڑ��m�F
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

// �J������Swagger��L����
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
