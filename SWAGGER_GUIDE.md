# Guía de Swagger UI

## ¿Qué es Swagger?

Swagger UI es una herramienta interactiva que permite visualizar y probar todos los endpoints de la API sin necesidad de usar herramientas externas como Postman o curl.

## Cómo Acceder

### 1. Iniciar el Backend

```bash
cd Sirefi
dotnet run
```

### 2. Abrir Swagger UI en el Navegador

**Opciones**:
- HTTP: `http://localhost:5201/swagger`
- HTTPS: `https://localhost:7186/swagger`

## Características de Swagger UI

### 📋 Ver Todos los Endpoints

Swagger muestra automáticamente todos los endpoints de la API organizados por controladores:

- **Auth** - Autenticación y Google OAuth
- **Categorias** - Gestión de categorías
- **Edificios** - Gestión de edificios
- **Reportes** - CRUD de reportes
- **Salones** - Gestión de salones

### 🔍 Ver Detalles de Endpoints

Para cada endpoint puedes ver:
- Método HTTP (GET, POST, PUT, DELETE)
- URL completa
- Parámetros requeridos
- Esquema de request (lo que envías)
- Esquema de response (lo que recibes)
- Códigos de estado HTTP
- Requisitos de autenticación

### ▶️ Probar Endpoints

#### Ejemplo: Probar GET /api/reportes

1. Encuentra el endpoint `GET /api/reportes` en la lista
2. Haz clic en el endpoint para expandirlo
3. Haz clic en el botón "Try it out"
4. Ajusta los parámetros si los hay
5. Haz clic en "Execute"
6. Ve la respuesta completa debajo

#### Ejemplo: Probar POST /api/reportes

1. Encuentra el endpoint `POST /api/reportes`
2. Haz clic en "Try it out"
3. Edita el JSON de ejemplo con tus datos:
```json
{
  "titulo": "Problema con el proyector",
  "descripcion": "El proyector del salón no enciende",
  "edificioId": 1,
  "salonId": 5,
  "categoriaId": 2,
  "prioridad": "media"
}
```
4. Haz clic en "Execute"
5. Ve la respuesta (201 Created si fue exitoso)

### 📊 Ver Esquemas de Datos

En la parte inferior de Swagger UI hay una sección "Schemas" que muestra:
- Estructura de todos los DTOs
- Tipos de datos de cada campo
- Campos requeridos vs opcionales
- Validaciones

## Endpoints Principales

### Autenticación

**POST /api/auth/google**
- Autentica con Google OAuth
- Body: `{ "token": "google_jwt_token" }`
- Response: Usuario autenticado

**POST /api/auth/logout**
- Cierra sesión
- No requiere body

**GET /api/auth/me**
- Obtiene información del usuario actual
- Requiere estar autenticado

### Reportes

**GET /api/reportes**
- Lista todos los reportes
- Query params opcionales: status, prioridad

**GET /api/reportes/stats**
- Obtiene estadísticas de reportes
- Útil para dashboards

**POST /api/reportes**
- Crea un nuevo reporte
- Requiere autenticación

**GET /api/reportes/{id}**
- Obtiene un reporte específico

**PUT /api/reportes/{id}**
- Actualiza un reporte existente
- Requiere autenticación y permisos

**DELETE /api/reportes/{id}**
- Elimina un reporte
- Solo administradores

### Edificios y Salones

**GET /api/edificios**
- Lista todos los edificios

**GET /api/salones**
- Lista todos los salones
- Query param opcional: edificioId

### Categorías

**GET /api/categorias**
- Lista todas las categorías de reportes

**GET /api/categorias/{id}/reportes**
- Obtiene reportes filtrados por categoría

## Autenticación en Swagger

Para probar endpoints que requieren autenticación:

### Opción 1: Usar Cookie de Navegador

1. Abre la aplicación frontend en otra pestaña
2. Inicia sesión con Google
3. Vuelve a Swagger
4. Los endpoints autenticados deberían funcionar automáticamente

### Opción 2: Probar sin Autenticación

Algunos endpoints públicos no requieren autenticación:
- GET /api/edificios
- GET /api/salones
- GET /api/categorias

