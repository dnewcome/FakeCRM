using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.SdkTypeProxy;

using Djn.Testing;

[WebService( Namespace = "http://schemas.microsoft.com/crm/2007/WebServices" )]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService//, ICrmService
{
	static Service() {
		string file = System.Configuration.ConfigurationSettings.AppSettings.Get( "datafile" );
		if( file != null ) {
			m_service = new MockCrmService( file, true );
		}
		else {
			m_service = new MockCrmService();
		}
	}
	// made this static for testing the service. Probably not a good idea overall.
	static MockCrmService m_service;
	[WebMethod(Description="http://schemas.microsoft.com/crm/2007/WebServices/Create")]
	public Guid Create( BusinessEntity entity ) {
		return m_service.Create( entity );
	}
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Delete")]
	public void Delete( string entityName, Guid id ) {
		m_service.Delete( entityName, id );
	}

	/**
	* Execute has been problematic. The first problem was that we had to rename the 
	* argument from 'request' to 'Request' in order to avoid getting a null value.
	* Also the data type needed to be changed to 'Request' and not 'object'. Note that
	* this is different than ICrmService so we can no longer implement that interface.
	* We need to return type 'Response' in order to avoid XML parsing errors. Serialization
	* won't work without this. However, even still, I'm getting null return values 
	* back on the client side. There are results here in the webservice and they are getting
	* parsed, but somehow the data is still getting lost.
	* I've switched back to using the service in-proc instead of using this service for now.
	*/
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Execute" )]
	public Response Execute( Request Request ) {
		Response res = (Response)m_service.Execute( Request );
		// object res = m_service.Execute( Request );
		return res;
	}
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Retrieve" )]
	public BusinessEntity Retrieve( string entityName, Guid id, ColumnSetBase columnSet ) {
		return m_service.Retrieve( entityName, id, columnSet );
	}
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/RetrieveMultiple" )]
	public BusinessEntityCollection RetrieveMultiple( QueryBase query ) {
		return m_service.RetrieveMultiple( query );
	}
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Update" )]
	public void Update( BusinessEntity entity ) {
		m_service.Update( entity );
	}
	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Fetch" )]
	public string Fetch( string fetchXML ) {
		throw new Exception( "Fetch not implemented" );
	} 
}