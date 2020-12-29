using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Assets.OsmGenerator.Scripts.Extensions
{
    public static class XmlExtensions
    {
        public static T GetAttribute<T>(this XmlNode node, string attributeName)
        {
            try
            {
                var attributes = node.Attributes;
                var value = attributes[attributeName].Value;

                return (T) Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (NullReferenceException e)
            {
                throw new Exception("This attribute does not exist", e);
            }
        }
    }
}
