using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.Archie
{
    [CollectionDataContract(Namespace = "")]
    public class TCCUnions : List<TCCUnion>
    {
    }

    [DataContract(Namespace = "")]

    public class TCCUnion
    {
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Filter { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public int fsdDocID { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public string PDFFileName { get; set; }
        [DataMember(Order = 40, EmitDefaultValue = false)]
        public bool PDFExists { get; set; }
        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string PDFPath { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = false)]
        public string XMLFileName { get; set; }
        [DataMember(Order = 70, EmitDefaultValue = false)]
        public bool XMLExists { get; set; }
        [DataMember(Order = 80, EmitDefaultValue = false)]
        public string XMLPath { get; set; }
        [DataMember(Order = 90, EmitDefaultValue = false)]
        public int Version { get; set; }
        [DataMember(Order = 100, EmitDefaultValue = false)]
        public int ImageSize { get; set; }
        [DataMember(Order = 110, EmitDefaultValue = false)]
        public string ImageStr { get; set; }
        [DataMember(Order = 120, EmitDefaultValue = false)]
        public short fsdDeleteFlag { get; set; }
        [DataMember(Order = 130, EmitDefaultValue = false)]
        public string fsdSatID { get; set; }
        [DataMember(Order = 140, EmitDefaultValue = false)]
        public string fsdDocType { get; set; }
        [DataMember(Order = 150, EmitDefaultValue = false)]
        public DateTime fsdDateStored { get; set; }
        [DataMember(Order = 160, EmitDefaultValue = false)]
        public DateTime fsdDateDelete { get; set; }
        [DataMember(Order = 170, EmitDefaultValue = false)]
        public string OfflinePath { get; set; }
        [DataMember(Order = 180, EmitDefaultValue = false)]
        public int OfflineFlag { get; set; }
        [DataMember(Order = 190, EmitDefaultValue = false)]
        public DateTime fsdOfflineDate { get; set; }
        [DataMember(Order = 200, EmitDefaultValue = false)]
        public int CatID { get; set; }
        [DataMember(Order = 210, EmitDefaultValue = false)]
        public int DocID { get; set; }
        [DataMember(Order = 220, EmitDefaultValue = false)]
        public int SatID { get; set; }
        [DataMember(Order = 230, EmitDefaultValue = false)]
        public DateTime DateStored { get; set; }
        [DataMember(Order = 240, EmitDefaultValue = false)]
        public DateTime DateDelete { get; set; }
        [DataMember(Order = 250, EmitDefaultValue = false)]
        public string DocType { get; set; }
        [DataMember(Order = 260, EmitDefaultValue = false)]
        public DateTime OfflineDate { get; set; }
        [DataMember(Order = 270, EmitDefaultValue = false)]
        public int DeleteFlag { get; set; }
        [DataMember(Order = 280, EmitDefaultValue = false)]
        public int FS_InboxOwner { get; set; }
        [DataMember(Order = 290, EmitDefaultValue = false)]
        public string FS_InboxSubject { get; set; }
        [DataMember(Order = 300, EmitDefaultValue = false)]
        public string FS_ReprintServer { get; set; }
        [DataMember(Order = 310, EmitDefaultValue = false)]
        public string FS_ReprintPrinter { get; set; }
        [DataMember(Order = 320, EmitDefaultValue = false)]
        public string CATEGORY { get; set; }
        [DataMember(Order = 330, EmitDefaultValue = false)]
        public string DOCSUBJECT { get; set; }
        [DataMember(Order = 340, EmitDefaultValue = false)]
        public string DOCLOCATION { get; set; }
        [DataMember(Order = 350, EmitDefaultValue = false)]
        public string DOCNUMBER1 { get; set; }
        [DataMember(Order = 360, EmitDefaultValue = false)]
        public string DOCNUMBER2 { get; set; }
        [DataMember(Order = 370, EmitDefaultValue = false)]
        public string DOCDESC1 { get; set; }
        [DataMember(Order = 380, EmitDefaultValue = false)]
        public DateTime DOCDATE_CALC { get; set; }
        [DataMember(Order = 390, EmitDefaultValue = false)]
        public string LOADNUM { get; set; }
        [DataMember(Order = 400, EmitDefaultValue = false)]
        public string CARRIER { get; set; }
        [DataMember(Order = 410, EmitDefaultValue = false)]
        public string BOL { get; set; }
        [DataMember(Order = 420, EmitDefaultValue = false)]
        public string PO1 { get; set; }
        [DataMember(Order = 430, EmitDefaultValue = false)]
        public string DOCDATE { get; set; }
        [DataMember(Order = 440, EmitDefaultValue = false)]
        public string DOCTYPE1 { get; set; }
        [DataMember(Order = 450, EmitDefaultValue = false)]
        public string DOCTYPE2 { get; set; }
        [DataMember(Order = 460, EmitDefaultValue = false)]
        public string DOCDESC2 { get; set; }
        [DataMember(Order = 470, EmitDefaultValue = false)]
        public string DOCTITLE { get; set; }
        [DataMember(Order = 480, EmitDefaultValue = false)]
        public string DOCAUTHOR { get; set; }
        [DataMember(Order = 490, EmitDefaultValue = false)]
        public DateTime DOCDATE1 { get; set; }
        [DataMember(Order = 500, EmitDefaultValue = false)]
        public DateTime DOCDATE2 { get; set; }
        [DataMember(Order = 510, EmitDefaultValue = false)]
        public string PO2 { get; set; }
        [DataMember(Order = 520, EmitDefaultValue = false)]
        public string PO3 { get; set; }
        [DataMember(Order = 530, EmitDefaultValue = false)]
        public string PO4 { get; set; }
        [DataMember(Order = 540, EmitDefaultValue = false)]
        public string PO5 { get; set; }
        [DataMember(Order = 550, EmitDefaultValue = false)]
        public string DOCSTAMP { get; set; }
        [DataMember(Order = 560, EmitDefaultValue = false)]
        public string DOCNAME { get; set; }
        [DataMember(Order = 570, EmitDefaultValue = false)]
        public string Hex2str { get; set; }
        [DataMember(Order = 580, EmitDefaultValue = false)]
        public string Hex3str { get; set; }

        [DataMember(Order = 580, EmitDefaultValue = false)]
        public bool AllowDeleting { get; set; }
    }
}
