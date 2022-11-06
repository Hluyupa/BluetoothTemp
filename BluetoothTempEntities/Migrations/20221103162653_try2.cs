using Microsoft.EntityFrameworkCore.Migrations;

namespace BluetoothTempEntities.Migrations
{
    public partial class try2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SerialNumber",
                table: "BluetoothDevicesWasСonnected",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "BluetoothDevicesWasСonnected");
        }
    }
}
