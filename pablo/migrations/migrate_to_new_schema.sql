/* ===========================
   Script: Migración de datos de SIREFI antiguo a SIREFI mejorado
   Autor: Sistema SIREFI
   Fecha: 2026-01-15
   Descripción: Migra datos existentes del esquema antiguo al nuevo
   
   IMPORTANTE: Ejecutar DESPUÉS de crear el nuevo esquema (SIREFI_MEJORADO.sql)
   =========================== */

USE SIREFI;
GO

PRINT '============================================';
PRINT 'INICIANDO MIGRACIÓN DE DATOS';
PRINT '============================================';
GO

-- =============================================
-- 1) MIGRAR USUARIOS
-- =============================================
PRINT 'Migrando usuarios...';

-- Nota: La nueva tabla Usuarios tiene más campos, pero mantenemos compatibilidad
-- Los usuarios ya deben estar en la nueva tabla si ejecutaste SIREFI_MEJORADO.sql

-- Si hay datos en tabla antigua, migrar:
-- INSERT INTO dbo.Usuarios (nombre, correo, contrasena, rol, telefono, activo, fecha_creacion)
-- SELECT nombre, correo, contrasena, 
--        CASE WHEN rol = 'mantenimiento' THEN 'tecnico' ELSE rol END, -- Cambiar 'mantenimiento' a 'tecnico'
--        telefono, activo, fecha_creacion
-- FROM dbo.Usuarios_OLD;

PRINT 'Usuarios migrados';
GO

-- =============================================
-- 2) CREAR MAPEO DE ESTADOS
-- =============================================
PRINT 'Creando mapeo de estados...';

-- Mapeo de estados antiguos a nuevos IDs
DECLARE @estado_recibido INT = (SELECT id FROM dbo.Estados WHERE nombre = 'Recibido');
DECLARE @estado_proceso INT = (SELECT id FROM dbo.Estados WHERE nombre = 'En proceso');
DECLARE @estado_solucionado INT = (SELECT id FROM dbo.Estados WHERE nombre = 'Solucionado');
DECLARE @estado_cancelado INT = (SELECT id FROM dbo.Estados WHERE nombre = 'Cancelado');

PRINT 'Mapeo de estados creado';
GO

-- =============================================
-- 3) CREAR MAPEO DE PRIORIDADES
-- =============================================
PRINT 'Creando mapeo de prioridades...';

DECLARE @prioridad_alta INT = (SELECT id FROM dbo.Prioridades WHERE nombre = 'Alta');
DECLARE @prioridad_media INT = (SELECT id FROM dbo.Prioridades WHERE nombre = 'Media');
DECLARE @prioridad_baja INT = (SELECT id FROM dbo.Prioridades WHERE nombre = 'Baja');

PRINT 'Mapeo de prioridades creado';
GO

-- =============================================
-- 4) CREAR MAPEO DE CATEGORÍAS
-- =============================================
PRINT 'Creando mapeo de categorías...';

-- Tabla temporal para mapear tipos antiguos a nuevas categorías
IF OBJECT_ID('tempdb..#MapeoCategoria', 'U') IS NOT NULL DROP TABLE #MapeoCategoria;

CREATE TABLE #MapeoCategoria (
    tipo_antiguo NVARCHAR(100),
    id_categoria_nueva INT
);

INSERT INTO #MapeoCategoria (tipo_antiguo, id_categoria_nueva)
SELECT 'Infraestructura', id FROM dbo.Categorias WHERE nombre = 'Infraestructura'
UNION ALL
SELECT 'Maquinaria de laboratorio', id FROM dbo.Categorias WHERE nombre = 'Maquinaria de laboratorio'
UNION ALL
SELECT 'Aseo de las áreas', id FROM dbo.Categorias WHERE nombre = 'Aseo de las áreas'
UNION ALL
SELECT 'Vehicular', id FROM dbo.Categorias WHERE nombre = 'Vehicular'
UNION ALL
SELECT 'Equipo de cómputo y comunicaciones', id FROM dbo.Categorias WHERE nombre = 'Equipo de cómputo y comunicaciones'
UNION ALL
SELECT 'Otro%', id FROM dbo.Categorias WHERE nombre = 'Otro'; -- Para tipos que empiezan con "Otro:"

PRINT 'Mapeo de categorías creado';
GO

-- =============================================
-- 5) MIGRAR REPORTES (SI EXISTEN EN TABLA ANTIGUA)
-- =============================================
PRINT 'Migrando reportes...';

