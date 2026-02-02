<?php
/**
 * API: CRUD de Edificios
 * Endpoints para gestión de edificios
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
                listEdificios($conn);
            } elseif ($action === 'get' && isset($_GET['id'])) {
                getEdificio($conn, $_GET['id']);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'POST':
            if ($action === 'create') {
                createEdificio($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'PUT':
            if ($action === 'update') {
                updateEdificio($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'DELETE':
            if ($action === 'delete' && isset($_GET['id'])) {
                deleteEdificio($conn, $_GET['id']);
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
 * List all edificios
 */
function listEdificios($conn) {
    $sql = "SELECT id, codigo, nombre, descripcion, ubicacion, pisos, activo, fecha_creacion 
            FROM Edificios 
            ORDER BY nombre";
    
    $stmt = sqlsrv_query($conn, $sql);
    
    if ($stmt === false) {
        sendError('Error al obtener edificios', 500);
    }
    
    $edificios = [];
    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        $row['fecha_creacion'] = $row['fecha_creacion'] ? $row['fecha_creacion']->format('Y-m-d H:i:s') : null;
        $edificios[] = $row;
    }
    
    sendSuccess($edificios);
}

/**
 * Get single edificio
 */
function getEdificio($conn, $id) {
    $id = intval($id);
    
    $sql = "SELECT id, codigo, nombre, descripcion, ubicacion, pisos, activo, fecha_creacion 
            FROM Edificios 
            WHERE id = ?";
    
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if ($stmt === false) {
        sendError('Error al obtener edificio', 500);
    }
    
    $edificio = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
    
    if (!$edificio) {
        sendError('Edificio no encontrado', 404);
    }
    
    $edificio['fecha_creacion'] = $edificio['fecha_creacion'] ? $edificio['fecha_creacion']->format('Y-m-d H:i:s') : null;
    
    sendSuccess($edificio);
}

/**
 * Create new edificio
 */
function createEdificio($conn) {
    try {
        $data = json_decode(file_get_contents('php://input'), true);
        
        // Validate CSRF token
        if (!isset($data['csrf_token']) || !checkCsrfToken($data['csrf_token'])) {
            sendError('Token CSRF inválido', 403);
        }
        
        // Validate input
        if (empty($data['nombre'])) {
            sendError('El nombre es requerido', 400);
        }
        
        if (empty($data['codigo'])) {
            sendError('El código es requerido', 400);
        }
        
        $codigo = trim(sanitizeString($data['codigo']));
        $nombre = trim(sanitizeString($data['nombre']));
        $descripcion = !empty($data['descripcion']) ? sanitizeString($data['descripcion']) : null;
        $ubicacion = !empty($data['ubicacion']) ? sanitizeString($data['ubicacion']) : null;
        $pisos = !empty($data['pisos']) ? intval($data['pisos']) : null;
        $activo = isset($data['activo']) ? (bool)$data['activo'] : true;
        
        // Additional validation
        if (strlen($nombre) < 2) {
            sendError('El nombre debe tener al menos 2 caracteres', 400);
        }
        
        if (strlen($codigo) < 2) {
            sendError('El código debe tener al menos 2 caracteres', 400);
        }
        
        if (strlen($codigo) > 20) {
            sendError('El código no puede exceder 20 caracteres', 400);
        }
        
        // Validate codigo format (alphanumeric and hyphens only)
        if (!preg_match('/^[A-Z0-9\-]+$/i', $codigo)) {
            sendError('El código solo puede contener letras, números y guiones', 400);
        }
        
        if ($pisos !== null && $pisos < 1) {
            sendError('El número de pisos debe ser mayor a 0', 400);
        }
        
        // Check if name already exists (case-insensitive)
        $checkSql = "SELECT nombre FROM Edificios WHERE LOWER(nombre) = LOWER(?)";
        $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre]);
        
        if ($checkStmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error checking edificio: " . print_r($errors, true));
            sendError('Error al verificar nombre de edificio', 500);
        }
        
        $checkRow = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
        
        if ($checkRow) {
            sendError('Ya existe un edificio con el nombre "' . htmlspecialchars($checkRow['nombre']) . '"', 400);
        }
        
        // Check if codigo already exists (case-insensitive)
        $checkCodigoSql = "SELECT codigo FROM Edificios WHERE LOWER(codigo) = LOWER(?)";
        $checkCodigoStmt = sqlsrv_query($conn, $checkCodigoSql, [$codigo]);
        
        if ($checkCodigoStmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error checking edificio codigo: " . print_r($errors, true));
            sendError('Error al verificar código de edificio', 500);
        }
        
        $checkCodigoRow = sqlsrv_fetch_array($checkCodigoStmt, SQLSRV_FETCH_ASSOC);
        
        if ($checkCodigoRow) {
            sendError('Ya existe un edificio con el código "' . htmlspecialchars($checkCodigoRow['codigo']) . '"', 400);
        }
        
        // Insert
        $sql = "INSERT INTO Edificios (codigo, nombre, descripcion, ubicacion, pisos, activo) 
                OUTPUT INSERTED.id
                VALUES (?, ?, ?, ?, ?, ?)";
        
        $stmt = sqlsrv_query($conn, $sql, [$codigo, $nombre, $descripcion, $ubicacion, $pisos, $activo]);
        
        if ($stmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error creating edificio: " . print_r($errors, true));
            sendError('Error al crear edificio. Por favor, contacte al administrador.', 500);
        }
        
        $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
        
        if (!$row || !isset($row['id'])) {
            error_log("No ID returned after insert");
            sendError('Error: No se pudo obtener el ID del edificio creado', 500);
        }
        
        $newId = $row['id'];
        
        sendSuccess(['id' => $newId, 'message' => 'Edificio creado exitosamente']);
    } catch (Exception $e) {
        error_log("Exception in createEdificio: " . $e->getMessage());
        sendError('Error interno: ' . $e->getMessage(), 500);
    }
}

