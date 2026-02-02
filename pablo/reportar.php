<?php
/**
 * Report Processing Handler
 * Processes incident report submissions from formulario.php
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/validation.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/includes/csrf.php';
require_once __DIR__ . '/conexion.php';

// Require user to be logged in
requireLogin();

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    header("Location: formulario.php");
    exit;
}

try {
    // Verify CSRF token
    verifyCSRFToken();
    $usuario_id = getCurrentUserId();
    
    // Validate and sanitize inputs
    $edificio_id = intval($_POST['edificio'] ?? 0);
    $aula = sanitizeString($_POST['aula'] ?? '');
    $area_destino = sanitizeString($_POST['area_destino'] ?? '');
    $descripcion_problema = sanitizeString($_POST['descripcion_problema'] ?? '');
    $descripcion_otro = sanitizeString($_POST['descripcion_otro'] ?? '');
    
    // Validate required fields
    if (empty($edificio_id) || empty($aula) || empty($area_destino) || empty($descripcion_problema)) {
        handleError('Por favor complete todos los campos obligatorios', null, 'formulario.php');
    }
    
    // Get salon ID by name and edificio
    $salonSQL = "SELECT id FROM Salones WHERE id_edificio = ? AND nombre = ? AND activo = 1";
    $salonStmt = sqlsrv_query($conn, $salonSQL, [$edificio_id, $aula]);
    $salon_id = null;
    if ($salonStmt && $salonRow = sqlsrv_fetch_array($salonStmt, SQLSRV_FETCH_ASSOC)) {
        $salon_id = $salonRow['id'];
    }
    
    // Get categoria ID by name
    $categoria_nombre = $area_destino;
    if ($area_destino === 'Otro' && !empty($descripcion_otro)) {
        $categoria_nombre = $descripcion_otro;
    }
    
    $categoriaSQL = "SELECT id FROM Categorias WHERE nombre = ? AND activo = 1";
    $categoriaStmt = sqlsrv_query($conn, $categoriaSQL, [$area_destino]);
    $categoria_id = null;
    if ($categoriaStmt && $categoriaRow = sqlsrv_fetch_array($categoriaStmt, SQLSRV_FETCH_ASSOC)) {
        $categoria_id = $categoriaRow['id'];
    }
    
    // If categoria doesn't exist, use a default or create one
    if (!$categoria_id) {
        // Try to get a default "Otro" category or use first available
        $defaultCatSQL = "SELECT TOP 1 id FROM Categorias WHERE activo = 1 ORDER BY id";
        $defaultCatStmt = sqlsrv_query($conn, $defaultCatSQL);
        if ($defaultCatStmt && $defaultCatRow = sqlsrv_fetch_array($defaultCatStmt, SQLSRV_FETCH_ASSOC)) {
            $categoria_id = $defaultCatRow['id'];
        } else {
            handleError('No hay categorías disponibles. Contacte al administrador.', null, 'formulario.php');
        }
    }
    
    // Get prioridad ID (default: Media)
    $prioridadSQL = "SELECT id FROM Prioridades WHERE nombre = 'Media' AND activo = 1";
    $prioridadStmt = sqlsrv_query($conn, $prioridadSQL);
    $prioridad_id = null;
    if ($prioridadStmt && $prioridadRow = sqlsrv_fetch_array($prioridadStmt, SQLSRV_FETCH_ASSOC)) {
        $prioridad_id = $prioridadRow['id'];
    }
    
    // Get estado ID (default: Recibido)
    $estadoSQL = "SELECT id FROM Estados WHERE nombre = 'Recibido' AND activo = 1";
    $estadoStmt = sqlsrv_query($conn, $estadoSQL);
    $estado_id = null;
    if ($estadoStmt && $estadoRow = sqlsrv_fetch_array($estadoStmt, SQLSRV_FETCH_ASSOC)) {
        $estado_id = $estadoRow['id'];
    }
    
    if (!$prioridad_id || !$estado_id) {
        handleError('Error de configuración del sistema. Contacte al administrador.', null, 'formulario.php');
    }
    
    // Generate titulo from first words of description
    $titulo = substr($descripcion_problema, 0, 200);
    
    // Generate folio manually (there's no trigger in the schema for this)
    $folio = 'REP-' . date('Ymd') . '-' . str_pad(rand(1, 9999), 4, '0', STR_PAD_LEFT);
    
    // Insert into database with new schema
    $sql = "INSERT INTO Reportes (
                folio, id_edificio, id_salon, id_categoria, titulo, descripcion,
                id_prioridad, id_estado, id_reportante
            ) 
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT SCOPE_IDENTITY() AS id;";
    
    $params = [
        $folio,
        $edificio_id,
        $salon_id,
        $categoria_id,
        $titulo,
        $descripcion_problema,
        $prioridad_id,
        $estado_id,
        $usuario_id
    ];
    
    $stmt = sqlsrv_query($conn, $sql, $params);
    
    if ($stmt) {
        // Get the inserted ID
        $newReportId = null;
        if (sqlsrv_next_result($stmt) && $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
            $newReportId = $row['id'];
        }
        
        if ($newReportId) {
            // Handle file upload if exists
            if (isset($_FILES['archivo_adjunto']) && $_FILES['archivo_adjunto']['error'] !== UPLOAD_ERR_NO_FILE) {
                $validation = validateFileUpload($_FILES['archivo_adjunto']);
                
                if ($validation['valid']) {
                    // Create uploads directory if it doesn't exist
                    if (!is_dir(UPLOAD_DIR)) {
                        mkdir(UPLOAD_DIR, 0755, true);
                    }
                    
                    $nombreArchivo = generateUniqueFilename($_FILES['archivo_adjunto']['name']);
                    $rutaDestino = UPLOAD_DIR . $nombreArchivo;
                    
                    if (move_uploaded_file($_FILES['archivo_adjunto']['tmp_name'], $rutaDestino)) {
                        // Insert into Archivos table
                        $archivoSQL = "INSERT INTO Archivos (id_reporte, nombre_original, nombre_archivo, ruta, tipo_mime, tamano_bytes, id_usuario)
                                      VALUES (?, ?, ?, ?, ?, ?, ?)";
                        $archivoParams = [
                            $newReportId,
                            $_FILES['archivo_adjunto']['name'],
                            $nombreArchivo,
                            'uploads/' . $nombreArchivo,
                            $_FILES['archivo_adjunto']['type'],
                            $_FILES['archivo_adjunto']['size'],
                            $usuario_id
                        ];
                        sqlsrv_query($conn, $archivoSQL, $archivoParams);
                    }
                }
            }
        }
        
        handleSuccess("Reporte enviado correctamente con folio: $folio", 'home.php');
    } else {
        $errorDetails = print_r(sqlsrv_errors(), true);
        handleError('Error al guardar el reporte. Por favor intente nuevamente.', 
                   "Error inserting report: $errorDetails", 
                   'formulario.php');
    }
    
} catch (Exception $e) {
    handleError('Error interno del servidor', 
               "Error in reportar.php: " . $e->getMessage(), 
               'formulario.php');
}
?>
