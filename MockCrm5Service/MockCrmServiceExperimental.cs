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
			
			// TODO: previously I thought that we had to use a DataContracSerializer for this.
			// However the old serializer appears to work also. Should investigate this.
			// Serializer.Serialize( type, data, fs );
			ContractSerializer.Serialize( type, data, fs, new Microsoft.Xrm.Sdk.KnownTypesResolver() );
			fs.Close();
		}

		public void ReadFromDisk( string in_filename ) {
			Type type = typeof( SerializableDictionary<string, EntityCollection> );
			// not sure if this behavior is good - we read file if it exists, otherwise we 
			// don't worry about it.
			if( File.Exists( in_filename ) ) {
				FileStream fs = new FileStream( in_filename, FileMode.Open, FileAccess.Read );
				try {
					Microsoft.Xrm.Sdk.KnownTypesResolver resolver = new Microsoft.Xrm.Sdk.KnownTypesResolver();
					data = ( SerializableDictionary<string, EntityCollection> )ContractSerializer.Deserialize( type, fs, resolver );
				}
				finally { fs.Close(); }
				
			}
		}

	} // class
} // namespace
