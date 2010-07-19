﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using System.IO;
using Djn.Framework;

namespace Djn.Testing
{
	// experimental extensions
	public partial class MockCrmService
	{
		// TODO: should we be using DynamicEntityCollection?
		private SerializableDictionary<string, BusinessEntityList> data =
		new SerializableDictionary<string, BusinessEntityList>();
	
		public void PersistToDisk( string in_filename ) {
			Type type = typeof( SerializableDictionary<string, BusinessEntityList> );
			FileStream fs = new FileStream( in_filename, FileMode.Create, FileAccess.Write );
			Serializer.Serialize( type, data, fs );
			fs.Close();
		}

		public void ReadFromDisk( string in_filename ) {
			Type type = typeof( SerializableDictionary<string, BusinessEntityList> );
			FileStream fs = new FileStream( in_filename, FileMode.Open, FileAccess.Read );
			data = ( SerializableDictionary<string, BusinessEntityList> )Serializer.Deserialize( type, fs );
			fs.Close();
		}

	} // class
} // namespace
