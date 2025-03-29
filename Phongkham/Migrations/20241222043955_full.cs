using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Phongkham.Migrations
{
    /// <inheritdoc />
    public partial class full : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chuyenmons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chuyenmons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dichvus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dichvus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhungGios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhungGios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loaitintucs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loaitintucs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Thuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thuocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cakhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DentistId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cakhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cakhams_AspNetUsers_DentistId",
                        column: x => x.DentistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserImages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "cTnhasis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chuyenmonId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cTnhasis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cTnhasis_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cTnhasis_Chuyenmons_chuyenmonId",
                        column: x => x.chuyenmonId,
                        principalTable: "Chuyenmons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DichvuImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DichvuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichvuImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DichvuImages_dichvus_DichvuId",
                        column: x => x.DichvuId,
                        principalTable: "dichvus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lichKhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KhungGioId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lichKhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lichKhams_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lichKhams_KhungGios_KhungGioId",
                        column: x => x.KhungGioId,
                        principalTable: "KhungGios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lichKhamvls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    KhungGioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lichKhamvls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lichKhamvls_KhungGios_KhungGioId",
                        column: x => x.KhungGioId,
                        principalTable: "KhungGios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tintucs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tieude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaitintucId = table.Column<int>(type: "int", nullable: false),
                    Mp3Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tintucs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tintucs_Loaitintucs_LoaitintucId",
                        column: x => x.LoaitintucId,
                        principalTable: "Loaitintucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CakhamKhungGios",
                columns: table => new
                {
                    CakhamId = table.Column<int>(type: "int", nullable: false),
                    KhungGioId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    KhungGioId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CakhamKhungGios", x => new { x.CakhamId, x.KhungGioId });
                    table.ForeignKey(
                        name: "FK_CakhamKhungGios_Cakhams_CakhamId",
                        column: x => x.CakhamId,
                        principalTable: "Cakhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CakhamKhungGios_KhungGios_KhungGioId",
                        column: x => x.KhungGioId,
                        principalTable: "KhungGios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CakhamKhungGios_KhungGios_KhungGioId1",
                        column: x => x.KhungGioId1,
                        principalTable: "KhungGios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "cTlichkhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichkhamId = table.Column<int>(type: "int", nullable: false),
                    TenNhaSi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenBenhNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhongKham = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cTlichkhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cTlichkhams_lichKhams_LichkhamId",
                        column: x => x.LichkhamId,
                        principalTable: "lichKhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ctlkvl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichkhamVLId = table.Column<int>(type: "int", nullable: false),
                    TenNhaSi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenBenhNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhongKham = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ctlkvl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ctlkvl_lichKhamvls_LichkhamVLId",
                        column: x => x.LichkhamVLId,
                        principalTable: "lichKhamvls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TintucImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TintucId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TintucImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TintucImages_Tintucs_TintucId",
                        column: x => x.TintucId,
                        principalTable: "Tintucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonThuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBenhNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenNhaSi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CTlichkhamId = table.Column<int>(type: "int", nullable: true),
                    CtlichkhamVLId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonThuocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonThuocs_Ctlkvl_CtlichkhamVLId",
                        column: x => x.CtlichkhamVLId,
                        principalTable: "Ctlkvl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonThuocs_cTlichkhams_CTlichkhamId",
                        column: x => x.CTlichkhamId,
                        principalTable: "cTlichkhams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "phieukhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayKham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenNhaSi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenBenhNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaiKham = table.Column<bool>(type: "bit", nullable: false),
                    TrangThaiTaiKham = table.Column<bool>(type: "bit", nullable: false),
                    Thoigiantaikham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CTlichkhamId = table.Column<int>(type: "int", nullable: true),
                    CtlichkhamVLId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phieukhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_phieukhams_Ctlkvl_CtlichkhamVLId",
                        column: x => x.CtlichkhamVLId,
                        principalTable: "Ctlkvl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_phieukhams_cTlichkhams_CTlichkhamId",
                        column: x => x.CTlichkhamId,
                        principalTable: "cTlichkhams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonThuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonThuocId = table.Column<int>(type: "int", nullable: false),
                    ThuocId = table.Column<int>(type: "int", nullable: false),
                    LieuLuong = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonThuocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuocs_DonThuocs_DonThuocId",
                        column: x => x.DonThuocId,
                        principalTable: "DonThuocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonThuocs_Thuocs_ThuocId",
                        column: x => x.ThuocId,
                        principalTable: "Thuocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dungdvs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuKhamId = table.Column<int>(type: "int", nullable: false),
                    DentistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DentistId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dungdvs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dungdvs_AspNetUsers_DentistId1",
                        column: x => x.DentistId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Dungdvs_phieukhams_PhieuKhamId",
                        column: x => x.PhieuKhamId,
                        principalTable: "phieukhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuKhamDichvus",
                columns: table => new
                {
                    PhieuKhamId = table.Column<int>(type: "int", nullable: false),
                    DichvuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuKhamDichvus", x => new { x.PhieuKhamId, x.DichvuId });
                    table.ForeignKey(
                        name: "FK_PhieuKhamDichvus_dichvus_DichvuId",
                        column: x => x.DichvuId,
                        principalTable: "dichvus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuKhamDichvus_phieukhams_PhieuKhamId",
                        column: x => x.PhieuKhamId,
                        principalTable: "phieukhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "Dentist", "DENTIST" },
                    { "3", null, "Patient", "PATIENT" },
                    { "4", null, "Staff", "STAFF" }
                });

            migrationBuilder.InsertData(
                table: "KhungGios",
                columns: new[] { "Id", "TimeSlot" },
                values: new object[,]
                {
                    { 1, "7:00 - 8:00" },
                    { 2, "8:00 - 9:00" },
                    { 3, "9:00 - 10:00" },
                    { 4, "10:00 - 11:00" },
                    { 5, "13:00 - 14:00" },
                    { 6, "14:00 - 15:00" },
                    { 7, "15:00 - 16:00" },
                    { 8, "16:00 - 17:00" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CakhamKhungGios_KhungGioId",
                table: "CakhamKhungGios",
                column: "KhungGioId");

            migrationBuilder.CreateIndex(
                name: "IX_CakhamKhungGios_KhungGioId1",
                table: "CakhamKhungGios",
                column: "KhungGioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cakhams_DentistId",
                table: "Cakhams",
                column: "DentistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonThuocs_DonThuocId",
                table: "ChiTietDonThuocs",
                column: "DonThuocId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonThuocs_ThuocId",
                table: "ChiTietDonThuocs",
                column: "ThuocId");

            migrationBuilder.CreateIndex(
                name: "IX_cTlichkhams_LichkhamId",
                table: "cTlichkhams",
                column: "LichkhamId");

            migrationBuilder.CreateIndex(
                name: "IX_Ctlkvl_LichkhamVLId",
                table: "Ctlkvl",
                column: "LichkhamVLId");

            migrationBuilder.CreateIndex(
                name: "IX_cTnhasis_chuyenmonId",
                table: "cTnhasis",
                column: "chuyenmonId");

            migrationBuilder.CreateIndex(
                name: "IX_cTnhasis_UserId",
                table: "cTnhasis",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DichvuImages_DichvuId",
                table: "DichvuImages",
                column: "DichvuId");

            migrationBuilder.CreateIndex(
                name: "IX_DonThuocs_CTlichkhamId",
                table: "DonThuocs",
                column: "CTlichkhamId");

            migrationBuilder.CreateIndex(
                name: "IX_DonThuocs_CtlichkhamVLId",
                table: "DonThuocs",
                column: "CtlichkhamVLId");

            migrationBuilder.CreateIndex(
                name: "IX_Dungdvs_DentistId1",
                table: "Dungdvs",
                column: "DentistId1");

            migrationBuilder.CreateIndex(
                name: "IX_Dungdvs_PhieuKhamId",
                table: "Dungdvs",
                column: "PhieuKhamId");

            migrationBuilder.CreateIndex(
                name: "IX_lichKhams_KhungGioId",
                table: "lichKhams",
                column: "KhungGioId");

            migrationBuilder.CreateIndex(
                name: "IX_lichKhams_PatientId",
                table: "lichKhams",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_lichKhamvls_KhungGioId",
                table: "lichKhamvls",
                column: "KhungGioId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuKhamDichvus_DichvuId",
                table: "PhieuKhamDichvus",
                column: "DichvuId");

            migrationBuilder.CreateIndex(
                name: "IX_phieukhams_CTlichkhamId",
                table: "phieukhams",
                column: "CTlichkhamId");

            migrationBuilder.CreateIndex(
                name: "IX_phieukhams_CtlichkhamVLId",
                table: "phieukhams",
                column: "CtlichkhamVLId");

            migrationBuilder.CreateIndex(
                name: "IX_TintucImages_TintucId",
                table: "TintucImages",
                column: "TintucId");

            migrationBuilder.CreateIndex(
                name: "IX_Tintucs_LoaitintucId",
                table: "Tintucs",
                column: "LoaitintucId");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UserId",
                table: "UserImages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CakhamKhungGios");

            migrationBuilder.DropTable(
                name: "ChiTietDonThuocs");

            migrationBuilder.DropTable(
                name: "cTnhasis");

            migrationBuilder.DropTable(
                name: "DichvuImages");

            migrationBuilder.DropTable(
                name: "Dungdvs");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PhieuKhamDichvus");

            migrationBuilder.DropTable(
                name: "TintucImages");

            migrationBuilder.DropTable(
                name: "UserImages");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Cakhams");

            migrationBuilder.DropTable(
                name: "DonThuocs");

            migrationBuilder.DropTable(
                name: "Thuocs");

            migrationBuilder.DropTable(
                name: "Chuyenmons");

            migrationBuilder.DropTable(
                name: "dichvus");

            migrationBuilder.DropTable(
                name: "phieukhams");

            migrationBuilder.DropTable(
                name: "Tintucs");

            migrationBuilder.DropTable(
                name: "Ctlkvl");

            migrationBuilder.DropTable(
                name: "cTlichkhams");

            migrationBuilder.DropTable(
                name: "Loaitintucs");

            migrationBuilder.DropTable(
                name: "lichKhamvls");

            migrationBuilder.DropTable(
                name: "lichKhams");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "KhungGios");
        }
    }
}
