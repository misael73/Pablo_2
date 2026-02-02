# SIREFI - Sistema de Reporte de Fallas e Incidencias

Sistema web para el reporte y seguimiento de fallas e incidencias en instalaciones educativas del TEC Serdán.

## Estructura del Proyecto

```
/
├── config.php                       # Configuración general del sistema
├── index.php                        # Página de inicio de sesión
├── formulario.php                   # Formulario de reporte de incidencias
├── reportar.php                     # Procesa el envío de reportes
├── home.php                         # Dashboard principal (admin)
├── dashboard_materiales.php         # Dashboard de recursos materiales (admin)
├── dashboard_tics.php               # Dashboard de TICs/Informática (admin)
├── gestionar_infraestructura.php    # Gestión de edificios y salones (admin)
├── mis_reportes.php                 # Dashboard de reportantes
├── logout.php                       # Cierre de sesión
├── verificar_google.php             # Verificación de autenticación Google OAuth
├── conexion.php                     # Conexión a base de datos (legacy support)
├── style.css                        # Estilos principales
├── SIREFI.sql                       # Script de creación de base de datos
├── api/                             # Endpoints REST
│   ├── edificios.php                # CRUD de edificios
│   ├── salones.php                  # CRUD de salones
│   └── get_salones.php              # Obtener salones por edificio
├── migrations/                      # Scripts de migración de BD
│   └── add_infrastructure_tables.sql # Agregar tablas de infraestructura
├── includes/
│   ├── db.php                       # Clase de conexión a base de datos
│   ├── auth.php                     # Funciones de autenticación
│   ├── user.php                     # Funciones de gestión de usuarios
│   ├── validation.php               # Funciones de validación y sanitización
│   ├── csrf.php                     # Protección CSRF
│   ├── error_handler.php            # Manejo de errores
│   ├── header.php                   # Componente de cabecera
│   └── footer.php                   # Componente de pie de página
└── uploads/                         # Directorio para archivos subidos
```

## Características

- ✅ Autenticación con Google OAuth
- ✅ Gestión de reportes de incidencias
- ✅ Subida segura de archivos
- ✅ Dashboard con estadísticas
- ✅ Sistema de folios únicos
- ✅ Validación de entradas
- ✅ Estructura modular y organizada
- ✅ **Dashboards especializados** (Materiales, TICs)
- ✅ **Gestión dinámica de infraestructura** (Edificios y Salones)
- ✅ **Gestión de categorías** (Administrador puede agregar/editar categorías)
- ✅ **Control de acceso por roles** (Administrador, Reportante)
- ✅ **API REST** para gestión de recursos

## Requisitos

### Software Requerido

- **PHP 7.4 o superior** con las siguientes extensiones:
  - `sqlsrv` - **REQUERIDA** para conexión con SQL Server
  - `pdo_sqlsrv` - Recomendada
- **SQL Server** (cualquier versión compatible)
- **Servidor web** (Apache/Nginx) o PHP Built-in Server para desarrollo

### Instalación de la Extensión sqlsrv

#### Windows
```bash
# 1. Descargar los drivers de Microsoft para PHP:
# https://docs.microsoft.com/en-us/sql/connect/php/download-drivers-php-sql-server

# 2. Copiar los archivos .dll apropiados a la carpeta de extensiones de PHP
# Ejemplo: C:\php\ext\

# 3. Agregar a php.ini:
extension=php_sqlsrv_84_ts.dll
extension=php_pdo_sqlsrv_84_ts.dll

# 4. Reiniciar el servidor web
```

#### Linux (Ubuntu/Debian)
```bash
# Instalar los drivers de Microsoft
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list

sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql17 mssql-tools

# Instalar la extensión PHP
sudo pecl install sqlsrv pdo_sqlsrv

# Agregar a php.ini
echo "extension=sqlsrv.so" | sudo tee -a /etc/php/8.4/cli/php.ini
echo "extension=pdo_sqlsrv.so" | sudo tee -a /etc/php/8.4/cli/php.ini
```

#### macOS
```bash
# Instalar Homebrew si no está instalado
# Luego instalar PHP y los drivers
brew install php
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew install msodbcsql17 mssql-tools

# Instalar la extensión
sudo pecl install sqlsrv pdo_sqlsrv

# Agregar a php.ini
echo "extension=sqlsrv.so" >> $(php --ini | grep "Loaded Configuration" | sed -e "s|.*:\s*||")
echo "extension=pdo_sqlsrv.so" >> $(php --ini | grep "Loaded Configuration" | sed -e "s|.*:\s*||")
```

### Verificar Instalación

```bash
# Verificar que la extensión esté cargada
php -m | grep sqlsrv

# Debería mostrar:
# sqlsrv
# pdo_sqlsrv
```

## Instalación del Sistema

1. **Configurar la base de datos** ejecutando `SIREFI.sql` en SQL Server
2. **Ejecutar migración de infraestructura** (opcional pero recomendado):
   ```sql
   -- En SQL Server Management Studio, ejecutar:
   -- migrations/add_infrastructure_tables.sql
   ```
   Esto creará las tablas `Edificios` y `Salones` con datos iniciales.
