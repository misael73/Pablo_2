# SIREFI - Guía de Desarrollo

## Arquitectura del Sistema

### Estructura de Carpetas

```
/
├── config.php                       # Configuración central del sistema
├── index.php                        # Página de inicio de sesión (público)
├── formulario.php                   # Formulario de nuevo reporte (autenticado)
├── reportar.php                     # Procesa envío de reportes (autenticado)
├── home.php                         # Dashboard principal (admin)
├── dashboard_materiales.php         # Dashboard de recursos materiales (admin)
├── dashboard_tics.php               # Dashboard de TICs/Informática (admin)
├── gestionar_infraestructura.php    # Gestión de edificios y salones (admin)
├── gestionar_categorias.php         # Gestión de categorías de reportes (admin)
├── mis_reportes.php                 # Dashboard de reportantes
├── logout.php                       # Cierre de sesión
├── verificar_google.php             # API: Verificación OAuth de Google
├── conexion.php                     # Legacy: Wrapper de conexión DB
├── guardar_reporte.php              # DEPRECATED: Usar reportar.php
├── style.css                        # Estilos de la página de login
├── SIREFI.sql                       # Script DDL de base de datos
├── api/                             # Endpoints REST
│   ├── edificios.php                # CRUD de edificios (admin)
│   ├── salones.php                  # CRUD de salones (admin)
│   ├── categorias.php               # CRUD de categorías (admin)
│   └── get_salones.php              # Obtener salones por edificio
├── migrations/                      # Scripts de migración de BD
│   └── add_infrastructure_tables.sql # Agregar tablas de infraestructura
├── includes/                        # Módulos reutilizables
│   ├── db.php                       # Singleton de conexión DB
│   ├── auth.php                     # Funciones de autenticación
│   ├── user.php                     # Gestión de usuarios
│   ├── validation.php               # Validación y sanitización
│   ├── error_handler.php            # Manejo de errores y mensajes
│   ├── csrf.php                     # Protección CSRF
│   ├── header.php                   # Componente de cabecera
│   └── footer.php                   # Componente de pie de página
└── uploads/                         # Archivos subidos (excluido del repo)
```

## Convenciones de Código

### Estilo PHP

- **Archivos**: UTF-8 sin BOM
- **Apertura**: Siempre `<?php`, nunca `<?`
- **Cierre**: Opcional al final del archivo (recomendado omitir para evitar whitespace)
- **Indentación**: 4 espacios (no tabs)
- **Llaves**: Estilo K&R (opening brace en misma línea)

### Nombres

- **Archivos**: snake_case (ej: `error_handler.php`)
- **Funciones**: camelCase (ej: `getCurrentUser()`)
- **Clases**: PascalCase (ej: `Database`)
- **Constantes**: UPPER_SNAKE_CASE (ej: `DB_SERVER`)
- **Variables**: snake_case o camelCase según contexto

### Comentarios

```php
/**
 * Descripción de la función
 * @param string $param Descripción del parámetro
 * @return type Descripción del retorno
 */
function myFunction($param) {
    // Comentario de implementación
}
```

## Flujo de Autenticación

1. Usuario accede a `index.php`
2. Click en botón de Google OAuth
3. Google devuelve `id_token` vía JavaScript
4. POST a `verificar_google.php` con el token
5. `verificar_google.php`:
   - Valida token con Google API
   - Busca/crea usuario en BD
   - Establece sesión PHP
6. Redirige a `home.php`

## Flujo de Reporte

1. Usuario autenticado accede a `formulario.php`
2. Completa formulario y envía
3. `reportar.php`:
   - Verifica autenticación
   - Valida CSRF token
   - Valida y sanitiza inputs
   - Valida archivo (si existe)
   - Genera folio único
   - Inserta en BD
   - Redirige con mensaje de éxito/error

## Seguridad

### Protecciones Implementadas

- ✅ **SQL Injection**: Prepared statements en todas las consultas
- ✅ **XSS**: Sanitización con `htmlspecialchars()` en todas las salidas
- ✅ **CSRF**: Tokens únicos por sesión en formularios POST
- ✅ **File Upload**: Validación de tipo, tamaño y extensión
- ✅ **Session Hijacking**: Sesiones seguras con configuración PHP
- ✅ **Authentication**: Verificación en cada página protegida

