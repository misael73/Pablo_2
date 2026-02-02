<?php
/**
 * API: CRUD de Categorias
 * Endpoints para gestión de categorías de reportes
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
                listCategorias($conn);
            } elseif ($action === 'get' && isset($_GET['id'])) {
                getCategoria($conn, $_GET['id']);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'POST':
            if ($action === 'create') {
                createCategoria($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'PUT':
            if ($action === 'update') {
                updateCategoria($conn);
            } elseif ($action === 'toggle') {
                toggleCategoria($conn);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        case 'DELETE':
            if ($action === 'delete' && isset($_GET['id'])) {
                deleteCategoria($conn, $_GET['id']);
            } else {
                sendError('Acción no válida', 400);
            }
            break;
            
        default:
            sendError('Método no permitido', 405);
    }
} catch (Exception $e) {
    error_log("Error in categorias API: " . $e->getMessage());
    sendError('Error interno del servidor: ' . $e->getMessage(), 500);
}

/**
 * List all categorias
 */
function listCategorias($conn) {
    $sql = "SELECT id, nombre, tipo_dashboard, descripcion, icono, color, activo, fecha_creacion
            FROM Categorias
            ORDER BY nombre ASC";
    
    $stmt = sqlsrv_query($conn, $sql);
    
    if (!$stmt) {
        sendError('Error al obtener categorías: ' . print_r(sqlsrv_errors(), true), 500);
    }
    
    $categorias = [];
    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        $categorias[] = [
            'id' => $row['id'],
            'nombre' => $row['nombre'],
            'tipo_dashboard' => $row['tipo_dashboard'],
            'descripcion' => $row['descripcion'],
            'icono' => $row['icono'],
            'color' => $row['color'],
            'activo' => (bool)$row['activo'],
            'fecha_creacion' => $row['fecha_creacion'] ? $row['fecha_creacion']->format('Y-m-d H:i:s') : null
        ];
    }
    
    sendSuccess($categorias, 'Categorías obtenidas correctamente');
}

/**
 * Get single categoria
 */
function getCategoria($conn, $id) {
    $id = intval($id);
    
    $sql = "SELECT id, nombre, tipo_dashboard, descripcion, icono, color, activo, fecha_creacion
            FROM Categorias
            WHERE id = ?";
    
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if (!$stmt) {
        sendError('Error al obtener categoría', 500);
    }
    
    $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
    
    if (!$row) {
        sendError('Categoría no encontrada', 404);
    }
    
    $categoria = [
        'id' => $row['id'],
        'nombre' => $row['nombre'],
        'tipo_dashboard' => $row['tipo_dashboard'],
        'descripcion' => $row['descripcion'],
        'icono' => $row['icono'],
        'color' => $row['color'],
        'activo' => (bool)$row['activo'],
        'fecha_creacion' => $row['fecha_creacion'] ? $row['fecha_creacion']->format('Y-m-d H:i:s') : null
    ];
    
    sendSuccess($categoria, 'Categoría obtenida correctamente');
}

/**
 * Create new categoria
 */
function createCategoria($conn) {
    try {
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            sendError('Datos inválidos', 400);
        }
        
        // Validate required fields
        $nombre = trim(sanitizeString($input['nombre'] ?? ''));
        $tipo_dashboard = sanitizeString($input['tipo_dashboard'] ?? '');
        
        if (empty($nombre)) {
            sendError('El nombre es requerido', 400);
        }
        
        if (strlen($nombre) < 2) {
            sendError('El nombre debe tener al menos 2 caracteres', 400);
        }
        
        if (empty($tipo_dashboard)) {
            sendError('El tipo de dashboard es requerido', 400);
        }
        
        // Validate tipo_dashboard values
        $valid_tipos = ['materiales', 'tics', 'infraestructura', 'general'];
        if (!in_array($tipo_dashboard, $valid_tipos)) {
            sendError('Tipo de dashboard inválido. Debe ser: materiales, tics, infraestructura o general', 400);
        }
        
        // Optional fields
        $descripcion = sanitizeString($input['descripcion'] ?? '');
        $icono = sanitizeString($input['icono'] ?? '');
        $color = sanitizeString($input['color'] ?? '');
        $activo = isset($input['activo']) ? (int)$input['activo'] : 1;
        
        // Validate color format if provided
        if (!empty($color) && !preg_match('/^#[0-9A-F]{6}$/i', $color)) {
            sendError('Formato de color inválido. Debe ser hexadecimal (ejemplo: #FF0000)', 400);
        }
        
        // Check if name already exists (case-insensitive)
        $checkSql = "SELECT nombre FROM Categorias WHERE LOWER(nombre) = LOWER(?)";
        $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre]);
        
        if ($checkStmt === false) {
            $errors = sqlsrv_errors();
            error_log("SQL Error checking categoria: " . print_r($errors, true));
            sendError('Error al verificar nombre de categoría', 500);
        }
        
        $checkRow = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
        if ($checkRow) {
            sendError('Ya existe la categoría "' . htmlspecialchars($checkRow['nombre']) . '"', 400);
        }
        
        // Insert categoria
        $sql = "INSERT INTO Categorias (nombre, tipo_dashboard, descripcion, icono, color, activo)
                OUTPUT INSERTED.id
                VALUES (?, ?, ?, ?, ?, ?)";
        
        $params = [
            $nombre,
            $tipo_dashboard,
            !empty($descripcion) ? $descripcion : null,
            !empty($icono) ? $icono : null,
            !empty($color) ? $color : null,
            $activo
        ];
        
        $stmt = sqlsrv_query($conn, $sql, $params);
        
        if (!$stmt) {
            $errors = sqlsrv_errors();
            error_log("SQL Error creating categoria: " . print_r($errors, true));
            sendError('Error al crear categoría. Por favor, contacte al administrador.', 500);
        }
        
        // Get the inserted ID
        $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC);
        
        if (!$row || !isset($row['id'])) {
            error_log("No ID returned after categoria insert");
            sendError('Error: No se pudo obtener el ID de la categoría creada', 500);
        }
        
        $newId = $row['id'];
        
        sendSuccess(['id' => $newId], 'Categoría creada correctamente');
    } catch (Exception $e) {
        error_log("Exception in createCategoria: " . $e->getMessage());
        sendError('Error interno: ' . $e->getMessage(), 500);
    }
}

