<?php
/**
 * Category Management - CRUD for Categorias
 */

require_once __DIR__ . '/config.php';
require_once __DIR__ . '/includes/auth.php';
require_once __DIR__ . '/includes/error_handler.php';
require_once __DIR__ . '/includes/csrf.php';
require_once __DIR__ . '/conexion.php';

// Require admin access
requireAdmin();

$usuario = getCurrentUser();
$pageTitle = 'Gestionar Categorías';

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
  }

  .btn-add {
    background-color: #28a745;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 8px;
    font-weight: 600;
    transition: all 0.3s;
  }

  .btn-add:hover {
    background-color: #218838;
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(40, 167, 69, 0.3);
  }

  .table {
    border-collapse: separate;
    border-spacing: 0;
  }

  .table thead th {
    background-color: #003366;
    color: white;
    font-weight: 600;
    text-transform: uppercase;
    font-size: 13px;
    padding: 15px;
    border: none;
  }

  .table tbody tr {
    transition: all 0.3s;
  }

  .table tbody tr:hover {
    background-color: #f8f9fa;
    transform: scale(1.01);
  }

  .badge-dashboard {
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
  }

  .badge-materiales {
    background-color: #ff6b6b;
    color: white;
  }

  .badge-tics {
    background-color: #4dabf7;
    color: white;
  }

  .badge-infraestructura {
    background-color: #51cf66;
    color: white;
  }

  .badge-general {
    background-color: #868e96;
    color: white;
  }

  .btn-action {
    padding: 6px 12px;
    border: none;
    border-radius: 6px;
    font-size: 13px;
    transition: all 0.3s;
    margin: 2px;
  }

  .btn-edit {
    background-color: #ffc107;
    color: #000;
  }

  .btn-edit:hover {
    background-color: #e0a800;
  }

  .btn-toggle {
    background-color: #6c757d;
    color: white;
  }

  .btn-toggle:hover {
    background-color: #5a6268;
  }

  .status-active {
    color: #28a745;
    font-weight: bold;
  }

  .status-inactive {
    color: #dc3545;
    font-weight: bold;
  }

  .modal-content {
    border-radius: 15px;
  }

  .modal-header {
    background-color: #003366;
    color: white;
    border-radius: 15px 15px 0 0;
  }

  .color-preview {
    width: 30px;
    height: 30px;
    border-radius: 5px;
    border: 2px solid #dee2e6;
    display: inline-block;
    vertical-align: middle;
  }
</style>
HTML;

include __DIR__ . '/includes/header.php';
?>

<div class="container">
  <div class="row">
    <div class="col-12">
      <div class="mb-3">
        <a href="home.php" class="btn btn-secondary">
          <i class="fas fa-arrow-left"></i> Volver al Dashboard
        </a>
      </div>
    </div>
  </div>

  <?php displayAlerts(); ?>

  <!-- Categorías Section -->
  <div class="section-card">
    <div class="section-header">
      <div>
        <div class="section-title">
          <i class="fas fa-tags"></i> Gestión de Categorías
        </div>
        <small class="text-muted">Administrar las categorías de reportes del sistema</small>
      </div>
      <button class="btn btn-add" data-bs-toggle="modal" data-bs-target="#modalCategoria">
        <i class="fas fa-plus"></i> Nueva Categoría
      </button>
    </div>

    <div class="table-responsive">
      <table class="table table-hover align-middle" id="tableCategorias">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Dashboard</th>
            <th>Descripción</th>
            <th>Color</th>
            <th>Icono</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody id="categoriasTableBody">
          <tr>
            <td colspan="8" class="text-center">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</div>

