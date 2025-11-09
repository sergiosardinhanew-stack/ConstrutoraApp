using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstrutoraApp.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEntradaParcelamentoPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpar dados antigos antes de remover a foreign key
            migrationBuilder.Sql("DELETE FROM Pagamentos");

            // Remover foreign key usando SQL direto (se existir)
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_Pagamentos_Entradas_EntradaId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` DROP FOREIGN KEY `FK_Pagamentos_Entradas_EntradaId`', 
                    'SELECT ''Foreign key does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Remover colunas apenas se existirem
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'NumeroParcelas' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` DROP COLUMN `NumeroParcelas`', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'Tipo' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` DROP COLUMN `Tipo`', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'NumeroParcelas' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Entradas');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Entradas` DROP COLUMN `NumeroParcelas`', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'ValorTotal' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Entradas');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Entradas` DROP COLUMN `ValorTotal`', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Renomear colunas apenas se existirem
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'ValorTotal' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` CHANGE `ValorTotal` `ValorParcela` decimal(65,30) NOT NULL', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'EntradaId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` CHANGE `EntradaId` `ParcelamentoId` int NOT NULL', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'Data' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` CHANGE `Data` `DataVencimento` datetime NOT NULL', 
                    'SELECT ''Column does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Renomear índice apenas se existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
                               WHERE index_name = 'IX_Pagamentos_EntradaId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist > 0, 
                    'ALTER TABLE `Pagamentos` DROP INDEX `IX_Pagamentos_EntradaId`', 
                    'SELECT ''Index does not exist''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Adicionar coluna apenas se não existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.columns 
                               WHERE column_name = 'NumeroParcela' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist = 0, 
                    'ALTER TABLE `Pagamentos` ADD `NumeroParcela` int NOT NULL DEFAULT 0', 
                    'SELECT ''Column already exists''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar tabela Parcelamentos apenas se não existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.tables 
                               WHERE table_schema = DATABASE() 
                               AND table_name = 'Parcelamentos');
                SET @sqlstmt := IF(@exist = 0, 
                    'CREATE TABLE `Parcelamentos` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `EntradaId` int NOT NULL,
                        `TipoPagamento` longtext CHARACTER SET utf8mb4 NOT NULL,
                        `NumeroParcelas` int NOT NULL,
                        `ValorTotal` decimal(65,30) NOT NULL,
                        PRIMARY KEY (`Id`),
                        KEY `IX_Parcelamentos_EntradaId` (`EntradaId`),
                        CONSTRAINT `FK_Parcelamentos_Entradas_EntradaId` FOREIGN KEY (`EntradaId`) REFERENCES `Entradas` (`Id`) ON DELETE CASCADE
                    ) CHARACTER SET=utf8mb4', 
                    'SELECT ''Table already exists''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar índice apenas se não existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
                               WHERE index_name = 'IX_Parcelamentos_EntradaId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Parcelamentos');
                SET @sqlstmt := IF(@exist = 0, 
                    'CREATE INDEX `IX_Parcelamentos_EntradaId` ON `Parcelamentos` (`EntradaId`)', 
                    'SELECT ''Index already exists''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar índice em Pagamentos apenas se não existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
                               WHERE index_name = 'IX_Pagamentos_ParcelamentoId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist = 0, 
                    'CREATE INDEX `IX_Pagamentos_ParcelamentoId` ON `Pagamentos` (`ParcelamentoId`)', 
                    'SELECT ''Index already exists''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Adicionar foreign key apenas se não existir
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_Pagamentos_Parcelamentos_ParcelamentoId' 
                               AND table_schema = DATABASE() 
                               AND table_name = 'Pagamentos');
                SET @sqlstmt := IF(@exist = 0, 
                    'ALTER TABLE `Pagamentos` ADD CONSTRAINT `FK_Pagamentos_Parcelamentos_ParcelamentoId` FOREIGN KEY (`ParcelamentoId`) REFERENCES `Parcelamentos` (`Id`) ON DELETE CASCADE', 
                    'SELECT ''Foreign key already exists''');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagamentos_Parcelamentos_ParcelamentoId",
                table: "Pagamentos");

            migrationBuilder.DropTable(
                name: "Parcelamentos");

            migrationBuilder.DropColumn(
                name: "NumeroParcela",
                table: "Pagamentos");

            migrationBuilder.RenameColumn(
                name: "ValorParcela",
                table: "Pagamentos",
                newName: "ValorTotal");

            migrationBuilder.RenameColumn(
                name: "ParcelamentoId",
                table: "Pagamentos",
                newName: "EntradaId");

            migrationBuilder.RenameColumn(
                name: "DataVencimento",
                table: "Pagamentos",
                newName: "Data");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamentos_ParcelamentoId",
                table: "Pagamentos",
                newName: "IX_Pagamentos_EntradaId");

            migrationBuilder.AddColumn<int>(
                name: "NumeroParcelas",
                table: "Pagamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Pagamentos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NumeroParcelas",
                table: "Entradas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "Entradas",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamentos_Entradas_EntradaId",
                table: "Pagamentos",
                column: "EntradaId",
                principalTable: "Entradas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
