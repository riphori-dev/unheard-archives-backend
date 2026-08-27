using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tywynh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "confessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    intensity = table.Column<short>(type: "smallint", nullable: false),
                    alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    moderation_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    moderated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    resonance_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    echo_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    burned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_confessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_echo_interactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    echo_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    visitor_token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ritual_completed = table.Column<bool>(type: "boolean", nullable: false),
                    echoed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_echo_interactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_echoes",
                columns: table => new
                {
                    echo_date = table.Column<DateTime>(type: "date", nullable: false),
                    confession_id = table.Column<Guid>(type: "uuid", nullable: false),
                    echo_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_echoes", x => x.echo_date);
                    table.ForeignKey(
                        name: "FK_daily_echoes_confessions_confession_id",
                        column: x => x.confession_id,
                        principalTable: "confessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resonances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    confession_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    visitor_token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resonances", x => x.id);
                    table.ForeignKey(
                        name: "FK_resonances_confessions_confession_id",
                        column: x => x.confession_id,
                        principalTable: "confessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_confessions_category",
                table: "confessions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_confessions_created_at",
                table: "confessions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_confessions_moderation_status",
                table: "confessions",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_confessions_resonance_count",
                table: "confessions",
                column: "resonance_count");

            migrationBuilder.CreateIndex(
                name: "IX_daily_echo_interactions_echo_date_visitor_token_hash",
                table: "daily_echo_interactions",
                columns: new[] { "echo_date", "visitor_token_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_echoes_confession_id",
                table: "daily_echoes",
                column: "confession_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_echoes_echo_date",
                table: "daily_echoes",
                column: "echo_date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resonances_confession_visitor",
                table: "resonances",
                columns: new[] { "confession_id", "visitor_token_hash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_echo_interactions");

            migrationBuilder.DropTable(
                name: "daily_echoes");

            migrationBuilder.DropTable(
                name: "resonances");

            migrationBuilder.DropTable(
                name: "confessions");
        }
    }
}
