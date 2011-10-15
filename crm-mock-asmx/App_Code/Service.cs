using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Djn.Testing;

[WebService( Namespace = "http://schemas.microsoft.com/crm/2007/WebServices" )]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService, ICrmService
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

	[WebMethod( Description = "http://schemas.microsoft.com/crm/2007/WebServices/Execute" )]
	public object Execute( object request ) {
		return m_service.Execute( request );
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