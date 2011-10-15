using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.SdkTypeProxy;
using Djn.Testing;

namespace crm5_mock_wcf
{
	public class Service1 : ICrmService // IService1
	{
		public void Dispose() { }

		// made this static for testing the service. Probably not a good idea overall.
		MockCrmService m_service = new MockCrmService( "db.xml", true);

		public Guid Create( BusinessEntity entity ) {
			return m_service.Create( entity );
		}

		public void Delete( string entityName, Guid id ) {
			m_service.Delete( entityName, id );
		}
		
		public object Execute( object request ) {
			return m_service.Execute( request );
		}

		public BusinessEntity Retrieve( string entityName, Guid id, ColumnSetBase columnSet ) {
			return m_service.Retrieve( entityName, id, columnSet );
		}

		public BusinessEntityCollection RetrieveMultiple( QueryBase query ) {
			return m_service.RetrieveMultiple( query );
		}

		public void Update( BusinessEntity entity ) {
			m_service.Update( entity );
		}
		public string Fetch( string fetchXML ) {
			throw new Exception( "Fetch not implemented" );
		}
	} // class
} // namespace
