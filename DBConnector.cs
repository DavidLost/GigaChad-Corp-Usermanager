using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GigaChad_Corp_Usermanager {

    internal class DBConnector {

        public const string DEFAULT_SERVER = "localhost";
        public const int DEFAULT_PORT = 3306;
        public static bool OPEN_ON_CREATE { get; set; } = true;
        private readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        public long LastExecutionTimeMS { get; private set; } = 0;
        public long LastDataTableSize { get; private set; } = 0;

        private readonly MySqlConnection connection;
        public DBConnector(MySqlConnection connection) {
            this.connection = connection;
            if (OPEN_ON_CREATE) Open();
        }
        public DBConnector(string connectionString) : this(new MySqlConnection(connectionString + ";Charset=latin1")) { }
        public DBConnector(string server, int port, string user, string password, string database) :
            this($"server={server};port={port};userid={user};password={password};database={database}") { }

        public DBConnector(string user, string password, string database) : this(DEFAULT_SERVER, DEFAULT_PORT, user, password, database) {
            Trace.WriteLine($"INFO - MySQL-Connection using default value for server={DEFAULT_SERVER} and port={DEFAULT_PORT}");
        }
        public bool IsConnectionAlive() {
            try {
                if (connection.Ping()) return true;
            }
            catch { }
            return false;
        }

        public void Open() {
            if (!IsConnectionAlive()) {
                try {
                    connection.Open();
                    Trace.WriteLine("INFO - Database connection sucessfully opend!");
                }
                catch (Exception ex) {
                    Trace.TraceError("ERROR - Can't connect to database!" + ex.Message);
                }
            }
        }
        public void Close() {
            if (IsConnectionAlive()) {
                connection.Close();
            }
        }
        public MySqlDataReader? Execute(string queryString) {
            Trace.WriteLine(IsConnectionAlive());
            if (!IsConnectionAlive() || queryString == null) return null;
            return new MySqlCommand(queryString, connection).ExecuteReader();
        }
        public DataTable? ExecuteTable(string queryString) {
            Stopwatch.Restart();
            var reader = Execute(queryString);
            if (reader == null) return null;
            var table = new DataTable();
            table.Load(reader);
            reader.Close();
            Stopwatch.Stop();
            LastExecutionTimeMS = Stopwatch.ElapsedMilliseconds;
            //LastDataTableSize = GetDataTableSizeInBytes(table);
            Trace.WriteLine($"INFO - Table was sucessfully loaded in {LastExecutionTimeMS}ms");
            return table;
        }



        public static long GetDataTableSizeInBytes(DataTable dataTable) {
            #pragma warning disable SYSLIB0011
            long sizeInBytes = 0;
            var binaryFormatter = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                binaryFormatter.Serialize(ms, dataTable);
                sizeInBytes = ms.Length;
            }
            #pragma warning restore SYSLIB0011
            return sizeInBytes;
        }
    }

}
