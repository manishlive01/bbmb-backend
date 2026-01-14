using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class InitialBudgetTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.CreateTable(
                name: "account_office",
                schema: "budget",
                columns: table => new
                {
                    acc_office = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    acc_office_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_office", x => x.acc_office);
                });

            migrationBuilder.CreateTable(
                name: "class",
                schema: "budget",
                columns: table => new
                {
                    class_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    class_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class", x => x.class_code);
                });

            migrationBuilder.CreateTable(
                name: "fa_detail_head",
                schema: "budget",
                columns: table => new
                {
                    detail_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    detail_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_detail_head", x => x.detail_acc_code);
                });

            migrationBuilder.CreateTable(
                name: "fa_financial_years",
                schema: "budget",
                columns: table => new
                {
                    fin_year_closing = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_financial_years", x => x.fin_year_closing);
                });

            migrationBuilder.CreateTable(
                name: "fa_major_head",
                schema: "budget",
                columns: table => new
                {
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    major_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_major_head", x => x.major_acc_code);
                });

            migrationBuilder.CreateTable(
                name: "fa_minor_head",
                schema: "budget",
                columns: table => new
                {
                    minor_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    minor_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_minor_head", x => x.minor_acc_code);
                });

            migrationBuilder.CreateTable(
                name: "fa_object_head",
                schema: "budget",
                columns: table => new
                {
                    object_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    object_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_object_head", x => x.object_acc_code);
                });

            migrationBuilder.CreateTable(
                name: "irr_pwr",
                schema: "budget",
                columns: table => new
                {
                    irr_pwr_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    irr_pwr_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_irr_pwr", x => x.irr_pwr_code);
                });

            migrationBuilder.CreateTable(
                name: "pool",
                schema: "budget",
                columns: table => new
                {
                    pool_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    pool_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pool", x => x.pool_code);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "budget",
                columns: table => new
                {
                    proj_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    proj_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.proj_code);
                });

            migrationBuilder.CreateTable(
                name: "region",
                schema: "budget",
                columns: table => new
                {
                    region_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    region_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_region", x => x.region_code);
                });

            migrationBuilder.CreateTable(
                name: "remit_office",
                schema: "budget",
                columns: table => new
                {
                    ro_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ro_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remit_office", x => x.ro_code);
                });

            migrationBuilder.CreateTable(
                name: "state_master",
                schema: "budget",
                columns: table => new
                {
                    state_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    state_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state_master", x => x.state_master_id);
                });

            migrationBuilder.CreateTable(
                name: "designation",
                schema: "budget",
                columns: table => new
                {
                    designation_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    designation_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    class_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    min_basic = table.Column<decimal>(type: "numeric", nullable: true),
                    max_basic = table.Column<decimal>(type: "numeric", nullable: true),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_designation", x => x.designation_code);
                    table.ForeignKey(
                        name: "FK_designation_class_class_code",
                        column: x => x.class_code,
                        principalSchema: "budget",
                        principalTable: "class",
                        principalColumn: "class_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fa_sub_major_head",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    sub_major_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    sub_major_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    MajorHeadMajorAccCode = table.Column<string>(type: "character varying(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_sub_major_head", x => x.id);
                    table.ForeignKey(
                        name: "FK_fa_sub_major_head_fa_major_head_MajorHeadMajorAccCode",
                        column: x => x.MajorHeadMajorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_major_head",
                        principalColumn: "major_acc_code");
                });

            migrationBuilder.CreateTable(
                name: "fa_sub_minor_head",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    minor_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sub_minor_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    sub_minor_acc_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    MinorHeadMinorAccCode = table.Column<string>(type: "character varying(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fa_sub_minor_head", x => x.id);
                    table.ForeignKey(
                        name: "FK_fa_sub_minor_head_fa_minor_head_MinorHeadMinorAccCode",
                        column: x => x.MinorHeadMinorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_minor_head",
                        principalColumn: "minor_acc_code");
                });

            migrationBuilder.CreateTable(
                name: "budget_head_master",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    major_acc_code_fk = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    sub_major_acc_code_fk = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    minor_acc_code_fk = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sub_minor_acc_code_fk = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    detail_acc_code_fk = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    object_acc_code_fk = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    head_desc = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    party_emp_required = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    party_ledg_to_print = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    tds_deducted = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    advance_head = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    bank_cash = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    monthly_carry_fwd = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    MajorHeadMajorAccCode = table.Column<string>(type: "character varying(4)", nullable: true),
                    MinorHeadMinorAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    DetailHeadDetailAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    ObjectClassificationObjectAccCode = table.Column<string>(type: "character varying(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_head_master", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_head_master_fa_detail_head_DetailHeadDetailAccCode",
                        column: x => x.DetailHeadDetailAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_detail_head",
                        principalColumn: "detail_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_head_master_fa_major_head_MajorHeadMajorAccCode",
                        column: x => x.MajorHeadMajorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_major_head",
                        principalColumn: "major_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_head_master_fa_minor_head_MinorHeadMinorAccCode",
                        column: x => x.MinorHeadMinorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_minor_head",
                        principalColumn: "minor_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_head_master_fa_object_head_ObjectClassificationObjec~",
                        column: x => x.ObjectClassificationObjectAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_object_head",
                        principalColumn: "object_acc_code");
                });

            migrationBuilder.CreateTable(
                name: "units",
                schema: "budget",
                columns: table => new
                {
                    unit_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    unit_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    proj_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    irr_pwr_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.unit_code);
                    table.ForeignKey(
                        name: "FK_units_irr_pwr_irr_pwr_code",
                        column: x => x.irr_pwr_code,
                        principalSchema: "budget",
                        principalTable: "irr_pwr",
                        principalColumn: "irr_pwr_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_units_projects_proj_code",
                        column: x => x.proj_code,
                        principalSchema: "budget",
                        principalTable: "projects",
                        principalColumn: "proj_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "district_master",
                schema: "budget",
                columns: table => new
                {
                    district_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_master_id = table.Column<int>(type: "integer", nullable: false),
                    district_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district_master", x => x.district_master_id);
                    table.ForeignKey(
                        name: "FK_district_master_state_master_state_master_id",
                        column: x => x.state_master_id,
                        principalSchema: "budget",
                        principalTable: "state_master",
                        principalColumn: "state_master_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "division",
                schema: "budget",
                columns: table => new
                {
                    division_code = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    div_section = table.Column<int>(type: "integer", nullable: false),
                    division_desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ctrl_office1 = table.Column<int>(type: "integer", nullable: true),
                    ctrl_office2 = table.Column<int>(type: "integer", nullable: true),
                    ro_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    acc_office = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    unit_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    pool_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    umd_unit = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    region_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    ddo_tds = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    parent_division_code = table.Column<int>(type: "integer", nullable: true),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    RemittanceOfficeRoCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    MajorHeadMajorAccCode = table.Column<string>(type: "character varying(4)", nullable: true),
                    AccountOfficeAccOffice = table.Column<string>(type: "character varying(2)", nullable: true),
                    UnitMastersUnitCode = table.Column<string>(type: "character varying(2)", nullable: true),
                    PoolCode1 = table.Column<string>(type: "character varying(1)", nullable: true),
                    RegionCode1 = table.Column<string>(type: "character varying(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_division", x => x.division_code);
                    table.ForeignKey(
                        name: "FK_division_account_office_AccountOfficeAccOffice",
                        column: x => x.AccountOfficeAccOffice,
                        principalSchema: "budget",
                        principalTable: "account_office",
                        principalColumn: "acc_office");
                    table.ForeignKey(
                        name: "FK_division_fa_major_head_MajorHeadMajorAccCode",
                        column: x => x.MajorHeadMajorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_major_head",
                        principalColumn: "major_acc_code");
                    table.ForeignKey(
                        name: "FK_division_pool_PoolCode1",
                        column: x => x.PoolCode1,
                        principalSchema: "budget",
                        principalTable: "pool",
                        principalColumn: "pool_code");
                    table.ForeignKey(
                        name: "FK_division_region_RegionCode1",
                        column: x => x.RegionCode1,
                        principalSchema: "budget",
                        principalTable: "region",
                        principalColumn: "region_code");
                    table.ForeignKey(
                        name: "FK_division_remit_office_RemittanceOfficeRoCode",
                        column: x => x.RemittanceOfficeRoCode,
                        principalSchema: "budget",
                        principalTable: "remit_office",
                        principalColumn: "ro_code");
                    table.ForeignKey(
                        name: "FK_division_units_UnitMastersUnitCode",
                        column: x => x.UnitMastersUnitCode,
                        principalSchema: "budget",
                        principalTable: "units",
                        principalColumn: "unit_code");
                });

            migrationBuilder.CreateTable(
                name: "work",
                schema: "budget",
                columns: table => new
                {
                    work_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    division_code = table.Column<int>(type: "integer", nullable: false),
                    div_section = table.Column<int>(type: "integer", nullable: false),
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    sub_major_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    minor_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sub_minor_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    detail_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    object_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fin_yr_start = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    fin_yr_end = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    sanction_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sanction_date = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    designation_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    estimated_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    work_desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nature = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    ref_work_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DivisionCode1 = table.Column<int>(type: "integer", nullable: true),
                    MajorHeadMajorAccCode = table.Column<string>(type: "character varying(4)", nullable: true),
                    SubMajorHeadId = table.Column<int>(type: "integer", nullable: true),
                    MinorHeadMinorAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    SubMinorHeadId = table.Column<int>(type: "integer", nullable: true),
                    DetailHeadDetailAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    ObjectClassificationObjectAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    DesignationCode1 = table.Column<string>(type: "character varying(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work", x => x.work_code);
                    table.ForeignKey(
                        name: "FK_work_designation_DesignationCode1",
                        column: x => x.DesignationCode1,
                        principalSchema: "budget",
                        principalTable: "designation",
                        principalColumn: "designation_code");
                    table.ForeignKey(
                        name: "FK_work_division_DivisionCode1",
                        column: x => x.DivisionCode1,
                        principalSchema: "budget",
                        principalTable: "division",
                        principalColumn: "division_code");
                    table.ForeignKey(
                        name: "FK_work_fa_detail_head_DetailHeadDetailAccCode",
                        column: x => x.DetailHeadDetailAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_detail_head",
                        principalColumn: "detail_acc_code");
                    table.ForeignKey(
                        name: "FK_work_fa_major_head_MajorHeadMajorAccCode",
                        column: x => x.MajorHeadMajorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_major_head",
                        principalColumn: "major_acc_code");
                    table.ForeignKey(
                        name: "FK_work_fa_minor_head_MinorHeadMinorAccCode",
                        column: x => x.MinorHeadMinorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_minor_head",
                        principalColumn: "minor_acc_code");
                    table.ForeignKey(
                        name: "FK_work_fa_object_head_ObjectClassificationObjectAccCode",
                        column: x => x.ObjectClassificationObjectAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_object_head",
                        principalColumn: "object_acc_code");
                    table.ForeignKey(
                        name: "FK_work_fa_sub_major_head_SubMajorHeadId",
                        column: x => x.SubMajorHeadId,
                        principalSchema: "budget",
                        principalTable: "fa_sub_major_head",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_work_fa_sub_minor_head_SubMinorHeadId",
                        column: x => x.SubMinorHeadId,
                        principalSchema: "budget",
                        principalTable: "fa_sub_minor_head",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "budget_table",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division_code = table.Column<int>(type: "integer", nullable: false),
                    div_section = table.Column<int>(type: "integer", nullable: false),
                    calendar_year = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    sub_major_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    minor_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sub_minor_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    detail_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    object_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    work_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    budget_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    budget_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    revised_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    proposed_budget = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    budget_type = table.Column<int>(type: "integer", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DivisionCode1 = table.Column<int>(type: "integer", nullable: true),
                    MajorHeadMajorAccCode = table.Column<string>(type: "character varying(4)", nullable: true),
                    SubMajorHeadId = table.Column<int>(type: "integer", nullable: true),
                    MinorHeadMinorAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    SubMinorHeadId = table.Column<int>(type: "integer", nullable: true),
                    DetailHeadDetailAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    ObjectClassificationObjectAccCode = table.Column<string>(type: "character varying(3)", nullable: true),
                    WorkCode1 = table.Column<string>(type: "character varying(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_table", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_table_division_DivisionCode1",
                        column: x => x.DivisionCode1,
                        principalSchema: "budget",
                        principalTable: "division",
                        principalColumn: "division_code");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_detail_head_DetailHeadDetailAccCode",
                        column: x => x.DetailHeadDetailAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_detail_head",
                        principalColumn: "detail_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_major_head_MajorHeadMajorAccCode",
                        column: x => x.MajorHeadMajorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_major_head",
                        principalColumn: "major_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_minor_head_MinorHeadMinorAccCode",
                        column: x => x.MinorHeadMinorAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_minor_head",
                        principalColumn: "minor_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_object_head_ObjectClassificationObjectAccCo~",
                        column: x => x.ObjectClassificationObjectAccCode,
                        principalSchema: "budget",
                        principalTable: "fa_object_head",
                        principalColumn: "object_acc_code");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_sub_major_head_SubMajorHeadId",
                        column: x => x.SubMajorHeadId,
                        principalSchema: "budget",
                        principalTable: "fa_sub_major_head",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_budget_table_fa_sub_minor_head_SubMinorHeadId",
                        column: x => x.SubMinorHeadId,
                        principalSchema: "budget",
                        principalTable: "fa_sub_minor_head",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_budget_table_work_WorkCode1",
                        column: x => x.WorkCode1,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "work_code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "DetailHeadDetailAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MinorHeadMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "ObjectClassificationObjectAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_table",
                column: "DetailHeadDetailAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_DivisionCode1",
                schema: "budget",
                table: "budget_table",
                column: "DivisionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "MinorHeadMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_table",
                column: "ObjectClassificationObjectAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMajorHeadId",
                schema: "budget",
                table: "budget_table",
                column: "SubMajorHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMinorHeadId",
                schema: "budget",
                table: "budget_table",
                column: "SubMinorHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_WorkCode1",
                schema: "budget",
                table: "budget_table",
                column: "WorkCode1");

            migrationBuilder.CreateIndex(
                name: "IX_designation_class_code",
                schema: "budget",
                table: "designation",
                column: "class_code");

            migrationBuilder.CreateIndex(
                name: "IX_district_master_state_master_id",
                schema: "budget",
                table: "district_master",
                column: "state_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_division_AccountOfficeAccOffice",
                schema: "budget",
                table: "division",
                column: "AccountOfficeAccOffice");

            migrationBuilder.CreateIndex(
                name: "IX_division_MajorHeadMajorAccCode",
                schema: "budget",
                table: "division",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_division_PoolCode1",
                schema: "budget",
                table: "division",
                column: "PoolCode1");

            migrationBuilder.CreateIndex(
                name: "IX_division_RegionCode1",
                schema: "budget",
                table: "division",
                column: "RegionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_division_RemittanceOfficeRoCode",
                schema: "budget",
                table: "division",
                column: "RemittanceOfficeRoCode");

            migrationBuilder.CreateIndex(
                name: "IX_division_UnitMastersUnitCode",
                schema: "budget",
                table: "division",
                column: "UnitMastersUnitCode");

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_minor_head_MinorHeadMinorAccCode",
                schema: "budget",
                table: "fa_sub_minor_head",
                column: "MinorHeadMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_units_irr_pwr_code",
                schema: "budget",
                table: "units",
                column: "irr_pwr_code");

            migrationBuilder.CreateIndex(
                name: "IX_units_proj_code",
                schema: "budget",
                table: "units",
                column: "proj_code");

            migrationBuilder.CreateIndex(
                name: "IX_work_DesignationCode1",
                schema: "budget",
                table: "work",
                column: "DesignationCode1");

            migrationBuilder.CreateIndex(
                name: "IX_work_DetailHeadDetailAccCode",
                schema: "budget",
                table: "work",
                column: "DetailHeadDetailAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_work_DivisionCode1",
                schema: "budget",
                table: "work",
                column: "DivisionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_work_MajorHeadMajorAccCode",
                schema: "budget",
                table: "work",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_work_MinorHeadMinorAccCode",
                schema: "budget",
                table: "work",
                column: "MinorHeadMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_work_ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "work",
                column: "ObjectClassificationObjectAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMajorHeadId",
                schema: "budget",
                table: "work",
                column: "SubMajorHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMinorHeadId",
                schema: "budget",
                table: "work",
                column: "SubMinorHeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_head_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_table",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "district_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_financial_years",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "work",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "state_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "designation",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "division",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_detail_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_object_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_sub_major_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_sub_minor_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "class",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "account_office",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "pool",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "region",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "remit_office",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "units",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_major_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "fa_minor_head",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "irr_pwr",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "budget");
        }
    }
}
