<?php
/**
 * View Report Details
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/conexion.php';

requireLogin();

$usuario_id = getCurrentUserId();
$usuario = getCurrentUser();

// Get user role
$sqlRole = "SELECT rol FROM Usuarios WHERE correo = ?";
$stmtRole = sqlsrv_query($conn, $sqlRole, [$usuario['email']]);
$userRole = 'reportante';
if ($stmtRole && $rowRole = sqlsrv_fetch_array($stmtRole, SQLSRV_FETCH_ASSOC)) {
    $userRole = $rowRole['rol'];
}

$reporte_id = isset($_GET['id']) ? intval($_GET['id']) : 0;

if ($reporte_id <= 0) {
    header("Location: home.php");
    exit;
}

// Get report details using view for normalized data
if ($userRole === 'administrador') {
    $sql = "SELECT r.*
            FROM vw_Reportes_Completo r
            WHERE r.id = ?";
    $stmt = sqlsrv_query($conn, $sql, [$reporte_id]);
} else {
    $sql = "SELECT r.*
            FROM vw_Reportes_Completo r
            INNER JOIN Reportes rp ON r.id = rp.id
            WHERE r.id = ? AND rp.id_reportante = ?";
    $stmt = sqlsrv_query($conn, $sql, [$reporte_id, $usuario_id]);
}

if (!$stmt || !($reporte = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC))) {
    header("Location: home.php");
    exit;
}

// Get comments/actions history
$sqlComentarios = "SELECT c.*, u.nombre as usuario_nombre, u.rol as usuario_rol
                   FROM Comentarios c
                   JOIN Usuarios u ON c.id_usuario = u.id
                   WHERE c.id_reporte = ? AND c.publico = 1 AND c.eliminado = 0
                   ORDER BY c.fecha_comentario DESC";
$stmtComentarios = sqlsrv_query($conn, $sqlComentarios, [$reporte_id]);

$pageTitle = 'Detalle del Reporte';
include __DIR__ . '/includes/header.php';
?>

<div class="container my-4">
    <div class="row">
        <div class="col-md-12">
            <a href="home.php" class="btn btn-secondary mb-3">
                <i class="fas fa-arrow-left"></i> Volver
            </a>
        </div>
    </div>

    <?php displayAlerts(); ?>

    <div class="card shadow-lg">
        <div class="card-header" style="background-color: #003366; color: white;">
            <h4 class="mb-0">
                <i class="fas fa-file-alt"></i> Reporte #<?= htmlspecialchars($reporte['folio']) ?>
            </h4>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <h5><i class="fas fa-info-circle"></i> Información General</h5>
                    <table class="table table-bordered">
                        <tr>
                            <th width="40%">Folio:</th>
                            <td><?= htmlspecialchars($reporte['folio']) ?></td>
                        </tr>
                        <tr>
                            <th>Área:</th>
                            <td><?= htmlspecialchars($reporte['edificio'] . ' - ' . $reporte['salon']) ?></td>
                        </tr>
                        <tr>
                            <th>Categoría:</th>
                            <td><?= htmlspecialchars($reporte['categoria']) ?></td>
                        </tr>
                        <tr>
                            <th>Prioridad:</th>
                            <td>
                                <span class="badge bg-<?= $reporte['prioridad'] === 'Alta' ? 'danger' : ($reporte['prioridad'] === 'Media' ? 'warning' : 'info') ?>">
                                    <?= htmlspecialchars($reporte['prioridad']) ?>
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <th>Estatus:</th>
                            <td>
                                <span class="badge bg-<?= $reporte['estado'] === 'Resuelto' ? 'success' : ($reporte['estado'] === 'En Proceso' ? 'warning' : 'secondary') ?>">
                                    <?= htmlspecialchars($reporte['estado']) ?>
                                </span>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5><i class="fas fa-calendar"></i> Fechas</h5>
                    <table class="table table-bordered">
                        <tr>
                            <th width="40%">Fecha Reporte:</th>
                            <td><?= $reporte['fecha_reporte']->format('d/m/Y H:i') ?></td>
                        </tr>
                        <?php if ($reporte['fecha_asignacion']): ?>
                        <tr>
                            <th>Fecha Asignación:</th>
                            <td><?= $reporte['fecha_asignacion']->format('d/m/Y H:i') ?></td>
                        </tr>
                        <?php endif; ?>
                        <?php if (isset($reporte['fecha_finalizacion']) && $reporte['fecha_finalizacion']): ?>
                        <tr>
                            <th>Fecha Finalización:</th>
                            <td><?= $reporte['fecha_finalizacion']->format('d/m/Y H:i') ?></td>
                        </tr>
                        <?php endif; ?>
                        <?php if ($userRole === 'administrador'): ?>
                        <tr>
                            <th>Reportado por:</th>
                            <td><?= htmlspecialchars($reporte['reportante_nombre']) ?><br>
                                <small class="text-muted"><?= htmlspecialchars($reporte['reportante_correo']) ?></small>
                            </td>
                        </tr>
                        <?php endif; ?>
                    </table>
                </div>
            </div>

            <div class="row mt-3">
                <div class="col-md-12">
                    <h5><i class="fas fa-align-left"></i> Descripción del Problema</h5>
                    <div class="alert alert-light">
                        <?= nl2br(htmlspecialchars($reporte['descripcion'])) ?>
                    </div>
                </div>
            </div>

            <?php if (!empty($reporte['archivo'])): ?>
            <div class="row mt-3">
                <div class="col-md-12">
                    <h5><i class="fas fa-paperclip"></i> Archivo Adjunto</h5>
                    <a href="<?= htmlspecialchars($reporte['archivo']) ?>" target="_blank" class="btn btn-outline-primary">
                        <i class="fas fa-download"></i> Descargar archivo
                    </a>
                </div>
            </div>
            <?php endif; ?>

            <!-- Historial de acciones -->
            <div class="row mt-4">
                <div class="col-md-12">
                    <h5><i class="fas fa-history"></i> Historial de Acciones</h5>
                    <?php if ($stmtComentarios && sqlsrv_has_rows($stmtComentarios)): ?>
                        <?php 
                        sqlsrv_fetch($stmtComentarios); // Reset cursor
                        $stmtComentarios = sqlsrv_query($conn, $sqlComentarios, [$reporte_id]); // Re-execute
                        while ($comentario = sqlsrv_fetch_array($stmtComentarios, SQLSRV_FETCH_ASSOC)): 
                        ?>
                        <div class="card mb-2">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <strong>
                                        <i class="fas fa-user"></i> <?= htmlspecialchars($comentario['usuario_nombre']) ?>
                                        <?php if ($comentario['usuario_rol'] === 'administrador'): ?>
                                            <span class="badge bg-primary">Admin</span>
                                        <?php endif; ?>
                                    </strong>
                                    <small class="text-muted">
                                        <?= $comentario['fecha_comentario']->format('d/m/Y H:i') ?>
                                    </small>
                                </div>
                                <p class="mb-0 mt-2"><?= nl2br(htmlspecialchars($comentario['comentario'])) ?></p>
                            </div>
                        </div>
                        <?php endwhile; ?>
                    <?php else: ?>
                        <div class="alert alert-info">
                            <i class="fas fa-info-circle"></i> No hay acciones registradas aún.
                        </div>
                    <?php endif; ?>
                </div>
            </div>

            <div class="row mt-4">
                <div class="col-md-12">
                    <a href="exportar_reporte.php?id=<?= $reporte['id'] ?>" target="_blank" class="btn btn-success">
                        <i class="fas fa-file-pdf"></i> Exportar a PDF
                    </a>
                    <?php if ($userRole === 'administrador'): ?>
                    <a href="editar_reporte.php?id=<?= $reporte['id'] ?>" class="btn btn-warning">
                        <i class="fas fa-edit"></i> Actualizar Estado
                    </a>
                    <?php endif; ?>
                </div>
            </div>
        </div>
    </div>
</div>

<?php
include __DIR__ . '/includes/footer.php';
?>
