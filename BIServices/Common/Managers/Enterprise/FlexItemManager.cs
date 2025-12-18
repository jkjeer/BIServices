using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;

namespace BIServices.Common.Managers.Enterprise
{
    public class FlexItemManager
    {

        public DataTable dt;

        public FlexItemManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("FlexItemManager.GetDescription", GetDescription());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        public FlexItemManager(BusinessSystemSession session, string programId)
        {
            dt = getDataTable(session, programId);
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("FlexItemManager.GetDescription", GetDescription());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /********************************************************************************
        * Commands
        *******************************************************************************/
        public iDB2Command GetDescription()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            *
            FROM
                F585810
            WHERE
	            DRPID = ?
            ORDER BY
                DRSEQ";
            cmd.Parameters.Add(new iDB2Parameter("ProgramId", iDB2DbType.iDB2Graphic, 10));
            return cmd;
        }

        public DataTable getDataTable(BusinessSystemSession session, string programId)
        {
            var dt = new DataTable();
            dt.Columns.Add("DRZ58ATNM", typeof(string));
            dt.Columns.Add("DRZ58APFX", typeof(string));
            dt.Columns.Add("DRZ58APFXL", typeof(decimal));
            dt.Columns.Add("DRZ58ASFX", typeof(string));
            dt.Columns.Add("DRZ58ASFXL", typeof(decimal));
            dt.Columns.Add("DRZ58ACHS", typeof(string));
            dt.Columns.Add("DRZ58AHSV", typeof(string));
            dt.Columns.Add("DRZ58CRLF", typeof(string));
            dt.Columns.Add("DRZ58ADSPU", typeof(string));
            try
            {
                var cmd = new iDB2Command();
                cmd = (session.DbCommands.ContainsKey("FlexItemManager.GetDescription")) ? session.DbCommands["FlexItemManager.GetDescription"] : GetDescription();
                cmd.Parameters[0].Value = programId;
                cmd.Connection = session.DbConnection;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        dr["DRZ58ATNM"] = CastDbNull.To<String>(reader["DRZ58ATNM"], String.Empty).Trim();
                        dr["DRZ58APFX"] = Data.CastDbNull.To<String>(reader["DRZ58APFX"], String.Empty).TrimEnd();
                        dr["DRZ58APFXL"] = CastDbNull.To<Decimal>(reader["DRZ58APFXL"], 0);
                        dr["DRZ58ASFX"] = CastDbNull.To<String>(reader["DRZ58ASFX"], String.Empty).Trim();
                        dr["DRZ58ASFXL"] = CastDbNull.To<Decimal>(reader["DRZ58ASFXL"], 0);
                        dr["DRZ58ACHS"] = CastDbNull.To<String>(reader["DRZ58ACHS"], String.Empty).Trim();
                        dr["DRZ58AHSV"] = CastDbNull.To<String>(reader["DRZ58AHSV"], String.Empty).Trim();
                        dr["DRZ58CRLF"] = CastDbNull.To<String>(reader["DRZ58CRLF"], String.Empty).Trim();
                        dr["DRZ58ADSPU"] = CastDbNull.To<String>(reader["DRZ58ADSPU"], String.Empty).Trim();
                        dt.Rows.Add(dr);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return dt;
        }

        /********************************************************************************
        * Methods - E1
        *******************************************************************************/
        public List<String> GetDescription(BusinessSystemSession session, string programId, ItemAttributes attributes)
        {
            var lines = new List<String>();
            var sb = new StringBuilder();
            try
            {
                var cmd = new iDB2Command();
                cmd = (session.DbCommands.ContainsKey("FlexItemManager.GetDescription")) ? session.DbCommands["FlexItemManager.GetDescription"] : GetDescription();
                cmd.Parameters[0].Value = programId;
                cmd.Connection = session.DbConnection;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var attName = CastDbNull.To<String>(reader["DRZ58ATNM"], String.Empty).Trim();
                        var prefix = CastDbNull.To<String>(reader["DRZ58APFX"], String.Empty).TrimEnd();
                        var prefixLength = CastDbNull.To<Decimal>(reader["DRZ58APFXL"], 0);
                        var suffix = CastDbNull.To<String>(reader["DRZ58ASFX"], String.Empty).Trim();
                        var suffixLength = CastDbNull.To<Decimal>(reader["DRZ58ASFXL"], 0);
                        var useIgnoreString = CastDbNull.To<String>(reader["DRZ58ACHS"], String.Empty).Trim();
                        var ignoreString = CastDbNull.To<String>(reader["DRZ58AHSV"], String.Empty).Trim();
                        var endLine = CastDbNull.To<String>(reader["DRZ58CRLF"], String.Empty).Trim();
                        var displayType = CastDbNull.To<String>(reader["DRZ58ADSPU"], String.Empty).Trim();
                        if (attributes.Contains(attName))
                        {
                            if (attributes[attName].Value != ignoreString || useIgnoreString == "N")
                            {
                                if (displayType == "1")
                                {
                                    sb.Append(prefix);
                                    sb.Append("".PadLeft(((int) prefixLength - prefix.Length), ' '));
                                    sb.Append(attributes[attName].Desc1);
                                    sb.Append(suffix);
                                    sb.Append("".PadLeft(((int) suffixLength - suffix.Length), ' '));
                                }
                                else
                                {
                                    if (displayType == "2")
                                    {
                                        sb.Append(prefix);
                                        sb.Append("".PadLeft(((int) prefixLength - prefix.Length), ' '));
                                        sb.Append(attributes[attName].Desc2);
                                        sb.Append(suffix);
                                        sb.Append("".PadLeft(((int) suffixLength - suffix.Length), ' '));
                                    }
                                    else
                                    {
                                        sb.Append(prefix);
                                        sb.Append("".PadLeft(((int) prefixLength - prefix.Length), ' '));
                                        sb.Append(attributes[attName].Value);
                                        sb.Append(suffix);
                                        sb.Append("".PadLeft(((int) suffixLength - suffix.Length), ' '));
                                    }
                                }
                            }
                        }

                        if (endLine == "Y")
                        {
                            lines.Add(sb.ToString());
                            sb.Clear();
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                sb.Append(" [ERROR IN FLEX DESC]");
                lines.Add(sb.ToString());
                throw new Exception(e.ToString());
            }
            return lines;
        }

        public List<String> GetDescription(BusinessSystemSession session, ItemAttributes attributes)
        {
            var lines = new List<String>();
            var sb = new StringBuilder();
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    var attName = (string)row["DRZ58ATNM"];
                    var prefix = (string)row["DRZ58APFX"];
                    var prefixLength = (decimal)row["DRZ58APFXL"];
                    var suffix = (string)row["DRZ58ASFX"];
                    var suffixLength = (decimal)row["DRZ58ASFXL"];
                    var useIgnoreString = (string)row["DRZ58ACHS"];
                    var ignoreString = (string)row["DRZ58AHSV"];
                    var endLine = (string)row["DRZ58CRLF"];
                    var displayType = (string)row["DRZ58ADSPU"];
                    if (attributes.Contains(attName))
                    {
                        if (attributes[attName].Value != ignoreString || useIgnoreString == "N")
                        {
                            if (displayType == "1")
                            {
                                sb.Append(prefix);
                                sb.Append("".PadLeft(((int)prefixLength - prefix.Length), ' '));
                                sb.Append(attributes[attName].Desc1);
                                sb.Append(suffix);
                                sb.Append("".PadLeft(((int)suffixLength - suffix.Length), ' '));
                            }
                            else
                            {
                                if (displayType == "2")
                                {
                                    sb.Append(prefix);
                                    sb.Append("".PadLeft(((int)prefixLength - prefix.Length), ' '));
                                    sb.Append(attributes[attName].Desc2);
                                    sb.Append(suffix);
                                    sb.Append("".PadLeft(((int)suffixLength - suffix.Length), ' '));
                                }
                                else
                                {
                                    sb.Append(prefix);
                                    sb.Append("".PadLeft(((int)prefixLength - prefix.Length), ' '));
                                    sb.Append(attributes[attName].Value);
                                    sb.Append(suffix);
                                    sb.Append("".PadLeft(((int)suffixLength - suffix.Length), ' '));
                                }
                            }
                        }
                    }
                    if (endLine == "Y")
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                sb.Append(" [ERROR IN FLEX DESC]");
                lines.Add(sb.ToString());
                throw new Exception(e.ToString());
            }
            return lines;
        }

        /********************************************************************************
        * Methods - Local
        *******************************************************************************/
        public string GetUnitConstruction(ItemAttributes atts, bool description = false)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(GetLiteGlassTypeCondensed(GetLite1GlassType(atts)));

                var spacer1Type = GetSpacerType(atts);
                if (!String.IsNullOrEmpty(spacer1Type))
                {
                    sb.Append(" / ");
                    sb.Append(spacer1Type);
                }

                var glass2Type = GetLiteGlassTypeCondensed(GetLite2GlassType(atts));
                if (!String.IsNullOrEmpty(glass2Type))
                {
                    sb.Append(" / ");
                    sb.Append(glass2Type);
                }

                var glass3Type = GetLiteGlassTypeCondensed(GetLite3GlassType(atts));
                var spacer2Type = GetSpacerType(atts);
                if (!String.IsNullOrEmpty(glass3Type))
                {
                    sb.Append(" / ");
                    sb.Append(spacer2Type);
                    sb.Append(" / ");
                    sb.Append(glass3Type);
                }
            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLite1GlassType(ItemAttributes atts)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(atts.GetDecimalAttributeValue("Z58L1TK"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeDesc2("Z58L1CL"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L1HT"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L11C"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L12C"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L1PX"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeDesc1("Z58L1LPT"));
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLite2GlassType(ItemAttributes atts)
        {
            try
            {
                var sb = new StringBuilder();
                if (atts.GetDecimalAttributeValue("Z58L2TK") > 0)
                {
                    sb.Append(atts.GetDecimalAttributeValue("Z58L2TK"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeDesc2("Z58L2CL"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeValue("Z58L2HT"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeValue("Z58L23C"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeValue("Z58L24C"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeValue("Z58L2PX"));
                    sb.Append(":");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L2LPT"));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLite3GlassType(ItemAttributes atts)
        {
                    try
                    {
                        var sb = new StringBuilder();
            if (atts.GetDecimalAttributeValue("Z58L3TK") > 0)
            {
                sb.Append(atts.GetDecimalAttributeValue("Z58L3TK"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeDesc2("Z58L3CL"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L3HT"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L35C"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L36C"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeValue("Z58L3PX"));
                sb.Append(":");
                sb.Append(atts.GetStringAttributeDesc1("Z58L3LPT"));
            }
            return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLiteGlassTypeCondensed(string input)
        {
            try
            {
                var tokens = input.Split(':').Where(s => !string.IsNullOrWhiteSpace(s));
                return string.Join(" ", tokens);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLiteRouting(ItemAttributes atts, bool description = false)
        {
            try
            {
                var sb = new StringBuilder();
                if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1RTG")))
                {
                    sb.Append("L1:");
                    sb.Append(atts.GetStringAttributeValue("Z58L1RTG"));
                    if (description)
                    {
                        sb.Append(" [");
                        sb.Append(atts.GetStringAttributeDesc1("Z58L1RTG"));
                        sb.Append("]");
                    }
                }
                if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L2RTG")))
                {
                    sb.Append(" / ");
                    sb.Append("L2:");
                    sb.Append(atts.GetStringAttributeValue("Z58L2RTG"));
                    if (description)
                    {
                        sb.Append(" [");
                        sb.Append(atts.GetStringAttributeDesc1("Z58L2RTG"));
                        sb.Append("]");
                    }
                }
                if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L3RTG")))
                {
                    sb.Append(" / ");
                    sb.Append("L3:");
                    sb.Append(atts.GetStringAttributeValue("Z58L3RTG"));
                    if (description)
                    {
                        sb.Append(" [");
                        sb.Append(atts.GetStringAttributeDesc1("Z58L3RTG"));
                        sb.Append("]");
                    }
                }
                return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetDimension(ItemAttributes atts)
        {
            try
            {
                var sb = new StringBuilder();
                if (atts.GetDecimalAttributeValue("Z58L1BDM") + atts.GetDecimalAttributeValue("Z58L1BNM") > 0)
                {
                    if (atts.GetDecimalAttributeValue("Z58L1BDM") > 0)
                    {
                        sb.Append(atts.GetStringAttributeValue("Z58L1BDM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1BNM") > 0)
                    {
                        sb.Append(" ");
                        sb.Append(atts.GetStringAttributeValue("Z58L1BNM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1BDN") > 0)
                    {
                        sb.Append("/");
                        sb.Append(atts.GetStringAttributeValue("Z58L1BDN"));
                    }
                    sb.Append(" X ");
                }

                if (atts.GetDecimalAttributeValue("Z58L1LDM") + atts.GetDecimalAttributeValue("Z58L1LNM") > 0)
                {
                    if (atts.GetDecimalAttributeValue("Z58L1LDM") > 0)
                    {
                        sb.Append(atts.GetStringAttributeValue("Z58L1LDM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1LNM") > 0)
                    {
                        sb.Append(" ");
                        sb.Append(atts.GetStringAttributeValue("Z58L1LNM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1LDN") > 0)
                    {
                        sb.Append("/");
                        sb.Append(atts.GetStringAttributeValue("Z58L1LDN"));
                    }
                    sb.Append(" X ");
                }

                if (atts.GetDecimalAttributeValue("Z58L1RDM") + atts.GetDecimalAttributeValue("Z58L1RNM") > 0)
                {
                    if (atts.GetDecimalAttributeValue("Z58L1RDM") > 0)
                    {
                        sb.Append(atts.GetStringAttributeValue("Z58L1RDM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1RNM") > 0)
                    {
                        sb.Append(" ");
                        sb.Append(atts.GetStringAttributeValue("Z58L1RNM"));
                    }
                    if (atts.GetDecimalAttributeValue("Z58L1RDN") > 0)
                    {
                        sb.Append("/");
                        sb.Append(atts.GetStringAttributeValue("Z58L1RDN"));
                    }
                    sb.Append(" X ");

                    if (atts.GetDecimalAttributeValue("Z58L1TDM") + atts.GetDecimalAttributeValue("Z58L1TNM") > 0)
                    {
                        if (atts.GetDecimalAttributeValue("Z58L1TDM") > 0)
                        {
                            sb.Append(atts.GetStringAttributeValue("Z58L1TDM"));
                        }
                        if (atts.GetDecimalAttributeValue("Z58L1TNM") > 0)
                        {
                            sb.Append(" ");
                            sb.Append(atts.GetStringAttributeValue("Z58L1TNM"));
                        }
                        if (atts.GetDecimalAttributeValue("Z58L1TDN") > 0)
                        {
                            sb.Append("/");
                            sb.Append(atts.GetStringAttributeValue("Z58L1TDN"));
                        }
                        sb.Append(" X ");
                    }

                    if (atts.GetDecimalAttributeValue("Z58L11DM") + atts.GetDecimalAttributeValue("Z58L11NM") > 0)
                    {
                        if (atts.GetDecimalAttributeValue("Z58L11DM") > 0)
                        {
                            sb.Append(atts.GetStringAttributeValue("Z58L11DM"));
                        }
                        if (atts.GetDecimalAttributeValue("Z58L11NM") > 0)
                        {
                            sb.Append(" ");
                            sb.Append(atts.GetStringAttributeValue("Z58L11NM"));
                        }
                        if (atts.GetDecimalAttributeValue("Z58L11DN") > 0)
                        {
                            sb.Append("/");
                            sb.Append(atts.GetStringAttributeValue("Z58L11DN"));
                        }
                        sb.Append(" X ");

                        if (atts.GetDecimalAttributeValue("Z58L12DM") + atts.GetDecimalAttributeValue("Z58L12NM") > 0)
                        {
                            if (atts.GetDecimalAttributeValue("Z58L12DM") > 0)
                            {
                                sb.Append(atts.GetStringAttributeValue("Z58L12DM"));
                            }
                            if (atts.GetDecimalAttributeValue("Z58L12NM") > 0)
                            {
                                sb.Append(" ");
                                sb.Append(atts.GetStringAttributeValue("Z58L12NM"));
                            }
                            if (atts.GetDecimalAttributeValue("Z58L12DN") > 0)
                            {
                                sb.Append("/");
                                sb.Append(atts.GetStringAttributeValue("Z58L12DN"));
                            }
                            sb.Append(" X ");
                        }
                    }
            }

            return (sb.Length >= 3) ? sb.ToString().Substring(0, sb.Length - 3) : sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetAirspace(ItemAttributes atts, bool description = false)
        {
                                    try
                                    {
                                        var sb = new StringBuilder();
            if (atts.GetStringAttributeValue("Z58TA1") == "Y")
            {
                sb.Append("TUBES:");
                sb.Append(atts.GetStringAttributeValue("Z58TA1"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58S1GM")))
            {
                sb.Append("GAS:");
                sb.Append(atts.GetStringAttributeValue("Z58S1GM"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58S1GM"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetSpacerType(ItemAttributes atts, bool description = false)
        {
                                        try
                                        {
                                            var sb = new StringBuilder();
            if (atts.GetDecimalAttributeValue("Z58L1SSZ") > 0)
            {
                sb.Append(atts.GetDecimalAttributeValue("Z58L1SSZ"));
                sb.Append(" ");
                sb.Append((atts.GetStringAttributeValue("Z58L1STP").Length >= 2)?atts.GetStringAttributeValue("Z58L1STP").Substring(0, 2): atts.GetStringAttributeValue("Z58L1STP"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L1STP"));
                    sb.Append("]");
                }
            }
            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetBarAssembly(ItemAttributes atts, bool description = false)
        {
                                            try
                                            {
                                                var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58M1A1")))
            {
                sb.Append("M1A1:");
                sb.Append(atts.GetStringAttributeValue("Z58M1A1"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58M1A1"));
                    sb.Append("]");
                }
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58M2A1")))
            {
                sb.Append(" / ");
                sb.Append("M2A1:");
                sb.Append(atts.GetStringAttributeValue("Z58M2A1"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58M2A1"));
                    sb.Append("]");
                }
            }
            return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetGrid(ItemAttributes atts, bool description = false)
        {
                                                try
                                                {
                                                    var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58GSS1")))
            {
                sb.Append(atts.GetStringAttributeValue("Z58GSS1"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58GSS1"));
                    sb.Append("]");
                }
                if (atts.GetDecimalAttributeValue("Z58GVA1") > 0)
                {
                    sb.Append(" V:");
                    sb.Append(atts.GetDecimalAttributeValue("Z58GVA1"));
                }
                if (atts.GetDecimalAttributeValue("Z58GHA1") > 0)
                {
                    sb.Append(" H:");
                    sb.Append(atts.GetDecimalAttributeValue("Z58GHA1"));
                }
                if (atts.GetDecimalAttributeValue("Z58OHA1") > 0)
                {
                    sb.Append(" OH:");
                    sb.Append(atts.GetDecimalAttributeValue("Z58OHA1"));
                }
                if (atts.GetDecimalAttributeValue("Z58OWA1") > 0)
                {
                    sb.Append(" OW:");
                    sb.Append(atts.GetDecimalAttributeValue("Z58OWA1"));
                }
                if (atts.GetDecimalAttributeValue("Z58OOA1") > 0)
                {
                    sb.Append(" OT:");
                    sb.Append(atts.GetDecimalAttributeValue("Z58OOA1"));
                }
            }
            return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetGridIndexes(ItemAttributes atts)
        {
                                                    try
                                                    {
                                                        var sb1 = new StringBuilder();
            if (atts.GetDecimalAttributeValue("Z58IGVP1") > 0)
            {
                sb1.Append("V [");
                sb1.Append(atts.GetDecimalAttributeValue("Z58IGVP1"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGVP2") > 0)
            {
                sb1.Append(" / ");
                sb1.Append(atts.GetDecimalAttributeValue("Z58IGVP2"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGVP3") > 0)
            {
                sb1.Append(" / ");
                sb1.Append(atts.GetDecimalAttributeValue("Z58IGVP3"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGVP4") > 0)
            {
                sb1.Append(" / ");
                sb1.Append(atts.GetDecimalAttributeValue("Z58IGVP4"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGVP5") > 0)
            {
                sb1.Append(" / ");
                sb1.Append(atts.GetDecimalAttributeValue("Z58IGVP5"));
            }
            if (sb1.Length > 0)
            {
                sb1.Append("] ");
            }

            var sb2 = new StringBuilder();
            if (atts.GetDecimalAttributeValue("Z58IGHP1") > 0)
            {
                sb2.Append("H [");
                sb2.Append(atts.GetDecimalAttributeValue("Z58IGHP1"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGHP2") > 0)
            {
                sb2.Append(" / ");
                sb2.Append(atts.GetDecimalAttributeValue("Z58IGHP2"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGHP3") > 0)
            {
                sb2.Append(" / ");
                sb2.Append(atts.GetDecimalAttributeValue("Z58IGHP3"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGHP4") > 0)
            {
                sb2.Append(" / ");
                sb2.Append(atts.GetDecimalAttributeValue("Z58IGHP4"));
            }
            if (atts.GetDecimalAttributeValue("Z58IGHP5") > 0)
            {
                sb2.Append(" / ");
                sb2.Append(atts.GetDecimalAttributeValue("Z58IGHP5"));
            }
            if (sb2.Length > 0)
            {
                sb2.Append("] ");
            }

            return $"{sb1} {sb2}".Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetPreserveProcessing(ItemAttributes atts, bool description = false)
        {
                                                        try
                                                        {
                                                            var sb = new StringBuilder();

            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587563")))
            {
                sb.Append("ExtCB:");
                sb.Append(atts.GetStringAttributeValue("Z587563"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587563"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587566")))
            {
                sb.Append("ExtCL:");
                sb.Append(atts.GetStringAttributeValue("Z587566"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587566"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587569")))
            {
                sb.Append("ExtSC:");
                sb.Append(atts.GetStringAttributeValue("Z587569"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587569"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }

            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587575")))
            {
                sb.Append("IntCB:");
                sb.Append(atts.GetStringAttributeValue("Z587575"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587575"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587578")))
            {
                sb.Append("IntCL:");
                sb.Append(atts.GetStringAttributeValue("Z587578"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587578"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z587581")))
            {
                sb.Append("IntSC:");
                sb.Append(atts.GetStringAttributeValue("Z587581"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z587581"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }

            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetLogo(ItemAttributes atts)
        {
                                                            try
                                                            {
                                                                var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1ALT")))
            {
                sb.Append("L1ALT:");
                sb.Append(atts.GetStringAttributeValue("Z58L1ALT"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1TL")) && atts.GetStringAttributeValue("Z58L1TL") != "N")
            {
                sb.Append("L1TL:");
                sb.Append(atts.GetStringAttributeValue("Z58L1TL"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L2TL")) && atts.GetStringAttributeValue("Z58L2TL") != "N")
            {
                sb.Append("L2TL:");
                sb.Append(atts.GetStringAttributeValue("Z58L2TL"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L3TL")) && atts.GetStringAttributeValue("Z58L3TL") != "N")
            {
                sb.Append("L3TL:");
                sb.Append(atts.GetStringAttributeValue("Z58L3TL"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5840435")) && atts.GetStringAttributeValue("Z5840435") != "N")
            {
                sb.Append("IGCC:");
                sb.Append(atts.GetStringAttributeValue("Z5840435"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1LX")))
            {
                sb.Append("X:");
                sb.Append(atts.GetStringAttributeValue("Z58L1LX"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1LY")))
            {
                sb.Append("Y:");
                sb.Append(atts.GetStringAttributeValue("Z58L1LY"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("NFRC")))
            {
                sb.Append("NFRC:");
                sb.Append(atts.GetStringAttributeValue("NFRC"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("NFRC2")))
            {
                sb.Append("NFRC2:");
                sb.Append(atts.GetStringAttributeValue("NFRC2"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5801895")))
            {
                sb.Append("ETCHDWG:");
                sb.Append(atts.GetStringAttributeValue("Z5801895"));
                sb.Append(" ");
            }
            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetEdgeProcessing(ItemAttributes atts, bool description = false)
        {
                                                                try
                                                                {
                                                                    var sb = new StringBuilder();
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1EW")) && atts.GetStringAttributeValue("Z58L1EW") != "NO")
            {
                sb.Append("L1EW:");
                sb.Append(atts.GetStringAttributeValue("Z58L1EW"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L1EW"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L2EW")) && atts.GetStringAttributeValue("Z58L2EW") != "NO")
            {
                sb.Append("L2EW:");
                sb.Append(atts.GetStringAttributeValue("Z58L2EW"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L2EW"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L3EW")) && atts.GetStringAttributeValue("Z58L3EW") != "NO")
            {
                sb.Append("L3EW:");
                sb.Append(atts.GetStringAttributeValue("Z58L3EW"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L3EW"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5805680")))
            {
                sb.Append("EDW:");
                sb.Append(atts.GetStringAttributeValue("Z5805680"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z5805680"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5853001")))
            {
                sb.Append("PLSide:");
                sb.Append(atts.GetStringAttributeValue("Z5853001"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5853002")))
            {
                sb.Append("EWSide:");
                sb.Append(atts.GetStringAttributeValue("Z5853002"));
                sb.Append(" ");
            }

            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1BL")))
            {
                sb.Append("L1BL:");
                sb.Append(atts.GetStringAttributeValue("Z58L1BL"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L1BL"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L2BL")))
            {
                sb.Append("L2BL:");
                sb.Append(atts.GetStringAttributeValue("Z58L2BL"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L2BL"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L3BL")))
            {
                sb.Append("L3BL:");
                sb.Append(atts.GetStringAttributeValue("Z58L3BL"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z58L3BL"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }

            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetHoleProcessing(ItemAttributes atts, bool description = false)
        {
                                                                    try
                                                                    {
                                                                        var sb = new StringBuilder();

            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L1HL")))
            {
                sb.Append("L1HL:");
                sb.Append(atts.GetStringAttributeValue("Z58L1HL"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L2HL")))
            {
                sb.Append("L2HL:");
                sb.Append(atts.GetStringAttributeValue("Z58L2HL"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z58L3HL")))
            {
                sb.Append("L3HL:");
                sb.Append(atts.GetStringAttributeValue("Z58L3HL"));
                sb.Append(" ");
            }

            if (atts.GetDecimalAttributeValue("Z581365") > 0)
            {
                sb.Append("L1HL#:");
                sb.Append(atts.GetStringAttributeValue("Z581365"));
                sb.Append(" ");
            }
            if (atts.GetDecimalAttributeValue("Z5803005") > 0)
            {
                sb.Append("L2HL#:");
                sb.Append(atts.GetStringAttributeValue("Z5803005"));
                sb.Append(" ");
            }
            if (atts.GetDecimalAttributeValue("Z5804595") > 0)
            {
                sb.Append("L3HL#:");
                sb.Append(atts.GetStringAttributeValue("Z5804595"));
                sb.Append(" ");
            }

            if (atts.GetDecimalAttributeValue("Z5860081") > 0)
            {
                sb.Append("UnitHL#:");
                sb.Append(atts.GetStringAttributeValue("Z5860081"));
                sb.Append(" ");
            }
            if (!String.IsNullOrEmpty(atts.GetStringAttributeValue("Z5860083")))
            {
                sb.Append("DIA:");
                sb.Append(atts.GetStringAttributeValue("Z5860083"));
                if (description)
                {
                    sb.Append(" [");
                    sb.Append(atts.GetStringAttributeDesc1("Z5860083"));
                    sb.Append("]");
                }
                sb.Append(" ");
            }

            return sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetHashValue(ItemAttributes atts)
        {
                                                                        try
                                                                        {
                                                                            MD5 md5 = System.Security.Cryptography.MD5.Create();
            string input = "";


            // Mono
            if (atts.GetDecimalAttributeValue("Z58L2TK") == 0 && atts.GetDecimalAttributeValue("Z58L3TK") == 0)
            {
                input = $@"
            { atts.GetStringAttributeValue("Z58L1TK")}
            { atts.GetStringAttributeValue("Z58L11C")}
            { atts.GetStringAttributeValue("Z58L1CL")}
            { atts.GetStringAttributeValue("Z58L1HT")}
            { atts.GetStringAttributeValue("Z58L12C")}
            { atts.GetStringAttributeValue("Z58L1PX")}
            { atts.GetStringAttributeValue("Z58L1LPT")}
            { atts.GetStringAttributeValue("Z58SHAPE")}
            { atts.GetStringAttributeValue("Z58L1BDM")}
            { atts.GetStringAttributeValue("Z58L1BNM")}
            { atts.GetStringAttributeValue("Z58L1BDN")}
            { atts.GetStringAttributeValue("Z58L1LDM")}
            { atts.GetStringAttributeValue("Z58L1LNM")}
            { atts.GetStringAttributeValue("Z58L1LDN")}
            { atts.GetStringAttributeValue("Z58L1RDM")}
            { atts.GetStringAttributeValue("Z58L1RNM")}
            { atts.GetStringAttributeValue("Z58L1RDN")}
            { atts.GetStringAttributeValue("Z58L1TDM")}
            { atts.GetStringAttributeValue("Z58L1TNM")}
            { atts.GetStringAttributeValue("Z58L1TDN")}
            { atts.GetStringAttributeValue("Z58L11DM")}
            { atts.GetStringAttributeValue("Z58L11NM")}
            { atts.GetStringAttributeValue("Z58L11DN")}
            { atts.GetStringAttributeValue("Z58L12DM")}
            { atts.GetStringAttributeValue("Z58L12NM")}
            { atts.GetStringAttributeValue("Z58L12DN")}
            { atts.GetStringAttributeValue("Z58L1VG")}
            { atts.GetStringAttributeValue("Z58L1VGS")}
            { atts.GetStringAttributeValue("Z58L1VGT")}
            { atts.GetStringAttributeValue("Z58L1VGW")}
            { atts.GetStringAttributeValue("Z58L1VGH")}
            { atts.GetStringAttributeValue("Z58L1BL")}
            { atts.GetStringAttributeValue("Z58L1HL")}
            { atts.GetStringAttributeValue("Z58L1SF")}
            { atts.GetStringAttributeValue("Z58L1SD")}
            { atts.GetStringAttributeValue("Z58L1EW")}
            { atts.GetStringAttributeValue("Z58L1PQL")}
            { atts.GetStringAttributeValue("Z58CC")}
            { atts.GetStringAttributeValue("Z58MC")}
            { atts.GetStringAttributeValue("Z58L1ALT")}
            { atts.GetStringAttributeValue("Z58L1PTS")}
            { atts.GetStringAttributeValue("Z5801895")}
            ";
            }

            // Double Pane
            if (atts.GetDecimalAttributeValue("Z58L2TK") > 0 && atts.GetDecimalAttributeValue("Z58L3TK") == 0) {
                input = $@"
            {atts.GetStringAttributeValue("Z58L1TK")}
            {atts.GetStringAttributeValue("Z58L11C")}
            {atts.GetStringAttributeValue("Z58L1CL")}
            {atts.GetStringAttributeValue("Z58L1HT")}
            {atts.GetStringAttributeValue("Z58L12C")}
            {atts.GetStringAttributeValue("Z58L1PX")}
            {atts.GetStringAttributeValue("Z58L1LPT")}
            {atts.GetStringAttributeValue("Z58SHAPE")}
            {atts.GetStringAttributeValue("Z58L1BDM")}
            {atts.GetStringAttributeValue("Z58L1BNM")}
            {atts.GetStringAttributeValue("Z58L1BDN")}
            {atts.GetStringAttributeValue("Z58L1LDM")}
            {atts.GetStringAttributeValue("Z58L1LNM")}
            {atts.GetStringAttributeValue("Z58L1LDN")}
            {atts.GetStringAttributeValue("Z58L1RDM")}
            {atts.GetStringAttributeValue("Z58L1RNM")}
            {atts.GetStringAttributeValue("Z58L1RDN")}
            {atts.GetStringAttributeValue("Z58L1TDM")}
            {atts.GetStringAttributeValue("Z58L1TNM")}
            {atts.GetStringAttributeValue("Z58L1TDN")}
            {atts.GetStringAttributeValue("Z58L11DM")}
            {atts.GetStringAttributeValue("Z58L11NM")}
            {atts.GetStringAttributeValue("Z58L11DN")}
            {atts.GetStringAttributeValue("Z58L12DM")}
            {atts.GetStringAttributeValue("Z58L12NM")}
            {atts.GetStringAttributeValue("Z58L12DN")}
            {atts.GetStringAttributeValue("Z58L1VG")}
            {atts.GetStringAttributeValue("Z58L1VGS")}
            {atts.GetStringAttributeValue("Z58L1VGT")}
            {atts.GetStringAttributeValue("Z58L1VGW")}
            {atts.GetStringAttributeValue("Z58L1VGH")}
            {atts.GetStringAttributeValue("Z58L1BL")}
            {atts.GetStringAttributeValue("Z58L1HL")}
            {atts.GetStringAttributeValue("Z58L1SF")}
            {atts.GetStringAttributeValue("Z58L1SD")}
            {atts.GetStringAttributeValue("Z58L1ALT")}
            {atts.GetStringAttributeValue("Z58L1PTS")}
            {atts.GetStringAttributeValue("Z5801895")}
            {atts.GetStringAttributeValue("Z58L1SSZ")}
            {atts.GetStringAttributeValue("Z58L1STP")}
            {atts.GetStringAttributeValue("Z58L2TK")}
            {atts.GetStringAttributeValue("Z58L23C")}
            {atts.GetStringAttributeValue("Z58L2CL")}
            {atts.GetStringAttributeValue("Z58L2HT")}
            {atts.GetStringAttributeValue("Z58L24C")}
            {atts.GetStringAttributeValue("Z58L2PX")}
            {atts.GetStringAttributeValue("Z58L2LPT")}
            {atts.GetStringAttributeValue("Z58L2VG")}
            {atts.GetStringAttributeValue("Z58L2VGS")}
            {atts.GetStringAttributeValue("Z58L2VGT")}
            {atts.GetStringAttributeValue("Z58L2VGW")}
            {atts.GetStringAttributeValue("Z58L2VGH")}
            {atts.GetStringAttributeValue("Z58L2BL")}
            {atts.GetStringAttributeValue("Z5803005")}
            {atts.GetStringAttributeValue("Z58L2SF")}
            {atts.GetStringAttributeValue("Z58L2SD")}
            {atts.GetStringAttributeValue("Z58L2PTS")}
            {atts.GetStringAttributeValue("Z5803515")}
            ";
            }

            // Triple Pane
            if (atts.GetDecimalAttributeValue("Z58L3TK") > 0)
            {
                input = $@"
            { atts.GetStringAttributeValue("Z58L1TK")}
            { atts.GetStringAttributeValue("Z58L11C")}
            { atts.GetStringAttributeValue("Z58L1CL")}
            { atts.GetStringAttributeValue("Z58L1HT")}
            { atts.GetStringAttributeValue("Z58L12C")}
            { atts.GetStringAttributeValue("Z58L1PX")}
            { atts.GetStringAttributeValue("Z58L1LPT")}
            { atts.GetStringAttributeValue("Z58SHAPE")}
            { atts.GetStringAttributeValue("Z58L1BDM")}
            { atts.GetStringAttributeValue("Z58L1BNM")}
            { atts.GetStringAttributeValue("Z58L1BDN")}
            { atts.GetStringAttributeValue("Z58L1LDM")}
            { atts.GetStringAttributeValue("Z58L1LNM")}
            { atts.GetStringAttributeValue("Z58L1LDN")}
            { atts.GetStringAttributeValue("Z58L1RDM")}
            { atts.GetStringAttributeValue("Z58L1RNM")}
            { atts.GetStringAttributeValue("Z58L1RDN")}
            { atts.GetStringAttributeValue("Z58L1TDM")}
            { atts.GetStringAttributeValue("Z58L1TNM")}
            { atts.GetStringAttributeValue("Z58L1TDN")}
            { atts.GetStringAttributeValue("Z58L11DM")}
            { atts.GetStringAttributeValue("Z58L11NM")}
            { atts.GetStringAttributeValue("Z58L11DN")}
            { atts.GetStringAttributeValue("Z58L12DM")}
            { atts.GetStringAttributeValue("Z58L12NM")}
            { atts.GetStringAttributeValue("Z58L12DN")}
            { atts.GetStringAttributeValue("Z58L1VG")}
            { atts.GetStringAttributeValue("Z58L1VGS")}
            { atts.GetStringAttributeValue("Z58L1VGT")}
            { atts.GetStringAttributeValue("Z58L1VGW")}
            { atts.GetStringAttributeValue("Z58L1VGH")}
            { atts.GetStringAttributeValue("Z58L1BL")}
            { atts.GetStringAttributeValue("Z58L1HL")}
            { atts.GetStringAttributeValue("Z58L1SF")}
            { atts.GetStringAttributeValue("Z58L1SD")}
            { atts.GetStringAttributeValue("Z58L1ALT")}
            { atts.GetStringAttributeValue("Z58L1PTS")}
            { atts.GetStringAttributeValue("Z5801895")}
            { atts.GetStringAttributeValue("Z58L1SSZ")}
            { atts.GetStringAttributeValue("Z58L1STP")}
            { atts.GetStringAttributeValue("Z58L2TK")}
            { atts.GetStringAttributeValue("Z58L23C")}
            { atts.GetStringAttributeValue("Z58L2CL")}
            { atts.GetStringAttributeValue("Z58L2HT")}
            { atts.GetStringAttributeValue("Z58L24C")}
            { atts.GetStringAttributeValue("Z58L2PX")}
            { atts.GetStringAttributeValue("Z58L2LPT")}
            { atts.GetStringAttributeValue("Z58L2VG")}
            { atts.GetStringAttributeValue("Z58L2VGS")}
            { atts.GetStringAttributeValue("Z58L2VGT")}
            { atts.GetStringAttributeValue("Z58L2VGW")}
            { atts.GetStringAttributeValue("Z58L2VGH")}
            { atts.GetStringAttributeValue("Z58L2BL")}
            { atts.GetStringAttributeValue("Z5803005")}
            { atts.GetStringAttributeValue("Z58L2SF")}
            { atts.GetStringAttributeValue("Z58L2SD")}
            { atts.GetStringAttributeValue("Z58L2PTS")}
            { atts.GetStringAttributeValue("Z5803515")}
            { atts.GetStringAttributeValue("Z58L2SSZ")}
            { atts.GetStringAttributeValue("Z58L2STP")}
            { atts.GetStringAttributeValue("Z58L3TK")}
            { atts.GetStringAttributeValue("Z58L35C")}
            { atts.GetStringAttributeValue("Z58L3CL")}
            { atts.GetStringAttributeValue("Z58L3HT")}
            { atts.GetStringAttributeValue("Z58L36C")}
            { atts.GetStringAttributeValue("Z58L3PX")}
            { atts.GetStringAttributeValue("Z58L3LPT")}
            { atts.GetStringAttributeValue("Z58L3VG")}
            { atts.GetStringAttributeValue("Z58L3VGS")}
            { atts.GetStringAttributeValue("Z58L3VGT")}
            { atts.GetStringAttributeValue("Z58L3VGW")}
            { atts.GetStringAttributeValue("Z58L3VGH")}
            { atts.GetStringAttributeValue("Z58L3BL")}
            { atts.GetStringAttributeValue("Z5804595")}
            { atts.GetStringAttributeValue("Z58L3SF")}
            { atts.GetStringAttributeValue("Z58L3SD")}
            { atts.GetStringAttributeValue("Z58L3PTS")}
            { atts.GetStringAttributeValue("Z5805105")}
            { atts.GetStringAttributeValue("Z58M1A2")}
            { atts.GetStringAttributeValue("Z58M1A2O")}
            { atts.GetStringAttributeValue("Z58M2A2")}
            { atts.GetStringAttributeValue("Z58M2A2O")}
            { atts.GetStringAttributeValue("Z58OWA2")}
            { atts.GetStringAttributeValue("Z58OHA2")}
            ";
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

 
    }
}
