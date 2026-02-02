<?php
/**
 * Logout Handler
 * Clears user session and redirects to login page
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';

// Clear session
clearUserSession();

// Redirect to login page
header("Location: index.php");
exit;
?>
