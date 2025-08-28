#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using Npgsql;
using System.Data;
using System.Collections.Generic;
using System.Text;
using FTOptix.ODBCStore;
using FTOptix.Store;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using System.Diagnostics.Tracing;
using FTOptix.WebUI;
#endregion

public class Postgresql_Interface : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    

    public class DataInserter
    {
        private readonly string _connectionString;
        public DataInserter(string connectionString)
        {
            _connectionString = connectionString;
        }


        public void InsertData(DateTime? localTimestamp = null, DateTime? utcTimestamp = null, float? sineData = null, float? cosineData = null, string equipmentId = "None")
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                string insertQuery = @"
                INSERT INTO tbl_production (timestamp_local, timestamp_utc, sine_data, cosine_data, equipment_id) 
                VALUES (@timestamp_local, @timestamp_utc, @sine_data, @cosine_data, @equipment_id)";

                using var command = new NpgsqlCommand(insertQuery, connection);

                // Add parameters
                //command.Parameters.AddWithValue("@utcTimestamp", utcTimestamp);
                command.Parameters.AddWithValue("@timestamp_local", localTimestamp);
                command.Parameters.AddWithValue("@timestamp_utc", utcTimestamp);
                
                command.Parameters.AddWithValue("@sine_data", sineData);
                command.Parameters.AddWithValue("@cosine_data", cosineData);
                command.Parameters.AddWithValue("@equipment_id", equipmentId);


                // Execute the insert
                int rowsAffected = command.ExecuteNonQuery();
                Log.Info($"Inserted {rowsAffected} row(s)");
            }
            catch (Exception ex)
            {
                Log.Error($"Error inserting data: {ex.Message}");
                throw;
            }
        }
    }
}
