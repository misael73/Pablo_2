<?php
/**
 * Google OAuth Verification Handler
 * Verifies Google ID tokens and manages user sessions
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/user.php';
require_once __DIR__ . '/includes/validation.php';

header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'error' => 'Método no permitido']);
    exit;
}

if (empty($_POST['id_token'])) {
    echo json_encode(['success' => false, 'error' => 'Token vacío']);
    exit;
}

$id_token = $_POST['id_token'];
$url = 'https://oauth2.googleapis.com/tokeninfo?id_token=' . urlencode($id_token);
$response = @file_get_contents($url);

if ($response === false) {
    echo json_encode(['success' => false, 'error' => 'Error al verificar token con Google']);
    exit;
}

$data = json_decode($response, true);

if (!isset($data['aud']) || $data['aud'] !== GOOGLE_CLIENT_ID) {
    echo json_encode(['success' => false, 'error' => 'Token inválido']);
    exit;
}

try {
    $nombre = sanitizeString($data['name'] ?? 'Sin nombre');
    $correo = $data['email'] ?? '';
    $foto = $data['picture'] ?? '';
    
    // Validate email
    if (!validateEmail($correo)) {
        echo json_encode(['success' => false, 'error' => 'Email inválido']);
        exit;
    }
    
    // Validate email domain - only @cdserdan.tecnm.mx allowed for reportantes
    $emailDomain = substr(strrchr($correo, "@"), 1);
    if ($emailDomain !== 'cdserdan.tecnm.mx') {
        echo json_encode([
            'success' => false, 
            'error' => 'Solo usuarios con correo institucional @cdserdan.tecnm.mx pueden acceder al sistema'
        ]);
        exit;
    }
    
    // Get or create user in database
    $usuario_id = getOrCreateUser($nombre, $correo);
    
    if ($usuario_id === null) {
        echo json_encode(['success' => false, 'error' => 'Error al crear usuario']);
        exit;
    }
    
    // Set session
    setUserSession($nombre, $correo, $foto, $usuario_id);
    
    echo json_encode([
        'success' => true,
        'usuario' => [
            'nombre' => $nombre,
            'email' => $correo,
            'foto' => $foto
        ]
    ]);
    
} catch (Exception $e) {
    error_log("Error in verificar_google.php: " . $e->getMessage());
    echo json_encode(['success' => false, 'error' => 'Error interno del servidor']);
}
?>
