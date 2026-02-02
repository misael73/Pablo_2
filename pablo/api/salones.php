<?php
/**
 * API: CRUD de Salones
 * Endpoints para gestión de salones/aulas
 */

require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/../includes/auth.php';
require_once __DIR__ . '/../includes/validation.php';
require_once __DIR__ . '/../includes/csrf.php';
require_once __DIR__ . '/../conexion.php';

// Require admin access
requireAdmin();

// Set JSON header
header('Content-Type: application/json');

// Get request method and action
$method = $_SERVER['REQUEST_METHOD'];
$action = $_GET['action'] ?? '';

try {
    switch ($method) {
        case 'GET':
            if ($action === 'list') {
                listSalones($conn);
            } elseif ($action === 'get' && isset($_GET['id'])) {
                getSalon($conn, $_GET['id']);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'POST':
            if ($action === 'create') {
                createSalon($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'PUT':
            if ($action === 'update') {
                updateSalon($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'DELETE':
            if ($action === 'delete' && isset($_GET['id'])) {
                deleteSalon($conn, $_GET['id']);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        default:
            sendError('Método no permitido', 405);
    }
} catch (Exception $e) {
    sendError($e->getMessage(), 500);
}

/**
 * List salones (optionally filtered by edificio)
 */
function listSalones($conn) {
    $edificioId = isset($_GET['edificio_id']) ? intval($_GET['edificio_id']) : null;
    
    if ($edificioId) {
        $sql = "SELECT s.id, s.id_edificio, s.nombre, s.descripcion, s.activo, s.fecha_creacion,
                e.nombre as edificio_nombre
                FROM Salones s
                JOIN Edificios e ON s.id_edificio = e.id
                WHERE s.id_edificio = ?
                ORDER BY s.nombre";
        $stmt = sqlsrv_query($conn, $sql, [$edificioId]);
    } else {
        $sql = "SELECT s.id, s.id_edificio, s.nombre, s.descripcion, s.activo, s.fecha_creacion,
                e.nombre as edificio_nombre
                FROM Salones s
                JOIN Edificios e ON s.id_edificio = e.id
                ORDER BY e.nombre, s.nombre";
        $stmt = sqlsrv_query($conn, $sql);
    }
    
    if ($stmt === false) {
        sendError('Error al obtener salones', 500);
    }
    
    $salones = [];
    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        $row['fecha_creacion'] = $row['fecha_creacion'] ? $row['fecha_creacion']->format('Y-m-d H:i:s') : null;
        $salones[] = $row;
    }
    
    sendSuccess($salones);
}

/**
 * Get single salon
 */
function getSalon($conn, $id) {
    $id = intval($id);
    
    $sql = "SELECT s.id, s.id_edificio, s.nombre, s.descripcion, s.activo, s.fecha_creacion,
            e.nombre as edificio_nombre
            FROM Salones s
            JOIN Edificios e ON s.id_edificio = e.id
            WHERE s.id = ?";
    
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if ($stmt === false) {
        sendError('Error al obtener salón', 500);
    }
    
    $salon = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
    
    if (!$salon) {
        sendError('Salón no encontrado', 404);
    }
    
    $salon['fecha_creacion'] = $salon['fecha_creacion'] ? $salon['fecha_creacion']->format('Y-m-d H:i:s') : null;
    
    sendSuccess($salon);
}

/**
 * Create new salon
 */
function createSalon($conn) {
    try {
        $data = json_decode(file_get_contents('php://input'), true);
        
        // Validate CSRF token
        if (!isset($data['csrf_token']) || !checkCsrfToken($data['csrf_token'])) {
            sendError('Token CSRF inválido', 403);
        }
        
        // Validate input
        if (empty($data['nombre']) || empty($data['id_edificio'])) {
            sendError('El nombre y edificio son requeridos', 400);
        }
        
        $nombre = trim(sanitizeString($data['nombre']));
        $id_edificio = intval($data['id_edificio']);
        $descripcion = !empty($data['descripcion']) ? sanitizeString($data['descripcion']) : null;
        $activo = isset($data['activo']) ? (bool)$data['activo'] : true;
        
        // Additional validation
        if (strlen($nombre) < 1) {
            sendError('El nombre es requerido', 400);
        }
        
        // Verify edificio exists
        $checkEdificio = "SELECT COUNT(*) as count FROM Edificios WHERE id = ?";
        $checkEdificioStmt = sqlsrv_query($conn, $checkEdificio, [$id_edificio]);
        
        if ($checkEdificioStmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error verifying edificio: " . print_r($errors, true));
            sendError('Error al verificar edificio', 500);
        }
        
        $checkEdificioRow = sqlsrv_fetch_array($checkEdificioStmt, SQLSRV_FETCH_ASSOC);
        
        if ($checkEdificioRow['count'] == 0) {
            sendError('El edificio especificado no existe', 400);
        }
        
        // Check if name already exists in this edificio (case-insensitive)
        $checkSql = "SELECT nombre FROM Salones WHERE LOWER(nombre) = LOWER(?) AND id_edificio = ?";
        $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre, $id_edificio]);
        
        if ($checkStmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error checking salon: " . print_r($errors, true));
            sendError('Error al verificar nombre de salón', 500);
        }
        
        $checkRow = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
    
    if ($checkRow) {
        sendError('Ya existe el salón "' . htmlspecialchars($checkRow['nombre']) . '" en este edificio', 400);
    }
    
    // Insert
    $sql = "INSERT INTO Salones (id_edificio, nombre, descripcion, activo) 
            OUTPUT INSERTED.id
            VALUES (?, ?, ?, ?)";
    
    $stmt = sqlsrv_query($conn, $sql, [$id_edificio, $nombre, $descripcion, $activo]);
    
    if ($stmt === false) {
        $errors = sqlsrv_errors();
        error_log("SQL Error creating salon: " . print_r($errors, true));
        sendError('Error al crear salón. Por favor, contacte al administrador.', 500);
    }
    
    $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
    
    if (!$row || !isset($row['id'])) {
        error_log("No ID returned after salon insert");
        sendError('Error: No se pudo obtener el ID del salón creado', 500);
    }
    
    $newId = $row['id'];
    
    sendSuccess(['id' => $newId, 'message' => 'Salón creado exitosamente']);
    } catch (Exception $e) {
        error_log("Exception in createSalon: " . $e->getMessage());
        sendError('Error interno: ' . $e->getMessage(), 500);
    }
}

/**
 * Update salon
 */
function updateSalon($conn) {
    $data = json_decode(file_get_contents('php://input'), true);
    
    // Validate CSRF token
    if (!isset($data['csrf_token']) || !checkCsrfToken($data['csrf_token'])) {
        sendError('Token CSRF inválido', 403);
    }
    
    // Validate input
    if (empty($data['id']) || empty($data['nombre']) || empty($data['id_edificio'])) {
        sendError('ID, nombre y edificio son requeridos', 400);
    }
    
    $id = intval($data['id']);
    $nombre = sanitizeString($data['nombre']);
    $id_edificio = intval($data['id_edificio']);
    $descripcion = !empty($data['descripcion']) ? sanitizeString($data['descripcion']) : null;
    $activo = isset($data['activo']) ? (bool)$data['activo'] : true;
    
    // Verify edificio exists
    $checkEdificio = "SELECT COUNT(*) as count FROM Edificios WHERE id = ?";
    $checkEdificioStmt = sqlsrv_query($conn, $checkEdificio, [$id_edificio]);
    $checkEdificioRow = sqlsrv_fetch_array($checkEdificioStmt, SQLSRV_FETCH_ASSOC);
    
    if ($checkEdificioRow['count'] == 0) {
        sendError('El edificio especificado no existe', 400);
    }
    
    // Check if name already exists in this edificio (excluding current record)
    $checkSql = "SELECT COUNT(*) as count FROM Salones 
                 WHERE nombre = ? AND id_edificio = ? AND id != ?";
    $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre, $id_edificio, $id]);
    $checkRow = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
    
    if ($checkRow['count'] > 0) {
        sendError('Ya existe otro salón con ese nombre en este edificio', 400);
    }
    
    // Update
    $sql = "UPDATE Salones 
            SET nombre = ?, id_edificio = ?, descripcion = ?, activo = ?
            WHERE id = ?";
    
    $stmt = sqlsrv_query($conn, $sql, [$nombre, $id_edificio, $descripcion, $activo, $id]);
    
    if ($stmt === false) {
        sendError('Error al actualizar salón', 500);
    }
    
    sendSuccess(['message' => 'Salón actualizado exitosamente']);
}

/**
 * Delete salon
 */
function deleteSalon($conn, $id) {
    $id = intval($id);
    
    $sql = "DELETE FROM Salones WHERE id = ?";
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if ($stmt === false) {
        sendError('Error al eliminar salón', 500);
    }
    
    sendSuccess(['message' => 'Salón eliminado exitosamente']);
}

/**
 * Send success response
 */
function sendSuccess($data) {
    echo json_encode(['success' => true, 'data' => $data]);
    exit;
}

/**
 * Send error response
 */
function sendError($message, $code = 400) {
    http_response_code($code);
    echo json_encode(['success' => false, 'error' => $message]);
    exit;
}
