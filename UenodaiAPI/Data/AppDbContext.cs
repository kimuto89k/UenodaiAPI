using Microsoft.EntityFrameworkCore;
using UenodaiCommon.Models;

namespace UenodaiAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Model型のコレクションに対するエントリーポイントを定義。
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Eventsというプロパティでアクセスを可能になった Event型のエンティティをデータベース上のT_EVENTテーブルに一致させる。
            modelBuilder.Entity<Event>().ToTable("T_EVENT");
            modelBuilder.Entity<User>().ToTable("T_User");
        }
    }
}
