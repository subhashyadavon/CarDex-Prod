using System;
using CarDexBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDexDatabase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:grade_enum.grade_enum", "factory,limited_run,nismo")
                .Annotation("Npgsql:Enum:reward_enum.reward_enum", "pack,currency,card_from_trade,currency_from_trade")
                .Annotation("Npgsql:Enum:trade_enum.trade_enum", "for_card,for_price");

            migrationBuilder.CreateTable(
                name: "collection",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    pack_price = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    vehicles = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stat1 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    stat2 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    stat3 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    image = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pack",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pack", x => x.id);
                    table.ForeignKey(
                        name: "FK_pack_collection_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pack_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rewards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<RewardEnum>(type: "reward_enum", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_rewards_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade = table.Column<GradeEnum>(type: "grade_enum", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_collection_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_card_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_card_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "completed_trade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<TradeEnum>(type: "trade_enum", nullable: false),
                    seller_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_card_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    executed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_completed_trade", x => x.id);
                    table.ForeignKey(
                        name: "FK_completed_trade_card_seller_card_id",
                        column: x => x.seller_card_id,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_completed_trade_users_buyer_user_id",
                        column: x => x.buyer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_completed_trade_users_seller_user_id",
                        column: x => x.seller_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "open_trade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<TradeEnum>(type: "trade_enum", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    want_card_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_open_trade", x => x.id);
                    table.ForeignKey(
                        name: "FK_open_trade_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_open_trade_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_collection_id",
                table: "card",
                column: "collection_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_user_id",
                table: "card",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_vehicle_id",
                table: "card",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_completed_trade_buyer_user_id",
                table: "completed_trade",
                column: "buyer_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_completed_trade_seller_card_id",
                table: "completed_trade",
                column: "seller_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_completed_trade_seller_user_id",
                table: "completed_trade",
                column: "seller_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_open_trade_card_id",
                table: "open_trade",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_open_trade_user_id",
                table: "open_trade",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pack_collection_id",
                table: "pack",
                column: "collection_id");

            migrationBuilder.CreateIndex(
                name: "IX_pack_user_id",
                table: "pack",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_user_id",
                table: "rewards",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "completed_trade");

            migrationBuilder.DropTable(
                name: "open_trade");

            migrationBuilder.DropTable(
                name: "pack");

            migrationBuilder.DropTable(
                name: "rewards");

            migrationBuilder.DropTable(
                name: "card");

            migrationBuilder.DropTable(
                name: "collection");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vehicle");
        }
    }
}
