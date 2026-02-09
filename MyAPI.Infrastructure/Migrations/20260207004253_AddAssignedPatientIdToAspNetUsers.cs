using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedPatientIdToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedPatientId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientRelative_PatientId",
                table: "PatientRelative",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientRelative_Patients_PatientId",
                table: "PatientRelative",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientRelative_Patients_PatientId",
                table: "PatientRelative");

            migrationBuilder.DropIndex(
                name: "IX_PatientRelative_PatientId",
                table: "PatientRelative");

            migrationBuilder.DropColumn(
                name: "AssignedPatientId",
                table: "AspNetUsers");
        }
    }
}