<!-- Modal Categoría -->
<div class="modal fade" id="modalCategoria" tabindex="-1">
  <div class="modal-dialog modal-lg">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="modalCategoriaTitle">
          <i class="fas fa-tag"></i> Nueva Categoría
        </h5>
        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <form id="formCategoria">
          <input type="hidden" id="categoriaId" name="id">
          
          <div class="row">
            <div class="col-md-8">
              <div class="mb-3">
                <label for="categoriaNombre" class="form-label">Nombre *</label>
                <input type="text" class="form-control" id="categoriaNombre" name="nombre" required maxlength="100">
              </div>
            </div>
            
            <div class="col-md-4">
              <div class="mb-3">
                <label for="categoriaTipoDashboard" class="form-label">Dashboard *</label>
                <select class="form-select" id="categoriaTipoDashboard" name="tipo_dashboard" required>
                  <option value="">Seleccionar...</option>
                  <option value="materiales">Materiales</option>
                  <option value="tics">TICs</option>
                  <option value="infraestructura">Infraestructura</option>
                  <option value="general">General</option>
                </select>
              </div>
            </div>
          </div>

          <div class="mb-3">
            <label for="categoriaDescripcion" class="form-label">Descripción</label>
            <textarea class="form-control" id="categoriaDescripcion" name="descripcion" rows="3" maxlength="500"></textarea>
            <small class="text-muted">Esta descripción se mostrará al usuario cuando seleccione la categoría</small>
          </div>

          <div class="row">
            <div class="col-md-6">
              <div class="mb-3">
                <label for="categoriaIcono" class="form-label">Icono (Font Awesome)</label>
                <input type="text" class="form-control" id="categoriaIcono" name="icono" maxlength="50" placeholder="fas fa-wrench">
                <small class="text-muted">Ejemplo: fas fa-wrench, fas fa-laptop, fas fa-tools</small>
              </div>
            </div>
            
            <div class="col-md-6">
              <div class="mb-3">
                <label for="categoriaColor" class="form-label">Color</label>
                <div class="input-group">
                  <input type="color" class="form-control form-control-color" id="categoriaColor" name="color" value="#0d6efd">
                  <input type="text" class="form-control" id="categoriaColorText" maxlength="7" placeholder="#0d6efd">
                </div>
              </div>
            </div>
          </div>

          <div class="form-check mb-3">
            <input class="form-check-input" type="checkbox" id="categoriaActivo" name="activo" checked>
            <label class="form-check-label" for="categoriaActivo">
              Activo (disponible para reportes)
            </label>
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
        <button type="button" class="btn btn-primary" id="btnGuardarCategoria">
          <i class="fas fa-save"></i> Guardar
        </button>
      </div>
    </div>
  </div>
</div>

<script>
// Global variables
let categorias = [];

// Load categorias on page load
document.addEventListener('DOMContentLoaded', function() {
  loadCategorias();
  
  // Sync color inputs
  document.getElementById('categoriaColor').addEventListener('input', function(e) {
    document.getElementById('categoriaColorText').value = e.target.value;
  });
  
  document.getElementById('categoriaColorText').addEventListener('input', function(e) {
    if (/^#[0-9A-F]{6}$/i.test(e.target.value)) {
      document.getElementById('categoriaColor').value = e.target.value;
    }
  });
  
  // Reset form when modal is fully hidden
  document.getElementById('modalCategoria').addEventListener('hidden.bs.modal', function() {
    document.getElementById('formCategoria').reset();
    document.getElementById('categoriaId').value = '';
    document.getElementById('categoriaActivo').checked = true;
    document.getElementById('categoriaColor').value = '#0d6efd';
    document.getElementById('categoriaColorText').value = '#0d6efd';
  });
});

// Load all categorias
function loadCategorias() {
  fetch('api/categorias.php?action=list')
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        categorias = data.data;
        renderCategoriasTable();
      } else {
        showAlert('danger', 'Error al cargar categorías: ' + (data.message || 'Error desconocido'));
      }
    })
    .catch(error => {
      console.error('Error:', error);
      showAlert('danger', 'Error al cargar categorías');
    });
}

// Render categorias table
function renderCategoriasTable() {
  const tbody = document.getElementById('categoriasTableBody');
  
  if (categorias.length === 0) {
    tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No hay categorías registradas</td></tr>';
    return;
  }
  
  tbody.innerHTML = categorias.map(cat => `
    <tr>
      <td>${cat.id}</td>
      <td><strong>${escapeHtml(cat.nombre)}</strong></td>
      <td>
        <span class="badge-dashboard badge-${cat.tipo_dashboard || 'general'}">
          ${escapeHtml(cat.tipo_dashboard || 'general').toUpperCase()}
        </span>
      </td>
      <td>${cat.descripcion ? escapeHtml(cat.descripcion) : '<em class="text-muted">Sin descripción</em>'}</td>
      <td>
        ${cat.color ? `<span class="color-preview" style="background-color: ${escapeHtml(cat.color)}"></span> ${escapeHtml(cat.color)}` : '-'}
      </td>
      <td>
        ${cat.icono ? `<i class="${escapeHtml(cat.icono)}"></i> ${escapeHtml(cat.icono)}` : '-'}
      </td>
      <td>
        <span class="${cat.activo ? 'status-active' : 'status-inactive'}">
          ${cat.activo ? '✓ Activo' : '✗ Inactivo'}
        </span>
      </td>
      <td>
        <button class="btn btn-action btn-edit" onclick="editCategoria(${cat.id})" title="Editar">
          <i class="fas fa-edit"></i>
        </button>
        <button class="btn btn-action btn-toggle" onclick="toggleCategoria(${cat.id}, ${cat.activo})" title="${cat.activo ? 'Desactivar' : 'Activar'}">
          <i class="fas fa-${cat.activo ? 'toggle-on' : 'toggle-off'}"></i>
        </button>
      </td>
    </tr>
  `).join('');
}