3. **Ajustar credenciales** en `config.php`:
   ```php
   define('DB_SERVER', 'tu_servidor\instancia');
   define('DB_NAME', 'SIREFI');
   define('DB_USER', 'tu_usuario');
   define('DB_PASS', 'tu_contraseña');
   ```
4. **Configurar Google OAuth** en `config.php`:
   ```php
   define('GOOGLE_CLIENT_ID', 'tu_client_id_aqui');
   ```
4. **Asegurar permisos** de escritura en la carpeta `uploads/`:
   ```bash
   chmod 755 uploads/
   ```
5. **Probar la instalación** ejecutando el script de verificación:
   ```bash
   php check_requirements.php
   ```

## Roles y Permisos

### Administrador
- Acceso a todos los dashboards (General, Materiales, TICs)
- Gestión de infraestructura (edificios y salones)
- Ver, editar y exportar todos los reportes
- Asignar técnicos y cambiar estados

### Reportante
- Crear nuevos reportes
- Ver solo sus propios reportes
- Exportar sus reportes a PDF
- **NO** tiene acceso a dashboards administrativos

## Dashboards Especializados

### Dashboard General (`home.php`)
Muestra todos los reportes del sistema con estadísticas globales.

### Dashboard de Recursos Materiales (`dashboard_materiales.php`)
Filtra y muestra solo reportes de:
- Infraestructura (mobiliario, puertas, ventanas)
- Maquinaria de laboratorio
- Aseo de áreas
- Vehicular

### Dashboard de TICs/Informática (`dashboard_tics.php`)
Filtra y muestra solo reportes de:
- Equipo de cómputo y comunicaciones
- Redes y conectividad
- Proyectores
- Software

## Gestión de Infraestructura

Los administradores pueden gestionar edificios y salones desde `gestionar_infraestructura.php`:
- **CRUD completo** de edificios
- **CRUD completo** de salones por edificio
- Activar/desactivar edificios y salones
- Los cambios se reflejan automáticamente en el formulario de reportes

### Validaciones de Negocio
- Los nombres de edificios deben ser únicos
- Los nombres de salones deben ser únicos dentro de cada edificio
- No se puede eliminar edificios que tengan salones asociados
- No se puede desactivar edificios que tengan salones activos

## Gestión de Categorías

Los administradores pueden gestionar las categorías de reportes desde `gestionar_categorias.php`:
- **CRUD completo** de categorías
- Asignar cada categoría a un dashboard específico (Materiales, TICs, Infraestructura, General)
- Personalizar icono, color y descripción de cada categoría
- Activar/desactivar categorías
- Las categorías activas aparecen automáticamente en el formulario de reportes

### Campos de Categoría
- **Nombre**: Identificador de la categoría (único)
- **Tipo de Dashboard**: Clasifica la categoría en uno de los dashboards especializados
- **Descripción**: Texto explicativo que se muestra al usuario
- **Icono**: Clase de Font Awesome (ej: fas fa-wrench)
- **Color**: Color hexadecimal para identificación visual
- **Activo**: Define si está disponible para nuevos reportes

### Validaciones
- Los nombres de categorías deben ser únicos
- No se puede eliminar categorías que tengan reportes activos asociados
- El tipo de dashboard debe ser: materiales, tics, infraestructura o general

## API REST

### Edificios
- `GET api/edificios.php?action=list` - Listar edificios
- `GET api/edificios.php?action=get&id=X` - Obtener edificio
- `POST api/edificios.php?action=create` - Crear edificio
- `PUT api/edificios.php?action=update` - Actualizar edificio
- `DELETE api/edificios.php?action=delete&id=X` - Eliminar edificio

### Salones
- `GET api/salones.php?action=list&edificio_id=X` - Listar salones
- `GET api/salones.php?action=get&id=X` - Obtener salón
- `POST api/salones.php?action=create` - Crear salón
- `PUT api/salones.php?action=update` - Actualizar salón
- `DELETE api/salones.php?action=delete&id=X` - Eliminar salón

### Categorías
- `GET api/categorias.php?action=list` - Listar categorías
- `GET api/categorias.php?action=get&id=X` - Obtener categoría
- `POST api/categorias.php?action=create` - Crear categoría
- `PUT api/categorias.php?action=update&id=X` - Actualizar categoría
- `PUT api/categorias.php?action=toggle&id=X` - Activar/desactivar categoría
- `DELETE api/categorias.php?action=delete&id=X` - Eliminar categoría (soft delete)

### Utilidades
- `GET api/get_salones.php?edificio_id=X` - Obtener salones activos por edificio (para formularios)

## Seguridad

- Validación y sanitización de todas las entradas
- Protección contra SQL injection mediante prepared statements
- Validación de tipos de archivo en uploads
- Límite de tamaño de archivo (2MB)
- Sesiones seguras
