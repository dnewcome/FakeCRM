using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

[XmlRoot( "dictionary" )]
public class SerializableDictionary<TKey, TValue>
	: Dictionary<TKey, TValue>, IXmlSerializable
{
	#region IXmlSerializable Members
	public System.Xml.Schema.XmlSchema GetSchema() {
		return null;
	}

	public void ReadXml( System.Xml.XmlReader reader ) {
		XmlSerializer keySerializer = new XmlSerializer( typeof( TKey ) );
		// XmlSerializer valueSerializer = new XmlSerializer( typeof( TValue ) );
		

		bool wasEmpty = reader.IsEmptyElement;
		reader.Read();

		if( wasEmpty )
			return;

		while( reader.NodeType != System.Xml.XmlNodeType.EndElement ) {
			reader.ReadStartElement( "item" );

			reader.ReadStartElement( "key" );
			TKey key = ( TKey )keySerializer.Deserialize( reader );
			reader.ReadEndElement();

			reader.ReadStartElement( "value" );
			System.Runtime.Serialization.DataContractSerializer serializer =
			new System.Runtime.Serialization.DataContractSerializer( typeof( TValue ), null, int.MaxValue, false, false, null, new Microsoft.Xrm.Sdk.KnownTypesResolver() );
			TValue value = (TValue)serializer.ReadObject( reader );


			reader.ReadEndElement();

			this.Add( key, value );

			reader.ReadEndElement();
			reader.MoveToContent();
		}
		reader.ReadEndElement();
	}

	public void WriteXml( System.Xml.XmlWriter writer ) {
		XmlSerializer keySerializer = new XmlSerializer( typeof( TKey ) );
		// XmlSerializer valueSerializer = new XmlSerializer( typeof( TValue ) );

		DataContractSerializer serializer =
			new DataContractSerializer( typeof( TValue ), null, int.MaxValue, false, false, null, new Microsoft.Xrm.Sdk.KnownTypesResolver() );
		
		

		foreach( TKey key in this.Keys ) {
			writer.WriteStartElement( "item" );

			writer.WriteStartElement( "key" );
			keySerializer.Serialize( writer, key );
			writer.WriteEndElement();

			writer.WriteStartElement( "value" );
			TValue value = this[ key ];
			// valueSerializer.Serialize( writer, value );
			serializer.WriteObject( writer, value );
			
			




			writer.WriteEndElement();

			writer.WriteEndElement();
		}
	}
	#endregion
}