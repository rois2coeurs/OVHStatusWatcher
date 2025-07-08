using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OVHStatusWatcher.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Datacenters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datacenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Datacenters_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Racks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatacenterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racks_Datacenters_DatacenterId",
                        column: x => x.DatacenterId,
                        principalTable: "Datacenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Trackers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WebHookUrl = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceTypeId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: true),
                    DatacenterId = table.Column<long>(type: "bigint", nullable: true),
                    RackId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trackers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trackers_Datacenters_DatacenterId",
                        column: x => x.DatacenterId,
                        principalTable: "Datacenters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trackers_Racks_RackId",
                        column: x => x.RackId,
                        principalTable: "Racks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trackers_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trackers_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Datacenters_RegionId",
                table: "Datacenters",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Racks_DatacenterId",
                table: "Racks",
                column: "DatacenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Trackers_DatacenterId",
                table: "Trackers",
                column: "DatacenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Trackers_RackId",
                table: "Trackers",
                column: "RackId");

            migrationBuilder.CreateIndex(
                name: "IX_Trackers_RegionId",
                table: "Trackers",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Trackers_ServiceTypeId",
                table: "Trackers",
                column: "ServiceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trackers");

            migrationBuilder.DropTable(
                name: "Racks");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Datacenters");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
