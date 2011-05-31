using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.IO;
using Djn.Framework;

namespace Djn.Testing
{
	// experimental extensions
	public partial class MockCrmService
	{
		// TODO: should we be using DynamicEntityCollection?
		private SerializableDictionary<string, EntityCollection> data =
		new SerializableDictionary<string, EntityCollection>();
	
		public void PersistToDisk( string in_filename ) {
			Type type = typeof( SerializableDictionary<string, EntityCollection> );
			FileStream fs = new FileStream( in_filename, FileMode.Create, FileAccess.Write );
			Serializer.Serialize( type, data, fs );
			fs.Close();
		}

		public void ReadFromDisk( string in_filename ) {
			Type type = typeof( SerializableDictionary<string, EntityCollection> );
			// not sure if this behavior is good - we read file if it exists, otherwise we 
			// don't worry about it.
			if( File.Exists( in_filename ) ) {
				FileStream fs = new FileStream( in_filename, FileMode.Open, FileAccess.Read );
				data = ( SerializableDictionary<string, EntityCollection> )Serializer.Deserialize( type, fs );
				fs.Close();
			}
		}

	} // class
} // namespace
