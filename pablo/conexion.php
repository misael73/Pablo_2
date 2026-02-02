<?php
/**
 * Database Connection - Legacy Support
 * This file is maintained for backwards compatibility
 * New code should use includes/db.php instead
 */

require_once __DIR__ . '/includes/db.php';

try {
    $db = Database::getInstance();
    $conn = $db->getConnection();
} catch (Exception $e) {
    die("Error al conectar con la base de datos: " . $e->getMessage());
}
?>

