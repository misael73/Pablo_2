#!/usr/bin/env php
<?php
/**
 * SIREFI - Requirements Checker
 * Verifies that all required PHP extensions and configurations are present
 */

echo "===========================================\n";
echo "   SIREFI - Verificador de Requisitos\n";
echo "===========================================\n\n";

$errors = [];
$warnings = [];
$success = [];

// Check PHP version
echo "Verificando versión de PHP...\n";
$phpVersion = PHP_VERSION;
$requiredVersion = '7.4.0';
if (version_compare($phpVersion, $requiredVersion, '>=')) {
    $success[] = "✓ PHP $phpVersion (>= $requiredVersion requerido)";
} else {
    $errors[] = "✗ PHP $phpVersion - Se requiere PHP >= $requiredVersion";
}

// Check sqlsrv extension
echo "Verificando extensión sqlsrv...\n";
if (extension_loaded('sqlsrv')) {
    $success[] = "✓ Extensión sqlsrv instalada";
} else {
    $errors[] = "✗ Extensión sqlsrv NO instalada (REQUERIDA)";
}

// Check pdo_sqlsrv extension
echo "Verificando extensión pdo_sqlsrv...\n";
if (extension_loaded('pdo_sqlsrv')) {
    $success[] = "✓ Extensión pdo_sqlsrv instalada";
} else {
    $warnings[] = "⚠ Extensión pdo_sqlsrv NO instalada (recomendada)";
}

// Check file permissions
echo "Verificando permisos de archivos...\n";
$uploadsDir = __DIR__ . '/uploads';
if (file_exists($uploadsDir)) {
    if (is_writable($uploadsDir)) {
        $success[] = "✓ Directorio uploads/ tiene permisos de escritura";
    } else {
        $errors[] = "✗ Directorio uploads/ NO tiene permisos de escritura";
    }
} else {
    $warnings[] = "⚠ Directorio uploads/ no existe (se creará automáticamente)";
}

// Check config file
echo "Verificando archivo de configuración...\n";
$configFile = __DIR__ . '/config.php';
if (file_exists($configFile)) {
    $success[] = "✓ Archivo config.php existe";
    
    // Try to include and check constants (config.php won't start session in CLI mode)
    require_once $configFile;
    if (defined('DB_SERVER') && defined('DB_NAME') && defined('DB_USER') && defined('DB_PASS')) {
        $success[] = "✓ Configuración de base de datos definida";
    } else {
        $warnings[] = "⚠ Configuración de base de datos incompleta";
    }
    
    if (defined('GOOGLE_CLIENT_ID')) {
        $success[] = "✓ Google OAuth Client ID configurado";
    } else {
        $warnings[] = "⚠ Google OAuth Client ID no configurado";
    }
} else {
    $errors[] = "✗ Archivo config.php no encontrado";
}

// Check session support
echo "Verificando soporte de sesiones...\n";
if (function_exists('session_start')) {
    $success[] = "✓ Soporte de sesiones disponible";
} else {
    $errors[] = "✗ Soporte de sesiones NO disponible";
}

// Check JSON support
echo "Verificando soporte JSON...\n";
if (function_exists('json_encode') && function_exists('json_decode')) {
    $success[] = "✓ Soporte JSON disponible";
} else {
    $errors[] = "✗ Soporte JSON NO disponible";
}

// Print results
echo "\n===========================================\n";
echo "            RESULTADOS\n";
echo "===========================================\n\n";

if (!empty($success)) {
    echo "✓ CORRECTO:\n";
    foreach ($success as $msg) {
        echo "  $msg\n";
    }
    echo "\n";
}

if (!empty($warnings)) {
    echo "⚠ ADVERTENCIAS:\n";
    foreach ($warnings as $msg) {
        echo "  $msg\n";
    }
    echo "\n";
}

if (!empty($errors)) {
    echo "✗ ERRORES:\n";
    foreach ($errors as $msg) {
        echo "  $msg\n";
    }
    echo "\n";
}

// Final verdict
echo "===========================================\n";
if (empty($errors)) {
    echo "✓ Sistema listo para ejecutar!\n";
    if (!empty($warnings)) {
        echo "  (con algunas advertencias menores)\n";
    }
    exit(0);
} else {
    echo "✗ Se encontraron " . count($errors) . " error(es) crítico(s).\n";
    echo "  Por favor, corrija los errores antes de ejecutar el sistema.\n";
    echo "\n";
    echo "Para instalar la extensión sqlsrv, consulte:\n";
    echo "  README.md - Sección 'Instalación de la Extensión sqlsrv'\n";
    echo "  o visite: https://docs.microsoft.com/en-us/sql/connect/php/\n";
    exit(1);
}
?>