### Mejoras Pendientes

- [ ] HTTPS enforcement
- [ ] Rate limiting en login
- [ ] Content Security Policy headers
- [ ] Registro de auditoría (logging)
- [ ] Sanitización de nombres de archivo

## Base de Datos

### Tablas Principales

**Usuarios**
- `id`: PK, auto-incremento
- `nombre`: Nombre completo
- `correo`: Email único
- `contraseña`: Hash (no usado para OAuth)
- `rol`: reportante | mantenimiento | administrador

**Reportes**
- `id`: PK, auto-incremento
- `folio`: Identificador único (ej: SIREFI-20250113-0001)
- `id_usuario`: FK a Usuarios
- `area`: Ubicación (Edificio + Aula)
- `tipo`: Categoría de incidencia
- `descripcion`: Detalle del problema
- `archivo`: Ruta opcional de archivo adjunto
- `prioridad`: Alta | Media | Baja
- `estatus`: Recibido | En proceso | Solucionado | Cancelado
- `fecha_reporte`: Timestamp de creación

**Edificios** (Opcional - requiere migración)
- `id`: PK, auto-incremento
- `nombre`: Nombre del edificio (único)
- `descripcion`: Descripción opcional
- `activo`: BIT (1=activo, 0=inactivo)
- `fecha_creacion`: Timestamp

**Salones** (Opcional - requiere migración)
- `id`: PK, auto-incremento
- `id_edificio`: FK a Edificios
- `nombre`: Nombre del salón
- `descripcion`: Descripción opcional
- `activo`: BIT (1=activo, 0=inactivo)
- `fecha_creacion`: Timestamp
- Constraint: Nombre único por edificio

### Consultas Comunes

```sql
-- Reportes de un usuario
SELECT * FROM Reportes WHERE id_usuario = ?

-- Reportes pendientes
SELECT * FROM Reportes WHERE estatus != 'Solucionado'

-- Reportes con información de usuario
SELECT r.*, u.nombre, u.correo 
FROM Reportes r 
JOIN Usuarios u ON r.id_usuario = u.id

-- Reportes filtrados por categoría (Materiales)
SELECT * FROM Reportes 
WHERE tipo IN ('Infraestructura', 'Maquinaria de laboratorio', 'Aseo de las áreas', 'Vehicular')

-- Reportes filtrados por categoría (TICs)
SELECT * FROM Reportes 
WHERE tipo = 'Equipo de cómputo y comunicaciones'

-- Edificios activos con conteo de salones
SELECT e.*, COUNT(s.id) as total_salones
FROM Edificios e
LEFT JOIN Salones s ON e.id = s.id_edificio
WHERE e.activo = 1
GROUP BY e.id, e.nombre, e.descripcion, e.activo, e.fecha_creacion

-- Salones de un edificio específico
SELECT * FROM Salones 
WHERE id_edificio = ? AND activo = 1
ORDER BY nombre
```

## API REST

### Arquitectura de APIs

Todas las APIs están en la carpeta `/api` y siguen estos principios:
- Requieren autenticación (y en algunos casos rol de administrador)
- Devuelven JSON con estructura estándar: `{success: bool, data: object, error: string}`
- Validan CSRF tokens en operaciones POST/PUT/DELETE
- Usan HTTP status codes apropiados

### Endpoints de Edificios (`api/edificios.php`)

**Listar todos los edificios**
```http
GET /api/edificios.php?action=list
Authorization: Session required (Admin)
Response: {success: true, data: [{id, nombre, descripcion, activo, fecha_creacion}]}
```

**Obtener un edificio**
```http
GET /api/edificios.php?action=get&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {id, nombre, descripcion, activo, fecha_creacion}}
```

**Crear edificio**
```http
POST /api/edificios.php?action=create
Authorization: Session required (Admin)
Body: {nombre, descripcion?, activo?, csrf_token}
Response: {success: true, data: {id, message}}
```

