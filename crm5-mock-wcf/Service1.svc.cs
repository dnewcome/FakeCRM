using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Xrm.Sdk;
using Djn.Testing;

namespace crm5_mock_wcf
{
	public class Service1 : IOrganizationService // IService1
	{
		// made this static for testing the service. Probably not a good idea overall.
		MockCrmService m_service = new MockCrmService( "db.xml", true);
		
		public void Associate( string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities ) {
			throw new NotImplementedException();
		}

		public Guid Create( Entity entity ) {
			return m_service.Create( entity );
		}

		public void Delete( string entityName, Guid id ) {
			m_service.Delete( entityName, id );
		}

		public void Disassociate( string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities ) {
			throw new NotImplementedException();
		}

		public OrganizationResponse Execute( OrganizationRequest request ) {
			return m_service.Execute( request );
		}

		public Entity Retrieve( string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columnSet ) {
			return m_service.Retrieve( entityName, id, columnSet );
		}

		public EntityCollection RetrieveMultiple( Microsoft.Xrm.Sdk.Query.QueryBase query ) {
			return m_service.RetrieveMultiple( query );
		}

		public void Update( Entity entity ) {
			m_service.Update( entity );
		}
		/*
		public string GetData( int value ) {
			return string.Format( "You entered: {0}", value );
		}

		public CompositeType GetDataUsingDataContract( CompositeType composite ) {
			if( composite == null ) {
				throw new ArgumentNullException( "composite" );
			}
			if( composite.BoolValue ) {
				composite.StringValue += "Suffix";
			}
			return composite;
		}
		*/
	} // class
} // namespace
