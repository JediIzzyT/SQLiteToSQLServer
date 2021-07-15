using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SQLiteToSQLServer
{
    class Program
    {
        /*
            1. Fill out the variables listed below with the information pertaining to your databases
            2. Code in the "Database Processing" region can be left unaltered
            3. Run the program through Visual Studio by pressing the "F5" key on your keyboard
        */
        static string sqliteDataSource = @"C:\Path\To\SqliteDatabase.sqlite3";
        static string sqlServerConnString = "Data Source=<Database Server>;Initial Catalog=<Database Name>;User ID=<>; Password=<>";
        static string sqliteTableName = "";
        static string sqlServerTableName = "";
        static List<dynamic[]> columns = new List<dynamic[]> {
            //new dynamic[]{ "<sqliteColumnName>", "<sqlServerColumnName>", System.Data.SqlDbType.<sqlServerDataType> }
        };

        #region Database Processing
        static void Main(string[] args)
        {
            SqliteConnectionStringBuilder sqliteConnString;
            SqliteConnection sqliteConn;
            SqlConnection sqlServerConn;
            string sqliteSelectString;
            SqliteCommand sqliteCommand;
            SqliteDataReader sqliteDataReader;
            string sqlServerInsertString;
            SqlCommand sqlServerCommand;
            SqlParameter sqlServerParam;
            SqlDataAdapter sqlServerDataAdapter;
            int count = 1;

            Console.WriteLine("-------------------- Start --------------------");

            //Instantiate SQLite Connection
            sqliteConnString = new SqliteConnectionStringBuilder();
            sqliteConnString.DataSource = sqliteDataSource;
            sqliteConn = new SqliteConnection(sqliteConnString.ConnectionString);

            //Instantiate SQL Server Connection
            sqlServerConn = new SqlConnection(sqlServerConnString);

            //Get the SQLite data
            sqliteSelectString = "SELECT ";
            for (var i=0; i < columns.Count - 1; i++)
            {
                sqliteSelectString += $"{columns[i][0]}, ";
            }
            sqliteSelectString += $"{columns[^1][0]} FROM {sqliteTableName};";
            Console.WriteLine("Connecting to SQLite...");
            sqliteConn.Open();
            Console.WriteLine("Connected to SQLite!");
            using (sqliteCommand = new SqliteCommand(sqliteSelectString, sqliteConn))
            {
                using (sqliteDataReader = sqliteCommand.ExecuteReader())
                {
                    //Connect to SQL Server to insert data
                    sqlServerInsertString = $"INSERT INTO {sqlServerTableName} (";
                    for (var i = 0; i < columns.Count - 1; i++)
                    {
                        sqlServerInsertString += $"{columns[i][1]}, ";
                    }
                    sqlServerInsertString += $"{columns[^1][1]}) VALUES (";
                    for (var i = 0; i < columns.Count - 1; i++)
                    {
                        sqlServerInsertString += $"@{columns[i][1]}, ";
                    }
                    sqlServerInsertString += $"@{columns[^1][1]});";
                    Console.WriteLine("Connecting to SQL Server...");
                    sqlServerConn.Open();
                    Console.WriteLine("Connected to SQL Server!");

                    while (sqliteDataReader.Read())
                    {
                        using (sqlServerCommand = new SqlCommand(sqlServerInsertString, sqlServerConn))
                        {
                            for (var i = 0; i < columns.Count; i++)
                            {
                                sqlServerParam = new SqlParameter(columns[i][1], columns[i][2]);
                                sqlServerParam.Value = sqliteDataReader.GetValue(i);
                                sqlServerCommand.Parameters.Add(sqlServerParam);
                            }
                            using (sqlServerDataAdapter = new SqlDataAdapter())
                            {
                                sqlServerDataAdapter.InsertCommand = sqlServerCommand;
                                sqlServerDataAdapter.InsertCommand.ExecuteNonQuery();
                            }
                            Console.WriteLine($"Inserted Row {count}");
                            count++;
                        }
                    }

                    sqlServerConn.Close();
                    Console.WriteLine("Disconnected from SQL Server");
                }
            }
            sqliteConn.Close();
            Console.WriteLine("Disconnected from SQLite");


            Console.WriteLine("-------------------- End --------------------");
        }
        #endregion 
    }
}
