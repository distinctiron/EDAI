using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAI.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    EdaiDocumentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentName = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentFileExtension = table.Column<string>(type: "TEXT", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DocumentFile = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.EdaiDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackComments",
                columns: table => new
                {
                    FeedbackCommentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommentType = table.Column<int>(type: "INTEGER", nullable: false),
                    CommentFeedback = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackComments", x => x.FeedbackCommentId);
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                columns: table => new
                {
                    StudentClassId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Class = table.Column<string>(type: "TEXT", nullable: false),
                    School = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClasses", x => x.StudentClassId);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Open = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReferenceDocumentId = table.Column<int>(type: "INTEGER", nullable: true)
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
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: false),
                    StudentClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    GraduationYear = table.Column<int>(type: "INTEGER", nullable: false)
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
                    EssayId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EdaiDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Evaluated = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    StudentSummaryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    FocusArea1 = table.Column<string>(type: "TEXT", nullable: false),
                    FocusArea2 = table.Column<string>(type: "TEXT", nullable: false),
                    FocusArea3 = table.Column<string>(type: "TEXT", nullable: false)
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
                    IndexedContentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParagraphIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    RunIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    FromCharInContent = table.Column<int>(type: "INTEGER", nullable: true),
                    ToCharInContent = table.Column<int>(type: "INTEGER", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    EssayId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    ScoreId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EssayId = table.Column<int>(type: "INTEGER", nullable: false),
                    OverallScore = table.Column<float>(type: "REAL", nullable: false),
                    ArgumentationScore = table.Column<float>(type: "REAL", nullable: false),
                    ArgumentationRecommendation = table.Column<string>(type: "TEXT", nullable: false),
                    GrammarScore = table.Column<float>(type: "REAL", nullable: false),
                    GrammarRecommendation = table.Column<string>(type: "TEXT", nullable: false),
                    EloquenceScore = table.Column<float>(type: "REAL", nullable: false),
                    EloquenceRecommendation = table.Column<string>(type: "TEXT", nullable: false),
                    EvaluatedEssayDocumentId = table.Column<int>(type: "INTEGER", nullable: true),
                    OverallStructure = table.Column<string>(type: "TEXT", nullable: false),
                    OverallStructureScore = table.Column<float>(type: "REAL", nullable: false),
                    OverallStructureRecommendation = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentAnswerScore = table.Column<float>(type: "REAL", nullable: false),
                    AssignmentAnswerRecommendation = table.Column<string>(type: "TEXT", nullable: false)
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
                    FeedbackCommentsFeedbackCommentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedTextsIndexedContentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackCommentIndexedContent", x => new { x.FeedbackCommentsFeedbackCommentId, x.RelatedTextsIndexedContentId });
                    table.ForeignKey(
                        name: "FK_FeedbackCommentIndexedContent_FeedbackComments_FeedbackCommentsFeedbackCommentId",
                        column: x => x.FeedbackCommentsFeedbackCommentId,
                        principalTable: "FeedbackComments",
                        principalColumn: "FeedbackCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackCommentIndexedContent_IndexedContents_RelatedTextsIndexedContentId",
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
