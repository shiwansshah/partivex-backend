using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReworkInventoryPurchasesForParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "Category" character varying(80) NOT NULL DEFAULT '';
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "CompatibleVehicle" character varying(120) NOT NULL DEFAULT '';
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "UnitPrice" numeric(18,2) NOT NULL DEFAULT 0;
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "MinimumStockLevel" integer NOT NULL DEFAULT 0;
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "CurrentStock" integer NOT NULL DEFAULT 0;
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "ImageUrl" character varying(500) NOT NULL DEFAULT '';
                ALTER TABLE "Parts" ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW();

                UPDATE "Parts" SET "UnitPrice" = "Price" WHERE EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'Parts' AND column_name = 'Price'
                );
                UPDATE "Parts" SET "CurrentStock" = "Stock" WHERE EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'Parts' AND column_name = 'Stock'
                );

                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "Price";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "Stock";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Parts_PartCode" ON "Parts" ("PartCode");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Vendors_Email" ON "Vendors" ("Email");

                ALTER TABLE "PurchaseInvoices" ADD COLUMN IF NOT EXISTS "VendorId" integer NOT NULL DEFAULT 0;

                ALTER TABLE "PurchaseInvoiceItems" ADD COLUMN IF NOT EXISTS "PartId" integer NOT NULL DEFAULT 0;
                ALTER TABLE "PurchaseInvoiceItems" ADD COLUMN IF NOT EXISTS "UnitPrice" numeric(18,2) NOT NULL DEFAULT 0;
                UPDATE "PurchaseInvoiceItems" SET "UnitPrice" = "UnitCost" WHERE EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'PurchaseInvoiceItems' AND column_name = 'UnitCost'
                );

                ALTER TABLE "InventoryStockChanges" ADD COLUMN IF NOT EXISTS "PartId" integer NOT NULL DEFAULT 0;
                ALTER TABLE "InventoryStockChanges" ADD COLUMN IF NOT EXISTS "VendorId" integer NOT NULL DEFAULT 0;

                ALTER TABLE "PurchaseInvoiceItems" DROP CONSTRAINT IF EXISTS "FK_PurchaseInvoiceItems_InventoryItems_InventoryItemId";
                DROP INDEX IF EXISTS "IX_PurchaseInvoiceItems_InventoryItemId";
                ALTER TABLE "PurchaseInvoiceItems" DROP COLUMN IF EXISTS "InventoryItemId";
                ALTER TABLE "PurchaseInvoiceItems" DROP COLUMN IF EXISTS "UnitCost";

                ALTER TABLE "InventoryStockChanges" DROP CONSTRAINT IF EXISTS "FK_InventoryStockChanges_InventoryItems_InventoryItemId";
                DROP INDEX IF EXISTS "IX_InventoryStockChanges_InventoryItemId";
                ALTER TABLE "InventoryStockChanges" DROP COLUMN IF EXISTS "InventoryItemId";

                CREATE INDEX IF NOT EXISTS "IX_PurchaseInvoices_VendorId" ON "PurchaseInvoices" ("VendorId");
                CREATE INDEX IF NOT EXISTS "IX_PurchaseInvoiceItems_PartId" ON "PurchaseInvoiceItems" ("PartId");
                CREATE INDEX IF NOT EXISTS "IX_InventoryStockChanges_PartId" ON "InventoryStockChanges" ("PartId");
                CREATE INDEX IF NOT EXISTS "IX_InventoryStockChanges_VendorId" ON "InventoryStockChanges" ("VendorId");

                ALTER TABLE "PurchaseInvoices" DROP CONSTRAINT IF EXISTS "FK_PurchaseInvoices_Vendors_VendorId";
                ALTER TABLE "PurchaseInvoiceItems" DROP CONSTRAINT IF EXISTS "FK_PurchaseInvoiceItems_Parts_PartId";
                ALTER TABLE "InventoryStockChanges" DROP CONSTRAINT IF EXISTS "FK_InventoryStockChanges_Parts_PartId";
                ALTER TABLE "InventoryStockChanges" DROP CONSTRAINT IF EXISTS "FK_InventoryStockChanges_Vendors_VendorId";

                INSERT INTO "Vendors" ("Name", "ContactPerson", "Email", "Phone", "Address", "IsActive", "CreatedAt")
                SELECT 'Legacy Vendor', 'System', 'legacy-vendor@partivex.local', '0000000', 'Imported stock history', TRUE, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM "Vendors");

                INSERT INTO "Parts" ("Name", "PartCode", "Category", "CompatibleVehicle", "UnitPrice", "MinimumStockLevel", "CurrentStock", "ImageUrl", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT 'Legacy Part', 'LEGACY-PART', 'General', '', 0, 0, 0, '', TRUE, NOW(), NOW()
                WHERE NOT EXISTS (SELECT 1 FROM "Parts");

                UPDATE "PurchaseInvoices"
                SET "VendorId" = (SELECT "Id" FROM "Vendors" ORDER BY "Id" LIMIT 1)
                WHERE "VendorId" = 0;
                UPDATE "PurchaseInvoiceItems"
                SET "PartId" = (SELECT "Id" FROM "Parts" ORDER BY "Id" LIMIT 1)
                WHERE "PartId" = 0;
                UPDATE "InventoryStockChanges"
                SET
                    "PartId" = (SELECT "Id" FROM "Parts" ORDER BY "Id" LIMIT 1),
                    "VendorId" = (SELECT "Id" FROM "Vendors" ORDER BY "Id" LIMIT 1)
                WHERE "PartId" = 0 OR "VendorId" = 0;

                ALTER TABLE "PurchaseInvoices"
                    ADD CONSTRAINT "FK_PurchaseInvoices_Vendors_VendorId"
                    FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT;
                ALTER TABLE "PurchaseInvoiceItems"
                    ADD CONSTRAINT "FK_PurchaseInvoiceItems_Parts_PartId"
                    FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE RESTRICT;
                ALTER TABLE "InventoryStockChanges"
                    ADD CONSTRAINT "FK_InventoryStockChanges_Parts_PartId"
                    FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE;
                ALTER TABLE "InventoryStockChanges"
                    ADD CONSTRAINT "FK_InventoryStockChanges_Vendors_VendorId"
                    FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "InventoryStockChanges" DROP CONSTRAINT IF EXISTS "FK_InventoryStockChanges_Vendors_VendorId";
                ALTER TABLE "InventoryStockChanges" DROP CONSTRAINT IF EXISTS "FK_InventoryStockChanges_Parts_PartId";
                ALTER TABLE "PurchaseInvoiceItems" DROP CONSTRAINT IF EXISTS "FK_PurchaseInvoiceItems_Parts_PartId";
                ALTER TABLE "PurchaseInvoices" DROP CONSTRAINT IF EXISTS "FK_PurchaseInvoices_Vendors_VendorId";

                DROP INDEX IF EXISTS "IX_InventoryStockChanges_VendorId";
                DROP INDEX IF EXISTS "IX_InventoryStockChanges_PartId";
                DROP INDEX IF EXISTS "IX_PurchaseInvoiceItems_PartId";
                DROP INDEX IF EXISTS "IX_PurchaseInvoices_VendorId";

                ALTER TABLE "InventoryStockChanges" DROP COLUMN IF EXISTS "VendorId";
                ALTER TABLE "InventoryStockChanges" DROP COLUMN IF EXISTS "PartId";
                ALTER TABLE "PurchaseInvoiceItems" DROP COLUMN IF EXISTS "UnitPrice";
                ALTER TABLE "PurchaseInvoiceItems" DROP COLUMN IF EXISTS "PartId";
                ALTER TABLE "PurchaseInvoices" DROP COLUMN IF EXISTS "VendorId";

                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "UpdatedAt";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "ImageUrl";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "CurrentStock";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "MinimumStockLevel";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "UnitPrice";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "CompatibleVehicle";
                ALTER TABLE "Parts" DROP COLUMN IF EXISTS "Category";
                """);
        }
    }
}
