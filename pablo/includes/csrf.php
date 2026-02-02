<?php
/**
 * CSRF Protection Functions
 */

/**
 * Generate a CSRF token
 */
function generateCSRFToken() {
    if (!isset($_SESSION['csrf_token'])) {
        $_SESSION['csrf_token'] = bin2hex(random_bytes(32));
    }
    return $_SESSION['csrf_token'];
}

/**
 * Get CSRF token (generates if doesn't exist)
 */
function getCSRFToken() {
    return generateCSRFToken();
}

/**
 * Validate CSRF token
 */
function validateCSRFToken($token) {
    if (!isset($_SESSION['csrf_token'])) {
        return false;
    }
    return hash_equals($_SESSION['csrf_token'], $token);
}

/**
 * Output CSRF token as hidden input field
 */
function csrfField() {
    $token = getCSRFToken();
    return '<input type="hidden" name="csrf_token" value="' . htmlspecialchars($token) . '">';
}

/**
 * Verify CSRF token from POST request
 * @throws Exception if token is invalid
 */
function verifyCSRFToken() {
    if (!isset($_POST['csrf_token']) || !validateCSRFToken($_POST['csrf_token'])) {
        throw new Exception('Token CSRF inválido');
    }
}

/**
 * Check if CSRF token is valid (used by API endpoints)
 * Returns boolean instead of throwing exception
 */
function checkCsrfToken($token) {
    return validateCSRFToken($token);
}
?>
