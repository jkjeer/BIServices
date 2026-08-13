using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.CustomerSvc
{
    public static class CustomerHierarchyManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /******************************************************************
        * ULT PARENT
        ******************************************************************/
        public static iDB2Command GetUltParent()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT DISTINCT
              COALESCE(A4.MAPA8, A3.MAPA8, A2.MAPA8, A1.MAPA8) AS MAPA8
            FROM
              F0150 A1
              LEFT OUTER JOIN F0150 A2
                ON A1.MAPA8 = A2.MAAN8
              LEFT OUTER JOIN F0150 A3
                ON A2.MAPA8 = A3.MAAN8
              LEFT OUTER JOIN F0150 A4
                ON A3.MAPA8 = A4.MAAN8
            WHERE
              A1.MAAN8 = ?";
            cmd.Parameters.Add(new iDB2Parameter("CustomerNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CustomerNum"].Precision = 8;
            cmd.Parameters["CustomerNum"].Scale = 0;
            return cmd;
        }

        public static Decimal GetUltParent(BusinessSystemSession session, Decimal customerNum)
        {
            Decimal parentNum = customerNum;
            try
            {
                var cmd = (session.DbCommands.ContainsKey("OrderQueryManager.GetUltParent")) ? session.DbCommands["OrderQueryManager.GetUltParent"] : GetUltParent();
                cmd.Parameters[0].Value = customerNum;
                cmd.Connection = session.DbConnection;
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    parentNum = CastDbNull.To<Decimal>(reader["MAPA8"], 0);
                }
                reader.Close();
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return parentNum;
        }

        /******************************************************************
        * ULT PARENT
        ******************************************************************/
        public static iDB2Command GetChildCustomerNums()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
              SELECT
                MAAN8
              FROM
                ( SELECT
                    A1.MAPA8 ,
                    A4.MAAN8
                  FROM
                    F0150 A1
                    INNER JOIN F0150 A2
                      ON A2.MAPA8 = A1.MAAN8
                    INNER JOIN F0150 A3
                      ON A3.MAPA8 = A2.MAAN8
                    INNER JOIN F0150 A4
                      ON A4.MAPA8 = A3.MAAN8
                  UNION
                  SELECT
                    A1.MAPA8 ,
                    A3.MAAN8
                  FROM
                    F0150 A1
                    INNER JOIN F0150 A2
                      ON A2.MAPA8 = A1.MAAN8
                    INNER JOIN F0150 A3
                      ON A3.MAPA8 = A2.MAAN8
                  UNION
                  SELECT
                    A1.MAPA8 ,
                    A2.MAAN8
                  FROM
                    F0150 A1
                    INNER JOIN F0150 A2
                      ON A2.MAPA8 = A1.MAAN8
                  UNION
                  SELECT
                    A1.MAPA8 ,
                    A1.MAAN8
                  FROM
                    F0150 A1
                ) Z
            WHERE
                Z.MAPA8 = ?";
            cmd.Parameters.Add(new iDB2Parameter("ParentNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ParentNum"].Precision = 8;
            cmd.Parameters["ParentNum"].Scale = 0;
            return cmd;
        }

        public static List<Decimal> GetChildCustomerNums(BusinessSystemSession session, Decimal parentNum)
        {
            var childNums = new List<Decimal>();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("OrderQueryManager.GetChildCustomerNums")) ? session.DbCommands["OrderQueryManager.GetChildCustomerNums"] : GetChildCustomerNums();
                cmd.Parameters[0].Value = parentNum;
                cmd.Connection = session.DbConnection;
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    childNums.Add(CastDbNull.To<Decimal>(reader["MAAN8"], 0));
                }
                reader.Close();
                if (!childNums.Any())
                {
                    childNums.Add(parentNum);
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return childNums;
        }

    }
}
