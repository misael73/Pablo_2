<?php
/**
 * Error Handling Functions
 */

/**
 * Display user-friendly error message and log technical details
 */
function handleError($userMessage, $technicalDetails = null, $redirectUrl = null) {
    // Log technical details
    if ($technicalDetails) {
        error_log("SIREFI Error: " . $technicalDetails);
    }
    
    // Store error in session to display on next page
    $_SESSION['error_message'] = $userMessage;
    
    // Redirect if URL provided
    if ($redirectUrl) {
        header("Location: $redirectUrl");
        exit;
    }
}

/**
 * Display success message
 */
function handleSuccess($message, $redirectUrl = null) {
    $_SESSION['success_message'] = $message;
    
    if ($redirectUrl) {
        header("Location: $redirectUrl");
        exit;
    }
}

/**
 * Get and clear error message from session
 */
function getErrorMessage() {
    if (isset($_SESSION['error_message'])) {
        $message = $_SESSION['error_message'];
        unset($_SESSION['error_message']);
        return $message;
    }
    return null;
}

/**
 * Get and clear success message from session
 */
function getSuccessMessage() {
    if (isset($_SESSION['success_message'])) {
        $message = $_SESSION['success_message'];
        unset($_SESSION['success_message']);
        return $message;
    }
    return null;
}

/**
 * Display alert messages (call this in your HTML)
 */
function displayAlerts() {
    $error = getErrorMessage();
    $success = getSuccessMessage();
    
    if ($error) {
        echo '<div class="alert alert-danger alert-dismissible fade show" role="alert">';
        echo '<i class="fas fa-exclamation-circle"></i> ' . htmlspecialchars($error);
        echo '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';
        echo '</div>';
    }
    
    if ($success) {
        echo '<div class="alert alert-success alert-dismissible fade show" role="alert">';
        echo '<i class="fas fa-check-circle"></i> ' . htmlspecialchars($success);
        echo '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';
        echo '</div>';
    }
}
?>
