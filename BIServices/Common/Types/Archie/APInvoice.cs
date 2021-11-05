using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BIServices.Common.Types.Response;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    [CollectionDataContract(Namespace = "")]
    public class APInvoices : List<APInvoice>
    {
    }

    [DataContract(Namespace = "")]
    public class APInvoice
    {
        public APInvoice()
        {
            HasTXT = false;
            HasPDF = false;
            IsInsertedDocHeader = false;
            IsInsertedAPInvoice = false;
            IsArchivedTXT = false;
            IsArchivedPDF = false;
        }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public int DocID { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public int SatID { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public DateTime DateStored { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public DateTime? DateDelete { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string DocType { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public DateTime? OfflineDate { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = false)]
        public int? DeleteFlag { get; set; }

        [DataMember(Order = 80, EmitDefaultValue = false)]
        public int? FS_InboxOwner { get; set; }

        [DataMember(Order = 90, EmitDefaultValue = false)]
        public string FS_InboxSubject { get; set; }

        [DataMember(Order = 110, EmitDefaultValue = false)]
        public string FS_ReprintServer { get; set; }

        [DataMember(Order = 120, EmitDefaultValue = false)]
        public string FS_ReprintPrinter { get; set; }

        [DataMember(Order = 130, EmitDefaultValue = false)]
        public string BATCHNUMBER { get; set; }

        [DataMember(Order = 140, EmitDefaultValue = false)]
        public string BUSINESSUNIT { get; set; }

        [DataMember(Order = 150, EmitDefaultValue = false)]
        public string DOCNUMBER { get; set; }

        [DataMember(Order = 160, EmitDefaultValue = false)]
        public string DOCUMENTTYPE { get; set; }

        [DataMember(Order = 170, EmitDefaultValue = false)]
        public string INVOICENUMBER { get; set; }

        [DataMember(Order = 180, EmitDefaultValue = false)]
        public DateTime INVOICEDATE { get; set; }

        [DataMember(Order = 190, EmitDefaultValue = false)]
        public string PURCHASEORDER { get; set; }

        [DataMember(Order = 200, EmitDefaultValue = false)]
        public string VENDORNUMBER { get; set; }

        [DataMember(Order = 210, EmitDefaultValue = false)]
        public string VENDORNAME { get; set; }

        [DataMember(Order = 220, EmitDefaultValue = false)]
        public string GROSSAMOUNT { get; set; }

        [DataMember(Order = 230, EmitDefaultValue = false)]
        public string REMITTANCE { get; set; }

        [DataMember(Order = 240, EmitDefaultValue = false)]
        public DateTime GLDATE { get; set; }

        [DataMember(Order = 250, EmitDefaultValue = false)]
        public string OBJECTACCOUNT { get; set; }

        [DataMember(Order = 260, EmitDefaultValue = false)]
        public string SUBLEDGERTYPE { get; set; }

        [DataMember(Order = 270, EmitDefaultValue = false)]
        public string ASSETNUMBER { get; set; }


        [DataMember(Order = 300, EmitDefaultValue = false)]
        public string DOC { get; set; }

        [DataMember(Order = 310, EmitDefaultValue = false)]
        public string ALPH { get; set; }

        [DataMember(Order = 320, EmitDefaultValue = false)]
        public string RP30 { get; set; }

        [DataMember(Order = 330, EmitDefaultValue = false)]
        public string MCU { get; set; }

        [DataMember(Order = 340, EmitDefaultValue = false)]
        public string RP31 { get; set; }

        [DataMember(Order = 350, EmitDefaultValue = false)]
        public string AN8 { get; set; }

        [DataMember(Order = 360, EmitDefaultValue = false)]
        public string RMK { get; set; }

        [DataMember(Order = 370, EmitDefaultValue = false)]
        public string EXA { get; set; }

        [DataMember(Order = 380, EmitDefaultValue = false)]
        public string DCT { get; set; }

        [DataMember(Order = 390, EmitDefaultValue = false)]
        public string AG { get; set; }

        [DataMember(Order = 400, EmitDefaultValue = false)]
        public string AAP { get; set; }

        [DataMember(Order = 410, EmitDefaultValue = false)]
        public string DIVJ { get; set; }

        [DataMember(Order = 420, EmitDefaultValue = false)]
        public string DGJ { get; set; }

        [DataMember(Order = 430, EmitDefaultValue = false)]
        public string DDJ { get; set; }

        [DataMember(Order = 440, EmitDefaultValue = false)]
        public string VINV { get; set; }

        [DataMember(Order = 450, EmitDefaultValue = false)]
        public string PO { get; set; }

        [DataMember(Order = 460, EmitDefaultValue = false)]
        public string SBL { get; set; }

        [DataMember(Order = 470, EmitDefaultValue = false)]
        public string DL01 { get; set; }

        [DataMember(Order = 480, EmitDefaultValue = false)]
        public string OBJ { get; set; }

        [DataMember(Order = 490, EmitDefaultValue = false)]
        public string SUB { get; set; }

        [DataMember(Order = 500, EmitDefaultValue = false)]
        public string KCO { get; set; }

        [DataMember(Order = 510, EmitDefaultValue = false)]
        public string ANI { get; set; }

        [DataMember(Order = 520, EmitDefaultValue = false)]
        public string AA { get; set; }

        [DataMember(Order = 530, EmitDefaultValue = false)]
        public string LNID { get; set; }

        [DataMember(Order = 540, EmitDefaultValue = false)]
        public string EXA2 { get; set; }

        [DataMember(Order = 500, EmitDefaultValue = false)]
        public string JELN { get; set; }

        [DataMember(Order = 510, EmitDefaultValue = false)]
        public string AN8GCA8 { get; set; }

        [DataMember(Order = 520, EmitDefaultValue = false)]
        public string SBLT { get; set; }

        [DataMember(Order = 530, EmitDefaultValue = false)]
        public string ADSC { get; set; }

        [DataMember(Order = 540, EmitDefaultValue = false)]
        public string DDNJ { get; set; }

        [DataMember(Order = 550, EmitDefaultValue = false)]
        public string PST { get; set; }

        [DataMember(Order = 560, EmitDefaultValue = false)]
        public string ASID { get; set; }

        [DataMember(Order = 570, EmitDefaultValue = false)]
        public string GTFILENM { get; set; }

        [DataMember(Order = 620, EmitDefaultValue = false)]
        public int Version { get; set; }
        [DataMember(Order = 630, EmitDefaultValue = false)]
        public byte[] Image { get; set; }
        [DataMember(Order = 640, EmitDefaultValue = false)]
        public string OfflinePath { get; set; }
        [DataMember(Order = 650, EmitDefaultValue = false)]
        public int? OfflineFlag { get; set; }
        [DataMember(Order = 660, EmitDefaultValue = false)]
        public int CatID { get; set; }

        [DataMember(Order = 600, EmitDefaultValue = false)]
        public string pdfFilePath { get; set; }

        [DataMember(Order = 600, EmitDefaultValue = false)]
        public byte[] PDF { get; set; }

        [DataMember(Order = 600, EmitDefaultValue = false)]
        public string txtFilePath { get; set; }

        [DataMember(Order = 610, EmitDefaultValue = false)]
        public bool HasTXT { get; set; }

        [DataMember(Order = 620, EmitDefaultValue = false)]
        public bool HasPDF { get; set; }

        [DataMember(Order = 630, EmitDefaultValue = false)]
        public ValidationError ValidationError { get; set; }

        [DataMember(Order = 640, EmitDefaultValue = false)]
        public bool IsInsertedDocHeader { get; set; }

        [DataMember(Order = 650, EmitDefaultValue = false)]
        public bool IsInsertedAPInvoice { get; set; }

        [DataMember(Order = 660, EmitDefaultValue = false)]
        public bool IsArchivedTXT { get; set; }

        [DataMember(Order = 670, EmitDefaultValue = false)]
        public bool IsArchivedPDF { get; set; }

        [DataMember(Order = 680, EmitDefaultValue = false)]
        public string DontKnow { get; set; }
    }
}