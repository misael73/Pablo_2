<?php
/**
 * Export Report to PDF
 * Generates a maintenance request document in PDF format
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/conexion.php';

requireLogin();

$usuario_id = getCurrentUserId();
$usuario = getCurrentUser();

// Get user role
$userRole = getUserRole();

$reporte_id = isset($_GET['id']) ? intval($_GET['id']) : 0;

if ($reporte_id <= 0) {
    header("Location: " . ($userRole === 'administrador' ? 'home.php' : 'mis_reportes.php'));
    exit;
}

// Get report details with assigned technician info using view
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
    header("Location: " . ($userRole === 'administrador' ? 'home.php' : 'mis_reportes.php'));
    exit;
}

// Get latest comment/action
$sqlComentario = "SELECT TOP 1 comentario, fecha_comentario
                  FROM Comentarios
                  WHERE id_reporte = ? AND publico = 1 AND eliminado = 0
                  ORDER BY fecha_comentario DESC";
$stmtComentario = sqlsrv_query($conn, $sqlComentario, [$reporte_id]);
$ultimaAccion = '';
$fechaAccion = '';
if ($stmtComentario && $comentarioRow = sqlsrv_fetch_array($stmtComentario, SQLSRV_FETCH_ASSOC)) {
    $ultimaAccion = $comentarioRow['comentario'];
    $fechaAccion = $comentarioRow['fecha_comentario']->format('d/m/Y H:i');
}

// Set headers for PDF download
header('Content-Type: text/html; charset=utf-8');

// Generate HTML that can be printed as PDF
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Solicitud de Mantenimiento - <?= htmlspecialchars($reporte['folio']) ?></title>
    <style>
        @page {
            size: letter;
            margin: 2cm;
        }
        
        body {
            font-family: Arial, sans-serif;
            font-size: 11pt;
            line-height: 1.4;
            color: #000;
            margin: 0;
            padding: 20px;
        }
        
        .header {
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 3px solid #003366;
            padding-bottom: 15px;
        }
        
        .header h1 {
            margin: 0;
            color: #003366;
            font-size: 20pt;
            text-transform: uppercase;
        }
        
        .header h2 {
            margin: 5px 0 0 0;
            color: #666;
            font-size: 14pt;
            font-weight: normal;
        }
        
        .logo-section {
            text-align: center;
            margin-bottom: 20px;
        }
        
        .info-section {
            margin-bottom: 20px;
        }
        
        .info-row {
            display: table;
            width: 100%;
            margin-bottom: 10px;
            page-break-inside: avoid;
        }
        
        .info-label {
            display: table-cell;
            width: 30%;
            font-weight: bold;
            padding: 8px;
            background-color: #f0f0f0;
            border: 1px solid #ccc;
            vertical-align: top;
        }
        
        .info-value {
            display: table-cell;
            width: 70%;
            padding: 8px;
            border: 1px solid #ccc;
            border-left: none;
            vertical-align: top;
        }
        
        .section-title {
            background-color: #003366;
            color: white;
            padding: 10px;
            margin-top: 20px;
            margin-bottom: 10px;
            font-weight: bold;
            font-size: 12pt;
        }
        
        .description-box {
            border: 1px solid #ccc;
            padding: 15px;
            min-height: 100px;
            background-color: #fafafa;
            white-space: pre-wrap;
        }
        
        .signature-section {
            margin-top: 40px;
            page-break-inside: avoid;
        }
        
        .signature-box {
            display: inline-block;
            width: 45%;
            text-align: center;
            margin-top: 60px;
        }
        
        .signature-line {
            border-top: 2px solid #000;
            margin-top: 5px;
            padding-top: 5px;
        }
        
        .footer {
            margin-top: 30px;
            text-align: center;
            font-size: 9pt;
            color: #666;
            border-top: 1px solid #ccc;
            padding-top: 10px;
        }
        
        .status-badge {
            display: inline-block;
            padding: 5px 15px;
            border-radius: 5px;
            font-weight: bold;
        }
        
        .status-recibido { background-color: #ff4444; color: white; }
        .status-proceso { background-color: #ffeb3b; color: black; }
        .status-solucionado { background-color: #00c851; color: white; }
        .status-cancelado { background-color: #999; color: white; }
        
        .priority-alta { color: #ff0000; font-weight: bold; }
        .priority-media { color: #ff9800; font-weight: bold; }
        .priority-baja { color: #4caf50; font-weight: bold; }
        
        @media print {
            body { padding: 0; }
            .no-print { display: none; }
        }
    </style>
</head>
<body>
    <!-- Print Button -->
    <div class="no-print" style="text-align: right; margin-bottom: 20px;">
        <button onclick="window.print()" style="padding: 10px 20px; background-color: #003366; color: white; border: none; cursor: pointer; border-radius: 5px;">
            <strong>🖨️ Imprimir / Guardar como PDF</strong>
        </button>
        <button onclick="window.close()" style="padding: 10px 20px; background-color: #666; color: white; border: none; cursor: pointer; border-radius: 5px; margin-left: 10px;">
            ✖ Cerrar
        </button>
    </div>

    <!-- Logo Section -->
    <div class="logo-section">
        <h3 style="margin: 0; color: #003366;">TECNOLÓGICO NACIONAL DE MÉXICO</h3>
        <h4 style="margin: 5px 0; color: #003366;">INSTITUTO TECNOLÓGICO DE CD. SERDÁN</h4>
    </div>

    <!-- Header -->
    <div class="header">
        <h1>Solicitud de Mantenimiento</h1>
        <h2>Sistema de Reporte de Fallas e Incidencias (SIREFI)</h2>
    </div>

    <!-- Report Information -->
    <div class="info-section">
        <div class="info-row">
            <div class="info-label">FOLIO:</div>
            <div class="info-value"><strong><?= htmlspecialchars($reporte['folio']) ?></strong></div>
        </div>
        
        <div class="info-row">
            <div class="info-label">FECHA DE REPORTE:</div>
            <div class="info-value"><?= $reporte['fecha_reporte']->format('d/m/Y H:i') ?></div>
        </div>
        
        <div class="info-row">
            <div class="info-label">REPORTA:</div>
            <div class="info-value"><?= htmlspecialchars($reporte['reportante_nombre']) ?><br>
                <small><?= htmlspecialchars($reporte['reportante_correo']) ?></small>
            </div>
        </div>
        
        <div class="info-row">
            <div class="info-label">ÁREA / UBICACIÓN:</div>
            <div class="info-value"><?= htmlspecialchars($reporte['edificio'] . ' - ' . $reporte['salon']) ?></div>
        </div>
        
        <div class="info-row">
            <div class="info-label">CATEGORÍA:</div>
            <div class="info-value"><?= htmlspecialchars($reporte['categoria']) ?></div>
        </div>
        
        <div class="info-row">
            <div class="info-label">PRIORIDAD:</div>
            <div class="info-value">
                <span class="priority-<?= strtolower($reporte['prioridad']) ?>">
                    <?= htmlspecialchars($reporte['prioridad']) ?>
                </span>
            </div>
        </div>
        
        <div class="info-row">
            <div class="info-label">ESTATUS:</div>
            <div class="info-value">
                <?php
                $statusClass = '';
                if ($reporte['estado'] === 'Resuelto') $statusClass = 'status-solucionado';
                elseif ($reporte['estado'] === 'En Proceso') $statusClass = 'status-proceso';
                elseif ($reporte['estado'] === 'Recibido') $statusClass = 'status-recibido';
                elseif ($reporte['estado'] === 'Cancelado') $statusClass = 'status-cancelado';
                ?>
                <span class="status-badge <?= $statusClass ?>">
                    <?= htmlspecialchars($reporte['estado']) ?>
                </span>
            </div>
        </div>
    </div>

    <!-- Problem Description -->
    <div class="section-title">DESCRIPCIÓN DEL PROBLEMA</div>
    <div class="description-box">
        <?= nl2br(htmlspecialchars($reporte['descripcion'])) ?>
    </div>

    <!-- Service/Action Details -->
    <?php if (!empty($ultimaAccion)): ?>
    <div class="section-title">SERVICIO REALIZADO / ACCIONES TOMADAS</div>
    <div class="description-box">
        <?= nl2br(htmlspecialchars($ultimaAccion)) ?>
        <?php if (!empty($fechaAccion)): ?>
        <br><br><small><strong>Fecha:</strong> <?= $fechaAccion ?></small>
        <?php endif; ?>
    </div>
    <?php endif; ?>

    <!-- Technical Details -->
    <div class="info-section" style="margin-top: 20px;">
        <?php if ($reporte['fecha_asignacion']): ?>
        <div class="info-row">
            <div class="info-label">FECHA DE ASIGNACIÓN:</div>
            <div class="info-value"><?= $reporte['fecha_asignacion']->format('d/m/Y H:i') ?></div>
        </div>
        <?php endif; ?>
        
        <?php if ($reporte['fecha_solucion']): ?>
        <div class="info-row">
            <div class="info-label">FECHA DE SOLUCIÓN:</div>
            <div class="info-value"><?= $reporte['fecha_solucion']->format('d/m/Y H:i') ?></div>
        </div>
        <?php endif; ?>
        
        <?php if (!empty($reporte['tecnico_nombre'])): ?>
        <div class="info-row">
            <div class="info-label">TÉCNICO ASIGNADO:</div>
            <div class="info-value"><?= htmlspecialchars($reporte['tecnico_nombre']) ?></div>
        </div>
        <?php endif; ?>
    </div>

    <!-- Signatures -->
    <div class="signature-section">
        <div class="signature-box" style="margin-right: 8%;">
            <div class="signature-line">
                <strong>SOLICITANTE</strong><br>
                <?= htmlspecialchars($reporte['reportante_nombre']) ?>
            </div>
        </div>
        
        <div class="signature-box" style="float: right;">
            <div class="signature-line">
                <strong>TÉCNICO RESPONSABLE</strong><br>
                <?= !empty($reporte['tecnico_nombre']) ? htmlspecialchars($reporte['tecnico_nombre']) : '______________________' ?>
            </div>
        </div>
        <div style="clear: both;"></div>
    </div>

    <!-- Footer -->
    <div class="footer">
        <p><strong>Sistema de Reporte de Fallas e Incidencias (SIREFI)</strong></p>
        <p>Instituto Tecnológico de Cd. Serdán - Tecnológico Nacional de México</p>
        <p>Documento generado el <?= date('d/m/Y H:i') ?></p>
    </div>

    <script>
        // Auto-print dialog on load (optional)
        // window.onload = function() { window.print(); }
    </script>
</body>
</html>
