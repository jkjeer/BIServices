using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;
// ReSharper disable InconsistentNaming

namespace BIServices.ArchieSvc
{
    [ServiceContract]
    public interface IArchieService
    {
        [OperationContract, OperationInvokerLogger]
        ProcessResponse TruncateLedgerTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TruncateLedgerTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TailTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TailTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TailTableGroup(string groupName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ArchiveTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = true);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ArchiveTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse BackupTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse BackupTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteNLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteNLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteSvcLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteSvcLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteIISLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteIISLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeletePSLogFiles(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeletePSLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DeleteSOALogFiles(string plantCode, int retentionDays, string serviceName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse UpdateTCCDeleteDate(int rows, bool verbose);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TCCDeleteDocs(int rows, bool verbose);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse TCCFindNewCategories(bool verbose);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ImportAPInvoices(bool verbose);
    }

    [ServiceContract]
    public interface IArchieRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TruncateLedgerTable?ServerName={serverName}&DatabaseName={databaseName}&SchemaName={schemaName}&TableName={tableName}&SendReport={sendReport}")]
        ProcessResponse TruncateLedgerTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TruncateLedgerTables?PlantCode={plantCode}&ServerName={serverName}&SendReport={sendReport}")]
        ProcessResponse TruncateLedgerTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TailTable?ServerName={serverName}&DatabaseName={databaseName}&SchemaName={schemaName}&TableName={tableName}&SendReport={sendReport}")]
        ProcessResponse TailTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TailTables?PlantCode={plantCode}&ServerName={serverName}&SendReport={sendReport}")]
        ProcessResponse TailTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TailTableGroup?GroupName={groupName}&SendReport={sendReport}")]
        ProcessResponse TailTableGroup(string groupName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ArchiveTable?ServerName={serverName}&DatabaseName={databaseName}&SchemaName={schemaName}&TableName={tableName}&SendReport={sendReport}")]
        ProcessResponse ArchiveTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ArchiveTables?PlantCode={plantCode}&ServerName={serverName}&SendReport={sendReport}")]
        ProcessResponse ArchiveTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/BackupTable?ServerName={serverName}&DatabaseName={databaseName}&SchemaName={schemaName}&TableName={tableName}&SendReport={sendReport}")]
        ProcessResponse BackupTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/BackupTables?PlantCode={plantCode}&ServerName={serverName}&SendReport={sendReport}")]
        ProcessResponse BackupTables(string plantCode, string serverName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteNLogFiles?ServerName={serverName}&RetentionDays={retentionDays}&SendReport={sendReport}")]
        ProcessResponse DeleteNLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteNLogFilesClient?PlantCode={plantCode}&RetentionDays={retentionDays}&SendReport={sendReport}")]
        ProcessResponse DeleteNLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteSvcLogFiles?ServerName={serverName}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeleteSvcLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteSvcLogFilesClient?PlantCode={plantCode}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeleteSvcLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteIISLogFiles?ServerName={serverName}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeleteIISLogFiles(string serverName, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteIISLogFilesClient?PlantCode={plantCode}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeleteIISLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeletePSLogFiles?PlantCode={plantCode}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeletePSLogFiles(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeleteSOALogFiles?PlantCode={plantCode}&RetentionDays={retentionDays}&ServiceName={serviceName}&SendReport={sendReport}")]
        ProcessResponse DeleteSOALogFiles(string plantCode, int retentionDays, string serviceName, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DeletePSLogFilesClient?PlantCode={plantCode}&RetentionDays={retentionDays}&sendReport={sendReport}")]
        ProcessResponse DeletePSLogFilesClient(string plantCode, int retentionDays, bool sendReport = false);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/UpdateTCCDeleteDate?Rows={rows}&Verbose={verbose}")]
        ProcessResponse UpdateTCCDeleteDate(int rows, bool verbose);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TCCDeleteDocs?Rows={rows}&Verbose={verbose}")]
        ProcessResponse TCCDeleteDocs(int rows, bool verbose);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/TCCFindNewCategories?Verbose={verbose}")]
        ProcessResponse TCCFindNewCategories(bool verbose);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ImportAPInvoices?Verbose={verbose}")]
        ProcessResponse ImportAPInvoices(bool verbose);
    }
}