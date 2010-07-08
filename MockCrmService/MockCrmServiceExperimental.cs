using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using System.IO;

namespace Djn.Testing
{
	// experimental extensions
	public partial class MockCrmService
	{
		private SerializableDictionary<string, BusinessEntityList> data =
		new SerializableDictionary<string, BusinessEntityList>();

		public static void Serialize( Type in_type, Object in_instance, Stream out_stream ) {
			System.Xml.Serialization.XmlSerializer xser = new System.Xml.Serialization.XmlSerializer( in_type );
			xser.Serialize( out_stream, in_instance );
		}

		public static object Deserialize( Type in_type, Stream in_stream ) {
			System.Xml.Serialization.XmlSerializer xser = new System.Xml.Serialization.XmlSerializer( in_type );
			return xser.Deserialize( in_stream );
		}

		public void PersistToDisk() {
			string filename = "database.xml";
			Type type = typeof( SerializableDictionary<string, BusinessEntityList> );
			FileStream fs = new FileStream( filename, FileMode.Create, FileAccess.Write );
			Serialize( type, data, fs );
			fs.Close();
		}

		public void ReadFromDisk() {
			string filename = "database.xml";
			Type type = typeof( SerializableDictionary<string, BusinessEntityList> );
			FileStream fs = new FileStream( filename, FileMode.Open, FileAccess.Read );
			data = ( SerializableDictionary<string, BusinessEntityList> )Deserialize( type, fs );
			fs.Close();
		}

	} // class
} // namespace
