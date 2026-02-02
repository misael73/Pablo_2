<?php
/**
 * Dashboard de Recursos Materiales
 * Displays reports related to material resources
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/conexion.php';

// Require admin or technician access
requireAdminOrTechnician();

$usuario = getCurrentUser();
$usuario_id = getCurrentUserId();

// Filter by tipo_dashboard = 'materiales' or 'infraestructura'
// Get all reports filtered by material categories using the view
$sql = "SELECT r.*, 
        (SELECT TOP 1 comentario 
         FROM Comentarios 
         WHERE id_reporte = r.id AND publico = 1 AND eliminado = 0
         ORDER BY fecha_comentario DESC) AS detalle_accion
        FROM vw_Reportes_Completo r
        WHERE EXISTS (
            SELECT 1 FROM Categorias cat 
            WHERE cat.id = (SELECT id_categoria FROM Reportes rep WHERE rep.id = r.id)
            AND cat.tipo_dashboard IN ('materiales', 'infraestructura')
        )
        ORDER BY r.fecha_reporte DESC";

$stmt = sqlsrv_query($conn, $sql);

if ($stmt === false) {
    die(print_r(sqlsrv_errors(), true));
}

// Get statistics for material resources
$statsSQL = "SELECT 
    COUNT(*) as total,
    SUM(CASE WHEN est.nombre = 'En Proceso' THEN 1 ELSE 0 END) as en_proceso,
    SUM(CASE WHEN est.nombre = 'Resuelto' THEN 1 ELSE 0 END) as resueltos,
    SUM(CASE WHEN est.nombre = 'Recibido' AND DATEDIFF(day, r.fecha_reporte, GETDATE()) > 7 THEN 1 ELSE 0 END) as retrasados,
    SUM(CASE WHEN CAST(r.fecha_reporte AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) as hoy
    FROM Reportes r
    INNER JOIN Estados est ON r.id_estado = est.id
    INNER JOIN Categorias cat ON r.id_categoria = cat.id
    WHERE cat.tipo_dashboard IN ('materiales', 'infraestructura') AND r.eliminado = 0";

$statsStmt = sqlsrv_query($conn, $statsSQL);

$stats = ['total' => 0, 'en_proceso' => 0, 'resueltos' => 0, 'retrasados' => 0, 'hoy' => 0];
if ($statsStmt && $statsRow = sqlsrv_fetch_array($statsStmt, SQLSRV_FETCH_ASSOC)) {
    $stats = array_merge($stats, $statsRow);
}

// Get chart data - monthly reports for material resources
$chartSQL = "SELECT 
    MONTH(r.fecha_reporte) as mes,
    YEAR(r.fecha_reporte) as anio,
    COUNT(*) as cantidad
    FROM Reportes r
    INNER JOIN Categorias cat ON r.id_categoria = cat.id
    WHERE cat.tipo_dashboard IN ('materiales', 'infraestructura')
    AND r.eliminado = 0
    AND r.fecha_reporte >= DATEADD(month, -12, GETDATE())
    GROUP BY YEAR(r.fecha_reporte), MONTH(r.fecha_reporte)
    ORDER BY YEAR(r.fecha_reporte), MONTH(r.fecha_reporte)";

$chartStmt = sqlsrv_query($conn, $chartSQL);

$chartData = [];
$meses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
while ($chartStmt && $chartRow = sqlsrv_fetch_array($chartStmt, SQLSRV_FETCH_ASSOC)) {
    $chartData[$chartRow['mes']] = $chartRow['cantidad'];
}

// Get type distribution for material resources
$typeSQL = "SELECT cat.nombre as tipo, COUNT(*) as cantidad
            FROM Reportes r
            INNER JOIN Categorias cat ON r.id_categoria = cat.id
            WHERE cat.tipo_dashboard IN ('materiales', 'infraestructura')
            AND r.eliminado = 0
            GROUP BY cat.nombre
            ORDER BY cantidad DESC";

$typeStmt = sqlsrv_query($conn, $typeSQL);

$typeData = ['labels' => [], 'values' => []];
while ($typeStmt && $typeRow = sqlsrv_fetch_array($typeStmt, SQLSRV_FETCH_ASSOC)) {
    $typeData['labels'][] = $typeRow['tipo'];
    $typeData['values'][] = $typeRow['cantidad'];
}

$pageTitle = 'Dashboard de Recursos Materiales';
$additionalStyles = <<<HTML
  <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
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
      background-color: #ff6b35;
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

    .dashboard-header {
      background: linear-gradient(135deg, #ff6b35 0%, #f7931e 100%);
      color: white;
      padding: 20px;
      border-radius: 15px;
      margin-bottom: 25px;
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    }
  </style>
HTML;

include __DIR__ . '/includes/header.php';
?>

  <div class="container">
    <?php displayAlerts(); ?>
    
    <div class="dashboard-header">
      <h2 class="fw-bold mb-2">
        <i class="fas fa-tools"></i> Dashboard de Recursos Materiales
      </h2>
      <p class="mb-0">Gestión de reportes de infraestructura, maquinaria, aseo y vehicular</p>
    </div>

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
          <h5>Retrasados</h5>
          <p class="stat-value text-danger"><?= $stats['retrasados'] ?></p>
          <p class="text-muted">Necesitan atención</p>
        </div>
      </div>
    </div>

    <!-- Gráficas -->
    <?php if ($stats['total'] > 0): ?>
    <div class="row">
      <div class="col-lg-8">
        <div class="chart-container">
          <h5 class="section-title">Tendencia de reportes (últimos 12 meses)</h5>
          <canvas id="chartTendencia"></canvas>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="chart-container">
          <h5 class="section-title">Tipos de problema</h5>
          <canvas id="chartTipos"></canvas>
        </div>
      </div>
    </div>
    <?php endif; ?>

    <!-- Tabla de reportes -->
    <div class="chart-container mt-4">
      <h4 class="section-title">Listado de reportes de recursos materiales</h4>
      <div class="table-responsive">
        <table class="table table-bordered table-striped align-middle">
          <thead>
            <tr>
              <th style="text-align: center;">Folio</th>
              <th>Descripción</th>
              <th style="text-align: center;">Área</th>
              <th style="text-align: center;">Tipo</th>
              <th style="text-align: center;">Reportante</th>
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
                <td style="text-align: center;"><?= htmlspecialchars($row['reportante_nombre']) ?></td>

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
                    <i class="fas fa-eye"></i>
                  </button>
                  <button class="btn btn-success btn-sm" onclick="exportarPDF(<?= $row['id'] ?>)" title="Exportar PDF">
                    <i class="fas fa-file-pdf"></i>
                  </button>
                  <button class="btn btn-warning btn-sm" onclick="editarReporte(<?= $row['id'] ?>)" title="Editar">
                    <i class="fas fa-edit"></i>
                  </button>
                </td>
              </tr>
            <?php endwhile; ?>
            <?php if (!$hasReports): ?>
              <tr>
                <td colspan="9" style="text-align: center; padding: 40px;">
                  <i class="fas fa-inbox" style="font-size: 48px; color: #ccc;"></i>
                  <p class="text-muted mt-3">No hay reportes de recursos materiales en el sistema</p>
                </td>
              </tr>
            <?php endif; ?>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- Scripts de Chart.js -->
  <script>
    <?php if ($stats['total'] > 0): ?>
    // Chart data from PHP
    const chartLabels = <?= json_encode(array_values($meses)) ?>;
    const chartValues = [
      <?php for ($i = 1; $i <= 12; $i++): ?>
        <?= isset($chartData[$i]) ? $chartData[$i] : 0 ?><?= $i < 12 ? ',' : '' ?>
      <?php endfor; ?>
    ];

    const ctx1 = document.getElementById('chartTendencia').getContext('2d');
    new Chart(ctx1, {
      type: 'line',
      data: {
        labels: chartLabels,
        datasets: [{
          label: 'Reportes',
          data: chartValues,
          borderColor: '#ff6b35',
          backgroundColor: 'rgba(255,107,53,0.2)',
          fill: true,
          tension: 0.3
        }]
      },
      options: {
        responsive: true,
        plugins: { 
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function(context) {
                return 'Reportes: ' + context.parsed.y;
              }
            }
          }
        },
        scales: { 
          y: { 
            beginAtZero: true,
            ticks: {
              stepSize: 1
            }
          } 
        }
      }
    });

    const typeLabels = <?= json_encode($typeData['labels']) ?>;
    const typeValues = <?= json_encode($typeData['values']) ?>;
    const colors = ['#ff6b35', '#f7931e', '#ffc93c', '#e63946', '#ff8c42', '#858796'];

    const ctx2 = document.getElementById('chartTipos').getContext('2d');
    new Chart(ctx2, {
      type: 'pie',
      data: {
        labels: typeLabels,
        datasets: [{
          data: typeValues,
          backgroundColor: colors.slice(0, typeLabels.length)
        }]
      },
      options: { 
        responsive: true,
        plugins: {
          legend: {
            position: 'bottom'
          }
        }
      }
    });
    <?php endif; ?>

    // View report detail
    function verDetalle(reporteId) {
      window.location.href = 'ver_reporte.php?id=' + reporteId;
    }

    // Export report to PDF
    function exportarPDF(reporteId) {
      window.open('exportar_reporte.php?id=' + reporteId, '_blank');
    }

    // Edit report (admin only)
    function editarReporte(reporteId) {
      window.location.href = 'editar_reporte.php?id=' + reporteId;
    }
  </script>

<?php
$additionalScripts = '';
include __DIR__ . '/includes/footer.php';
?>