## Códigos de Estado HTTP

Swagger muestra los códigos que cada endpoint puede devolver:

- **200 OK** - Solicitud exitosa
- **201 Created** - Recurso creado exitosamente
- **204 No Content** - Éxito sin contenido de respuesta
- **400 Bad Request** - Datos inválidos
- **401 Unauthorized** - No autenticado
- **403 Forbidden** - Sin permisos
- **404 Not Found** - Recurso no encontrado
- **500 Internal Server Error** - Error del servidor

## Ejemplos de Uso

### Ejemplo 1: Ver Todos los Reportes

```
1. GET /api/reportes
2. Click "Try it out"
3. Click "Execute"
4. Ver lista de reportes en Response
```

### Ejemplo 2: Crear Reporte

```
1. POST /api/reportes
2. Click "Try it out"
3. Editar JSON:
{
  "titulo": "Luz fundida",
  "descripcion": "La luz del salón 101 no funciona",
  "edificioId": 1,
  "salonId": 3,
  "categoriaId": 1,
  "prioridad": "alta"
}
4. Click "Execute"
5. Ver reporte creado en Response
```

### Ejemplo 3: Obtener Estadísticas

```
1. GET /api/reportes/stats
2. Click "Try it out"
3. Click "Execute"
4. Ver estadísticas:
{
  "total": 45,
  "recibidos": 12,
  "enProceso": 20,
  "resueltos": 13
}
```

## Ventajas de Usar Swagger

✅ **No Necesitas Postman** - Todo en el navegador
✅ **Documentación Automática** - Siempre actualizada
✅ **Pruebas Rápidas** - Un clic para probar
✅ **Ver Esquemas** - Sabes exactamente qué enviar
✅ **Códigos de Error** - Ves qué puede fallar
✅ **Ejemplos Incluidos** - JSON de ejemplo ya listo

## Problemas Comunes

### Swagger no Carga

**Problema**: La página /swagger no carga

**Soluciones**:
1. Verifica que el backend esté corriendo
2. Verifica la URL (http://localhost:5201/swagger)
3. Revisa la consola del backend por errores
4. Verifica que estés en modo Development

### Endpoint da 401 Unauthorized

**Problema**: El endpoint requiere autenticación

**Solución**:
1. Inicia sesión en el frontend primero
2. O usa endpoints públicos (GET edificios, salones, categorías)

### No Puedo Ver los Datos

**Problema**: Response está vacío o es null

**Soluciones**:
1. Verifica que la base de datos tenga datos
2. Revisa los logs del backend
3. Verifica los parámetros enviados

## Swagger vs Postman

| Característica | Swagger | Postman |
|---------------|---------|---------|
| Instalación | No requiere | Requiere instalación |
| Documentación | Automática | Manual |
| Actualización | Automática | Manual |
| Compartir | URL pública | Exportar colección |
| Probar | ✅ | ✅ |
| Organizar | ✅ | ✅ Mejor |
| Guardar peticiones | ❌ | ✅ |

**Conclusión**: Usa Swagger para desarrollo rápido y documentación. Usa Postman para tests más complejos y organizados.

## Configuración Avanzada

### Personalizar Swagger UI

En `Program.cs`, puedes personalizar:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "SIREFI API", 
        Version = "v1",
        Description = "Tu descripción aquí",
        Contact = new() { Name = "Tu Nombre", Email = "email@example.com" }
    });
    
    // Añadir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### Agregar Seguridad a Swagger

Para probar con JWT tokens:

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "bearer"
});
```

## Recursos Adicionales

- **Documentación Oficial**: https://swagger.io/tools/swagger-ui/
- **Swashbuckle Docs**: https://github.com/domaindrivendev/Swashbuckle.AspNetCore
- **OpenAPI Spec**: https://spec.openapis.org/oas/v3.0.0

## Conclusión

Swagger UI es una herramienta esencial para desarrollo de APIs:

✅ Documentación automática y siempre actualizada
✅ Pruebas rápidas sin herramientas externas
✅ Validación visual de requests/responses
✅ Compartir fácilmente con el equipo
✅ Estándar de la industria

**¡Empieza a usarlo ahora mismo en /swagger!** 🚀
