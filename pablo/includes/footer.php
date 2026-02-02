    <!-- Footer -->
    <footer class="mt-5 py-3 bg-light text-center">
        <div class="container">
            <p class="text-muted mb-0">
                &copy; <?php echo date('Y'); ?> SIREFI - Sistema de Reporte de Fallas e Incidencias
            </p>
            <p class="text-muted small mb-0">
                TEC Serdán - Tecnológico Nacional de México
            </p>
        </div>
    </footer>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <?php if (isset($additionalScripts)) echo $additionalScripts; ?>
</body>
</html>
