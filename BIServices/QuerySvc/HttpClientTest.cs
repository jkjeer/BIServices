using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BIServices.Common.Types.Enterprise;
using Newtonsoft.Json;

namespace BIServices.QuerySvc
{
    public static class HttpClientTest
    {

        public static async Task<DataTable> TestMethod()
        {

            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            var client = new HttpClient(httpClientHandler);
            string result =
                await client.GetStringAsync(
                    "http://svcpy/CGServices/CGServices.RackTagRestService.svc/GetRackTagsDataTable?Plant=NFCG&RackId=7060604&GroupStrategy=WorkOrderInterleaved");

            var dt = JsonConvert.DeserializeObject<DataTable>(result);

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(result);
            //var srXMLtext = new System.IO.StringReader(doc.InnerXml);

            //var dt = new DataTable();
            //dt.ReadXml(srXMLtext);

            return dt;
        }

        public static async Task<DataTable> TestMethod1()
        {

            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            var client = new HttpClient(httpClientHandler);
            string result =
                await client.GetStringAsync(
                    "http://cgd2intsvcqa/LGServices/LGServices.ReferenceJsonService.svc/GetCustomers?CustomerName=PELLA");

            var cust = JsonConvert.DeserializeObject<Customers>(result);

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(result);
            //var srXMLtext = new System.IO.StringReader(doc.InnerXml);

            var dt = new DataTable();
            //dt.ReadXml(srXMLtext);

            return dt;
        }

        public static async Task<DataTable> TestMethod2()
        {

            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            var client = new HttpClient(httpClientHandler);
            string result =
                await client.GetStringAsync(
                    $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.RackTagRestService.svc/GetRackTagsDataTable?Plant=NFCG&RackId=6958547&GroupStrategy=WorkOrderInterleaved");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            var srXMLtext = new System.IO.StringReader(doc.InnerXml);

            var dt = new DataTable();
            dt.ReadXml(srXMLtext);

            return dt;
        }

        public static async Task<DataTable> TestMethod3()
        {

            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            var client = new HttpClient(httpClientHandler);
            string result =
                await client.GetStringAsync(
                    $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.RackTagRestService.svc/GetRackTagsDataTable?Plant=NFCG&RackId=6958547&GroupStrategy=WorkOrderInterleaved");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            var srXMLtext = new System.IO.StringReader(doc.InnerXml);

            var dt = new DataTable();

            dt.ReadXml(srXMLtext);

            var json = JsonConvert.SerializeObject(dt);
            DataTable dt1 = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

            return dt1;
        }

        [CollectionDataContract(Namespace = "")]
        public class Customers : List<Customer>
        {
        }

        [DataContract(Namespace = "")]
        public class Customer
        {
            [DataMember(Order = 10, EmitDefaultValue = false)]
            public decimal CustomerNum { get; set; }
            [DataMember(Order = 11, EmitDefaultValue = false)]
            public string CustomerType { get; set; }
            [DataMember(Order = 12, EmitDefaultValue = false)]
            public string LegacyCustomerNum { get; set; }
            [DataMember(Order = 15, EmitDefaultValue = false)]
            public string CustomerName { get; set; }

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsPlant { get; set; }
            [DataMember(Order = 22, EmitDefaultValue = false)]
            public string PlantNum { get; set; }
            [DataMember(Order = 23, EmitDefaultValue = false)]
            public string PlantCode { get; set; }
            [DataMember(Order = 24, EmitDefaultValue = false)]
            public string ShortCompanyNum { get; set; }
            [DataMember(Order = 26, EmitDefaultValue = false)]
            public string CompanyNum { get; set; }

            [DataMember(Order = 30, EmitDefaultValue = false)]
            public int CTCustomerNum { get; set; }

            [DataMember(Order = 40, EmitDefaultValue = false)]
            public string TaxRateCode { get; set; }
            [DataMember(Order = 42, EmitDefaultValue = false)]
            public string PaymentTermsCode { get; set; }
            [DataMember(Order = 44, EmitDefaultValue = false)]
            public string PaymentTermsDesc { get; set; }

            [DataMember(Order = 100, EmitDefaultValue = false)]
            public Address Address { get; set; }
            [DataMember(Order = 200, EmitDefaultValue = false)]
            public List<PhoneContact> PhoneContacts { get; set; }
            [DataMember(Order = 200, EmitDefaultValue = false)]
            public List<MailContact> MailContacts { get; set; }
        }


        [DataContract(Namespace = "")]
        public class Address
        {
            [DataMember(Order = 10)]
            public decimal AddressNum { get; set; }

            [DataMember(Order = 12)]
            public string Name { get; set; }

            [DataMember(Order = 14)]
            public string Address1 { get; set; }

            [DataMember(Order = 16)]
            public string Address2 { get; set; }

            [DataMember(Order = 18)]
            public string Address3 { get; set; }

            [DataMember(Order = 20)]
            public string Address4 { get; set; }

            [DataMember(Order = 22)]
            public string Address5 { get; set; }

            [DataMember(Order = 24)]
            public string City { get; set; }

            [DataMember(Order = 26)]
            public string State { get; set; }

            [DataMember(Order = 28)]
            public string PostalCode { get; set; }

            [DataMember(Order = 30)]
            public string Country { get; set; }
        }

        [CollectionDataContract(Namespace = "")]
        public class PhoneContacts : List<PhoneContact>
        {
        }

        [DataContract(Namespace = "")]
        public class PhoneContact
        {
            [DataMember(Order = 10)]
            public string PhoneType { get; set; }
            [DataMember(Order = 12)]
            public string PhoneTypeDesc { get; set; }
            [DataMember(Order = 14)]
            public string Prefix { get; set; }
            [DataMember(Order = 16)]
            public string Number { get; set; }
            [DataMember(Order = 18)]
            public decimal SequenceNum { get; set; }
            [DataMember(Order = 20)]
            public DateTime UpdateTimestamp { get; set; }
        }

        [CollectionDataContract(Namespace = "")]
        public class MailContacts : List<MailContact>
        {
        }

        [DataContract(Namespace = "")]
        public class MailContact
        {
            [DataMember(Order = 10)]
            public string MailType { get; set; }
            [DataMember(Order = 12)]
            public string MailTypeDesc { get; set; }
            [DataMember(Order = 14)]
            public string Address { get; set; }
            [DataMember(Order = 18)]
            public decimal SequenceNum { get; set; }
            [DataMember(Order = 20)]
            public DateTime UpdateTimestamp { get; set; }
        }
    }
}
