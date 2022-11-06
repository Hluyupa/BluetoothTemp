using Microsoft.EntityFrameworkCore.Migrations;

namespace BluetoothTempEntities.Migrations
{
    public partial class try1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BluetoothDevicesWasСonnected",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    MacAddress = table.Column<string>(nullable: true),
                    IsAutoconnect = table.Column<int>(nullable: false),
                    IsNfcWrited = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BluetoothDevicesWasСonnected", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BluetoothDevicesWasСonnected");
        }
    }
}
