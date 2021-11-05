using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using VelocityApp.Common.Utilities;


namespace VelocityApp.Common.Types
{
    [CollectionDataContract(Namespace = "")]

    public class ProcessResponses : List<ProcessResponse>
    {
    }

    [DataContract(Namespace = "")]
    public class ProcessResponse
    {
        public ProcessResponse()
        {
            StartDate = DateTime.Now;
            TransferDate = DateTime.Now;
            ServerName = System.Environment.MachineName;
            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Environment = ConfigurationManager.AppSettings["Environment"];
            ConnectionStrings = new ConnectionStrings();
            ProcessResponses = new ProcessResponses();
            Messages = new Messages();
            SerializedObjects = new SerializedObjects();
            Errors = new Errors();
            ProcessSteps = new ProcessSteps();
            RowsAffected = 0;
            TransferStatus = "";
            TransferMessage = "";
        }

        public ProcessResponse(string name)
        {
            StartDate = DateTime.Now;
            TransferDate = DateTime.Now;
            ProcessName = name;
            ServerName = System.Environment.MachineName;
            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Environment = ConfigurationManager.AppSettings["Environment"];
            ConnectionStrings = new ConnectionStrings();
            ProcessResponses = new ProcessResponses();
            Messages = new Messages();
            SerializedObjects = new SerializedObjects();
            Errors = new Errors();
            ProcessSteps = new ProcessSteps();
            RowsAffected = 0;
            TransferStatus = "";
            TransferMessage = "";
        }

        [DataMember(Order = 10)]
        public string ProcessName { get; set; }
        [DataMember(Order = 12)]
        public string TransferStatus { get; set; }
        [DataMember(Order = 15)]
        public DateTime TransferDate { get; set; }

        [DataMember(Order = 15)]
        public string TransferMessage { get; set; }
        
        [DataMember(Order = 20)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 22)]
        public DateTime EndDate { get; set; }
        [DataMember(Order = 24)]
        public decimal ElapsedSeconds { get; set; }
        [DataMember(Order = 26)]
        public Errors Errors { get; set; }
        [DataMember(Order = 28)]
        public Messages Messages { get; set; }
        [DataMember(Order = 30)]
        public ConnectionStrings ConnectionStrings { get; set; }

        [DataMember(Order = 50)]
        public int RowsAffected { get; set; }
        [DataMember(Order = 60)]
        public DateTime LookbackDate { get; set; }

        [XmlIgnore]
        public SerializedObjects SerializedObjects { get; set; }

        [DataMember(Order = 120, EmitDefaultValue = false)]
        public string Environment { get; set; }
        [DataMember(Order = 122, EmitDefaultValue = false)]
        public string AssemblyVersion { get; set; }
        [DataMember(Order = 123, EmitDefaultValue = false)]
        public string ServerName { get; set; }
        [DataMember(Order = 124, EmitDefaultValue = false)]
        public ServiceContext ServiceContext { get; set; }

        [DataMember(Order = 150)]
        public ProcessSteps ProcessSteps { get; set; }

        [DataMember(Order = 200)]
        public ProcessResponses ProcessResponses { get; set; }

        public void Close()
        {
            foreach (var childProcessResponse in ProcessResponses)
            {
                if (childProcessResponse.HasErrors())
                {
                    foreach (var error in childProcessResponse.Errors)
                    {
                        if (Errors == null) Errors = new Errors();
                        Errors.Add(error);
                    }
                }
                EndDate = DateTime.Now;
            }
        }

        public string Serialize()
        {
            try
            {
                var root = new XElement("root", XElement.Load(new StringReader(XmlSerializationUtility.SerializeObject(Encoding.UTF8, this))));
                foreach (var serializedObject in SerializedObjects)
                {
                    var element = new XElement(serializedObject.Name, XElement.Load(new StringReader(serializedObject.Value)));
                    root.Add(element);
                }
                var value = root.ToString();
                return value;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public void AddStep(ProcessStep step)
        {
            ProcessSteps.Add(step);
            //if (step.Errors.Any())
            //{
            //    Errors.AddRange(step.Errors);
            //}
        }

        public void Done()
        {
            EndDate = DateTime.Now;
            ElapsedSeconds = (decimal)(EndDate - StartDate).TotalSeconds;
        }

        public bool HasErrors()
        {
            return ResponseHasErrors(this);
        }


        public bool ResponseHasErrors(ProcessResponse response)
        {
            if (response.Errors.Any() || response.ProcessSteps.Any(step => step.Errors.Any()))
            {
                return true;
            }
            return false;
        }
    }
}
