<?php
/**
 * Login Page - SIREFI
 * Redirect logged-in users to appropriate page based on role
 */

require_once __DIR__ . '/config.php';

// Check if user is already logged in
if (isset($_SESSION['usuario']) && isset($_SESSION['usuario_id'])) {
    require_once __DIR__ . '/includes/auth.php';
    $role = getUserRole();
    
    if ($role === 'administrador') {
        header("Location: home.php");
    } else {
        header("Location: mis_reportes.php");
    }
    exit;
}
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Inicio de Sesión - SIREFI</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="style.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/7.0.1/css/all.min.css">

    <!-- SDK de Google Identity -->
    <script src="https://accounts.google.com/gsi/client" async defer></script>

    <style>
        /* Centrar el botón de Google y dar margen */
        .g_id_signin {
            display: flex;
            justify-content: center;
            margin: 15px 0;
        }

        form#loginForm {
            max-width: 400px;
            margin: 50px auto;
            padding: 30px;
            border-radius: 10px;
            background-color: #f8f9fa;
            box-shadow: 0px 4px 10px rgba(145, 130, 130, 0.1);
        }

        form#loginForm h1 {
            text-align: center;
            margin-bottom: 20px;
        }

        form#loginForm hr {
            margin: 20px 0;
        }

        form#loginForm a {
            display: block;
            text-align: center;
            margin-top: 10px;
        }
    </style>
</head>

<body>
    <form id="loginForm">
        <h1>INICIAR SESIÓN</h1>
        <hr>

        <!-- Contenedor del botón de Google -->
        <?php require_once __DIR__ . '/config.php'; ?>
        <div id="g_id_onload"
            data-client_id="<?php echo GOOGLE_CLIENT_ID; ?>"
            data-context="signin"
            data-ux_mode="popup"
            data-callback="handleCredentialResponse"
            data-auto_prompt="false">
        </div>

        <div class="g_id_signin"
            data-type="standard"
            data-shape="rectangular"
            data-theme="filled_blue"
            data-text="signin_with"
            data-size="large"
            data-logo_alignment="left">
        </div>

        <hr>
        <a href="olvidaste-tu-contraseña.php">¿Olvidaste tu contraseña?</a>
    </form>

    <script>
        // Callback que se ejecuta cuando el usuario inicia sesión con Google
        function handleCredentialResponse(response) {
            console.log("Token JWT recibido:", response.credential);

            // Enviar el token al servidor para validación
            fetch('verificar_google.php', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: 'id_token=' + response.credential
            })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    console.log('Usuario autenticado:', data.usuario);
                    // Redirect to appropriate page - mis_reportes for students
                    window.location.href = 'mis_reportes.php';
                } else {
                    alert(data.error || 'Error al iniciar sesión con Google');
                }
            })
            .catch(err => {
                console.error('Error de conexión:', err);
                alert('Error al conectar con el servidor');
            });
        }
    </script>
</body>
</html>
