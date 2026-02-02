<?php
/**
 * My Reports - Student View
 * Students can only see their own reports
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/conexion.php';

// Require user to be logged in
requireLogin();

$usuario = getCurrentUser();
$usuario_id = getCurrentUserId();

// Get user role - if admin, redirect to dashboard
$userRole = getUserRole();
if ($userRole === 'administrador') {
    header("Location: home.php");
    exit;
}

// Regular user sees only their reports - use view for normalized data
$sql = "SELECT r.*, 
        (SELECT TOP 1 comentario 
         FROM Comentarios 
         WHERE id_reporte = r.id AND publico = 1 AND eliminado = 0
         ORDER BY fecha_comentario DESC) AS detalle_accion
        FROM vw_Reportes_Completo r
        INNER JOIN Reportes rp ON r.id = rp.id
        WHERE rp.id_reportante = ? AND rp.eliminado = 0
        ORDER BY r.fecha_reporte DESC";

$stmt = sqlsrv_query($conn, $sql, [$usuario_id]);

if ($stmt === false) {
    die(print_r(sqlsrv_errors(), true));
}

// Get statistics for this user using new schema
$statsSQL = "SELECT 
    COUNT(*) as total,
    SUM(CASE WHEN est.nombre = 'En Proceso' THEN 1 ELSE 0 END) as en_proceso,
    SUM(CASE WHEN est.nombre = 'Resuelto' THEN 1 ELSE 0 END) as resueltos,
    SUM(CASE WHEN est.nombre = 'Recibido' THEN 1 ELSE 0 END) as pendientes,
    SUM(CASE WHEN CAST(r.fecha_reporte AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) as hoy
    FROM Reportes r
    INNER JOIN Estados est ON r.id_estado = est.id
    WHERE r.id_reportante = ? AND r.eliminado = 0";

$statsStmt = sqlsrv_query($conn, $statsSQL, [$usuario_id]);

$stats = ['total' => 0, 'en_proceso' => 0, 'resueltos' => 0, 'pendientes' => 0, 'hoy' => 0];
if ($statsStmt && $statsRow = sqlsrv_fetch_array($statsStmt, SQLSRV_FETCH_ASSOC)) {
    $stats = array_merge($stats, $statsRow);
}

$pageTitle = 'Mis Reportes';
$additionalStyles = <<<HTML
    <style>
        body {
            background-color: #f5f6fa;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .container {
            margin-top: 20px;
            max-width: 1300px;
        }

        .card {
            border-radius: 15px;
            border: none;
            box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
        }

        .stat-value {
            font-size: 28px;
            font-weight: bold;
        }

        .section-title {
            font-weight: bold;
            color: #333;
            margin-bottom: 15px;
        }

        .chart-container {
            background-color: white;
            border-radius: 15px;
            padding: 25px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            margin-bottom: 25px;
        }

        .table th {
            background-color: #6c63ff;
            color: white;
            text-align: center;
        }

        .table td {
            text-align: center;
        }

        .status-green {
            background-color: #00c851 !important;
            color: white;
            font-weight: bold;
        }

        .status-yellow {
            background-color: #ffeb3b !important;
            color: black;
            font-weight: bold;
        }

        .status-red {
            background-color: #ff4444 !important;
            color: white;
            font-weight: bold;
        }

        .description-cell {
            text-align: left;
        }
    </style>
HTML;

include __DIR__ . '/includes/header.php';
?>

<div class="container">
    <?php displayAlerts(); ?>
    
    <h2 class="fw-bold mb-4 text-center">Mis Reportes</h2>

    <!-- Tarjetas resumen -->
    <div class="row g-4 mb-4">
        <div class="col-md-3">
            <div class="card p-4 text-center">
                <h5>Total Reportes</h5>
                <p class="stat-value text-primary"><?= $stats['total'] ?></p>
                <p class="text-muted">Hoy: <?= $stats['hoy'] ?></p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card p-4 text-center">
                <h5>En proceso</h5>
                <p class="stat-value text-info"><?= $stats['en_proceso'] ?></p>
                <p class="text-muted">Atendiendo</p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card p-4 text-center">
                <h5>Resueltos</h5>
                <p class="stat-value text-success"><?= $stats['resueltos'] ?></p>
                <p class="text-muted"><?= $stats['total'] > 0 ? round(($stats['resueltos'] / $stats['total']) * 100, 1) : 0 ?>% completado</p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card p-4 text-center">
                <h5>Pendientes</h5>
                <p class="stat-value text-warning"><?= $stats['pendientes'] ?></p>
                <p class="text-muted">Sin atender</p>
            </div>
        </div>
    </div>

    <!-- Tabla de reportes -->
    <div class="chart-container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="section-title mb-0">Mis Reportes</h4>
            <a href="formulario.php" class="btn btn-primary">
                <i class="fas fa-plus-circle"></i> Nuevo Reporte
            </a>
        </div>
        <div class="table-responsive">
            <table class="table table-bordered table-striped align-middle">
                <thead>
                    <tr>
                        <th style="text-align: center;">Folio</th>
                        <th>Descripción</th>
                        <th style="text-align: center;">Área</th>
                        <th style="text-align: center;">Tipo</th>
                        <th style="text-align: center;">Estatus</th>
                        <th>Última Acción</th>
                        <th style="text-align: center;">Fecha</th>
                        <th style="text-align: center;">Acciones</th>
                    </tr>
                </thead>

                <tbody>
                    <?php 
                    $hasReports = false;
                    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)): 
                        $hasReports = true;
                    ?>
                        <tr>
                            <td style="text-align: center;"><?= htmlspecialchars($row['folio']) ?></td>
                            <td class="description-cell"><?= htmlspecialchars($row['titulo'] ?? $row['descripcion']) ?></td>
                            <td style="text-align: center;"><?= htmlspecialchars($row['edificio'] . ' - ' . $row['salon']) ?></td>
                            <td style="text-align: center;"><?= htmlspecialchars($row['categoria']) ?></td>

                            <?php
                            $clase = '';
                            if ($row['estado'] === 'Resuelto') $clase = 'status-green';
                            elseif ($row['estado'] === 'En Proceso') $clase = 'status-yellow';
                            elseif ($row['estado'] === 'Recibido') $clase = 'status-red';
                            ?>

                            <td class="<?= $clase ?>" style="text-align: center;"><?= htmlspecialchars($row['estado']) ?></td>
                            <td><?= htmlspecialchars($row['detalle_accion'] ?? 'Sin acciones registradas') ?></td>
                            <td style="text-align: center;">
                                <?php 
                                if ($row['fecha_reporte'] instanceof DateTime) {
                                    echo $row['fecha_reporte']->format('d/m/Y H:i');
                                } else {
                                    echo 'N/A';
                                }
                                ?>
                            </td>
                            <td style="text-align: center;">
                                <button class="btn btn-info btn-sm" onclick="verDetalle(<?= $row['id'] ?>)" title="Ver detalle">
                                    <i class="fas fa-eye"></i> Ver
                                </button>
                                <button class="btn btn-success btn-sm" onclick="exportarPDF(<?= $row['id'] ?>)" title="Exportar PDF">
                                    <i class="fas fa-file-pdf"></i>
                                </button>
                            </td>
                        </tr>
                    <?php endwhile; ?>
                    <?php if (!$hasReports): ?>
                        <tr>
                            <td colspan="8" style="text-align: center; padding: 40px;">
                                <i class="fas fa-inbox" style="font-size: 48px; color: #ccc;"></i>
                                <p class="text-muted mt-3">No tienes reportes registrados</p>
                                <a href="formulario.php" class="btn btn-primary mt-2">
                                    <i class="fas fa-plus-circle"></i> Crear primer reporte
                                </a>
                            </td>
                        </tr>
                    <?php endif; ?>
                </tbody>
            </table>
        </div>
    </div>
</div>

<script>
    // View report detail
    function verDetalle(reporteId) {
        window.location.href = 'ver_reporte.php?id=' + reporteId;
    }

    // Export report to PDF
    function exportarPDF(reporteId) {
        window.open('exportar_reporte.php?id=' + reporteId, '_blank');
    }
</script>

<?php
$additionalScripts = '';
include __DIR__ . '/includes/footer.php';
?>
