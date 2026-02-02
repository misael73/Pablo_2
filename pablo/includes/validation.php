<?php
/**
 * Input Validation and Sanitization Functions
 */

/**
 * Sanitize string input
 */
function sanitizeString($input) {
    return htmlspecialchars(trim($input), ENT_QUOTES, 'UTF-8');
}

/**
 * Validate email
 */
function validateEmail($email) {
    return filter_var($email, FILTER_VALIDATE_EMAIL) !== false;
}

/**
 * Validate file upload
 * @param array $file $_FILES array element
 * @return array ['valid' => bool, 'error' => string]
 */
function validateFileUpload($file) {
    if (!isset($file['error']) || is_array($file['error'])) {
        return ['valid' => false, 'error' => 'Parámetros inválidos'];
    }
    
    // Check for upload errors
    switch ($file['error']) {
        case UPLOAD_ERR_OK:
            break;
        case UPLOAD_ERR_NO_FILE:
            return ['valid' => true, 'error' => '']; // No file uploaded is OK
        case UPLOAD_ERR_INI_SIZE:
        case UPLOAD_ERR_FORM_SIZE:
            return ['valid' => false, 'error' => 'El archivo excede el tamaño máximo permitido'];
        default:
            return ['valid' => false, 'error' => 'Error desconocido al subir el archivo'];
    }
    
    // Check file size
    if ($file['size'] > MAX_FILE_SIZE) {
        return ['valid' => false, 'error' => 'El archivo excede el tamaño máximo de 2MB'];
    }
    
    // Check file extension
    $ext = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
    if (!in_array($ext, ALLOWED_EXTENSIONS)) {
        return ['valid' => false, 'error' => 'Tipo de archivo no permitido. Use: ' . implode(', ', ALLOWED_EXTENSIONS)];
    }
    
    return ['valid' => true, 'error' => ''];
}

/**
 * Generate unique filename for upload
 */
function generateUniqueFilename($originalName) {
    $ext = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
    return uniqid('report_', true) . '.' . $ext;
}
?>
