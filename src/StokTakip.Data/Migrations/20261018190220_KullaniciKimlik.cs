using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StokTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciKimlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParolaHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    ParolaTuzu = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    Iterasyon = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    BasarisizDenemeSayisi = table.Column<int>(type: "int", nullable: false),
                    KilitBitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SonGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciID);
                    table.CheckConstraint("CK_Kullanicilar_BasarisizDenemeSayisi", "[BasarisizDenemeSayisi] >= 0");
                    table.CheckConstraint("CK_Kullanicilar_Iterasyon", "[Iterasyon] > 0");
                    table.CheckConstraint("CK_Kullanicilar_KullaniciAdi", "[KullaniciAdi] <> ''");
                    table.CheckConstraint("CK_Kullanicilar_Rol", "[Rol] IN (0, 1)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