// Open modal for new categoria
document.getElementById('modalCategoria').addEventListener('show.bs.modal', function(e) {
  if (!e.relatedTarget || !e.relatedTarget.dataset.edit) {
    // New categoria
    document.getElementById('modalCategoriaTitle').innerHTML = '<i class="fas fa-tag"></i> Nueva Categoría';
    document.getElementById('formCategoria').reset();
    document.getElementById('categoriaId').value = '';
    document.getElementById('categoriaActivo').checked = true;
    document.getElementById('categoriaColor').value = '#0d6efd';
    document.getElementById('categoriaColorText').value = '#0d6efd';
  }
});

// Edit categoria
function editCategoria(id) {
  const cat = categorias.find(c => c.id === id);
  if (!cat) return;
  
  document.getElementById('modalCategoriaTitle').innerHTML = '<i class="fas fa-edit"></i> Editar Categoría';
  document.getElementById('categoriaId').value = cat.id;
  document.getElementById('categoriaNombre').value = cat.nombre;
  document.getElementById('categoriaTipoDashboard').value = cat.tipo_dashboard || '';
  document.getElementById('categoriaDescripcion').value = cat.descripcion || '';
  document.getElementById('categoriaIcono').value = cat.icono || '';
  document.getElementById('categoriaColor').value = cat.color || '#0d6efd';
  document.getElementById('categoriaColorText').value = cat.color || '#0d6efd';
  document.getElementById('categoriaActivo').checked = cat.activo;
  
  new bootstrap.Modal(document.getElementById('modalCategoria')).show();
}

// Flag to prevent concurrent submissions
let isSavingCategoria = false;

// Save categoria
document.getElementById('btnGuardarCategoria').addEventListener('click', function() {
  // Prevent concurrent submissions
  if (isSavingCategoria) {
    console.log('Ya hay una operación en curso');
    return;
  }
  
  const btnSave = this;
  const form = document.getElementById('formCategoria');
  if (!form.checkValidity()) {
    form.reportValidity();
    return;
  }
  
  const id = document.getElementById('categoriaId').value;
  const data = {
    nombre: document.getElementById('categoriaNombre').value.trim(),
    tipo_dashboard: document.getElementById('categoriaTipoDashboard').value,
    descripcion: document.getElementById('categoriaDescripcion').value.trim(),
    icono: document.getElementById('categoriaIcono').value.trim(),
    color: document.getElementById('categoriaColor').value,
    activo: document.getElementById('categoriaActivo').checked ? 1 : 0
  };
  
  const url = id ? `api/categorias.php?action=update&id=${id}` : 'api/categorias.php?action=create';
  const method = id ? 'PUT' : 'POST';
  
  // Set flag and disable button to prevent double submission
  isSavingCategoria = true;
  btnSave.disabled = true;
  btnSave.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Guardando...';
  
  fetch(url, {
    method: method,
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  })
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      showAlert('success', data.message || (id ? 'Categoría actualizada correctamente' : 'Categoría creada correctamente'));
      bootstrap.Modal.getInstance(document.getElementById('modalCategoria')).hide();
      // Reset form after successful save
      document.getElementById('formCategoria').reset();
      document.getElementById('categoriaId').value = '';
      loadCategorias();
    } else {
      showAlert('danger', 'Error: ' + (data.message || 'Error desconocido'));
    }
  })
  .catch(error => {
    console.error('Error:', error);
    showAlert('danger', 'Error al guardar la categoría');
  })
  .finally(() => {
    // Re-enable button and clear flag
    isSavingCategoria = false;
    btnSave.disabled = false;
    btnSave.innerHTML = '<i class="fas fa-save"></i> Guardar';
  });
});

// Toggle categoria active status
function toggleCategoria(id, currentStatus) {
  const newStatus = currentStatus ? 0 : 1;
  const action = newStatus ? 'activar' : 'desactivar';
  
  if (!confirm(`¿Estás seguro de ${action} esta categoría?`)) {
    return;
  }
  
  fetch(`api/categorias.php?action=toggle&id=${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ activo: newStatus })
  })
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      showAlert('success', data.message || 'Estado actualizado correctamente');
      loadCategorias();
    } else {
      showAlert('danger', 'Error: ' + (data.message || 'Error desconocido'));
    }
  })
  .catch(error => {
    console.error('Error:', error);
    showAlert('danger', 'Error al actualizar el estado');
  });
}

// Helper function to escape HTML
function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

// Helper function to show alerts
function showAlert(type, message) {
  const alertDiv = document.createElement('div');
  alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
  alertDiv.innerHTML = `
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
  `;
  
  const container = document.querySelector('.container');
  container.insertBefore(alertDiv, container.firstChild);
  
  // Auto dismiss after 5 seconds
  setTimeout(() => {
    alertDiv.remove();
  }, 5000);
}
</script>

<?php include __DIR__ . '/includes/footer.php'; ?>
