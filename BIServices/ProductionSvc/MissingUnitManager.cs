using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.ProductionSvc
{
    public class MissingUnitManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public MissingUnitManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("MissingUnitManager.GetLockLoad", GetLockLoad());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _flexItemMgr = new FlexItemManager(session);
        }

        /*********************************************************************************
         * Get Open Units Dynamic
         *******************************************************************************/
        public iDB2Command GetMissingUnits(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
	            RDMCU, 
	            RDZ58RID,
	            RDZ58SN,
	            RDLOTN, 
	            RDZ58DST,
	            RDITM,
                RHAN8,
                AB1.ABALPH AS AN8ALPH,
                RHSHAN,
                AB2.ABALPH AS SHANALPH,
	            IBSTKT,
	            IMSRP4,
	            SDTRDJ,
	            S5Z58SCD,
                SDKCOO,
                SDDOCO,
                SDDCTO,
                SDLNID,
	            SDDRQJ,
	            SDVR01,
                SDAN8,
                SDSHAN,
                WADOCO,
                WASRST,
	            ATZ58CP2,
	            ATZ58CP3,
	            ATZ58CP4,
                S1Z58SCTYP,
                S5Z58SCD,
                S5Z58SCN,
	            B.DLZ58RID AS SCHD_RACK,
	            B.RHZ58SCN AS SCHD_NO,
	            B.RHZ58BOX AS SCHD_BOX
            FROM 
	            F580056 RD
	            INNER JOIN F580055 RH
	            ON RH.RHZ58RID = RD.RDZ58RID
	            INNER JOIN F4102
	            ON IBMCU = RDMCU AND
	            IBITM = RDITM
	            INNER JOIN F4101
	            ON IMITM = IBITM
	            INNER JOIN F0101 AB1
	            ON AB1.ABAN8 = RH.RHAN8
	            INNER JOIN F0101 AB2
	            ON AB2.ABAN8 = RH.RHSHAN
	            INNER JOIN (
			            SELECT 
				            DLMCU, 
				            DLLOTN, 
				            DLZ58SN, 
				            DLZ58RID, 
				            RHZ58HST,
				            RHZ58SCN, 
				            RHZ58BOX 
			            FROM 
				            F580569 
				            INNER JOIN F580055 
				            ON	DLZ58RID = RHZ58RID AND 
				            DLIOPR = 'Insert' AND 
				            RHZ58HST IN ('7','8')) B
	            ON	B.DLMCU = RD.RDMCU AND 
		            B.DLLOTN = RD.RDLOTN AND 
		            B.DLZ58SN = RD.RDZ58SN
	            LEFT OUTER JOIN F4211 SD
	            ON	SD.SDDOCO = RD.RDDOCO AND
		            SD.SDDCTO = RD.RDDCTO AND
		            SD.SDLNID = RD.RDLNID
	            LEFT OUTER JOIN F580032 
	            ON	ATDOCO = RD.RDDOCO AND
		            ATDCTO = RD.RDDCTO AND
		            ATLNID = RD.RDLNID
                LEFT OUTER JOIN F4801
                ON WADOCO = SDRORN
	            LEFT OUTER JOIN F5834A11 
	            ON	S5DOCO = WADOCO
                LEFT OUTER JOIN F5834A01
                ON S1Z58SCN = S5Z58SCN AND
                    S1Z58SCD = S5Z58SCD AND
                    S1MCU = S5MCU
            WHERE 
	            {0}
	            RDZ58DST IN ('1','2','4','5') AND 
	            RDLOTN != '' AND
                WASRST != '980' AND
	            NOT EXISTS (
			            SELECT 
				            1 
			            FROM 
				            F580056 RD2 
			            WHERE 
				            RD2.RDLOTN = RD.RDLOTN AND 
				            RD2.RDZ58SN = RD.RDZ58SN AND 
				            RD2.RDZ58DST = '8')
            ORDER BY 
	            1,2,3,4,5", filterString.ToString());
            return cmd;
        }

    }
}
