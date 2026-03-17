using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechPro.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNgayHoanThanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm NgayHoanThanh vào PhieuSuaChuas để tính doanh thu đúng kỳ kế toán
            // Nullable vì các phiếu cũ chưa có giá trị này
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHoanThanh",
                table: "PhieuSuaChuas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayHoanThanh",
                table: "PhieuSuaChuas");
        }
    }
}
