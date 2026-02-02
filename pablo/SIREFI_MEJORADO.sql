/* ===========================
   Script: Base de datos SIREFI - VERSIÓN DEFINITIVA (Sin Ciclos/Cascadas)
   Autor: Sistema SIREFI
   Fecha: 2026-01-14
   Descripción: Se han eliminado los CASCADE en tablas conflictivas (Archivos, Notificaciones)
                para evitar el error 1785 (Multiple Cascade Paths).
                Se confía en el Soft Delete (campo 'eliminado') para la lógica de negocio.
   =========================== */

USE master;
GO

IF DB_ID('SIREFI') IS NULL
BEGIN
    CREATE DATABASE SIREFI;
    PRINT 'Base de datos SIREFI creada.';
END
ELSE
BEGIN
    PRINT 'Base de datos SIREFI ya existe.';
END
GO

USE SIREFI;
GO

-- =============================================
-- 1) TABLAS DE CATÁLOGOS
-- =============================================

IF OBJECT_ID('dbo.Prioridades', 'U') IS NOT NULL DROP TABLE dbo.Prioridades;
GO
CREATE TABLE dbo.Prioridades (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(20) NOT NULL UNIQUE,
    nivel INT NOT NULL,
    color NVARCHAR(7) NULL,
    descripcion NVARCHAR(255) NULL,
    activo BIT NOT NULL DEFAULT(1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
);
GO

IF OBJECT_ID('dbo.Estados', 'U') IS NOT NULL DROP TABLE dbo.Estados;
GO
CREATE TABLE dbo.Estados (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(30) NOT NULL UNIQUE,
    orden INT NOT NULL,
    es_final BIT NOT NULL DEFAULT(0),
    color NVARCHAR(7) NULL,
    descripcion NVARCHAR(255) NULL,
    activo BIT NOT NULL DEFAULT(1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
);
GO

IF OBJECT_ID('dbo.Categorias', 'U') IS NOT NULL DROP TABLE dbo.Categorias;
GO
CREATE TABLE dbo.Categorias (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL UNIQUE,
    tipo_dashboard NVARCHAR(50) NULL,
    descripcion NVARCHAR(500) NULL,
    icono NVARCHAR(50) NULL,
    color NVARCHAR(7) NULL,
    activo BIT NOT NULL DEFAULT(1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME())
);
GO

-- =============================================
-- 2) TABLAS DE INFRAESTRUCTURA
-- =============================================

IF OBJECT_ID('dbo.Edificios', 'U') IS NOT NULL DROP TABLE dbo.Edificios;
GO
CREATE TABLE dbo.Edificios (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    codigo NVARCHAR(20) NULL UNIQUE,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    ubicacion NVARCHAR(255) NULL,
    pisos INT NULL,
    activo BIT NOT NULL DEFAULT(1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    fecha_actualizacion DATETIME2 NULL,
    creado_por INT NULL,
    actualizado_por INT NULL,
    CONSTRAINT UQ_Edificio_Nombre UNIQUE (nombre)
);
GO

IF OBJECT_ID('dbo.Salones', 'U') IS NOT NULL DROP TABLE dbo.Salones;
GO
CREATE TABLE dbo.Salones (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_edificio INT NOT NULL,
    codigo NVARCHAR(20) NULL,
    nombre NVARCHAR(100) NOT NULL,
    tipo_espacio NVARCHAR(50) NULL,
    capacidad INT NULL,
    piso INT NULL,
    descripcion NVARCHAR(500) NULL,
    activo BIT NOT NULL DEFAULT(1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    fecha_actualizacion DATETIME2 NULL,
    creado_por INT NULL,
    actualizado_por INT NULL,
    CONSTRAINT FK_Salones_Edificio FOREIGN KEY (id_edificio)
        REFERENCES dbo.Edificios(id) ON DELETE CASCADE, -- Esto es seguro (1 nivel)
    CONSTRAINT UQ_Salon_Edificio UNIQUE (id_edificio, nombre)
);
GO

-- =============================================
-- 3) TABLA DE USUARIOS
-- =============================================
IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL DROP TABLE dbo.Usuarios;
GO
CREATE TABLE dbo.Usuarios (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(150) NOT NULL,
    correo NVARCHAR(150) NOT NULL UNIQUE,
    contrasena NVARCHAR(255) NULL,
    rol NVARCHAR(50) NOT NULL,
    telefono NVARCHAR(20) NULL,
    foto NVARCHAR(500) NULL,
    departamento NVARCHAR(100) NULL,
    activo BIT NOT NULL DEFAULT(1),
    ultimo_acceso DATETIME2 NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    fecha_actualizacion DATETIME2 NULL,
    CONSTRAINT CHK_Usuarios_Rol CHECK (rol IN ('reportante','tecnico','administrador'))
);
GO

-- =============================================
-- 4) TABLA DE REPORTES
-- =============================================
IF OBJECT_ID('dbo.Reportes', 'U') IS NOT NULL DROP TABLE dbo.Reportes;
GO
CREATE TABLE dbo.Reportes (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    folio NVARCHAR(30) NOT NULL UNIQUE,

    -- Ubicación
    id_edificio INT NULL,
    id_salon INT NULL,
    ubicacion_adicional NVARCHAR(255) NULL,

    -- Categorización
    id_categoria INT NOT NULL,
    subcategoria NVARCHAR(100) NULL,

    -- Descripción
    titulo NVARCHAR(200) NULL,
    descripcion NVARCHAR(MAX) NOT NULL,

    -- Clasificación
    id_prioridad INT NOT NULL,
    id_estado INT NOT NULL,

    -- Asignación
    id_reportante INT NOT NULL,
    id_asignado_a INT NULL,

    -- Tiempos
    fecha_reporte DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    fecha_asignacion DATETIME2 NULL,
    fecha_inicio_atencion DATETIME2 NULL,
    fecha_finalizacion DATETIME2 NULL,
    tiempo_respuesta_minutos INT NULL,
    tiempo_resolucion_minutos INT NULL,

    -- Evaluación y Costos
    calificacion INT NULL,
    comentario_calificacion NVARCHAR(500) NULL,
    costo_estimado DECIMAL(10,2) NULL,
    costo_real DECIMAL(10,2) NULL,

    -- Auditoría
    fecha_actualizacion DATETIME2 NULL,
    actualizado_por INT NULL,
    eliminado BIT NOT NULL DEFAULT(0),
    fecha_eliminacion DATETIME2 NULL,

    -- Constraints (SIN CASCADE NI SET NULL para máxima seguridad)
    CONSTRAINT FK_Reportes_Edificio FOREIGN KEY (id_edificio) REFERENCES dbo.Edificios(id),
    CONSTRAINT FK_Reportes_Salon FOREIGN KEY (id_salon) REFERENCES dbo.Salones(id),
    CONSTRAINT FK_Reportes_Categoria FOREIGN KEY (id_categoria) REFERENCES dbo.Categorias(id),
    CONSTRAINT FK_Reportes_Prioridad FOREIGN KEY (id_prioridad) REFERENCES dbo.Prioridades(id),
    CONSTRAINT FK_Reportes_Estado FOREIGN KEY (id_estado) REFERENCES dbo.Estados(id),
    CONSTRAINT FK_Reportes_Reportante FOREIGN KEY (id_reportante) REFERENCES dbo.Usuarios(id),
    CONSTRAINT FK_Reportes_Asignado FOREIGN KEY (id_asignado_a) REFERENCES dbo.Usuarios(id),
    CONSTRAINT FK_Reportes_ActualizadoPor FOREIGN KEY (actualizado_por) REFERENCES dbo.Usuarios(id),
    CONSTRAINT CHK_Reportes_Calificacion CHECK (calificacion BETWEEN 1 AND 5)
);
GO

-- =============================================
-- 5) TABLA DE COMENTARIOS
-- =============================================
IF OBJECT_ID('dbo.Comentarios', 'U') IS NOT NULL DROP TABLE dbo.Comentarios;
GO
CREATE TABLE dbo.Comentarios (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_reporte INT NOT NULL,
    id_usuario INT NOT NULL,
    comentario NVARCHAR(MAX) NOT NULL,
    tipo NVARCHAR(30) NULL DEFAULT('comentario'),
    publico BIT NOT NULL DEFAULT(1),
    fecha_comentario DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    editado BIT NOT NULL DEFAULT(0),
    fecha_edicion DATETIME2 NULL,
    eliminado BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_Comentarios_Reporte FOREIGN KEY (id_reporte)
        REFERENCES dbo.Reportes(id) ON DELETE CASCADE, -- Mantenemos este, es útil
    CONSTRAINT FK_Comentarios_Usuario FOREIGN KEY (id_usuario)
        REFERENCES dbo.Usuarios(id)
);
GO

