<?php
/**
 * Infrastructure Management - CRUD for Buildings (Edificios) and Rooms (Salones)
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/includes/csrf.php';
require_once __DIR__ . '/conexion.php';

// Require admin access
requireAdmin();

$usuario = getCurrentUser();
$pageTitle = 'Gestionar Infraestructura';

$additionalStyles = <<<HTML
<style>
  body {
    background-color: #f5f6fa;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  }

  .container {
    margin-top: 20px;
    max-width: 1400px;
  }

  .section-card {
    background-color: white;
    border-radius: 15px;
    padding: 30px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    margin-bottom: 30px;
  }

  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
    padding-bottom: 15px;
    border-bottom: 3px solid #0d6efd;
  }

  .section-title {
    font-size: 24px;
    font-weight: bold;
    color: #003366;
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .section-title i {
    font-size: 28px;
    color: #0d6efd;
  }

  .table {
    margin-top: 20px;
  }

  .table thead {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
  }

  .table thead th {
    border: none;
    padding: 15px;
    font-weight: 600;
    text-align: center;
    vertical-align: middle;
  }

  .table tbody td {
    text-align: center;
    vertical-align: middle;
    padding: 12px;
  }

  .table tbody tr:hover {
    background-color: #f8f9fa;
  }

  .badge-active {
    background-color: #28a745;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 12px;
  }

  .badge-inactive {
    background-color: #dc3545;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 12px;
  }

  .btn-action {
    padding: 6px 12px;
    margin: 2px;
    border-radius: 6px;
    font-size: 13px;
    transition: all 0.3s ease;
  }

  .btn-action:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  }

  .modal-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 10px 10px 0 0;
  }

  .modal-header .btn-close {
    filter: brightness(0) invert(1);
  }

  .form-label {
    font-weight: 600;
    color: #333;
    margin-bottom: 8px;
  }

  .form-control, .form-select {
    border-radius: 8px;
    border: 1px solid #ced4da;
    padding: 10px 15px;
  }

  .form-control:focus, .form-select:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
  }

  .alert {
    border-radius: 10px;
    border: none;
    padding: 15px 20px;
  }

  .page-header {
    text-align: center;
    margin-bottom: 40px;
    padding: 30px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 15px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  }

  .page-header h2 {
    margin: 0;
    font-size: 32px;
    font-weight: bold;
  }

  .page-header p {
    margin: 10px 0 0 0;
    font-size: 16px;
    opacity: 0.95;
  }

  .filter-section {
    background-color: #f8f9fa;
    padding: 15px;
    border-radius: 10px;
    margin-bottom: 20px;
  }

  .empty-state {
    text-align: center;
    padding: 60px 20px;
    color: #6c757d;
  }

  .empty-state i {
    font-size: 64px;
    margin-bottom: 20px;
    opacity: 0.3;
  }

  .btn-add-new {
    background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
    border: none;
    color: white;
    padding: 10px 25px;
    border-radius: 8px;
    font-weight: 600;
    transition: all 0.3s ease;
  }

  .btn-add-new:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 12px rgba(40, 167, 69, 0.3);
    color: white;
  }

  .loading-overlay {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 9999;
    justify-content: center;
    align-items: center;
  }

  .loading-spinner {
    width: 50px;
    height: 50px;
    border: 5px solid #f3f3f3;
    border-top: 5px solid #667eea;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
</style>
HTML;

include __DIR__ . '/includes/header.php';
?>

<div class="loading-overlay" id="loadingOverlay">
  <div class="loading-spinner"></div>
</div>

<div class="container">
  <!-- Alert Container -->
  <div id="alertContainer"></div>

  <!-- Page Header -->
  <div class="page-header">
    <h2><i class="fas fa-building"></i> Gestión de Infraestructura</h2>
    <p>Administre edificios y salones de la institución</p>
  </div>

  <!-- Edificios Section -->
  <div class="section-card">
    <div class="section-header">
      <div class="section-title">
        <i class="fas fa-city"></i>
        <span>Edificios</span>
      </div>
      <button class="btn btn-add-new" onclick="showEdificioModal()">
        <i class="fas fa-plus"></i> Agregar Edificio
      </button>
    </div>

    <div class="table-responsive">
      <table class="table table-hover" id="edificiosTable">
        <thead>
          <tr>
            <th style="width: 5%">ID</th>
            <th style="width: 10%">Código</th>
            <th style="width: 20%">Nombre</th>
            <th style="width: 25%">Descripción</th>
            <th style="width: 15%">Ubicación</th>
            <th style="width: 5%">Pisos</th>
            <th style="width: 8%">Estado</th>
            <th style="width: 12%">Acciones</th>
          </tr>
        </thead>
        <tbody id="edificiosTableBody">
          <tr>
            <td colspan="8" class="empty-state">
              <i class="fas fa-spinner fa-spin"></i>
              <p>Cargando edificios...</p>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- Salones Section -->
  <div class="section-card">
    <div class="section-header">
      <div class="section-title">
        <i class="fas fa-door-open"></i>
        <span>Salones</span>
      </div>
      <button class="btn btn-add-new" onclick="showSalonModal()" id="btnAddSalon">
        <i class="fas fa-plus"></i> Agregar Salón
      </button>
    </div>

    <!-- Filter Section -->
    <div class="filter-section">
      <div class="row align-items-center">
        <div class="col-md-6">
          <label for="filterEdificio" class="form-label">
            <i class="fas fa-filter"></i> Filtrar por Edificio:
          </label>
          <select class="form-select" id="filterEdificio" onchange="loadSalones()">
            <option value="">Todos los edificios</option>
          </select>
        </div>
        <div class="col-md-6">
          <div class="text-end pt-4">
            <span class="badge bg-primary" id="salonesCount">0 salones</span>
          </div>
        </div>
      </div>
    </div>

    <div class="table-responsive">
      <table class="table table-hover" id="salonesTable">
        <thead>
          <tr>
            <th style="width: 5%">ID</th>
            <th style="width: 20%">Edificio</th>
            <th style="width: 20%">Nombre</th>
            <th style="width: 30%">Descripción</th>
            <th style="width: 10%">Estado</th>
            <th style="width: 15%">Acciones</th>
          </tr>
        </thead>
        <tbody id="salonesTableBody">
          <tr>
            <td colspan="6" class="empty-state">
              <i class="fas fa-info-circle"></i>
              <p>Seleccione un edificio para ver sus salones</p>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</div>

<!-- Modal: Add/Edit Edificio -->
<div class="modal fade" id="edificioModal" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="edificioModalTitle">
          <i class="fas fa-building"></i> Agregar Edificio
        </h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <form id="edificioForm">
          <input type="hidden" id="edificioId" name="id">
          <div class="mb-3">
            <label for="edificioCodigo" class="form-label">
              Código <span class="text-danger">*</span>
            </label>
            <input type="text" class="form-control" id="edificioCodigo" 
                   name="codigo" required maxlength="20" minlength="2"
                   placeholder="Ej: ED-A, TORRE-1"
                   pattern="[A-Za-z0-9\-]+"
                   title="Solo letras, números y guiones">
            <small class="form-text text-muted">
              Código único del edificio (2-20 caracteres, solo letras, números y guiones)
            </small>
          </div>
          <div class="mb-3">
            <label for="edificioNombre" class="form-label">
              Nombre del Edificio <span class="text-danger">*</span>
            </label>
            <input type="text" class="form-control" id="edificioNombre" 
                   name="nombre" required maxlength="100"
                   placeholder="Ej: Edificio A, Torre Principal">
          </div>
          <div class="mb-3">
            <label for="edificioDescripcion" class="form-label">
              Descripción
            </label>
            <textarea class="form-control" id="edificioDescripcion" 
                      name="descripcion" rows="3" maxlength="500"
                      placeholder="Descripción adicional del edificio (opcional)"></textarea>
          </div>
          <div class="mb-3">
            <label for="edificioUbicacion" class="form-label">
              Ubicación
            </label>
            <input type="text" class="form-control" id="edificioUbicacion" 
                   name="ubicacion" maxlength="255"
                   placeholder="Ej: Campus Norte, Zona 2 (opcional)">
          </div>
          <div class="mb-3">
            <label for="edificioPisos" class="form-label">
              Número de Pisos
            </label>
            <input type="number" class="form-control" id="edificioPisos" 
                   name="pisos" min="1" max="50"
                   placeholder="Ej: 3 (opcional)">
          </div>
          <div class="mb-3 form-check">
            <input type="checkbox" class="form-check-input" id="edificioActivo" 
                   name="activo" checked>
            <label class="form-check-label" for="edificioActivo">
              Edificio activo
            </label>
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
          <i class="fas fa-times"></i> Cancelar
        </button>
        <button type="button" class="btn btn-primary" id="btnSaveEdificio" onclick="saveEdificio()">
          <i class="fas fa-save"></i> Guardar
        </button>
      </div>
    </div>
  </div>
</div>

<!-- Modal: Add/Edit Salon -->
<div class="modal fade" id="salonModal" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="salonModalTitle">
          <i class="fas fa-door-open"></i> Agregar Salón
        </h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <form id="salonForm">
          <input type="hidden" id="salonId" name="id">
          <div class="mb-3">
            <label for="salonEdificio" class="form-label">
              Edificio <span class="text-danger">*</span>
            </label>
            <select class="form-select" id="salonEdificio" name="id_edificio" required>
              <option value="">Seleccione un edificio</option>
            </select>
          </div>
          <div class="mb-3">
            <label for="salonNombre" class="form-label">
              Nombre del Salón <span class="text-danger">*</span>
            </label>
            <input type="text" class="form-control" id="salonNombre" 
                   name="nombre" required maxlength="100"
                   placeholder="Ej: Salón 101, Aula Magna, Laboratorio">
          </div>
          <div class="mb-3">
            <label for="salonDescripcion" class="form-label">
              Descripción
            </label>
            <textarea class="form-control" id="salonDescripcion" 
                      name="descripcion" rows="3" maxlength="500"
                      placeholder="Descripción adicional del salón (opcional)"></textarea>
          </div>
          <div class="mb-3 form-check">
            <input type="checkbox" class="form-check-input" id="salonActivo" 
                   name="activo" checked>
            <label class="form-check-label" for="salonActivo">
              Salón activo
            </label>
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
          <i class="fas fa-times"></i> Cancelar
        </button>
        <button type="button" class="btn btn-primary" id="btnSaveSalon" onclick="saveSalon()">
          <i class="fas fa-save"></i> Guardar
        </button>
      </div>
    </div>
  </div>
</div>

<script>
  // CSRF Token
  const csrfToken = '<?php echo getCSRFToken(); ?>';
  
  // Bootstrap Modal instances
  let edificioModal, salonModal;
  
  // Initialize on page load
  document.addEventListener('DOMContentLoaded', function() {
    edificioModal = new bootstrap.Modal(document.getElementById('edificioModal'));
    salonModal = new bootstrap.Modal(document.getElementById('salonModal'));
    
    // Reset edificio form when modal is fully hidden
    document.getElementById('edificioModal').addEventListener('hidden.bs.modal', function() {
      document.getElementById('edificioForm').reset();
      document.getElementById('edificioId').value = '';
      document.getElementById('edificioActivo').checked = true;
    });
    
    // Reset salon form when modal is fully hidden
    document.getElementById('salonModal').addEventListener('hidden.bs.modal', function() {
      document.getElementById('salonForm').reset();
      document.getElementById('salonId').value = '';
      document.getElementById('salonActivo').checked = true;
    });
    
    loadEdificios();
    loadEdificiosForFilters();
  });
  
  // Show loading overlay
  function showLoading() {
    document.getElementById('loadingOverlay').style.display = 'flex';
  }
  
  // Hide loading overlay
  function hideLoading() {
    document.getElementById('loadingOverlay').style.display = 'none';
  }
  
  // Show alert message
  function showAlert(message, type = 'success') {
    const alertContainer = document.getElementById('alertContainer');
    const alertId = 'alert-' + Date.now();
    
    const alertHTML = `
      <div class="alert alert-${type} alert-dismissible fade show" role="alert" id="${alertId}">
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
      </div>
    `;
    
    alertContainer.innerHTML = alertHTML;
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
      const alert = document.getElementById(alertId);
      if (alert) {
        alert.remove();
      }
    }, 5000);
    
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  
  // ==================== EDIFICIOS ====================
  
  // Load all edificios
  async function loadEdificios() {
    try {
      const response = await fetch('api/edificios.php?action=list');
      const result = await response.json();
      
      if (result.success) {
        displayEdificios(result.data);
      } else {
        showAlert(result.error || 'Error al cargar edificios', 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión al cargar edificios', 'danger');
    }
  }
  
  // Display edificios in table
  function displayEdificios(edificios) {
    const tbody = document.getElementById('edificiosTableBody');
    
    if (edificios.length === 0) {
      tbody.innerHTML = `
        <tr>
          <td colspan="8" class="empty-state">
            <i class="fas fa-building"></i>
            <p>No hay edificios registrados</p>
            <button class="btn btn-primary mt-2" onclick="showEdificioModal()">
              <i class="fas fa-plus"></i> Agregar primer edificio
            </button>
          </td>
        </tr>
      `;
      return;
    }
    
    tbody.innerHTML = edificios.map(edificio => `
      <tr>
        <td>${edificio.id}</td>
        <td>${edificio.codigo ? escapeHtml(edificio.codigo) : '<em class="text-muted">N/A</em>'}</td>
        <td style="text-align: left; font-weight: 600;">${escapeHtml(edificio.nombre)}</td>
        <td style="text-align: left;">${edificio.descripcion ? escapeHtml(edificio.descripcion) : '<em class="text-muted">Sin descripción</em>'}</td>
        <td style="text-align: left;">${edificio.ubicacion ? escapeHtml(edificio.ubicacion) : '<em class="text-muted">N/A</em>'}</td>
        <td>${edificio.pisos ? edificio.pisos : '<em class="text-muted">N/A</em>'}</td>
        <td>
          <span class="badge ${edificio.activo ? 'badge-active' : 'badge-inactive'}">
            ${edificio.activo ? 'Activo' : 'Inactivo'}
          </span>
        </td>
        <td>
          <button class="btn btn-sm btn-warning btn-action" onclick="editEdificio(${edificio.id})" title="Editar">
            <i class="fas fa-edit"></i>
          </button>
          <button class="btn btn-sm btn-${edificio.activo ? 'secondary' : 'success'} btn-action" 
                  onclick="toggleEdificioStatus(${edificio.id}, ${!edificio.activo})" 
                  title="${edificio.activo ? 'Desactivar' : 'Activar'}">
            <i class="fas fa-${edificio.activo ? 'ban' : 'check'}"></i>
          </button>
        </td>
      </tr>
    `).join('');
  }
  
  // Show edificio modal for add/edit
  function showEdificioModal(edificioId = null) {
    document.getElementById('edificioForm').reset();
    document.getElementById('edificioId').value = '';
    document.getElementById('edificioActivo').checked = true;
    
    if (edificioId) {
      document.getElementById('edificioModalTitle').innerHTML = '<i class="fas fa-edit"></i> Editar Edificio';
      loadEdificioData(edificioId);
    } else {
      document.getElementById('edificioModalTitle').innerHTML = '<i class="fas fa-building"></i> Agregar Edificio';
    }
    
    edificioModal.show();
  }
  
  // Load edificio data for editing
  async function loadEdificioData(id) {
    showLoading();
    try {
      const response = await fetch(`api/edificios.php?action=get&id=${id}`);
      const result = await response.json();
      
      if (result.success) {
        const edificio = result.data;
        document.getElementById('edificioId').value = edificio.id;
        document.getElementById('edificioCodigo').value = edificio.codigo || '';
        document.getElementById('edificioNombre').value = edificio.nombre;
        document.getElementById('edificioDescripcion').value = edificio.descripcion || '';
        document.getElementById('edificioUbicacion').value = edificio.ubicacion || '';
        document.getElementById('edificioPisos').value = edificio.pisos || '';
        document.getElementById('edificioActivo').checked = edificio.activo;
      } else {
        showAlert(result.error || 'Error al cargar edificio', 'danger');
        edificioModal.hide();
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión', 'danger');
      edificioModal.hide();
    } finally {
      hideLoading();
    }
  }
  
  // Edit edificio
  function editEdificio(id) {
    showEdificioModal(id);
  }
  
  // Flag to prevent concurrent submissions
  let isSavingEdificio = false;
  
  // Save edificio (create or update)
  async function saveEdificio() {
    // Prevent concurrent submissions
    if (isSavingEdificio) {
      console.log('Ya hay una operación en curso');
      return;
    }
    
    const form = document.getElementById('edificioForm');
    const btnSave = document.getElementById('btnSaveEdificio');
    
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }
    
    const id = document.getElementById('edificioId').value;
    const codigo = document.getElementById('edificioCodigo').value.trim();
    const nombre = document.getElementById('edificioNombre').value.trim();
    const descripcion = document.getElementById('edificioDescripcion').value.trim();
    const ubicacion = document.getElementById('edificioUbicacion').value.trim();
    const pisos = document.getElementById('edificioPisos').value.trim();
    const activo = document.getElementById('edificioActivo').checked;
    
    if (!nombre) {
      showAlert('El nombre es requerido', 'danger');
      return;
    }
    
    if (!codigo) {
      showAlert('El código es requerido', 'danger');
      return;
    }
    
    if (codigo.length < 2 || codigo.length > 20) {
      showAlert('El código debe tener entre 2 y 20 caracteres', 'danger');
      return;
    }
    
    // Validate codigo format
    if (!/^[A-Za-z0-9\-]+$/.test(codigo)) {
      showAlert('El código solo puede contener letras, números y guiones', 'danger');
      return;
    }
    
    const data = {
      codigo,
      nombre,
      descripcion,
      activo,
      csrf_token: csrfToken
    };
    
    // Add optional fields only if they have values
    if (ubicacion) data.ubicacion = ubicacion;
    if (pisos) data.pisos = parseInt(pisos);
    
    if (id) {
      data.id = id;
    }
    
    const action = id ? 'update' : 'create';
    
    // Set flag and disable button to prevent double submission
    isSavingEdificio = true;
    btnSave.disabled = true;
    btnSave.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Guardando...';
    
    showLoading();
    try {
      const response = await fetch(`api/edificios.php?action=${action}`, {
        method: id ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
      });
      
      const result = await response.json();
      
      if (result.success) {
        showAlert(result.data.message, 'success');
        edificioModal.hide();
        // Reset form after successful save
        document.getElementById('edificioForm').reset();
        document.getElementById('edificioId').value = '';
        loadEdificios();
        loadEdificiosForFilters();
      } else {
        showAlert(result.error || 'Error al guardar edificio', 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión al guardar', 'danger');
    } finally {
      hideLoading();
      // Re-enable button and clear flag
      isSavingEdificio = false;
      btnSave.disabled = false;
      btnSave.innerHTML = '<i class="fas fa-save"></i> Guardar';
    }
  }
  
  // Toggle edificio active status
  async function toggleEdificioStatus(id, newStatus) {
    const action = newStatus ? 'activar' : 'desactivar';
    
    if (!confirm(`¿Está seguro de ${action} este edificio?`)) {
      return;
    }
    
    showLoading();
    try {
      // Get current edificio data
      const getResponse = await fetch(`api/edificios.php?action=get&id=${id}`);
      const getResult = await getResponse.json();
      
      if (!getResult.success) {
        showAlert(getResult.error || 'Error al obtener datos del edificio', 'danger');
        return;
      }
      
      const edificio = getResult.data;
      
      // Update with new status
      const response = await fetch('api/edificios.php?action=update', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          id,
          codigo: edificio.codigo,
          nombre: edificio.nombre,
          descripcion: edificio.descripcion,
          ubicacion: edificio.ubicacion,
          pisos: edificio.pisos,
          activo: newStatus,
          csrf_token: csrfToken
        })
      });
      
      const result = await response.json();
      
      if (result.success) {
        showAlert(`Edificio ${newStatus ? 'activado' : 'desactivado'} exitosamente`, 'success');
        loadEdificios();
        loadEdificiosForFilters();
        // Reload salones if they're being displayed
        if (document.getElementById('filterEdificio').value === id.toString()) {
          loadSalones();
        }
      } else {
        showAlert(result.error || `Error al ${action} edificio`, 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión', 'danger');
    } finally {
      hideLoading();
    }
  }
  
  // ==================== SALONES ====================
  
  // Load edificios for dropdown filters
  async function loadEdificiosForFilters() {
    try {
      const response = await fetch('api/edificios.php?action=list');
      const result = await response.json();
      
      if (result.success) {
        const activeEdificios = result.data.filter(e => e.activo);
        
        // Update filter dropdown
        const filterSelect = document.getElementById('filterEdificio');
        filterSelect.innerHTML = '<option value="">Todos los edificios</option>' +
          activeEdificios.map(e => `<option value="${e.id}">${escapeHtml(e.nombre)}</option>`).join('');
        
        // Update salon form dropdown
        const salonSelect = document.getElementById('salonEdificio');
        salonSelect.innerHTML = '<option value="">Seleccione un edificio</option>' +
          activeEdificios.map(e => `<option value="${e.id}">${escapeHtml(e.nombre)}</option>`).join('');
      }
    } catch (error) {
      console.error('Error:', error);
    }
  }
  
  // Load salones (optionally filtered by edificio)
  async function loadSalones() {
    const edificioId = document.getElementById('filterEdificio').value;
    
    if (!edificioId) {
      const tbody = document.getElementById('salonesTableBody');
      tbody.innerHTML = `
        <tr>
          <td colspan="6" class="empty-state">
            <i class="fas fa-info-circle"></i>
            <p>Seleccione un edificio para ver sus salones</p>
          </td>
        </tr>
      `;
      document.getElementById('salonesCount').textContent = '0 salones';
      return;
    }
    
    try {
      const response = await fetch(`api/salones.php?action=list&edificio_id=${edificioId}`);
      const result = await response.json();
      
      if (result.success) {
        displaySalones(result.data);
      } else {
        showAlert(result.error || 'Error al cargar salones', 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión al cargar salones', 'danger');
    }
  }
  
  // Display salones in table
  function displaySalones(salones) {
    const tbody = document.getElementById('salonesTableBody');
    document.getElementById('salonesCount').textContent = `${salones.length} salon${salones.length !== 1 ? 'es' : ''}`;
    
    if (salones.length === 0) {
      tbody.innerHTML = `
        <tr>
          <td colspan="6" class="empty-state">
            <i class="fas fa-door-open"></i>
            <p>No hay salones registrados en este edificio</p>
            <button class="btn btn-primary mt-2" onclick="showSalonModal()">
              <i class="fas fa-plus"></i> Agregar primer salón
            </button>
          </td>
        </tr>
      `;
      return;
    }
    
    tbody.innerHTML = salones.map(salon => `
      <tr>
        <td>${salon.id}</td>
        <td style="text-align: left; font-weight: 600;">${escapeHtml(salon.edificio_nombre)}</td>
        <td style="text-align: left; font-weight: 600;">${escapeHtml(salon.nombre)}</td>
        <td style="text-align: left;">${salon.descripcion ? escapeHtml(salon.descripcion) : '<em class="text-muted">Sin descripción</em>'}</td>
        <td>
          <span class="badge ${salon.activo ? 'badge-active' : 'badge-inactive'}">
            ${salon.activo ? 'Activo' : 'Inactivo'}
          </span>
        </td>
        <td>
          <button class="btn btn-sm btn-warning btn-action" onclick="editSalon(${salon.id})" title="Editar">
            <i class="fas fa-edit"></i>
          </button>
          <button class="btn btn-sm btn-${salon.activo ? 'secondary' : 'success'} btn-action" 
                  onclick="toggleSalonStatus(${salon.id}, ${!salon.activo})" 
                  title="${salon.activo ? 'Desactivar' : 'Activar'}">
            <i class="fas fa-${salon.activo ? 'ban' : 'check'}"></i>
          </button>
        </td>
      </tr>
    `).join('');
  }
  
  // Show salon modal for add/edit
  function showSalonModal(salonId = null) {
    document.getElementById('salonForm').reset();
    document.getElementById('salonId').value = '';
    document.getElementById('salonActivo').checked = true;
    
    // Pre-select edificio from filter if adding new
    if (!salonId) {
      const filterEdificioId = document.getElementById('filterEdificio').value;
      if (filterEdificioId) {
        document.getElementById('salonEdificio').value = filterEdificioId;
      }
    }
    
    if (salonId) {
      document.getElementById('salonModalTitle').innerHTML = '<i class="fas fa-edit"></i> Editar Salón';
      loadSalonData(salonId);
    } else {
      document.getElementById('salonModalTitle').innerHTML = '<i class="fas fa-door-open"></i> Agregar Salón';
    }
    
    salonModal.show();
  }
  
  // Load salon data for editing
  async function loadSalonData(id) {
    showLoading();
    try {
      const response = await fetch(`api/salones.php?action=get&id=${id}`);
      const result = await response.json();
      
      if (result.success) {
        const salon = result.data;
        document.getElementById('salonId').value = salon.id;
        document.getElementById('salonEdificio').value = salon.id_edificio;
        document.getElementById('salonNombre').value = salon.nombre;
        document.getElementById('salonDescripcion').value = salon.descripcion || '';
        document.getElementById('salonActivo').checked = salon.activo;
      } else {
        showAlert(result.error || 'Error al cargar salón', 'danger');
        salonModal.hide();
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión', 'danger');
      salonModal.hide();
    } finally {
      hideLoading();
    }
  }
  
  // Edit salon
  function editSalon(id) {
    showSalonModal(id);
  }
  
  // Flag to prevent concurrent submissions
  let isSavingSalon = false;
  
  // Save salon (create or update)
  async function saveSalon() {
    // Prevent concurrent submissions
    if (isSavingSalon) {
      console.log('Ya hay una operación en curso');
      return;
    }
    
    const form = document.getElementById('salonForm');
    const btnSave = document.getElementById('btnSaveSalon');
    
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }
    
    const id = document.getElementById('salonId').value;
    const id_edificio = document.getElementById('salonEdificio').value;
    const nombre = document.getElementById('salonNombre').value.trim();
    const descripcion = document.getElementById('salonDescripcion').value.trim();
    const activo = document.getElementById('salonActivo').checked;
    
    if (!nombre || !id_edificio) {
      showAlert('El nombre y edificio son requeridos', 'danger');
      return;
    }
    
    const data = {
      id_edificio,
      nombre,
      descripcion,
      activo,
      csrf_token: csrfToken
    };
    
    if (id) {
      data.id = id;
    }
    
    const action = id ? 'update' : 'create';
    
    // Set flag and disable button to prevent double submission
    isSavingSalon = true;
    btnSave.disabled = true;
    btnSave.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Guardando...';
    
    showLoading();
    try {
      const response = await fetch(`api/salones.php?action=${action}`, {
        method: id ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
      });
      
      const result = await response.json();
      
      if (result.success) {
        showAlert(result.data.message, 'success');
        salonModal.hide();
        // Reset form after successful save
        document.getElementById('salonForm').reset();
        document.getElementById('salonId').value = '';
        
        // Update filter to show the edificio of the saved salon
        document.getElementById('filterEdificio').value = id_edificio;
        loadSalones();
      } else {
        showAlert(result.error || 'Error al guardar salón', 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión al guardar', 'danger');
    } finally {
      hideLoading();
      // Re-enable button and clear flag
      isSavingSalon = false;
      btnSave.disabled = false;
      btnSave.innerHTML = '<i class="fas fa-save"></i> Guardar';
    }
  }
  
  // Toggle salon active status
  async function toggleSalonStatus(id, newStatus) {
    const action = newStatus ? 'activar' : 'desactivar';
    
    if (!confirm(`¿Está seguro de ${action} este salón?`)) {
      return;
    }
    
    showLoading();
    try {
      // Get current salon data
      const getResponse = await fetch(`api/salones.php?action=get&id=${id}`);
      const getResult = await getResponse.json();
      
      if (!getResult.success) {
        showAlert('Error al obtener datos del salón', 'danger');
        return;
      }
      
      const salon = getResult.data;
      
      // Update with new status
      const response = await fetch('api/salones.php?action=update', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          id,
          id_edificio: salon.id_edificio,
          nombre: salon.nombre,
          descripcion: salon.descripcion,
          activo: newStatus,
          csrf_token: csrfToken
        })
      });
      
      const result = await response.json();
      
      if (result.success) {
        showAlert(`Salón ${newStatus ? 'activado' : 'desactivado'} exitosamente`, 'success');
        loadSalones();
      } else {
        showAlert(result.error || `Error al ${action} salón`, 'danger');
      }
    } catch (error) {
      console.error('Error:', error);
      showAlert('Error de conexión', 'danger');
    } finally {
      hideLoading();
    }
  }
  
  // ==================== UTILITY FUNCTIONS ====================
  
  // Escape HTML to prevent XSS
  function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }
  
  // Format date
  function formatDate(dateStr) {
    if (!dateStr) return 'N/A';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('es-MX', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      }) + ' ' + date.toLocaleTimeString('es-MX', {
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch (e) {
      return dateStr;
    }
  }
</script>

<?php
$additionalScripts = '';
include __DIR__ . '/includes/footer.php';
?>
