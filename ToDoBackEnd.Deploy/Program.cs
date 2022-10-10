﻿using System;
using DbUp;
using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace ToDoBackEnd.Deploy
{
    class Program
    {
        static int Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var backEndUserPassword = Environment.GetEnvironmentVariable("BackEndUserPassword");
            
            Console.WriteLine(connectionString)
            Console.WriteLine(BackEndUserPassword)

            var branchName = Environment.GetEnvironmentVariable("GITHUB_REF");
            branchName = branchName.Replace("refs/heads/", string.Empty);
            branchName = branchName == "main" ? string.Empty : "_" + branchName;

            var csb = new SqlConnectionStringBuilder(connectionString);
            csb.InitialCatalog += branchName;
            Console.WriteLine($"Deploying database: {csb.InitialCatalog}");

            Console.WriteLine("Testing connection...");
            var conn = new SqlConnection(csb.ToString());
            conn.Open();
            conn.Close();

            Console.WriteLine("Starting deployment...");
            var dbup = DeployChanges.To
                .SqlDatabase(csb.ConnectionString)
                .WithScriptsFromFileSystem("./sql") 
                .JournalToSqlTable("dbo", "$__dbup_journal")                                               
                .WithVariable("BackEndUserPassword", backEndUserPassword)
                .LogToConsole()
                .Build();
         
            var result = dbup.PerformUpgrade();

            if (!result.Successful)
            {
                Console.WriteLine(result.Error);
                return -1;
            }

            Console.WriteLine("Success!");
            return 0;
        }
        
    }
}