/**
 * Update categoria
 */
function updateCategoria($conn) {
    $id = intval($_GET['id'] ?? 0);
    
    if (!$id) {
        sendError('ID de categoría no válido', 400);
    }
    
    $input = json_decode(file_get_contents('php://input'), true);
    
    if (!$input) {
        sendError('Datos inválidos', 400);
    }
    
    // Validate required fields
    $nombre = sanitizeString($input['nombre'] ?? '');
    $tipo_dashboard = sanitizeString($input['tipo_dashboard'] ?? '');
    
    if (empty($nombre)) {
        sendError('El nombre es requerido', 400);
    }
    
    if (empty($tipo_dashboard)) {
        sendError('El tipo de dashboard es requerido', 400);
    }
    
    // Validate tipo_dashboard values
    $valid_tipos = ['materiales', 'tics', 'infraestructura', 'general'];
    if (!in_array($tipo_dashboard, $valid_tipos)) {
        sendError('Tipo de dashboard inválido', 400);
    }
    
    // Optional fields
    $descripcion = sanitizeString($input['descripcion'] ?? '');
    $icono = sanitizeString($input['icono'] ?? '');
    $color = sanitizeString($input['color'] ?? '');
    $activo = isset($input['activo']) ? (int)$input['activo'] : 1;
    
    // Validate color format if provided
    if (!empty($color) && !preg_match('/^#[0-9A-F]{6}$/i', $color)) {
        sendError('Formato de color inválido', 400);
    }
    
    // Check if name already exists (excluding current record)
    $checkSql = "SELECT id FROM Categorias WHERE nombre = ? AND id != ?";
    $checkStmt = sqlsrv_query($conn, $checkSql, [$nombre, $id]);
    
    if ($checkStmt && sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC)) {
        sendError('Ya existe una categoría con ese nombre', 400);
    }
    
    // Update categoria
    $sql = "UPDATE Categorias 
            SET nombre = ?, tipo_dashboard = ?, descripcion = ?, icono = ?, color = ?, activo = ?
            WHERE id = ?";
    
    $params = [
        $nombre,
        $tipo_dashboard,
        !empty($descripcion) ? $descripcion : null,
        !empty($icono) ? $icono : null,
        !empty($color) ? $color : null,
        $activo,
        $id
    ];
    
    $stmt = sqlsrv_query($conn, $sql, $params);
    
    if (!$stmt) {
        sendError('Error al actualizar categoría: ' . print_r(sqlsrv_errors(), true), 500);
    }
    
    sendSuccess(['id' => $id], 'Categoría actualizada correctamente');
}

/**
 * Toggle categoria active status
 */
function toggleCategoria($conn) {
    $id = intval($_GET['id'] ?? 0);
    
    if (!$id) {
        sendError('ID de categoría no válido', 400);
    }
    
    $input = json_decode(file_get_contents('php://input'), true);
    $activo = isset($input['activo']) ? (int)$input['activo'] : 0;
    
    $sql = "UPDATE Categorias SET activo = ? WHERE id = ?";
    $stmt = sqlsrv_query($conn, $sql, [$activo, $id]);
    
    if (!$stmt) {
        sendError('Error al actualizar estado', 500);
    }
    
    sendSuccess(['id' => $id, 'activo' => $activo], 'Estado actualizado correctamente');
}

/**
 * Delete categoria (soft delete - mark as inactive)
 */
function deleteCategoria($conn, $id) {
    $id = intval($id);
    
    // Check if categoria has active reports
    $checkSql = "SELECT COUNT(*) as count 
                 FROM Reportes 
                 WHERE id_categoria = ? AND eliminado = 0";
    $checkStmt = sqlsrv_query($conn, $checkSql, [$id]);
    $row = sqlsrv_fetch_array($checkStmt, SQLSRV_FETCH_ASSOC);
    
    if ($row['count'] > 0) {
        sendError('No se puede eliminar la categoría porque tiene reportes activos asociados', 400);
    }
    
    // Soft delete - mark as inactive
    $sql = "UPDATE Categorias SET activo = 0 WHERE id = ?";
    $stmt = sqlsrv_query($conn, $sql, [$id]);
    
    if (!$stmt) {
        sendError('Error al eliminar categoría', 500);
    }
    
    sendSuccess(['id' => $id], 'Categoría desactivada correctamente');
}

/**
 * Send success response
 */
function sendSuccess($data, $message = '') {
    echo json_encode([
        'success' => true,
        'message' => $message,
        'data' => $data
    ]);
    exit;
}

/**
 * Send error response
 */
function sendError($message, $code = 400) {
    http_response_code($code);
    echo json_encode([
        'success' => false,
        'message' => $message
    ]);
    exit;
}
