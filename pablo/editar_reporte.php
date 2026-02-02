<?php
/**
 * Edit Report (Admin Only)
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/includes/csrf.php';
require_once __DIR__ . '/includes/validation.php';
require_once __DIR__ . '/conexion.php';

// Require admin access
requireAdmin();

$usuario_id = getCurrentUserId();
$usuario = getCurrentUser();

$reporte_id = isset($_GET['id']) ? intval($_GET['id']) : 0;

if ($reporte_id <= 0) {
    header("Location: home.php");
    exit;
}

// Handle form submission
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        verifyCSRFToken();
        
        $estado_nombre = sanitizeString($_POST['estatus'] ?? '');
        $comentario = sanitizeString($_POST['comentario'] ?? '');
        $prioridad_nombre = sanitizeString($_POST['prioridad'] ?? '');
        
        if (empty($estado_nombre)) {
            handleError('El estatus es requerido', null, 'editar_reporte.php?id=' . $reporte_id);
        }
        
        // Get OLD state before updating
        $getOldStateSQL = "SELECT id_estado FROM Reportes WHERE id = ?";
        $oldStateStmt = sqlsrv_query($conn, $getOldStateSQL, [$reporte_id]);
        $old_estado_id = null;
        if ($oldStateStmt && $oldStateRow = sqlsrv_fetch_array($oldStateStmt, SQLSRV_FETCH_ASSOC)) {
            $old_estado_id = $oldStateRow['id_estado'];
        }
        
        // Get estado ID
        $estadoSQL = "SELECT id FROM Estados WHERE nombre = ?";
        $estadoStmt = sqlsrv_query($conn, $estadoSQL, [$estado_nombre]);
        $estado_id = null;
        if ($estadoStmt && $estadoRow = sqlsrv_fetch_array($estadoStmt, SQLSRV_FETCH_ASSOC)) {
            $estado_id = $estadoRow['id'];
        }
        
        // Get prioridad ID
        $prioridadSQL = "SELECT id FROM Prioridades WHERE nombre = ?";
        $prioridadStmt = sqlsrv_query($conn, $prioridadSQL, [$prioridad_nombre]);
        $prioridad_id = null;
        if ($prioridadStmt && $prioridadRow = sqlsrv_fetch_array($prioridadStmt, SQLSRV_FETCH_ASSOC)) {
            $prioridad_id = $prioridadRow['id'];
        }
        
        if (!$estado_id || !$prioridad_id) {
            handleError('Estado o prioridad inválidos', null, 'editar_reporte.php?id=' . $reporte_id);
        }
        
        // Update report status using new schema
        $updateSQL = "UPDATE Reportes SET id_estado = ?, id_prioridad = ?, actualizado_por = ?, fecha_actualizacion = GETDATE()";
        $params = [$estado_id, $prioridad_id, $usuario_id];
        
        if ($estado_nombre === 'En Proceso' && empty($_POST['skip_fecha_asignacion'])) {
            $updateSQL .= ", fecha_asignacion = GETDATE()";
        }
        
        if ($estado_nombre === 'Resuelto') {
            $updateSQL .= ", fecha_finalizacion = GETDATE()";
        }
        
        $updateSQL .= " WHERE id = ?";
        $params[] = $reporte_id;
        
        $updateStmt = sqlsrv_query($conn, $updateSQL, $params);
        
        if (!$updateStmt) {
            handleError('Error al actualizar el reporte', print_r(sqlsrv_errors(), true), 'editar_reporte.php?id=' . $reporte_id);
        }
        
        // Add comment if provided
        if (!empty($comentario)) {
            $commentSQL = "INSERT INTO Comentarios (id_reporte, id_usuario, comentario, publico) VALUES (?, ?, ?, 1)";
            $commentStmt = sqlsrv_query($conn, $commentSQL, [$reporte_id, $usuario_id, $comentario]);
            
            if (!$commentStmt) {
                error_log("Error adding comment: " . print_r(sqlsrv_errors(), true));
            }
        }
        
        // Record status change in history (using old state fetched before update)
        $historySQL = "INSERT INTO HistorialEstados (id_reporte, id_estado_anterior, id_estado_nuevo, id_usuario) 
                       VALUES (?, ?, ?, ?)";
        sqlsrv_query($conn, $historySQL, [$reporte_id, $old_estado_id, $estado_id, $usuario_id]);
        
        handleSuccess('Reporte actualizado correctamente', 'ver_reporte.php?id=' . $reporte_id);
        
    } catch (Exception $e) {
        handleError('Error al procesar la solicitud', $e->getMessage(), 'editar_reporte.php?id=' . $reporte_id);
    }
}

// Get report details using view
$sql = "SELECT r.*
        FROM vw_Reportes_Completo r
        WHERE r.id = ?";
$stmt = sqlsrv_query($conn, $sql, [$reporte_id]);

if (!$stmt || !($reporte = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC))) {
    header("Location: home.php");
    exit;
}

$pageTitle = 'Editar Reporte';
include __DIR__ . '/includes/header.php';
?>

<div class="container my-4">
    <div class="row">
        <div class="col-md-12">
            <a href="ver_reporte.php?id=<?= $reporte_id ?>" class="btn btn-secondary mb-3">
                <i class="fas fa-arrow-left"></i> Volver
            </a>
        </div>
    </div>

    <?php displayAlerts(); ?>

    <div class="card shadow-lg">
        <div class="card-header" style="background-color: #003366; color: white;">
            <h4 class="mb-0">
                <i class="fas fa-edit"></i> Actualizar Reporte #<?= htmlspecialchars($reporte['folio']) ?>
            </h4>
        </div>
        <div class="card-body">
            <!-- Report summary -->
            <div class="alert alert-light">
                <div class="row">
                    <div class="col-md-6">
                        <strong>Área:</strong> <?= htmlspecialchars($reporte['edificio'] . ' - ' . $reporte['salon']) ?><br>
                        <strong>Categoría:</strong> <?= htmlspecialchars($reporte['categoria']) ?>
                    </div>
                    <div class="col-md-6">
                        <strong>Reportado por:</strong> <?= htmlspecialchars($reporte['reportante_nombre']) ?><br>
                        <strong>Fecha:</strong> <?= $reporte['fecha_reporte']->format('d/m/Y H:i') ?>
                    </div>
                </div>
                <div class="mt-2">
                    <strong>Descripción:</strong><br>
                    <?= nl2br(htmlspecialchars($reporte['descripcion'])) ?>
                </div>
            </div>

            <!-- Update form -->
            <form method="POST" action="">
                <?php echo csrfField(); ?>
                
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="estatus" class="form-label">Estatus *</label>
                            <select name="estatus" id="estatus" class="form-select" required>
                                <option value="Recibido" <?= $reporte['estado'] === 'Recibido' ? 'selected' : '' ?>>Recibido</option>
                                <option value="En Proceso" <?= $reporte['estado'] === 'En Proceso' ? 'selected' : '' ?>>En Proceso</option>
                                <option value="Resuelto" <?= $reporte['estado'] === 'Resuelto' ? 'selected' : '' ?>>Resuelto</option>
                                <option value="Cancelado" <?= $reporte['estado'] === 'Cancelado' ? 'selected' : '' ?>>Cancelado</option>
                            </select>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="prioridad" class="form-label">Prioridad *</label>
                            <select name="prioridad" id="prioridad" class="form-select" required>
                                <option value="Baja" <?= $reporte['prioridad'] === 'Baja' ? 'selected' : '' ?>>Baja</option>
                                <option value="Media" <?= $reporte['prioridad'] === 'Media' ? 'selected' : '' ?>>Media</option>
                                <option value="Alta" <?= $reporte['prioridad'] === 'Alta' ? 'selected' : '' ?>>Alta</option>
                            </select>
                        </div>
                    </div>
                </div>

                <div class="mb-3">
                    <label for="comentario" class="form-label">
                        Agregar Comentario/Acción Realizada *
                        <small class="text-muted">(Este comentario será visible para el usuario)</small>
                    </label>
                    <textarea name="comentario" id="comentario" class="form-control" rows="4" required 
                              placeholder="Ejemplo: Se revisó el equipo y se reemplazó el cable de red defectuoso. El problema ha sido solucionado."></textarea>
                    <small class="form-text text-muted">
                        Describe la acción tomada, el diagnóstico o cualquier información relevante sobre el reporte.
                    </small>
                </div>

                <?php if ($reporte['fecha_asignacion']): ?>
                <input type="hidden" name="skip_fecha_asignacion" value="1">
                <?php endif; ?>

                <div class="d-grid gap-2 d-md-flex justify-content-md-end">
                    <a href="ver_reporte.php?id=<?= $reporte_id ?>" class="btn btn-secondary">
                        <i class="fas fa-times"></i> Cancelar
                    </a>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-save"></i> Guardar Cambios
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>

<?php
include __DIR__ . '/includes/footer.php';
?>