**Actualizar edificio**
```http
PUT /api/edificios.php?action=update
Authorization: Session required (Admin)
Body: {id, nombre, descripcion?, activo?, csrf_token}
Response: {success: true, data: {message}}
```

**Eliminar edificio**
```http
DELETE /api/edificios.php?action=delete&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {message}}
Error: 400 si tiene salones asociados
```

### Endpoints de Salones (`api/salones.php`)

**Listar salones**
```http
GET /api/salones.php?action=list&edificio_id=1 (opcional)
Authorization: Session required (Admin)
Response: {success: true, data: [{id, id_edificio, nombre, descripcion, activo, fecha_creacion, edificio_nombre}]}
```

**Obtener un salón**
```http
GET /api/salones.php?action=get&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {id, id_edificio, nombre, descripcion, activo, fecha_creacion, edificio_nombre}}
```

**Crear salón**
```http
POST /api/salones.php?action=create
Authorization: Session required (Admin)
Body: {id_edificio, nombre, descripcion?, activo?, csrf_token}
Response: {success: true, data: {id, message}}
```

**Actualizar salón**
```http
PUT /api/salones.php?action=update
Authorization: Session required (Admin)
Body: {id, id_edificio, nombre, descripcion?, activo?, csrf_token}
Response: {success: true, data: {message}}
```

**Eliminar salón**
```http
DELETE /api/salones.php?action=delete&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {message}}
```

### Endpoints de Categorías (`api/categorias.php`)

**Listar todas las categorías**
```http
GET /api/categorias.php?action=list
Authorization: Session required (Admin)
Response: {success: true, data: [{id, nombre, tipo_dashboard, descripcion, icono, color, activo, fecha_creacion}]}
```

**Obtener una categoría**
```http
GET /api/categorias.php?action=get&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {id, nombre, tipo_dashboard, descripcion, icono, color, activo, fecha_creacion}}
```

**Crear categoría**
```http
POST /api/categorias.php?action=create
Authorization: Session required (Admin)
Body: {nombre, tipo_dashboard, descripcion?, icono?, color?, activo?}
Response: {success: true, data: {id, message}}
Validations:
  - nombre: requerido, único
  - tipo_dashboard: requerido, valores: materiales|tics|infraestructura|general
  - color: formato hexadecimal #RRGGBB
```

**Actualizar categoría**
```http
PUT /api/categorias.php?action=update&id=1
Authorization: Session required (Admin)
Body: {nombre, tipo_dashboard, descripcion?, icono?, color?, activo?}
Response: {success: true, data: {id, message}}
```

**Toggle estado de categoría**
```http
PUT /api/categorias.php?action=toggle&id=1
Authorization: Session required (Admin)
Body: {activo: 0|1}
Response: {success: true, data: {id, activo, message}}
```

**Eliminar categoría (soft delete)**
```http
DELETE /api/categorias.php?action=delete&id=1
Authorization: Session required (Admin)
Response: {success: true, data: {message}}
Error: 400 si tiene reportes activos asociados
```

### Endpoint de Utilidades (`api/get_salones.php`)

**Obtener salones activos por edificio**
```http
GET /api/get_salones.php?edificio_id=1
Authorization: Session required (Any authenticated user)
Response: {success: true, data: [{id, nombre}]}
Usage: Usado en formulario.php para cargar salones dinámicamente
```

## Dashboards Especializados

### Dashboard General (`home.php`)
- Acceso: Solo administradores
- Muestra: Todos los reportes del sistema
- Estadísticas: Total, En proceso, Resueltos, Retrasados
- Gráficos: Tendencia mensual, Distribución por tipo

### Dashboard de Materiales (`dashboard_materiales.php`)
- Acceso: Solo administradores
- Filtro: `tipo IN ('Infraestructura', 'Maquinaria de laboratorio', 'Aseo de las áreas', 'Vehicular')`
- Estadísticas: Calculadas solo para reportes filtrados
- Color: Naranja/Amber

