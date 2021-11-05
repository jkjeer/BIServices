using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class InterlayerLotManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public InterlayerLotManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
           // hash.Add("RackManager.GetRack", GetRack());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
          * Stock Orders
          *******************************************************************************/
        public iDB2Command GetLots(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT 
	            LIMCU,
	            LILOTN,
	            LILOCN,
	            LIITM,
                IBSRP4,
                IBSTKT,
                IBGLPT,
	            IMDSC1,
	            IMDSC2,
	            LIPQOH,
	            IMUOM1,
	            IMUOM2,
	            COALESCE(UMCONV / 10000000, 1.00) AS UMCONV,
	            CASE WHEN UMCONV IS NULL THEN LIPQOH / 1000 ELSE LIPQOH / 1000 * UMCONV / 10000000 END AS SQFT,
	            LILRCJ,
	            LIUPMJ,
	            LITDAY,
	            LGILS.RDTSV1 AS PRODUCT_CODE,
	            S1.RDTVAL AS THICKNESS,
	            S2.RDTVAL AS MATERIAL,
	            S3.RDTVAL AS COLOR,
	            S4.RDTVAL AS DIRECTION,
	            S5.RDTVAL AS BASEDIM,
	            S6.RDTVAL AS LEFTDIM,
	            AB1.ABAN8 AS SUPPLIER_NUM,
	            AB1.ABALPH AS SUPPLIER_NAME
            FROM 
	            F41021
	            INNER JOIN F4102
	            ON	IBMCU = LIMCU AND
		            IBITM = LIITM
	            INNER JOIN F4101
	            ON	IMITM = IBITM
	            LEFT OUTER JOIN F41002
	            ON	UMMCU = IBMCU AND
		            UMITM = IBITM AND
		            UMUM = IMUOM1 AND
		            UMRUM = 'SF'
	            LEFT OUTER JOIN F3283 LGILS
	            ON  LGILS.RDMCU = IBMCU AND
		            LGILS.RDITM = IBITM AND
		            LGILS.RDTBLC = 'LGILS'
	            LEFT OUTER JOIN F3283 S1
	            ON	LGILS.RDMCU = S1.RDMCU AND
		            LGILS.RDTSV1 = S1.RDTSV1 AND
		            S1.RDBSEQ = 1 AND
		            S1.RDTBLC = 'PRDCDESUBS' 
	            LEFT OUTER JOIN F3283 S2
	            ON	LGILS.RDMCU = S2.RDMCU AND
		            LGILS.RDTSV1 = S2.RDTSV1 AND
		            S2.RDBSEQ = 2 AND
		            S2.RDTBLC = 'PRDCDESUBS' 
	            LEFT OUTER JOIN F3283 S3
	            ON	LGILS.RDMCU = S3.RDMCU AND
		            LGILS.RDTSV1 = S3.RDTSV1 AND
		            S3.RDBSEQ = 3 AND
		            S3.RDTBLC = 'PRDCDESUBS' 
	            LEFT OUTER JOIN F3283 S4
	            ON	LGILS.RDMCU = S4.RDMCU AND
		            LGILS.RDTSV1 = S4.RDTSV1 AND
		            S4.RDBSEQ = 4 AND
		            S4.RDTBLC = 'PRDCDESUBS' 
	            LEFT OUTER JOIN F3283 S5
	            ON	LGILS.RDMCU = S5.RDMCU AND
		            LGILS.RDTSV1 = S5.RDTSV1 AND
		            S5.RDBSEQ = 5 AND
		            S5.RDTBLC = 'PRDCDESUBS'
	            LEFT OUTER JOIN F3283 S6
	            ON	LGILS.RDMCU = S6.RDMCU AND
		            LGILS.RDTSV1 = S6.RDTSV1 AND
		            S6.RDBSEQ = 6 AND
		            S6.RDTBLC = 'PRDCDESUBS' 					 
	            LEFT OUTER JOIN F0101 AB1
	            ON	AB1.ABAN8 = IBVEND
            WHERE 
                {filterString}
	            LIGLPT IN ('INLR') AND
	            LILOTN <> ''
            FOR READ ONLY";
            return cmd;
        }

        public InterlayerLots GetLots(BusinessSystemSession session, List<string> plantList)
        {
            var lots = new InterlayerLots();
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (plantList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string plantItem in plantList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("LIMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                var cmd = GetLots(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                Logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lot = new InterlayerLot();
                        lot.PlantNum = CastDbNull.To(reader["LIMCU"], string.Empty).Trim();
                        lot.LotNum = CastDbNull.To(reader["LILOTN"], string.Empty).Trim();
                        lot.Location = CastDbNull.To(reader["LILOCN"], string.Empty).Trim();
                        lot.ItemNum = CastDbNull.To<decimal>(reader["LIITM"], 0);
                        lot.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        lot.Desc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        lot.Desc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        lot.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        lot.GlCode = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        lot.QtyOnHand = Data.CastDbNull.To<decimal>(reader["LIPQOH"], 0) / 1000;
                        lot.Sqft = Data.CastDbNull.To<decimal>(reader["SQFT"], 0) * 1;
                        lot.UOM1 = CastDbNull.To(reader["IMUOM1"], string.Empty).Trim();
                        lot.UOM2 = CastDbNull.To(reader["IMUOM2"], string.Empty).Trim();
                        lot.SqftRate = Data.CastDbNull.To<decimal>(reader["UMCONV"], 0) * 1;
                        lot.ProductCode = CastDbNull.To(reader["PRODUCT_CODE"], string.Empty).Trim();
                        lot.Thickness =
                            (Decimal.TryParse(CastDbNull.To(reader["THICKNESS"], string.Empty).Trim(), out decimal val1))
                                ? val1
                                : 0;
                        lot.Material = CastDbNull.To(reader["MATERIAL"], string.Empty).Trim();
                        lot.Color = CastDbNull.To(reader["COLOR"], string.Empty).Trim();
                        lot.Direction = CastDbNull.To(reader["DIRECTION"], string.Empty).Trim();
                        lot.BaseDim = (Decimal.TryParse(CastDbNull.To(reader["BASEDIM"], string.Empty).Trim(), out decimal val2))
                            ? val2
                            : 0;
                        lot.LeftDim = (Decimal.TryParse(CastDbNull.To(reader["LEFTDIM"], string.Empty).Trim(), out decimal val3))
                            ? val3
                            : 0;
                        lot.SupplierNum = Data.CastDbNull.To<decimal>(reader["SUPPLIER_NUM"], 0);
                        lot.SupplierName = CastDbNull.To(reader["SUPPLIER_NAME"], string.Empty).Trim();
                        var rcdj = Data.CastDbNull.To<decimal>(reader["LILRCJ"], 0);
                        lot.ReceivedDate = (rcdj > 0) ? (DateTime?) E1TimeManager.CastToDateTime(rcdj) : null;
                        lots.Add(lot);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lots;
        }
    }
}
