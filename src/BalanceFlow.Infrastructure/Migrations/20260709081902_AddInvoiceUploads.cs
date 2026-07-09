using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalanceFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadedFilePath",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UploadedFilePath",
                table: "Invoices");
        }
    }
}
