using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sirefi.Models;

namespace Sirefi.Data;

public partial class FormsDbContext : DbContext
{
    public FormsDbContext()
    {
    }

    public FormsDbContext(DbContextOptions<FormsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Archivo> Archivos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<Edificio> Edificios { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<HistorialEstado> HistorialEstados { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Prioridade> Prioridades { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Salone> Salones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwReportesCompleto> VwReportesCompletos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Archivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Archivos__3213E83F0C2C74CE");

            entity.HasIndex(e => e.IdReporte, "IX_Archivos_IdReporte").HasFilter("([eliminado]=(0))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Eliminado).HasColumnName("eliminado");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdComentario).HasColumnName("id_comentario");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.NombreOriginal)
                .HasMaxLength(255)
                .HasColumnName("nombre_original");
            entity.Property(e => e.Ruta)
                .HasMaxLength(500)
                .HasColumnName("ruta");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.TipoMime)
                .HasMaxLength(100)
                .HasColumnName("tipo_mime");

            entity.HasOne(d => d.IdComentarioNavigation).WithMany(p => p.Archivos)
                .HasForeignKey(d => d.IdComentario)
                .HasConstraintName("FK_Archivos_Comentario");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.Archivos)
                .HasForeignKey(d => d.IdReporte)
                .HasConstraintName("FK_Archivos_Reporte");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Archivos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Archivos_Usuario");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3213E83F3466C8F9");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__72AFBCC642B7CD96").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasColumnName("color");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.TipoDashboard)
                .HasMaxLength(50)
                .HasColumnName("tipo_dashboard");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comentar__3213E83F4475A049");

            entity.HasIndex(e => e.IdReporte, "IX_Comentarios_IdReporte").HasFilter("([eliminado]=(0))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comentario1).HasColumnName("comentario");
            entity.Property(e => e.Editado).HasColumnName("editado");
            entity.Property(e => e.Eliminado).HasColumnName("eliminado");
            entity.Property(e => e.FechaComentario)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_comentario");
            entity.Property(e => e.FechaEdicion).HasColumnName("fecha_edicion");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Publico)
                .HasDefaultValue(true)
                .HasColumnName("publico");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasDefaultValue("comentario")
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdReporte)
                .HasConstraintName("FK_Comentarios_Reporte");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comentarios_Usuario");
        });

        modelBuilder.Entity<Edificio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Edificio__3213E83F58F9F29E");

            entity.HasIndex(e => e.Nombre, "UQ_Edificio_Nombre").IsUnique();

            entity.HasIndex(e => e.Codigo, "UQ__Edificio__40F9A20678D91774").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoPor).HasColumnName("actualizado_por");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Pisos).HasColumnName("pisos");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(255)
                .HasColumnName("ubicacion");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Estados__3213E83FE03DD0C3");

            entity.HasIndex(e => e.Nombre, "UQ__Estados__72AFBCC6F9090F5D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasColumnName("color");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsFinal).HasColumnName("es_final");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden).HasColumnName("orden");
        });

        modelBuilder.Entity<HistorialEstado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Historia__3213E83F60F7FDAB");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comentario)
                .HasMaxLength(500)
                .HasColumnName("comentario");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.IdEstadoAnterior).HasColumnName("id_estado_anterior");
            entity.Property(e => e.IdEstadoNuevo).HasColumnName("id_estado_nuevo");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdEstadoAnteriorNavigation).WithMany(p => p.HistorialEstadoIdEstadoAnteriorNavigations)
                .HasForeignKey(d => d.IdEstadoAnterior)
                .HasConstraintName("FK_HistEstados_EstadoAnt");

            entity.HasOne(d => d.IdEstadoNuevoNavigation).WithMany(p => p.HistorialEstadoIdEstadoNuevoNavigations)
                .HasForeignKey(d => d.IdEstadoNuevo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistEstados_EstadoNuevo");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdReporte)
                .HasConstraintName("FK_HistEstados_Reporte");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistEstados_Usuario");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83FF9600BC7");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaLeido).HasColumnName("fecha_leido");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leido).HasColumnName("leido");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(500)
                .HasColumnName("mensaje");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdReporte)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notif_Reporte");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notif_Usuario");
        });

        modelBuilder.Entity<Prioridade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Priorida__3213E83F5BD54263");

            entity.HasIndex(e => e.Nombre, "UQ__Priorida__72AFBCC62C025064").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasColumnName("color");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nivel).HasColumnName("nivel");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reportes__3213E83FBD375856");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateReporteTimestamp"));

            entity.HasIndex(e => e.Eliminado, "IX_Reportes_Eliminado");

            entity.HasIndex(e => e.IdAsignadoA, "IX_Reportes_IdAsignado").HasFilter("([eliminado]=(0))");

            entity.HasIndex(e => e.IdEstado, "IX_Reportes_IdEstado").HasFilter("([eliminado]=(0))");

            entity.HasIndex(e => e.IdReportante, "IX_Reportes_IdReportante").HasFilter("([eliminado]=(0))");

            entity.HasIndex(e => e.Folio, "UQ__Reportes__E8F12C9F2B75C4B5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoPor).HasColumnName("actualizado_por");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.ComentarioCalificacion)
                .HasMaxLength(500)
                .HasColumnName("comentario_calificacion");
            entity.Property(e => e.CostoEstimado)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("costo_estimado");
            entity.Property(e => e.CostoReal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("costo_real");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Eliminado).HasColumnName("eliminado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            entity.Property(e => e.FechaFinalizacion).HasColumnName("fecha_finalizacion");
            entity.Property(e => e.FechaInicioAtencion).HasColumnName("fecha_inicio_atencion");
            entity.Property(e => e.FechaReporte)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_reporte");
            entity.Property(e => e.Folio)
                .HasMaxLength(30)
                .HasColumnName("folio");
            entity.Property(e => e.IdAsignadoA).HasColumnName("id_asignado_a");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdEdificio).HasColumnName("id_edificio");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdPrioridad).HasColumnName("id_prioridad");
            entity.Property(e => e.IdReportante).HasColumnName("id_reportante");
            entity.Property(e => e.IdSalon).HasColumnName("id_salon");
            entity.Property(e => e.Subcategoria)
                .HasMaxLength(100)
                .HasColumnName("subcategoria");
            entity.Property(e => e.TiempoResolucionMinutos).HasColumnName("tiempo_resolucion_minutos");
            entity.Property(e => e.TiempoRespuestaMinutos).HasColumnName("tiempo_respuesta_minutos");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");
            entity.Property(e => e.UbicacionAdicional)
                .HasMaxLength(255)
                .HasColumnName("ubicacion_adicional");

            entity.HasOne(d => d.ActualizadoPorNavigation).WithMany(p => p.ReporteActualizadoPorNavigations)
                .HasForeignKey(d => d.ActualizadoPor)
                .HasConstraintName("FK_Reportes_ActualizadoPor");

            entity.HasOne(d => d.IdAsignadoANavigation).WithMany(p => p.ReporteIdAsignadoANavigations)
                .HasForeignKey(d => d.IdAsignadoA)
                .HasConstraintName("FK_Reportes_Asignado");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Categoria");

            entity.HasOne(d => d.IdEdificioNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdEdificio)
                .HasConstraintName("FK_Reportes_Edificio");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Estado");

            entity.HasOne(d => d.IdPrioridadNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdPrioridad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Prioridad");

            entity.HasOne(d => d.IdReportanteNavigation).WithMany(p => p.ReporteIdReportanteNavigations)
                .HasForeignKey(d => d.IdReportante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Reportante");

            entity.HasOne(d => d.IdSalonNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdSalon)
                .HasConstraintName("FK_Reportes_Salon");
        });

        modelBuilder.Entity<Salone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Salones__3213E83F218A52C6");

            entity.HasIndex(e => new { e.IdEdificio, e.Nombre }, "UQ_Salon_Edificio").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoPor).HasColumnName("actualizado_por");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdEdificio).HasColumnName("id_edificio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Piso).HasColumnName("piso");
            entity.Property(e => e.TipoEspacio)
                .HasMaxLength(50)
                .HasColumnName("tipo_espacio");

            entity.HasOne(d => d.IdEdificioNavigation).WithMany(p => p.Salones)
                .HasForeignKey(d => d.IdEdificio)
                .HasConstraintName("FK_Salones_Edificio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83F63D2E5D0");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__2A586E0B5DC66A7A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Foto)
                .HasMaxLength(500)
                .HasColumnName("foto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");
        });

        modelBuilder.Entity<VwReportesCompleto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Reportes_Completo");

            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Edificio)
                .HasMaxLength(100)
                .HasColumnName("edificio");
            entity.Property(e => e.Eliminado).HasColumnName("eliminado");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoColor)
                .HasMaxLength(7)
                .HasColumnName("estado_color");
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion");
            entity.Property(e => e.FechaReporte).HasColumnName("fecha_reporte");
            entity.Property(e => e.Folio)
                .HasMaxLength(30)
                .HasColumnName("folio");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(20)
                .HasColumnName("prioridad");
            entity.Property(e => e.PrioridadColor)
                .HasMaxLength(7)
                .HasColumnName("prioridad_color");
            entity.Property(e => e.ReportanteNombre)
                .HasMaxLength(150)
                .HasColumnName("reportante_nombre");
            entity.Property(e => e.Salon)
                .HasMaxLength(100)
                .HasColumnName("salon");
            entity.Property(e => e.TecnicoNombre)
                .HasMaxLength(150)
                .HasColumnName("tecnico_nombre");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");
            entity.Property(e => e.UbicacionAdicional)
                .HasMaxLength(255)
                .HasColumnName("ubicacion_adicional");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
