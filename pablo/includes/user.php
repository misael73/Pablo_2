<?php
/**
 * User Management Functions
 */

require_once __DIR__ . '/db.php';

/**
 * Find user by email
 * @param string $correo User email
 * @return array|null User data or null if not found
 */
function findUserByEmail($correo) {
    $db = Database::getInstance();
    $sql = "SELECT id, nombre, correo, rol FROM Usuarios WHERE correo = ?";
    $stmt = $db->query($sql, [$correo]);
    
    if ($stmt && $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        return $row;
    }
    return null;
}

/**
 * Create new user
 * @param string $nombre User name
 * @param string $correo User email
 * @param string $rol User role (default: 'reportante')
 * @return int|null User ID or null on failure
 */
function createUser($nombre, $correo, $rol = 'reportante') {
    $db = Database::getInstance();
    
    // Check if user already exists
    $existing = findUserByEmail($correo);
    if ($existing) {
        return $existing['id'];
    }
    
    // For OAuth users, we use a placeholder password since they login via Google
    $passwordPlaceholder = 'OAUTH_USER_' . bin2hex(random_bytes(16));
    
    $sql = "INSERT INTO Usuarios (nombre, correo, contrasena, rol) VALUES (?, ?, ?, ?)";
    $stmt = $db->query($sql, [$nombre, $correo, $passwordPlaceholder, $rol]);
    
    if ($stmt) {
        // Get the newly created user ID
        $user = findUserByEmail($correo);
        return $user ? $user['id'] : null;
    }
    
    return null;
}

/**
 * Get or create user by email (for OAuth login)
 * @param string $nombre User name
 * @param string $correo User email
 * @return int|null User ID or null on failure
 */
function getOrCreateUser($nombre, $correo) {
    $user = findUserByEmail($correo);
    if ($user) {
        return $user['id'];
    }
    return createUser($nombre, $correo);
}
?>