-- =============================================
-- 6) TABLA DE HISTORIAL DE ESTADOS
-- =============================================
IF OBJECT_ID('dbo.HistorialEstados', 'U') IS NOT NULL DROP TABLE dbo.HistorialEstados;
GO
CREATE TABLE dbo.HistorialEstados (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_reporte INT NOT NULL,
    id_estado_anterior INT NULL,
    id_estado_nuevo INT NOT NULL,
    id_usuario INT NOT NULL,
    comentario NVARCHAR(500) NULL,
    fecha_cambio DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    CONSTRAINT FK_HistEstados_Reporte FOREIGN KEY (id_reporte)
        REFERENCES dbo.Reportes(id) ON DELETE CASCADE, -- Seguro porque no tiene hijos complejos
    CONSTRAINT FK_HistEstados_EstadoAnt FOREIGN KEY (id_estado_anterior)
        REFERENCES dbo.Estados(id),
    CONSTRAINT FK_HistEstados_EstadoNuevo FOREIGN KEY (id_estado_nuevo)
        REFERENCES dbo.Estados(id),
    CONSTRAINT FK_HistEstados_Usuario FOREIGN KEY (id_usuario)
        REFERENCES dbo.Usuarios(id)
);
GO

-- =============================================
-- 7) TABLA DE ARCHIVOS (CORREGIDA)
-- =============================================
IF OBJECT_ID('dbo.Archivos', 'U') IS NOT NULL DROP TABLE dbo.Archivos;
GO
CREATE TABLE dbo.Archivos (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_reporte INT NULL,
    id_comentario INT NULL,
    nombre_original NVARCHAR(255) NOT NULL,
    nombre_archivo NVARCHAR(255) NOT NULL,
    ruta NVARCHAR(500) NOT NULL,
    tipo_mime NVARCHAR(100) NULL,
    tamano_bytes BIGINT NULL,
    id_usuario INT NOT NULL,
    fecha_subida DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    eliminado BIT NOT NULL DEFAULT(0),

    -- AQUÍ ESTABA EL ERROR: Se eliminaron los ON DELETE CASCADE
    CONSTRAINT FK_Archivos_Reporte FOREIGN KEY (id_reporte)
        REFERENCES dbo.Reportes(id), -- No Action (Default)

    CONSTRAINT FK_Archivos_Comentario FOREIGN KEY (id_comentario)
        REFERENCES dbo.Comentarios(id), -- No Action (Default)

    CONSTRAINT FK_Archivos_Usuario FOREIGN KEY (id_usuario)
        REFERENCES dbo.Usuarios(id),

    CONSTRAINT CHK_Archivos_Entidad CHECK (
        (id_reporte IS NOT NULL AND id_comentario IS NULL) OR
        (id_reporte IS NULL AND id_comentario IS NOT NULL)
    )
);
GO