### Dashboard de TICs (`dashboard_tics.php`)
- Acceso: Solo administradores
- Filtro: `tipo = 'Equipo de cómputo y comunicaciones'`
- Estadísticas: Calculadas solo para reportes filtrados
- Color: Azul

## Gestión de Infraestructura

### Página de Gestión (`gestionar_infraestructura.php`)
- Acceso: Solo administradores
- Secciones: Edificios y Salones
- Operaciones: CRUD completo con modals de Bootstrap
- Validaciones: Frontend (JavaScript) y Backend (PHP/SQL)
SELECT r.*, u.nombre, u.correo 
FROM Reportes r 
JOIN Usuarios u ON r.id_usuario = u.id
```

## Testing

### Validación Manual

1. **Login**: Verificar OAuth funciona
2. **Crear Reporte**: Probar con/sin archivo
3. **Ver Reportes**: Dashboard muestra datos correctos
4. **Logout**: Limpia sesión y redirige

### Casos de Prueba

- [ ] Login exitoso con Google
- [ ] Crear reporte sin archivo
- [ ] Crear reporte con imagen válida
- [ ] Crear reporte con archivo inválido (> 2MB)
- [ ] Crear reporte con tipo inválido (.exe)
- [ ] Acceso sin autenticación redirige a login
- [ ] CSRF inválido rechaza formulario
- [ ] Logout limpia sesión

## Configuración

### Variables de Entorno (config.php)

```php
// Base de datos
DB_SERVER        // Servidor SQL Server
DB_NAME          // Nombre de la BD
DB_USER          // Usuario de BD
DB_PASS          // Contraseña de BD

// OAuth
GOOGLE_CLIENT_ID // Client ID de Google Console

// Uploads
UPLOAD_DIR       // Directorio de archivos
MAX_FILE_SIZE    // Tamaño máximo (bytes)
ALLOWED_EXTENSIONS // Array de extensiones permitidas
```

### Producción

1. **Error Reporting**: Cambiar a `0` en `config.php`
2. **HTTPS**: Configurar en servidor web
3. **Permisos**: `chmod 755 uploads/` 
4. **Backups**: Configurar backup automático de BD
5. **Logs**: Configurar rotación de logs

## Solución de Problemas

### Error: "Call to undefined function sqlsrv_connect()"

**Causa**: La extensión PHP `sqlsrv` no está instalada.

**Solución**:
1. Verificar instalación: `php -m | grep sqlsrv`
2. Si no aparece, instalar la extensión según tu sistema operativo (ver README.md)
3. Para desarrollo local con PHP built-in server, reiniciar el servidor después de instalar
4. Ejecutar `php check_requirements.php` para verificar

### Error: "Database connection failed"

**Causa**: Credenciales incorrectas o SQL Server no accesible.

**Solución**:
1. Verificar que SQL Server esté ejecutándose
2. Revisar credenciales en `config.php`
3. Verificar que la base de datos SIREFI exista
4. Comprobar conectividad de red al servidor
5. Revisar logs de error en `error_log` o archivo de log del servidor web

### Archivos no se suben

**Causa**: Directorio `uploads/` sin permisos de escritura.

**Solución**:
```bash
chmod 755 uploads/
# O en algunos casos
chmod 777 uploads/  # Solo para desarrollo, NO en producción
```

### Sesión no se mantiene

**Causa**: Configuración de sesiones PHP incorrecta.

**Solución**:
1. Verificar que `session.save_path` en php.ini sea escribible
2. Verificar permisos del directorio de sesiones
3. En desarrollo local, usar: `php -S localhost:8000` desde el directorio raíz

## Contribuir

1. Fork el repositorio
2. Crear branch: `git checkout -b feature/nueva-funcionalidad`
3. Commit cambios: `git commit -m 'Agregar nueva funcionalidad'`
4. Push: `git push origin feature/nueva-funcionalidad`
5. Crear Pull Request

## Soporte

Para problemas o preguntas:
- Crear issue en GitHub
- Consultar la sección "Solución de Problemas" arriba
- Ejecutar `php check_requirements.php` para diagnóstico automático
- Contactar al equipo de desarrollo
