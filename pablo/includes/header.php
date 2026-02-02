<?php
/**
 * Common Header Component
 * Includes navigation and user info
 */

if (!function_exists('getCurrentUser')) {
    require_once __DIR__ . '/auth.php';
}

$usuario = getCurrentUser();
$header_userRole = getUserRole();
// Set home page based on role - admins go to home.php, technicians can go to their specialized dashboards, reportantes to their reports
$header_homePage = ($header_userRole === 'administrador' || $header_userRole === 'tecnico') ? 'home.php' : 'mis_reportes.php';
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo isset($pageTitle) ? htmlspecialchars($pageTitle) . ' - SIREFI' : 'SIREFI'; ?></title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <?php if (isset($additionalStyles)) echo $additionalStyles; ?>
</head>
<body>
    <!-- Navigation Bar -->
    <nav class="navbar navbar-expand-lg navbar-dark" style="background-color: #003366;">
        <div class="container-fluid">
            <a class="navbar-brand" href="<?php echo $header_homePage; ?>">
                <i class="fas fa-tools"></i> SIREFI
            </a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link" href="<?php echo $header_homePage; ?>">
                            <i class="fas fa-home"></i> Inicio
                        </a>
                    </li>
                    <?php if ($header_userRole === 'administrador' || $header_userRole === 'tecnico'): ?>
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="dashboardsDropdown" role="button" data-bs-toggle="dropdown">
                            <i class="fas fa-chart-line"></i> Dashboards
                        </a>
                        <ul class="dropdown-menu">
                            <?php if ($header_userRole === 'administrador'): ?>
                            <li><a class="dropdown-item" href="home.php">
                                <i class="fas fa-tachometer-alt"></i> General
                            </a></li>
                            <?php endif; ?>
                            <li><a class="dropdown-item" href="dashboard_materiales.php">
                                <i class="fas fa-tools"></i> Recursos Materiales
                            </a></li>
                            <li><a class="dropdown-item" href="dashboard_tics.php">
                                <i class="fas fa-laptop-code"></i> TICs/Informática
                            </a></li>
                        </ul>
                    </li>
                    <?php endif; ?>
                    <?php if ($header_userRole === 'administrador'): ?>
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="adminDropdown" role="button" data-bs-toggle="dropdown">
                            <i class="fas fa-cog"></i> Administración
                        </a>
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="gestionar_infraestructura.php">
                                <i class="fas fa-building"></i> Infraestructura
                            </a></li>
                            <li><a class="dropdown-item" href="gestionar_categorias.php">
                                <i class="fas fa-tags"></i> Categorías
                            </a></li>
                        </ul>
                    </li>
                    <?php endif; ?>
                    <li class="nav-item">
                        <a class="nav-link" href="formulario.php">
                            <i class="fas fa-plus-circle"></i> Nuevo Reporte
                        </a>
                    </li>
                </ul>
                <ul class="navbar-nav">
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-bs-toggle="dropdown">
                            <?php if (isset($usuario['foto'])): ?>
                                <img src="<?php echo htmlspecialchars($usuario['foto']); ?>" 
                                     alt="Avatar" 
                                     class="rounded-circle" 
                                     style="width: 30px; height: 30px; object-fit: cover;">
                            <?php endif; ?>
                            <?php echo htmlspecialchars($usuario['nombre']); ?>
                        </a>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <li><a class="dropdown-item" href="logout.php">
                                <i class="fas fa-sign-out-alt"></i> Cerrar sesión
                            </a></li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
