using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;

namespace BIServices.QuerySvc
{
    public class AndersenEdiManager
    {
        public AndersenEdiManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
          * Get Open Units Dynamic
        *******************************************************************************/
        public iDB2Command GetEdiRecords(List<string> filters)
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
                A1.EAEKCO,
	            A1.EAEDBT,
	            A1.EAEDOC,
	            A1.EAEDLN,
	            TRIM(A1.EASGVL) AS P96181,
	            TRIM(A6.EASGVL) AS P95154,
	            TRIM(A2.EASGVL) AS P95000,
	            TRIM(A3.EASGVL) AS P95001,
	            TRIM(A4.EASGVL) AS P95801,
	            TRIM(A5.EASGVL) AS P95901,
	            TRIM(A7.EASGVL) AS P95160,
	            A1.EAUPMJ,
	            A1.EATDAY,
	            SDMCU,
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
	            SDAN8,
	            SDDSC1,
	            SDDSC2,
	            SDCATNM,
	            AB1.ABALPH AS SDAN8NM,
	            SDVR01
            FROM 
	            F580100 A1
	            INNER JOIN F580100 A6
	            ON	A6.EAEKCO = A1.EAEKCO AND
		            A6.EAEDOC = A1.EAEDOC AND
		            A6.EAEDCT = A1.EAEDCT AND
		            A6.EAEDLN = A1.EAEDLN AND
		            A6.EAPEL = 95154 
                LEFT OUTER JOIN F47012 
                ON	SZEDOC = A1.EAEDOC and 
		            SZEDLN = A1.EAEDLN
                LEFT OUTER JOIN F4211
                ON	SDDOCO = SZDOCO AND
		            SDDCTO = SZDCTO AND
		            SDLNID = SZLNID AND
		            SDKCOO= SZKCOO
	            LEFT OUTER JOIN F0101 AB1
	            ON AB1.ABAN8 = SDAN8
	            LEFT OUTER JOIN F580100 A2
	            ON	A2.EAEKCO = A1.EAEKCO AND
		            A2.EAEDOC = A1.EAEDOC AND
		            A2.EAEDCT = A1.EAEDCT AND
		            A2.EAEDLN = A1.EAEDLN AND
		            A2.EAPEL = 95000
	            LEFT OUTER JOIN F580100 A3
	            ON	A3.EAEKCO = A1.EAEKCO AND
		            A3.EAEDOC = A1.EAEDOC AND
		            A3.EAEDCT = A1.EAEDCT AND
		            A3.EAEDLN = A1.EAEDLN AND
		            A3.EAPEL = 95001	
	            LEFT OUTER JOIN F580100 A4
	            ON	A4.EAEKCO = A1.EAEKCO AND
		            A4.EAEDOC = A1.EAEDOC AND
		            A4.EAEDCT = A1.EAEDCT AND
		            A4.EAEDLN = A1.EAEDLN AND
		            A4.EAPEL = 95801		
	            LEFT OUTER JOIN F580100 A5
	            ON	A5.EAEKCO = A1.EAEKCO AND
		            A5.EAEDOC = A1.EAEDOC AND
		            A5.EAEDCT = A1.EAEDCT AND
		            A5.EAEDLN = A1.EAEDLN AND
		            A5.EAPEL = 95901		
	            LEFT OUTER JOIN F580100 A7
	            ON	A7.EAEKCO = A1.EAEKCO AND
		            A7.EAEDOC = A1.EAEDOC AND
		            A7.EAEDCT = A1.EAEDCT AND
		            A7.EAEDLN = A1.EAEDLN AND
		            A7.EAPEL = 95160
            WHERE
                {0}
	            A1.EAPEL = 96181 AND 
	            A1.EASGVL = '0.0'
	            --A1.EAUPMJ = 115316 and
	            --A1.EAEKCO = '15316'
            ORDER BY 1,2,3,4", filterString.ToString());
            return cmd;
        }

        public EdiRecords GetEdiRecords(BusinessSystemSession session, DateTime startDate, DateTime endDate)
        {
            var lines = new EdiRecords();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {
                // Date Filter
                string strInClause = "";
                TimeSpan ts = endDate - startDate;
                int x = 0;
                do
                {
                    var z = startDate.AddDays(x);
                    var zz = Common.Utilities.Common.CastToE1Date(z).ToString().Substring(1, 5);
                    strInClause = strInClause + "?,";
                    parameters.Add(new iDB2Parameter("EKCO", iDB2DbType.iDB2Graphic, 10));
                    parameters.Last().Value = zz;
                    x++;
                } while (x < ts.Days);
                filters.Add("A1.EAEKCO IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");

                cmd = GetEdiRecords(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new EdiRecord();
                        line.EAEKCO = CastDbNull.To<string>(reader["EAEKCO"], String.Empty).Trim();
                        line.EAEDBT = CastDbNull.To<string>(reader["EAEDBT"], String.Empty).Trim();
                        line.EAUPMJ = CastDbNull.To<decimal>(reader["EAUPMJ"], 0);
                        line.EATDAY = CastDbNull.To<decimal>(reader["EATDAY"], 0);
                        line.EATIME = Common.Utilities.Common.CastToDateTime(line.EAUPMJ, line.EATDAY);
                        line.EAEDOC = CastDbNull.To<decimal>(reader["EAEDOC"], 0);
                        line.EAEDLN = CastDbNull.To<decimal>(reader["EAEDLN"], 0);
                        line.P96181 = CastDbNull.To<string>(reader["P96181"], String.Empty).Trim();
                        line.P95154 = CastDbNull.To<string>(reader["P95154"], String.Empty).Trim();
                        line.P95000 = CastDbNull.To<string>(reader["P95000"], String.Empty).Trim();
                        line.P95001 = CastDbNull.To<string>(reader["P95001"], String.Empty).Trim();
                        line.P95801 = CastDbNull.To<string>(reader["P95801"], String.Empty).Trim();
                        line.P95901 = CastDbNull.To<string>(reader["P95901"], String.Empty).Trim();
                        line.P95160 = CastDbNull.To<string>(reader["P95160"], String.Empty).Trim();
                        line.SDMCU = CastDbNull.To<string>(reader["SDMCU"], String.Empty).Trim();
                        line.SDDOCO = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.SDDCTO = CastDbNull.To<string>(reader["SDDCTO"], String.Empty).Trim();
                        line.SDLNID = CastDbNull.To<decimal>(reader["SDLNID"], 0);
                        line.SDAN8 = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.SDAN8NM = CastDbNull.To<string>(reader["SDAN8NM"], String.Empty).Trim();
                        line.SDVR01 = CastDbNull.To<string>(reader["SDVR01"], String.Empty).Trim();
                        line.SDDSC1 = CastDbNull.To<string>(reader["SDDSC1"], String.Empty).Trim();
                        line.SDDSC2 = CastDbNull.To<string>(reader["SDDSC2"], String.Empty).Trim();
                        line.SDCATNM = CastDbNull.To<string>(reader["SDCATNM"], String.Empty).Trim();
                        lines.Add(line);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return lines;
        }
    }
}
