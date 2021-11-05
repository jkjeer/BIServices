using System;
using System.Configuration;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ArchieSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ArchieService : IArchieService
    {
        public ProcessResponse TruncateLedgerTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            if (!databaseName.ToUpper().Equals("INTEGRATION"))
            {
                throw new ApplicationException("Database Name must equal INTEGRATION.");
            }
            //if (!schemaName.ToUpper().Equals("EBS"))
            //{
            //    throw new ApplicationException("Schema Name must equal EBS.");
            //}
            if (!tableName.ToUpper().Contains("LEDGER"))
            {
                throw new ApplicationException("Table Name must contain LEDGER.");
            }
            try
            {
                return ArchieProcessor.TruncateLedgerTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TruncateLedgerTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TruncateLedgerTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTableGroup(string groupName, bool sendReport)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new ApplicationException("Group Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTableGroup(groupName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ArchiveTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.ArchiveTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ArchiveTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.ArchiveTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse BackupTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.BackupTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse BackupTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.BackupTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteNLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteNLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteNLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteNLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSvcLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSvcLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSvcLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSvcLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteIISLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteIISLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteIISLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteIISLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeletePSLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeletePSLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeletePSLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeletePSLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSOALogFiles(string serverName, int retentionDays, string serviceName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSOALogFiles(serverName.ToUpper(), retentionDays, sendReport, serviceName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse UpdateTCCDeleteDate(int rows = 0, bool verbose = false)
        {
            try
            {
                return TCCProcessor.UpdateTCCDeleteDate(rows, verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TCCDeleteDocs(int rows = 0, bool verbose = false)
        {
            try
            {
                return TCCProcessor.TCCDeleteDocs(rows, verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TCCFindNewCategories(bool verbose = false)
        {
            try
            {
                return TCCProcessor.TCCFindNewCategories( verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ImportAPInvoices(bool verbose = false)
        {
            try
            {
                return APInvoiceProcessor.ImportAPInvoices(verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ArchieRestService : IArchieRestService
    {
        public ProcessResponse TruncateLedgerTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            if (!databaseName.ToUpper().Equals("INTEGRATION"))
            {
                throw new ApplicationException("Database Name must equal INTEGRATION.");
            }
            //if (!schemaName.ToUpper().Equals("EBS"))
            //{
            //    throw new ApplicationException("Schema Name must equal EBS.");
            //}
            if (!tableName.ToUpper().Contains("LEDGER"))
            {
                throw new ApplicationException("Table Name must contain LEDGER.");
            }
            try
            {
                return ArchieProcessor.TruncateLedgerTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TruncateLedgerTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TruncateLedgerTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TailTableGroup(string groupName, bool sendReport)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new ApplicationException("Group Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.TailDataTableGroup(groupName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ArchiveTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.ArchiveTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ArchiveTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.ArchiveTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse BackupTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ApplicationException("Database Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ApplicationException("Schema Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ApplicationException("Table Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.BackupTable(serverName, databaseName, schemaName, tableName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse BackupTables(string plantCode, string serverName, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            try
            {
                return ArchieProcessor.BackupTables(plantCode, serverName, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteNLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteNLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteNLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteNLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSvcLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSvcLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSvcLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSvcLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteIISLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteIISLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteIISLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteIISLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeletePSLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeletePSLogFiles(serverName.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeletePSLogFilesClient(string plantCode, int retentionDays, bool sendReport)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeletePSLogFilesClient(plantCode.ToUpper(), retentionDays, sendReport);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DeleteSOALogFiles(string serverName, int retentionDays, string serviceName, bool sendReport)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ApplicationException("Server Name is REQUIRED.");
            }
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (retentionDays < minRetentionDays)
            {
                throw new ApplicationException($"Retention Days must be at least {minRetentionDays}.");
            }
            try
            {
                return ArchieProcessor.DeleteSOALogFiles(serverName.ToUpper(), retentionDays, sendReport, serviceName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse UpdateTCCDeleteDate(int rows, bool verbose)
        {
            try
            {
                return TCCProcessor.UpdateTCCDeleteDate(rows, verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TCCDeleteDocs(int rows, bool verbose)
        {
            try
            {
                return TCCProcessor.TCCDeleteDocs(rows, verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse TCCFindNewCategories(bool verbose = false)
        {
            try
            {
                return TCCProcessor.TCCFindNewCategories(verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ImportAPInvoices(bool verbose = false)
        {
            try
            {
                return APInvoiceProcessor.ImportAPInvoices(verbose);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
