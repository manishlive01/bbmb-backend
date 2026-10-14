using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1StateTransitionSeed : Migration
    {
        private const string MigrationName = "BudgetP1StateTransitionSeed";
        private const string Gate = "p1_state_model";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered (design §11.1 rule 2): gate guard first, then the generated DDL, then the hand SQL.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1StateTransitionSeed_00_gate.sql"));

            migrationBuilder.CreateTable(
                name: "state_transition",
                schema: "budget",
                columns: table => new
                {
                    from_state = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    action_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_state = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    remark_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state_transition", x => new { x.from_state, x.action_code, x.to_state });
                    table.ForeignKey(
                        name: "FK_state_transition_state_from_state",
                        column: x => x.from_state,
                        principalSchema: "budget",
                        principalTable: "state",
                        principalColumn: "state_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_state_transition_state_to_state",
                        column: x => x.to_state,
                        principalSchema: "budget",
                        principalTable: "state",
                        principalColumn: "state_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proposal_movement_from_state_action_code_to_state",
                schema: "budget",
                table: "proposal_movement",
                columns: new[] { "from_state", "action_code", "to_state" });

            migrationBuilder.CreateIndex(
                name: "IX_state_transition_to_state",
                schema: "budget",
                table: "state_transition",
                column: "to_state");

        
            migrationBuilder.Sql(SqlResource.Read("BudgetP1StateTransitionSeed_02_rules_triggers.sql"));
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1StateTransitionSeed_03_seed.sql"));
            // Step 4: T-33 composite FK to T-16, after the seed (DV-07).
            migrationBuilder.AddForeignKey(
                name: "fk_proposal_movement_state_transition",
                schema: "budget",
                table: "proposal_movement",
                columns: new[] { "from_state", "action_code", "to_state" },
                principalSchema: "budget",
                principalTable: "state_transition",
                principalColumns: new[] { "from_state", "action_code", "to_state" },
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql(SqlResource.Read("BudgetP1StateTransitionSeed_05_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_proposal_movement_state_transition",
                schema: "budget",
                table: "proposal_movement");

            migrationBuilder.DropTable(
                name: "state_transition",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_proposal_movement_from_state_action_code_to_state",
                schema: "budget",
                table: "proposal_movement");
        }
    }
}
