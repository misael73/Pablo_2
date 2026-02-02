<?php
/**
 * DEPRECATED: This file is no longer used
 * Report saving functionality has been moved to reportar.php
 * This file is kept for reference only
 */

session_start();
require 'conexion.php';

// Verificar sesión
if (!isset($_SESSION['usuario'])) {
    header("Location: login.php");
    exit;
}

// Verificar que los datos vengan del formulario
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Recibir los datos del formulario
    $area = $_POST['area'] ?? '';
    $tipo = $_POST['tipo'] ?? '';
    $descripcion = $_POST['descripcion'] ?? '';
    $prioridad = $_POST['prioridad'] ?? 'Media';

    // Datos del usuario que reporta
    $usuario = $_SESSION['usuario'];
    $correoUsuario = $usuario['email'];

    // Buscar el id del usuario en la tabla Usuarios (según su correo)
    $sqlUsuario = "SELECT id FROM Usuarios WHERE correo = ?";
    $stmtUsuario = sqlsrv_query($conn, $sqlUsuario, [$correoUsuario]);

    if ($stmtUsuario && $row = sqlsrv_fetch_array($stmtUsuario, SQLSRV_FETCH_ASSOC)) {
        $id_usuario = $row['id'];
    } else {
        // Si no existe, lo insertamos automáticamente
        $insertUser = "INSERT INTO Usuarios (nombre, correo, rol) VALUES (?, ?, 'Reportante')";
        sqlsrv_query($conn, $insertUser, [$usuario['name'], $correoUsuario]);

        // Obtener el ID recién insertado
        $sqlUltimo = "SELECT SCOPE_IDENTITY() AS id";
        $stmtUltimo = sqlsrv_query($conn, $sqlUltimo);
        $rowUltimo = sqlsrv_fetch_array($stmtUltimo, SQLSRV_FETCH_ASSOC);
        $id_usuario = $rowUltimo['id'];
    }

    // Generar un folio único (por ejemplo: REP-2025-xxxx)
    $folio = 'REP-' . date('Y') . '-' . str_pad(rand(0, 9999), 4, '0', STR_PAD_LEFT);

    // Insertar reporte en la base de datos
    $sql = "INSERT INTO Reportes (folio, id_usuario, area, tipo, descripcion, prioridad, estatus)
            VALUES (?, ?, ?, ?, ?, ?, ?)";

    $params = [$folio, $id_usuario, $area, $tipo, $descripcion, $prioridad, 'Pendiente'];

    $stmt = sqlsrv_query($conn, $sql, $params);

    if ($stmt) {
        echo "<script>alert('✅ Reporte registrado correctamente con folio: $folio'); window.location='home.php';</script>";
    } else {
        echo "<pre>";
        print_r(sqlsrv_errors());
        echo "</pre>";
    }
} else {
    echo "Método no permitido.";
}
?>
