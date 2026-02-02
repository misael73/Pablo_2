/* ===========================
   Script: Agregar tablas de Edificios y Salones
   Propósito: Gestión dinámica de infraestructura física
   Fecha: 2026-01-15
   =========================== */

USE SIREFI;
GO

-- 1) Tabla Edificios
IF OBJECT_ID('dbo.Edificios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Edificios (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL UNIQUE,
        descripcion NVARCHAR(255) NULL,
        activo BIT NOT NULL DEFAULT(1),
        fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
    );
    
    PRINT 'Tabla Edificios creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla Edificios ya existe';
END
GO

-- 2) Tabla Salones
IF OBJECT_ID('dbo.Salones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Salones (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        id_edificio INT NOT NULL,
        nombre NVARCHAR(100) NOT NULL,
        descripcion NVARCHAR(255) NULL,
        activo BIT NOT NULL DEFAULT(1),
        fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_Salones_Edificio FOREIGN KEY (id_edificio) REFERENCES dbo.Edificios(id) ON DELETE CASCADE,
        CONSTRAINT UQ_Salon_Edificio UNIQUE (id_edificio, nombre)
    );
    
    PRINT 'Tabla Salones creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla Salones ya existe';
END
GO

-- 3) Índices para mejorar performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Edificios_Activo' AND object_id = OBJECT_ID('dbo.Edificios'))
BEGIN
    CREATE INDEX IX_Edificios_Activo ON dbo.Edificios(activo);
    PRINT 'Índice IX_Edificios_Activo creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Salones_Edificio' AND object_id = OBJECT_ID('dbo.Salones'))
BEGIN
    CREATE INDEX IX_Salones_Edificio ON dbo.Salones(id_edificio);
    PRINT 'Índice IX_Salones_Edificio creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Salones_Activo' AND object_id = OBJECT_ID('dbo.Salones'))
BEGIN
    CREATE INDEX IX_Salones_Activo ON dbo.Salones(activo);
    PRINT 'Índice IX_Salones_Activo creado';
END
GO

-- 4) Insertar datos iniciales de edificios
IF NOT EXISTS (SELECT 1 FROM dbo.Edificios WHERE nombre = 'Edificio 1')
BEGIN
    INSERT INTO dbo.Edificios (nombre, descripcion) VALUES 
    ('Edificio 1', 'Edificio principal con laboratorios y aulas'),
    ('Edificio 2', 'Edificio de aulas y espacios administrativos'),
    ('Edificio 3', 'Edificio de laboratorios especializados');
    
    PRINT 'Edificios iniciales insertados';
END
ELSE
BEGIN
    PRINT 'Edificios ya existen en la base de datos';
END
GO

-- 5) Insertar salones por edificio
-- Edificio 1
DECLARE @ed1_id INT = (SELECT id FROM dbo.Edificios WHERE nombre = 'Edificio 1');

IF @ed1_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Salones WHERE id_edificio = @ed1_id)
BEGIN
    INSERT INTO dbo.Salones (id_edificio, nombre, descripcion) VALUES
    (@ed1_id, 'Aula1', NULL),
    (@ed1_id, 'Aula2', NULL),
    (@ed1_id, 'Aula3', NULL),
    (@ed1_id, 'Aula4', NULL),
    (@ed1_id, 'Aula5', NULL),
    (@ed1_id, 'Aula6', NULL),
    (@ed1_id, 'Aula7', NULL),
    (@ed1_id, 'Laboratorio de física', NULL),
    (@ed1_id, 'Multi 1', NULL),
    (@ed1_id, 'Multi 2', NULL),
    (@ed1_id, 'Electrónica', NULL),
    (@ed1_id, 'Nodo Sirectec', NULL);
    
    PRINT 'Salones del Edificio 1 insertados';
END
GO

-- Edificio 2
DECLARE @ed2_id INT = (SELECT id FROM dbo.Edificios WHERE nombre = 'Edificio 2');

IF @ed2_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Salones WHERE id_edificio = @ed2_id)
BEGIN
    INSERT INTO dbo.Salones (id_edificio, nombre, descripcion) VALUES
    (@ed2_id, 'Aula1', NULL),
    (@ed2_id, 'Aula2', NULL),
    (@ed2_id, 'Aula3', NULL),
    (@ed2_id, 'Aula4', NULL),
    (@ed2_id, 'Aula5', NULL),
    (@ed2_id, 'Aula6', NULL),
    (@ed2_id, 'Aula7', NULL),
    (@ed2_id, 'Aula8', NULL),
    (@ed2_id, 'Aula9', NULL),
    (@ed2_id, 'Aula10', NULL),
    (@ed2_id, 'Aula11', NULL),
    (@ed2_id, 'Aula12', NULL),
    (@ed2_id, 'PRE-1', NULL),
    (@ed2_id, 'PRE-2', NULL);
    
    PRINT 'Salones del Edificio 2 insertados';
END
GO

-- Edificio 3
DECLARE @ed3_id INT = (SELECT id FROM dbo.Edificios WHERE nombre = 'Edificio 3');

IF @ed3_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Salones WHERE id_edificio = @ed3_id)
BEGIN
    INSERT INTO dbo.Salones (id_edificio, nombre, descripcion) VALUES
    (@ed3_id, 'S.O', 'Laboratorio de Sistemas Operativos'),
    (@ed3_id, 'CISCO', 'Laboratorio CISCO'),
    (@ed3_id, 'Biomatología', NULL),
    (@ed3_id, 'Frutas', NULL),
    (@ed3_id, 'Cárnicos', NULL),
    (@ed3_id, 'Métodos', NULL),
    (@ed3_id, 'Idiomas', NULL);
    
    PRINT 'Salones del Edificio 3 insertados';
END
GO

-- 6) Índice adicional en tabla Reportes para área
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reportes_Area' AND object_id = OBJECT_ID('dbo.Reportes'))
BEGIN
    CREATE INDEX IX_Reportes_Area ON dbo.Reportes(area);
    PRINT 'Índice IX_Reportes_Area creado';
END
GO

-- 7) Verificación final
PRINT '========================================';
PRINT 'RESUMEN DE MIGRACIÓN';
PRINT '========================================';
SELECT 'Edificios creados' AS Tabla, COUNT(*) AS Cantidad FROM dbo.Edificios
UNION ALL
SELECT 'Salones creados' AS Tabla, COUNT(*) AS Cantidad FROM dbo.Salones;
PRINT '========================================';
PRINT 'Migración completada exitosamente';
PRINT '========================================';
GO

/*
INSTRUCCIONES DE EJECUCIÓN:

1. Abrir SQL Server Management Studio (SSMS)
2. Conectarse al servidor de base de datos
3. Abrir este archivo
4. Ejecutar el script completo (F5)
5. Verificar que no haya errores en los mensajes
6. Confirmar que las tablas y datos se crearon correctamente:
   
   SELECT * FROM dbo.Edificios;
   SELECT * FROM dbo.Salones;

ROLLBACK (si es necesario):

-- Para eliminar las tablas (esto eliminará todos los datos):
DROP TABLE IF EXISTS dbo.Salones;
DROP TABLE IF EXISTS dbo.Edificios;

-- Para limpiar solo los datos:
DELETE FROM dbo.Salones;
DELETE FROM dbo.Edificios;
DBCC CHECKIDENT ('dbo.Salones', RESEED, 0);
DBCC CHECKIDENT ('dbo.Edificios', RESEED, 0);
*/