/**
 * Update edificio
 */
function updateEdificio($conn) {
    $data = json_decode(file_get_contents('php://input'), true);
    
    // Validate CSRF token
    if (!isset($data['csrf_token']) || !checkCsrfToken($data['csrf_token'])) {
        sendError('Token CSRF inválido', 403);
    }
    
    // Validate input
    if (empty($data['id']) || empty($data['nombre'])) {
        sendError('ID y nombre son requeridos', 400);
    }
    
    if (empty($data['codigo'])) {
        sendError('El código es requerido', 400);
    }
    
    $id = intval($data['id']);
    $codigo = trim(sanitizeString($data['codigo']));
    $nombre = trim(sanitizeString($data['nombre']));
    $descripcion = !empty($data['descripcion']) ? sanitizeString($data['descripcion']) : null;
    $ubicacion = !empty($data['ubicacion']) ? sanitizeString($data['ubicacion']) : null;
    $pisos = !empty($data['pisos']) ? intval($data['pisos']) : null;
    $activo = isset($data['activo']) ? (bool)$data['activo'] : true;
    
    // Additional validation
    if (strlen($nombre) < 2) {
        sendError('El nombre debe tener al menos 2 caracteres', 400);
    }
    
    if (strlen($codigo) < 2) {
        sendError('El código debe tener al menos 2 caracteres', 400);
    }
    
    if (strlen($codigo) > 20) {
        sendError('El código no puede exceder 20 caracteres', 400);
    }
    
    // Validate codigo format (alphanumeric and hyphens only)
    if (!preg_match('/^[A-Z0-9\-]+$/i', $codigo)) {
        sendError('El código solo puede contener letras, números y guiones', 400);
    }
    
    if ($pisos !== null && $pisos < 1) {
        sendError('El número de pisos debe ser mayor a 0', 400);
    }
    
    // Check if name already exists (excluding current record, case-insensitive)
    $checkSql = "SELECT nombre FROM Edificios WHERE LOWER(nombre) = LOWER(?) AND id != ?";
    $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre, $id]);
    $checkRow = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
    
    if ($checkRow) {
        sendError('Ya existe otro edificio con el nombre "' . htmlspecialchars($checkRow['nombre']) . '"', 400);
    }
    
    // Check if codigo already exists (excluding current record, case-insensitive)
    $checkCodigoSql = "SELECT codigo FROM Edificios WHERE LOWER(codigo) = LOWER(?) AND id != ?";
    $checkCodigoStmt = sqlsrv_query($conn, $checkCodigoSql, [$codigo, $id]);
    $checkCodigoRow = sqlsrv_fetch_array($checkCodigoStmt, SQLSRV_FETCH_ASSOC);
    
    if ($checkCodigoRow) {
        sendError('Ya existe otro edificio con el código "' . htmlspecialchars($checkCodigoRow['codigo']) . '"', 400);
    }
    
    // If deactivating, check for active salones
    if (!$activo) {
        $salonSql = "SELECT COUNT(*) as count FROM Salones WHERE id_edificio = ? AND activo = 1";
        $salonStmt = sqlsrv_query($conn, $salonSql, [$id]);
        $salonRow = sqlsrv_fetch_array($salonStmt, SQLSRV_FETCH_ASSOC);
        
        if ($salonRow['count'] > 0) {
            sendError('No se puede desactivar un edificio con salones activos', 400);
        }
    }
    
    // Update
    $sql = "UPDATE Edificios 
            SET codigo = ?, nombre = ?, descripcion = ?, ubicacion = ?, pisos = ?, activo = ?
            WHERE id = ?";
    
    $stmt = sqlsrv_query($conn, $sql, [$codigo, $nombre, $descripcion, $ubicacion, $pisos, $activo, $id]);
    
    if ($stmt === false) {
        sendError('Error al actualizar edificio', 500);
    }
    
    sendSuccess(['message' => 'Edificio actualizado exitosamente']);
}

/**
 * Delete edificio
 */
function deleteEdificio($conn, $id) {
    $id = intval($id);
    
    // Check for associated salones
    $salonSql = "SELECT COUNT(*) as count FROM Salones WHERE id_edificio = ?";
    $salonStmt = sqlsrv_query($conn, $salonSql, [$id]);
    $salonRow = sqlsrv_fetch_array($salonStmt, SQLSRV_FETCH_ASSOC);
    
    if ($salonRow['count'] > 0) {
        sendError('No se puede eliminar un edificio que tiene salones asociados', 400);
    }
    
    $sql = "DELETE FROM Edificios WHERE id = ?";
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if ($stmt === false) {
        sendError('Error al eliminar edificio', 500);
    }
    
    sendSuccess(['message' => 'Edificio eliminado exitosamente']);
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
