using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StokTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class KategoriVeDusukStokEsigi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DusukStokEsigi",
                table: "Urunler",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "KategoriID",
                table: "Urunler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    KategoriID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.KategoriID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_KategoriID",
                table: "Urunler",
                column: "KategoriID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Urunler_DusukStokEsigi",
                table: "Urunler",
                sql: "[DusukStokEsigi] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_Ad",
                table: "Kategoriler",
                column: "Ad",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Urunler_Kategoriler_KategoriID",
                table: "Urunler",
                column: "KategoriID",
                principalTable: "Kategoriler",
                principalColumn: "KategoriID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urunler_Kategoriler_KategoriID",
                table: "Urunler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_Urunler_KategoriID",
                table: "Urunler");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Urunler_DusukStokEsigi",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "DusukStokEsigi",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "KategoriID",
                table: "Urunler");
        }
    }
}
