<?php
/**
 * Report Form - Incident Reporting Form
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/includes/csrf.php';
require_once __DIR__ . '/conexion.php';

// Require user to be logged in
requireLogin();

// Get active edificios from database
$edificiosSQL = "SELECT id, nombre FROM Edificios WHERE activo = 1 ORDER BY nombre";
$edificiosStmt = sqlsrv_query($conn, $edificiosSQL);
$edificios = [];
if ($edificiosStmt) {
    while ($row = sqlsrv_fetch_array($edificiosStmt, SQLSRV_FETCH_ASSOC)) {
        $edificios[] = $row;
    }
} else {
    // If query fails, log error but continue (fallback to empty list)
    error_log("Error loading edificios: " . print_r(sqlsrv_errors(), true));
}

// Get active categories from database
$categoriasSQL = "SELECT id, nombre, descripcion FROM Categorias WHERE activo = 1 ORDER BY nombre";
$categoriasStmt = sqlsrv_query($conn, $categoriasSQL);
$categorias = [];
if ($categoriasStmt) {
    while ($row = sqlsrv_fetch_array($categoriasStmt, SQLSRV_FETCH_ASSOC)) {
        $categorias[] = $row;
    }
} else {
    error_log("Error loading categorias: " . print_r(sqlsrv_errors(), true));
}

$pageTitle = 'Nuevo Reporte';
$additionalStyles = <<<HTML
    <style>
        body {
            background-color: #f5f7fa;
        }
        .card-header {
            background-color: #003366;
            color: white;
            text-align: center;
            padding: 20px;
        }
        .btn-tecnm {
            background-color: #003366;
            color: white;
            border: none;
        }
        .btn-tecnm:hover {
            background-color: #00264d;
            color: white;
        }
        .info-box {
            background-color: #fff8e1;
            border-left: 4px solid #ffcc00;
            padding: 10px;
            margin-top: 10px;
            display: none;
        }
    </style>
HTML;

include __DIR__ . '/includes/header.php';
?>

    <div class="container my-4">
        <?php displayAlerts(); ?>
        
        <div class="card shadow-lg">
            <div class="card-header">
                <h1 class="h4 mb-0">🚨 Reporte de Falla o Incidencia</h1>
            </div>

            <div class="card-body">
                <form action="reportar.php" method="post" enctype="multipart/form-data">
                    <?php echo csrfField(); ?>

                    <!-- Sección única: Detalles de la Incidencia -->
                    <h5 class="mt-2">Detalles de la Incidencia</h5>
                    <hr>

                    <div class="row g-3 mb-4">

                        <!-- Edificio -->
                        <div class="col-md-6">
                            <label for="edificio" class="form-label">¿En qué edificio se encuentra el problema? (*)</label>
                            <select id="edificio" name="edificio" class="form-select" required>
                                <option value="">Seleccione el edificio...</option>
                                <?php foreach ($edificios as $edificio): ?>
                                    <option value="<?= $edificio['id'] ?>"><?= htmlspecialchars($edificio['nombre']) ?></option>
                                <?php endforeach; ?>
                            </select>
                        </div>

                        <!-- Aulas dinámicas -->
                        <div class="col-md-6">
                            <label for="aula" class="form-label">Seleccione aula / espacio (*)</label>
                            <select id="aula" name="aula" class="form-select" required>
                                <option value="">Primero seleccione un edificio...</option>
                            </select>
                        </div>
                    </div>
                    
                    <!-- Área Destino -->
                    <div class="mb-3">
                        <label for="area_destino" class="form-label">Categoría del Reporte (*)</label>
                        <select id="area_destino" name="area_destino" class="form-select" required>
                            <option value="">Seleccione la categoría...</option>
                            <?php foreach ($categorias as $categoria): ?>
                                <option value="<?= htmlspecialchars($categoria['nombre']) ?>" 
                                        data-desc="<?= htmlspecialchars($categoria['descripcion'] ?? '') ?>">
                                    <?= htmlspecialchars($categoria['nombre']) ?>
                                </option>
                            <?php endforeach; ?>
                        </select>
                        
                        <div id="categoria_descripcion" class="alert alert-info mt-2" style="display: none;">
                            <i class="fas fa-info-circle"></i> <span id="desc_text"></span>
                        </div>
                    </div>

                    <!-- Descripción Problema -->
                    <div class="mb-4">
                        <label for="descripcion_problema" class="form-label">Descripción Detallada del Problema (*)</label>
                        <textarea class="form-control" id="descripcion_problema" name="descripcion_problema" rows="4" required></textarea>
                    </div>

                    <!-- Archivo -->
                    <div class="mb-4">
                        <label for="archivo_adjunto" class="form-label">Adjuntar Fotografía o Archivo (Opcional)</label>
                        <input class="form-control" type="file" id="archivo_adjunto" name="archivo_adjunto">
                        <small class="form-text text-muted">Formatos permitidos: .jpg, .png, .pdf. Máx. 2MB.</small>
                    </div>

                    <!-- Botón -->
                    <div class="d-grid">
                        <button type="submit" class="btn btn-tecnm btn-lg">Enviar Reporte</button>
                    </div>

                </form>
            </div>
        </div>
    </div>

    <script>
        // Load salones dynamically when edificio changes
        const edificioSelect = document.getElementById("edificio");
        const aulaSelect = document.getElementById("aula");

        edificioSelect.addEventListener("change", async function() {
            const edificioId = this.value;
            
            // Clear current options
            aulaSelect.innerHTML = '<option value="">Cargando...</option>';
            
            if (!edificioId) {
                aulaSelect.innerHTML = '<option value="">Primero seleccione un edificio...</option>';
                return;
            }
            
            try {
                // Fetch salones for the selected edificio
                const response = await fetch(`api/get_salones.php?edificio_id=${edificioId}`);
                const result = await response.json();
                
                if (result.success && result.data) {
                    aulaSelect.innerHTML = '<option value="">Seleccione un salón...</option>';
                    
                    result.data.forEach(salon => {
                        const option = document.createElement('option');
                        option.value = salon.nombre;
                        option.textContent = salon.nombre;
                        aulaSelect.appendChild(option);
                    });
                } else {
                    aulaSelect.innerHTML = '<option value="">No hay salones disponibles</option>';
                }
            } catch (error) {
                console.error('Error loading salones:', error);
                aulaSelect.innerHTML = '<option value="">Error al cargar salones</option>';
            }
        });

        // Category selection - show description
        const areaDestino = document.getElementById("area_destino");
        const categoriaDescDiv = document.getElementById("categoria_descripcion");
        const descText = document.getElementById("desc_text");

        areaDestino.addEventListener("change", function () {
            const selectedOption = this.options[this.selectedIndex];
            const descripcion = selectedOption.getAttribute('data-desc');
            
            if (descripcion && descripcion.trim() !== '') {
                descText.textContent = descripcion;
                categoriaDescDiv.style.display = "block";
            } else {
                categoriaDescDiv.style.display = "none";
            }
        });
    </script>

<?php
$additionalScripts = '';
include __DIR__ . '/includes/footer.php';
?>