-- =============================================
-- 8) TABLA DE NOTIFICACIONES
-- =============================================
IF OBJECT_ID('dbo.Notificaciones', 'U') IS NOT NULL DROP TABLE dbo.Notificaciones;
GO
CREATE TABLE dbo.Notificaciones (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_reporte INT NULL,
    tipo NVARCHAR(50) NOT NULL,
    titulo NVARCHAR(200) NOT NULL,
    mensaje NVARCHAR(500) NULL,
    leido BIT NOT NULL DEFAULT(0),
    fecha_creacion DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
    fecha_leido DATETIME2 NULL,
    -- Quitamos CASCADE aquí también para evitar sorpresas, aunque no era el error principal
    CONSTRAINT FK_Notif_Usuario FOREIGN KEY (id_usuario)
        REFERENCES dbo.Usuarios(id),
    CONSTRAINT FK_Notif_Reporte FOREIGN KEY (id_reporte)
        REFERENCES dbo.Reportes(id) ON DELETE CASCADE -- Este sí lo dejamos para limpiar notificaciones si muere el reporte
);
GO

-- =============================================
-- 9) ÍNDICES Y VISTAS
-- =============================================

CREATE INDEX IX_Reportes_IdReportante ON dbo.Reportes(id_reportante) WHERE eliminado = 0;
CREATE INDEX IX_Reportes_IdAsignado ON dbo.Reportes(id_asignado_a) WHERE eliminado = 0;
CREATE INDEX IX_Reportes_IdEstado ON dbo.Reportes(id_estado) WHERE eliminado = 0;
CREATE INDEX IX_Reportes_Eliminado ON dbo.Reportes(eliminado);
CREATE INDEX IX_Comentarios_IdReporte ON dbo.Comentarios(id_reporte) WHERE eliminado = 0;
CREATE INDEX IX_Archivos_IdReporte ON dbo.Archivos(id_reporte) WHERE eliminado = 0;
GO