-- Verificar si hay tabla antigua de reportes
IF OBJECT_ID('dbo.Reportes_OLD', 'U') IS NOT NULL
BEGIN
    -- Migrar reportes antiguos
    INSERT INTO dbo.Reportes (
        folio,
        id_edificio,
        id_salon,
        id_categoria,
        titulo,
        descripcion,
        archivos,
        id_prioridad,
        id_estado,
        id_reportante,
        id_asignado_a,
        fecha_reporte,
        fecha_asignacion,
        fecha_finalizacion
    )
    SELECT 
        r.folio,
        -- Intentar extraer edificio del campo 'area' (formato: "Edificio X - Aula")
        (SELECT TOP 1 e.id FROM dbo.Edificios e 
         WHERE r.area LIKE e.nombre + '%'),
        -- Intentar extraer salón del campo 'area'
        (SELECT TOP 1 s.id FROM dbo.Salones s 
         INNER JOIN dbo.Edificios e ON s.id_edificio = e.id
         WHERE r.area LIKE e.nombre + ' - ' + s.nombre + '%'),
        -- Mapear categoría
        COALESCE(
            (SELECT id_categoria_nueva FROM #MapeoCategoria WHERE tipo_antiguo = r.tipo),
            (SELECT id_categoria_nueva FROM #MapeoCategoria WHERE r.tipo LIKE tipo_antiguo),
            (SELECT id FROM dbo.Categorias WHERE nombre = 'Otro')
        ),
        -- Extraer primeras palabras como título
        LEFT(r.descripcion, 200),
        r.descripcion,
        -- Si hay archivo, crear JSON simple
        CASE WHEN r.archivo IS NOT NULL THEN '["' + r.archivo + '"]' ELSE NULL END,
        -- Mapear prioridad
        CASE r.prioridad
            WHEN 'Alta' THEN (SELECT id FROM dbo.Prioridades WHERE nombre = 'Alta')
            WHEN 'Media' THEN (SELECT id FROM dbo.Prioridades WHERE nombre = 'Media')
            WHEN 'Baja' THEN (SELECT id FROM dbo.Prioridades WHERE nombre = 'Baja')
            ELSE (SELECT id FROM dbo.Prioridades WHERE nombre = 'Media')
        END,
        -- Mapear estado
        CASE r.estatus
            WHEN 'Recibido' THEN (SELECT id FROM dbo.Estados WHERE nombre = 'Recibido')
            WHEN 'En proceso' THEN (SELECT id FROM dbo.Estados WHERE nombre = 'En proceso')
            WHEN 'Solucionado' THEN (SELECT id FROM dbo.Estados WHERE nombre = 'Solucionado')
            WHEN 'Cancelado' THEN (SELECT id FROM dbo.Estados WHERE nombre = 'Cancelado')
            ELSE (SELECT id FROM dbo.Estados WHERE nombre = 'Recibido')
        END,
        r.id_usuario,
        r.asignado_a,
        r.fecha_reporte,
        r.fecha_asignacion,
        r.fecha_solucion
    FROM dbo.Reportes_OLD r;
    
    PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' reportes migrados';
END
ELSE
BEGIN
    PRINT 'No hay tabla Reportes_OLD para migrar (primera instalación)';
END
GO

-- =============================================
-- 6) MIGRAR COMENTARIOS (SI EXISTEN)
-- =============================================
PRINT 'Migrando comentarios...';

IF OBJECT_ID('dbo.Comentarios_OLD', 'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.Comentarios (
        id_reporte,
        id_usuario,
        comentario,
        tipo,
        publico,
        fecha_comentario
    )
    SELECT 
        c.id_reporte,
        c.id_usuario,
        c.comentario,
        'comentario',
        c.publico,
        c.fecha_comentario
    FROM dbo.Comentarios_OLD c;
    
    PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' comentarios migrados';
END
ELSE
BEGIN
    PRINT 'No hay comentarios antiguos para migrar';
END
GO

-- =============================================
-- 7) MIGRAR HISTORIAL DE ESTADOS (SI EXISTE)
-- =============================================
PRINT 'Migrando historial de estados...';

IF OBJECT_ID('dbo.HistorialEstatus_OLD', 'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.HistorialEstados (
        id_reporte,
        id_estado_anterior,
        id_estado_nuevo,
        id_usuario,
        fecha_cambio
    )
    SELECT 
        h.id_reporte,
        -- Mapear estados antiguos a nuevos
        (SELECT id FROM dbo.Estados WHERE nombre = h.estatus_anterior),
        (SELECT id FROM dbo.Estados WHERE nombre = h.estatus_nuevo),
        h.id_usuario,
        h.fecha_cambio
    FROM dbo.HistorialEstatus_OLD h;
    
    PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' registros de historial migrados';
