using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Essays_Documents_EdaiDocumentId",
                table: "Essays");

            migrationBuilder.AddColumn<string>(
                name: "ArgumentationRecommendation",
                table: "Scores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "ArgumentationScore",
                table: "Scores",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentAnswerRecommendation",
                table: "Scores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EloquenceRecommendation",
                table: "Scores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GrammarRecommendation",
                table: "Scores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OverallStructureRecommendation",
                table: "Scores",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "OverallStructureScore",
                table: "Scores",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AlterColumn<int>(
                name: "CommentType",
                table: "FeedbackComments",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "EdaiDocumentId",
                table: "Essays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Essays_Documents_EdaiDocumentId",
                table: "Essays",
                column: "EdaiDocumentId",
                principalTable: "Documents",
                principalColumn: "EdaiDocumentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Essays_Documents_EdaiDocumentId",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "ArgumentationRecommendation",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "ArgumentationScore",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "AssignmentAnswerRecommendation",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "EloquenceRecommendation",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "GrammarRecommendation",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "OverallStructureRecommendation",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "OverallStructureScore",
                table: "Scores");

            migrationBuilder.AlterColumn<int>(
                name: "CommentType",
                table: "FeedbackComments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "EdaiDocumentId",
                table: "Essays",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Essays_Documents_EdaiDocumentId",
                table: "Essays",
                column: "EdaiDocumentId",
                principalTable: "Documents",
                principalColumn: "EdaiDocumentId");
        }
    }
}
