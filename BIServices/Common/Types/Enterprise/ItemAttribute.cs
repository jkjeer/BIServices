using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [DataContract(Namespace = "")]
    public class ItemAttribute
    {
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string Value { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public string Desc1 { get; set; }
        [DataMember(Order = 40, EmitDefaultValue = false)]
        public string Desc2 { get; set; }
        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string Desc3 { get; set; }
    }

    [CollectionDataContract(Namespace = "")]
    public class ItemAttributes : KeyedCollection<string, ItemAttribute>
    {


        protected override string GetKeyForItem(ItemAttribute attribute)
        {
            return attribute.Name;
        }

        public Decimal GetIntAttributeValue(String attributeName)
        {
            if (this.Contains(attributeName))
            {
                return Convert.ToInt32(this[attributeName].Value.Trim());
            }
            return 0;
        }

        public Decimal GetDecimalAttributeValue(String attributeName)
        {
            if (this.Contains(attributeName))
            {
                return Convert.ToDecimal(this[attributeName].Value.Trim());
            }
            return 0;
        }

        public String GetStringAttributeValue(String attributeName, String defaultValue = null)
        {
            if (this.Contains(attributeName))
            {
                return this[attributeName].Value.Trim();
            }
            return defaultValue ?? null;
        }

        public String GetStringAttributeDesc1(String attributeName, String defaultValue = null)
        {
            if (this.Contains(attributeName))
            {
                return this[attributeName].Desc1.Trim();
            }
            return defaultValue ?? null;
        }

        public String GetStringAttributeDesc2(String attributeName)
        {
            if (this.Contains(attributeName))
            {
                return this[attributeName].Desc2.Trim();
            }
            return null;
        }

        public String GetStringAttributeDesc3(String attributeName)
        {
            if (this.Contains(attributeName))
            {
                return this[attributeName].Desc3.Trim();
            }
            return String.Empty;
        }

    }

}