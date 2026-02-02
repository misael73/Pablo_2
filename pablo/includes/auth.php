<?php
/**
 * Authentication Helper Functions
 */

require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/db.php';

/**
 * Check if user is logged in
 */
function isLoggedIn() {
    return isset($_SESSION['usuario']) && isset($_SESSION['usuario_id']);
}

/**
 * Require user to be logged in, redirect to index.php if not
 */
function requireLogin() {
    if (!isLoggedIn()) {
        header("Location: index.php");
        exit;
    }
}

/**
 * Get user role from database
 */
function getUserRole() {
    if (!isLoggedIn()) {
        return null;
    }
    
    $usuario = getCurrentUser();
    $db = Database::getInstance();
    
    $sql = "SELECT rol FROM Usuarios WHERE correo = ?";
    $stmt = $db->query($sql, [$usuario['email']]);
    
    if ($stmt && $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        return $row['rol'];
    }
    
    return 'reportante'; // default
}

/**
 * Require admin role, redirect if not admin
 */
function requireAdmin() {
    requireLogin();
    $role = getUserRole();
    
    if ($role !== 'administrador') {
        header("Location: mis_reportes.php");
        exit;
    }
}

/**
 * Require admin or technician role for dashboards
 */
function requireAdminOrTechnician() {
    requireLogin();
    $role = getUserRole();
    
    if ($role !== 'administrador' && $role !== 'tecnico') {
        header("Location: mis_reportes.php");
        exit;
    }
}

/**
 * Get current user data
 */
function getCurrentUser() {
    return $_SESSION['usuario'] ?? null;
}

/**
 * Get current user ID
 */
function getCurrentUserId() {
    return $_SESSION['usuario_id'] ?? null;
}

/**
 * Set user session
 */
function setUserSession($nombre, $correo, $foto, $usuario_id) {
    $_SESSION['usuario'] = [
        'nombre' => $nombre,
        'email' => $correo,
        'foto' => $foto
    ];
    $_SESSION['usuario_id'] = $usuario_id;
}

/**
 * Clear user session (logout)
 */
function clearUserSession() {
    session_unset();
    session_destroy();
}
?>
