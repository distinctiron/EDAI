using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EDAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    EdaiDocumentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentName = table.Column<string>(type: "text", nullable: false),
                    DocumentFileExtension = table.Column<string>(type: "text", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentFile = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.EdaiDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackComments",
                columns: table => new
                {
                    FeedbackCommentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommentType = table.Column<int>(type: "integer", nullable: false),
                    CommentFeedback = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackComments", x => x.FeedbackCommentId);
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                columns: table => new
                {
                    StudentClassId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Class = table.Column<string>(type: "text", nullable: false),
                    School = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClasses", x => x.StudentClassId);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Open = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceDocumentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_Assignments_Documents_ReferenceDocumentId",
                        column: x => x.ReferenceDocumentId,
                        principalTable: "Documents",
                        principalColumn: "EdaiDocumentId");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    StudentClassId = table.Column<int>(type: "integer", nullable: false),
                    GraduationYear = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_StudentClasses_StudentClassId",
                        column: x => x.StudentClassId,
                        principalTable: "StudentClasses",
                        principalColumn: "StudentClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Essays",
                columns: table => new
                {
                    EssayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EdaiDocumentId = table.Column<int>(type: "integer", nullable: false),
                    AssignmentId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Evaluated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Essays", x => x.EssayId);
                    table.ForeignKey(
                        name: "FK_Essays_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Essays_Documents_EdaiDocumentId",
                        column: x => x.EdaiDocumentId,
                        principalTable: "Documents",
                        principalColumn: "EdaiDocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Essays_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentSummaries",
                columns: table => new
                {
                    StudentSummaryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    FocusArea1 = table.Column<string>(type: "text", nullable: false),
                    FocusArea2 = table.Column<string>(type: "text", nullable: false),
                    FocusArea3 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSummaries", x => x.StudentSummaryId);
                    table.ForeignKey(
                        name: "FK_StudentSummaries_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndexedContents",
                columns: table => new
                {
                    IndexedContentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParagraphIndex = table.Column<int>(type: "integer", nullable: false),
                    RunIndex = table.Column<int>(type: "integer", nullable: false),
                    FromCharInContent = table.Column<int>(type: "integer", nullable: true),
                    ToCharInContent = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    EssayId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexedContents", x => x.IndexedContentId);
                    table.ForeignKey(
                        name: "FK_IndexedContents_Essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "Essays",
                        principalColumn: "EssayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    ScoreId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EssayId = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<float>(type: "real", nullable: false),
                    ArgumentationScore = table.Column<float>(type: "real", nullable: false),
                    ArgumentationRecommendation = table.Column<string>(type: "text", nullable: false),
                    GrammarScore = table.Column<float>(type: "real", nullable: false),
                    GrammarRecommendation = table.Column<string>(type: "text", nullable: false),
                    EloquenceScore = table.Column<float>(type: "real", nullable: false),
                    EloquenceRecommendation = table.Column<string>(type: "text", nullable: false),
                    EvaluatedEssayDocumentId = table.Column<int>(type: "integer", nullable: true),
                    OverallStructure = table.Column<string>(type: "text", nullable: false),
                    OverallStructureScore = table.Column<float>(type: "real", nullable: false),
                    OverallStructureRecommendation = table.Column<string>(type: "text", nullable: false),
                    AssignmentAnswer = table.Column<string>(type: "text", nullable: false),
                    AssignmentAnswerScore = table.Column<float>(type: "real", nullable: false),
                    AssignmentAnswerRecommendation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_Scores_Documents_EvaluatedEssayDocumentId",
                        column: x => x.EvaluatedEssayDocumentId,
                        principalTable: "Documents",
                        principalColumn: "EdaiDocumentId");
                    table.ForeignKey(
                        name: "FK_Scores_Essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "Essays",
                        principalColumn: "EssayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackCommentIndexedContent",
                columns: table => new
                {
                    FeedbackCommentsFeedbackCommentId = table.Column<int>(type: "integer", nullable: false),
                    RelatedTextsIndexedContentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackCommentIndexedContent", x => new { x.FeedbackCommentsFeedbackCommentId, x.RelatedTextsIndexedContentId });
                    table.ForeignKey(
                        name: "FK_FeedbackCommentIndexedContent_FeedbackComments_FeedbackComm~",
                        column: x => x.FeedbackCommentsFeedbackCommentId,
                        principalTable: "FeedbackComments",
                        principalColumn: "FeedbackCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackCommentIndexedContent_IndexedContents_RelatedTextsI~",
                        column: x => x.RelatedTextsIndexedContentId,
                        principalTable: "IndexedContents",
                        principalColumn: "IndexedContentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ReferenceDocumentId",
                table: "Assignments",
                column: "ReferenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Essays_AssignmentId",
                table: "Essays",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Essays_EdaiDocumentId",
                table: "Essays",
                column: "EdaiDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Essays_StudentId",
                table: "Essays",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackCommentIndexedContent_RelatedTextsIndexedContentId",
                table: "FeedbackCommentIndexedContent",
                column: "RelatedTextsIndexedContentId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexedContents_EssayId",
                table: "IndexedContents",
                column: "EssayId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_EssayId",
                table: "Scores",
                column: "EssayId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_EvaluatedEssayDocumentId",
                table: "Scores",
                column: "EvaluatedEssayDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentClassId",
                table: "Students",
                column: "StudentClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSummaries_StudentId",
                table: "StudentSummaries",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackCommentIndexedContent");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "StudentSummaries");

            migrationBuilder.DropTable(
                name: "FeedbackComments");

            migrationBuilder.DropTable(
                name: "IndexedContents");

            migrationBuilder.DropTable(
                name: "Essays");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "StudentClasses");
        }
    }
}
