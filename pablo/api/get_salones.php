<?php
/**
 * API: Get Salones by Edificio
 * Returns active salones for a specific building (used in form)
 */

require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/../includes/auth.php';
require_once __DIR__ . '/../conexion.php';

// Require user to be logged in
requireLogin();

// Set JSON header
header('Content-Type: application/json');

// Get edificio ID from query parameter
$edificio_id = isset($_GET['edificio_id']) ? intval($_GET['edificio_id']) : 0;

if ($edificio_id <= 0) {
    sendError('ID de edificio inválido');
}

try {
    // Get active salones for the specified edificio
    $sql = "SELECT id, nombre 
            FROM Salones 
            WHERE id_edificio = ? AND activo = 1
            ORDER BY nombre";
    
    $stmt = sqlsrv_query($conn, $sql, [$edificio_id]);
    
    if ($stmt === false) {
        sendError('Error al obtener salones');
    }
    
    $salones = [];
    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        $salones[] = [
            'id' => $row['id'],
            'nombre' => $row['nombre']
        ];
    }
    
    sendSuccess($salones);
    
} catch (Exception $e) {
    sendError('Error del servidor: ' . $e->getMessage());
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
function sendError($message) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => $message]);
    exit;
}
