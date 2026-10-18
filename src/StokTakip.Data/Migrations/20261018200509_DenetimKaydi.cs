using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StokTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class DenetimKaydi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DenetimKayitlari",
                columns: table => new
                {
                    DenetimID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zaman = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    KullaniciID = table.Column<int>(type: "int", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Islem = table.Column<int>(type: "int", nullable: false),
                    VarlikTuru = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VarlikID = table.Column<int>(type: "int", nullable: true),
                    VarlikAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EskiDeger = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    YeniDeger = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Fark = table.Column<int>(type: "int", nullable: true),
                    Ozet = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenetimKayitlari", x => x.DenetimID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DenetimKayitlari_Islem_Zaman",
                table: "DenetimKayitlari",
                columns: new[] { "Islem", "Zaman" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DenetimKayitlari_KullaniciID_Zaman",
                table: "DenetimKayitlari",
                columns: new[] { "KullaniciID", "Zaman" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DenetimKayitlari_Zaman",
                table: "DenetimKayitlari",
                column: "Zaman",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenetimKayitlari");
        }
    }
}