END
ELSE
BEGIN
    PRINT 'No hay historial antiguo para migrar';
END
GO

-- =============================================
-- 8) MIGRAR ARCHIVOS A NUEVA TABLA
-- =============================================
PRINT 'Migrando archivos a tabla dedicada...';

-- Insertar archivos desde reportes que tienen archivo adjunto
INSERT INTO dbo.Archivos (
    id_reporte,
    nombre_original,
    nombre_archivo,
    ruta,
    id_usuario,
    fecha_subida
)
SELECT 
    r.id,
    'archivo_' + r.folio + '.ext', -- Nombre genérico
    'archivo_' + r.folio + '.ext',
    JSON_VALUE(r.archivos, '$[0]'), -- Extraer primera ruta del JSON
    r.id_reportante,
    r.fecha_reporte
FROM dbo.Reportes r
WHERE r.archivos IS NOT NULL;

PRINT CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' archivos migrados a tabla dedicada';
GO

-- =============================================
-- 9) LIMPIAR TABLAS TEMPORALES
-- =============================================
IF OBJECT_ID('tempdb..#MapeoCategoria', 'U') IS NOT NULL DROP TABLE #MapeoCategoria;
GO

-- =============================================
-- 10) VERIFICAR INTEGRIDAD
-- =============================================
PRINT '';
PRINT '============================================';
PRINT 'VERIFICANDO INTEGRIDAD DE DATOS MIGRADOS';
PRINT '============================================';

-- Contar registros en cada tabla
SELECT 'Usuarios' AS Tabla, COUNT(*) AS Total FROM dbo.Usuarios WHERE activo = 1
UNION ALL
SELECT 'Reportes', COUNT(*) FROM dbo.Reportes WHERE eliminado = 0
UNION ALL
SELECT 'Comentarios', COUNT(*) FROM dbo.Comentarios WHERE eliminado = 0
UNION ALL
SELECT 'HistorialEstados', COUNT(*) FROM dbo.HistorialEstados
UNION ALL
SELECT 'Archivos', COUNT(*) FROM dbo.Archivos WHERE eliminado = 0
UNION ALL
SELECT 'Prioridades', COUNT(*) FROM dbo.Prioridades WHERE activo = 1
UNION ALL
SELECT 'Estados', COUNT(*) FROM dbo.Estados WHERE activo = 1
UNION ALL
SELECT 'Categorias', COUNT(*) FROM dbo.Categorias WHERE activo = 1
UNION ALL
SELECT 'Edificios', COUNT(*) FROM dbo.Edificios WHERE activo = 1
UNION ALL
SELECT 'Salones', COUNT(*) FROM dbo.Salones WHERE activo = 1;
GO

-- Verificar reportes sin categoría válida
PRINT '';
PRINT 'Reportes sin categoría válida:';
SELECT COUNT(*) AS Sin_Categoria 
FROM dbo.Reportes r
WHERE r.id_categoria IS NULL OR r.id_categoria NOT IN (SELECT id FROM dbo.Categorias);
GO

-- Verificar reportes sin usuario válido
PRINT 'Reportes sin usuario válido:';
SELECT COUNT(*) AS Sin_Usuario
FROM dbo.Reportes r
WHERE r.id_reportante NOT IN (SELECT id FROM dbo.Usuarios);
GO

PRINT '';
PRINT '============================================';
PRINT 'MIGRACIÓN COMPLETADA EXITOSAMENTE';
PRINT '============================================';
PRINT '';
PRINT 'NOTAS IMPORTANTES:';
PRINT '1. Verificar que todos los reportes tienen categoría válida';
PRINT '2. Algunos reportes pueden no tener edificio/salón si el formato del campo area no era estándar';
PRINT '3. Revisar archivos migrados para validar que las rutas son correctas';
PRINT '4. Actualizar código PHP para usar nueva estructura de tablas';
PRINT '5. Las tablas antiguas (_OLD) pueden ser eliminadas después de validar';
GO

/*
============================================
ROLLBACK (EN CASO DE PROBLEMAS)
============================================

-- Para revertir la migración y volver a empezar:
DELETE FROM dbo.Archivos;
DELETE FROM dbo.HistorialEstados;
DELETE FROM dbo.Comentarios;
DELETE FROM dbo.Reportes;
DELETE FROM dbo.Salones;
DELETE FROM dbo.Edificios;
DELETE FROM dbo.Categorias;
DELETE FROM dbo.Estados;
DELETE FROM dbo.Prioridades;

-- Resetear secuencias
ALTER SEQUENCE dbo.Seq_Reportes RESTART WITH 1;

-- Y volver a ejecutar este script
*/
