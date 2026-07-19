using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingManagement.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DailyParking",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parkingid = table.Column<int>(type: "int", nullable: true),
                    ownername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vehiclenumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    intime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    outtime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datetime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    outdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyParking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HiddenSpot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ownername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    parkingname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hourrate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    operatinghours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    map = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    covered = table.Column<bool>(type: "bit", nullable: false),
                    evcharging = table.Column<bool>(type: "bit", nullable: false),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    verification = table.Column<bool>(type: "bit", nullable: false),
                    carspace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bikespace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiddenSpot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingOwner",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parkingid = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingOwner", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "DailyParking");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "HiddenSpot");

            migrationBuilder.DropTable(
                name: "ParkingOwner");
        }
    }
}
