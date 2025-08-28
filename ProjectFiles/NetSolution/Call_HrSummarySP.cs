#region Using directives
using System;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.NetLogic;
using FTOptix.ODBCStore;
using FTOptix.Retentivity;
using FTOptix.Store;
using FTOptix.UI;
using Npgsql;
using System.Data;
using UAManagedCore;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.WebUI;
using FTOptix.Alarm;
using FTOptix.EventLogger;
using OpcUa = UAManagedCore.OpcUa;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
#endregion

public class Call_HrSummarySP : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        Log.Info("Started HRSummary Logic");
        variableSynchronizer = new RemoteVariableSynchronizer();
        myLongRunningTask = new LongRunningTask(CallVariableCheck, LogicObject);
        myLongRunningTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        Log.Info("Stopped HRSummary Logic");
    }

    [ExportMethod]
    public void CallInsertSummary()
  {
        InsertHrSummary_Config insertData = (InsertHrSummary_Config)Owner;
        PostgreSQL_Config DB_Config = (PostgreSQL_Config)Owner.GetAlias("DB_Config");


        string connectionString = $"Host={DB_Config.Hostname};Port={DB_Config.Port};Database={DB_Config.DatabaseName};Username={DB_Config.Username};Password={DB_Config.Password}";
        var dataInserter = new Postgresql_Interface.DataInserter(connectionString);
        
        DateTime utcTimestamp = DateTime.UtcNow.ToUniversalTime();
        DateTime localTimestamp = DateTime.Parse(insertData.PLC_DateTime).ToUniversalTime();
        float sine_data = insertData.Sine;
        float cosine_data = insertData.Cosine;
        string equipment_id = insertData.Equipment_ID;

        dataInserter.InsertData(localTimestamp, utcTimestamp, sine_data, cosine_data, equipment_id);

        insertData.LastQuery = localTimestamp.ToString() + ", " + sine_data.ToString() + ", " + cosine_data.ToString() + ", " + equipment_id;
    }

    public void CallVariableCheck()
    {
        foreach (var item in Owner.Children)
        {
            foreach(var child in item.Children)
            {
                if (child is DynamicLink)
                {
                    Log.Info("RecursiveSearch.Add", "Adding " + item.BrowseName + " of type " + item.GetType().ToString());
                    variableSynchronizer.Add(Owner.GetVariable(item.BrowseName));
                }
            }

        }

    }

    private void RecursiveSearch(IUANode startingNode)
    {        IUAVariable sourceVar = null;
            //IUAVariable destVar = null;
            //UAValue destNode = null;
            try
            {
                sourceVar = InformationModel.GetVariable(startingNode.NodeId);
            }
            catch
            {
                Log.Error("RecursiveSearch.Exception", "Skipping " + startingNode.BrowseName + " of type " + startingNode.GetType().ToString());
            }

            if (sourceVar != null)
            {
                Log.Info("RecursiveSearch.Add", "Adding " + startingNode.BrowseName + " of type " + startingNode.GetType().ToString());
                variableSynchronizer.Add(sourceVar);
            }
        if (startingNode.Children.Count > 0)
        {
            foreach (IUANode children in startingNode.Children)
            {
                RecursiveSearch(children);
            }
        }
        else
        {
            Log.Debug("RecursiveSearch.Skip", "Skipping " + startingNode.BrowseName + " of type " + startingNode.GetType().ToString());
        }
    }
    private RemoteVariableSynchronizer variableSynchronizer;
    private LongRunningTask myLongRunningTask;

}
