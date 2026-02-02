<?php
/**
 * SIREFI Configuration File
 * Contains all system configuration and constants
 */

// Error reporting (set to 0 in production)
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Database Configuration
define('DB_SERVER', 'localhost');
define('DB_NAME', 'SIREFI');
define('DB_USER', 'sa');
define('DB_PASS', 'Alucard12#');

// Google OAuth Configuration
define('GOOGLE_CLIENT_ID', '76807928556-odta2gs029semalkn0osp9cbdt7t274k.apps.googleusercontent.com');

// File Upload Configuration
define('UPLOAD_DIR', __DIR__ . '/uploads/');
define('MAX_FILE_SIZE', 2097152); // 2MB in bytes
define('ALLOWED_EXTENSIONS', ['jpg', 'jpeg', 'png', 'pdf']);

// Session Configuration (only for web requests, not CLI)
if (session_status() === PHP_SESSION_NONE && PHP_SAPI !== 'cli') {
    session_start();
}

// System Constants
define('SITE_NAME', 'SIREFI - Sistema de Reporte de Fallas e Incidencias');
define('SITE_URL', 'http://localhost/');

// Priority levels
define('PRIORITY_HIGH', 'Alta');
define('PRIORITY_MEDIUM', 'Media');
define('PRIORITY_LOW', 'Baja');

// Status levels
define('STATUS_RECEIVED', 'Recibido');
define('STATUS_IN_PROGRESS', 'En proceso');
define('STATUS_RESOLVED', 'Solucionado');
define('STATUS_CANCELLED', 'Cancelado');
?>