IF OBJECT_ID('dbo.vw_Reportes_Completo', 'V') IS NOT NULL DROP VIEW dbo.vw_Reportes_Completo;
GO
CREATE VIEW dbo.vw_Reportes_Completo AS
SELECT
    r.id, r.folio, r.titulo, r.descripcion,
    ISNULL(e.nombre, 'Sin Edificio') AS edificio,
    ISNULL(s.nombre, 'Sin Salón') AS salon,
    r.ubicacion_adicional, c.nombre AS categoria,
    est.nombre AS estado, est.color AS estado_color,
    p.nombre AS prioridad, p.color AS prioridad_color,
    rep.nombre AS reportante_nombre, tec.nombre AS tecnico_nombre,
    r.fecha_reporte, r.fecha_asignacion, r.fecha_finalizacion, r.eliminado
FROM dbo.Reportes r
LEFT JOIN dbo.Edificios e ON r.id_edificio = e.id
LEFT JOIN dbo.Salones s ON r.id_salon = s.id
INNER JOIN dbo.Categorias c ON r.id_categoria = c.id
INNER JOIN dbo.Estados est ON r.id_estado = est.id
INNER JOIN dbo.Prioridades p ON r.id_prioridad = p.id
INNER JOIN dbo.Usuarios rep ON r.id_reportante = rep.id
LEFT JOIN dbo.Usuarios tec ON r.id_asignado_a = tec.id
WHERE r.eliminado = 0;
GO

IF OBJECT_ID('dbo.trg_UpdateReporteTimestamp', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_UpdateReporteTimestamp;
GO
CREATE TRIGGER dbo.trg_UpdateReporteTimestamp
ON dbo.Reportes AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(fecha_actualizacion)
    BEGIN
        UPDATE dbo.Reportes SET fecha_actualizacion = SYSDATETIME()
        FROM dbo.Reportes r INNER JOIN Inserted i ON r.id = i.id;
    END
END
GO

-- Datos Semilla
IF NOT EXISTS (SELECT 1 FROM dbo.Prioridades)
    INSERT INTO dbo.Prioridades (nombre, nivel, color) VALUES ('Alta', 1, '#dc3545'), ('Media', 2, '#ffc107'), ('Baja', 3, '#28a745');
IF NOT EXISTS (SELECT 1 FROM dbo.Estados)
    INSERT INTO dbo.Estados (nombre, orden, es_final, color) VALUES ('Recibido', 1, 0, '#6c757d'), ('En Proceso', 2, 0, '#ffc107'), ('Resuelto', 3, 1, '#28a745');
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias)
    INSERT INTO dbo.Categorias (nombre, tipo_dashboard) VALUES ('Electricidad', 'infraestructura'), ('Internet', 'tics');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correo = 'admin@sirefi.com')
    INSERT INTO dbo.Usuarios (nombre, correo, contrasena, rol, departamento) VALUES ('Admin', 'admin@sirefi.com', 'admin123', 'administrador', 'TI');
GO

PRINT 'Script ejecutado exitosamente. Estructura limpia de ciclos.';
