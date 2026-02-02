<?php
/**
 * Database Connection Handler
 * Provides database connection and helper methods
 */

require_once __DIR__ . '/../config.php';

class Database {
    private static $instance = null;
    private $conn = null;
    
    private function __construct() {
        // Check if sqlsrv extension is loaded
        if (!extension_loaded('sqlsrv')) {
            $error_msg = "La extensión PHP 'sqlsrv' no está instalada. " .
                        "Esta extensión es requerida para conectar con SQL Server. " .
                        "Por favor, instale la extensión sqlsrv siguiendo las instrucciones en README.md";
            error_log("SIREFI Error: " . $error_msg);
            throw new Exception($error_msg);
        }
        
        $connectionOptions = [
            "Database" => DB_NAME,
            "Uid" => DB_USER,
            "PWD" => DB_PASS,
    "Encrypt" => "no",                  // 🔧 Desactiva SSL
    "TrustServerCertificate" => "yes"
        ];
        
        $this->conn = sqlsrv_connect(DB_SERVER, $connectionOptions);
        
        if ($this->conn === false) {
            error_log("Database connection failed: " . print_r(sqlsrv_errors(), true));
            throw new Exception("Error al conectar con la base de datos");
        }
    }
    
    public static function getInstance() {
        if (self::$instance === null) {
            self::$instance = new Database();
        }
        return self::$instance;
    }
    
    public function getConnection() {
        return $this->conn;
    }
    
    public function query($sql, $params = []) {
        $stmt = sqlsrv_query($this->conn, $sql, $params);
        if ($stmt === false) {
            error_log("Query failed: " . print_r(sqlsrv_errors(), true));
            return false;
        }
        return $stmt;
    }
    
    public function __destruct() {
        if ($this->conn !== null) {
            sqlsrv_close($this->conn);
        }
    }
}
?>
