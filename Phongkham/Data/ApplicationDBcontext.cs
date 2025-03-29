using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Phongkham.Models;

namespace Phongkham.Data
{
    public class ApplicationDBcontext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBcontext(DbContextOptions<ApplicationDBcontext> options) : base(options) { }

        public DbSet<Chuyenmon> Chuyenmons { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Cakham> Cakhams { get; set; }
        public DbSet<KhungGio> KhungGios { get; set; }
        public DbSet<lichKham> lichKhams { get; set; }
        public DbSet<LichKhamVL> lichKhamvls { get; set; }
        public DbSet<CTlichkham> cTlichkhams { get; set; }
        public DbSet<CtlichkhamVL> Ctlkvl { get; set; }
        public DbSet<Loaitintuc> Loaitintucs { get; set; }
        public DbSet<Tintuc> Tintucs { get; set; }
        public DbSet<TintucImage> TintucImages { get; set; }
        public DbSet<CTnhasi> cTnhasis { get; set; }
        public DbSet<CakhamKhungGio> CakhamKhungGios { get; set; }
        public DbSet<Phieukham> phieukhams { get; set; }
        public DbSet<dichvu> dichvus { get; set; }
        public DbSet<DichvuImage> DichvuImages { get; set; }
        public DbSet<PhieuKhamDichvu> PhieuKhamDichvus { get; set; }
        public DbSet<Thuoc> Thuocs { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }
        public DbSet<ChiTietDonThuoc> ChiTietDonThuocs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Dungdv> Dungdvs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thêm dữ liệu mặc định cho bảng KhungGio
            modelBuilder.Entity<KhungGio>().HasData(
                new KhungGio { Id = 1, TimeSlot = "7:00 - 8:00" },
                new KhungGio { Id = 2, TimeSlot = "8:00 - 9:00" },
                new KhungGio { Id = 3, TimeSlot = "9:00 - 10:00" },
                new KhungGio { Id = 4, TimeSlot = "10:00 - 11:00" },
                new KhungGio { Id = 5, TimeSlot = "13:00 - 14:00" },
                new KhungGio { Id = 6, TimeSlot = "14:00 - 15:00" },
                new KhungGio { Id = 7, TimeSlot = "15:00 - 16:00" },
                new KhungGio { Id = 8, TimeSlot = "16:00 - 17:00" }
            );

            // Cấu hình bảng nối CakhamKhungGio
            modelBuilder.Entity<CakhamKhungGio>()
                .HasKey(ck => new { ck.CakhamId, ck.KhungGioId });

            modelBuilder.Entity<CakhamKhungGio>()
                .HasOne(ck => ck.Cakham)
                .WithMany(c => c.CakhamKhungGios)
                .HasForeignKey(ck => ck.CakhamId);

            modelBuilder.Entity<CakhamKhungGio>()
                .HasOne(ck => ck.KhungGio)
                .WithMany()
                .HasForeignKey(ck => ck.KhungGioId);

            // Thêm vai trò mặc định vào bảng Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Dentist", NormalizedName = "DENTIST" },
                new IdentityRole { Id = "3", Name = "Patient", NormalizedName = "PATIENT" },
                new IdentityRole { Id = "4", Name = "Staff", NormalizedName = "STAFF" }
            );
            modelBuilder.Entity<PhieuKhamDichvu>()
           .HasKey(pd => new { pd.PhieuKhamId, pd.DichvuId });

            modelBuilder.Entity<PhieuKhamDichvu>()
                .HasOne(pd => pd.PhieuKham)
                .WithMany(p => p.PhieuKhamDichvus)  // Liên kết với danh sách các dịch vụ trong Phiếu Khám
                .HasForeignKey(pd => pd.PhieuKhamId);

            modelBuilder.Entity<PhieuKhamDichvu>()
                .HasOne(pd => pd.Dichvu)
                .WithMany(d => d.PhieuKhamDichvus)  // Liên kết với danh sách các phiếu khám trong Dịch Vụ
                .HasForeignKey(pd => pd.DichvuId);


            modelBuilder.Entity<ChiTietDonThuoc>()
                .HasOne(ct => ct.DonThuoc)
                .WithMany(d => d.ChiTietDonThuocs)
                .HasForeignKey(ct => ct.DonThuocId);

            modelBuilder.Entity<ChiTietDonThuoc>()
                 .HasOne(ct => ct.Thuoc)
                 .WithMany(d => d.ChiTietDonThuocs)
                 .HasForeignKey(ct => ct.ThuocId);
        }
    }
}
