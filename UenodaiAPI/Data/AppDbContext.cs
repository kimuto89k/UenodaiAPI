using Microsoft.EntityFrameworkCore;
using UenodaiCommon.Models;

namespace UenodaiAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Model�^�̃R���N�V�����ɑ΂���G���g���[�|�C���g���`�B
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Events�Ƃ����v���p�e�B�ŃA�N�Z�X���\�ɂȂ��� Event�^�̃G���e�B�e�B���f�[�^�x�[�X���T_EVENT�e�[�u���Ɉ�v������B
            modelBuilder.Entity<Event>().ToTable("T_EVENT");
            modelBuilder.Entity<User>().ToTable("T_User");
        }
    }
}
