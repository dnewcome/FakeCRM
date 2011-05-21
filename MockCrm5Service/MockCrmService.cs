using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

namespace Djn.Testing
{
	public partial class MockCrmService : IOrganizationService
	{
		// configuration for whether we save changes to disk
		private bool m_persist = false;
		private string m_filename;

		public MockCrmService( string in_filename, bool in_persist )
			: this( in_filename ) {
			m_persist = in_persist;
		}

		public MockCrmService( string in_filename ) {
			m_filename = in_filename;
			ReadFromDisk( in_filename );
		}

		public MockCrmService() { }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }       

		public Guid Create( Entity entity ) {
			Guid id = Guid.NewGuid();
			
			string name = entity.GetType().Name;
			if( data.ContainsKey( name ) == false ) {
				data.Add( name, new EntityCollection() );
			}

            if( name == "Entity" ) {
				Entity de = ( Entity )entity;
				// We set name here to support DynamicEntity
                name = de.LogicalName;
				de[name + "id"] = id;
			}
			else {
				entity.GetType().GetProperty( name + "id" ).SetValue( entity, id, null );
			}


			if( !data.ContainsKey( name ) ) {
				data[ name ] = new EntityCollection();
			}
			data[ name ].Entities.Add( entity );

			if( m_persist ) {
				PersistToDisk( m_filename );
			}

			return id;
		}

		public void Delete( string entityName, Guid id ) {
			foreach( Entity entity in data[ entityName ].Entities ) {
				Guid key = ( Guid )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
				Guid keyID = key;
				if( keyID == id ) {
					data[ entityName ].Entities.Remove( entity );
					break;
				}
			}

			if( m_persist ) {
				PersistToDisk( m_filename );
			}
		}

		public OrganizationResponse Execute( OrganizationRequest request ) {
			if( request.GetType().Name == "RetrieveMultipleRequest" ) {
				RetrieveMultipleResponse response = new RetrieveMultipleResponse();
			    EntityCollection result = RetrieveMultiple( ( ( RetrieveMultipleRequest )request ).Query );
                foreach( Entity entity in result.Entities ) {
                    response.EntityCollection.Entities.Add(entity);
                }
				return response;
			}
			else if(request.GetType().Name == "RetrieveRequest" ) {
				RetrieveResponse response = new RetrieveResponse();
				RetrieveRequest retrieveRequest = ( RetrieveRequest )request;
				EntityReference target = retrieveRequest.Target;
				
                
                if( target.GetType().Name == "TargetRetrieveDynamic" ) {
				/*
                    TargetRetrieveDynamic trd = ( TargetRetrieveDynamic )target;
					response.Entity = Retrieve( trd.EntityName, trd.EntityId, retrieveRequest.ColumnSet );
				*/
                }
                

                // TODO: entity is readonly .. will have to set this with reflection
                // response.Entity = Retrieve( target.LogicalName, target.Id, retrieveRequest.ColumnSet );

				else {
						// request sent using a specific strongly-typed business entity
						// rather than a DynamicEntity
						throw new NotImplementedException();
				}
				return response;
			}
			else {
				throw new NotImplementedException();
			}
		}

		public string Fetch( string fetchXml ) {
			throw new NotImplementedException();
		}

		// Note: we ignore columnset
		public Entity Retrieve( string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columnSet ) {
			foreach( Entity entity in data[ entityName ].Entities ) {
				// TODO: factor this check out to method	
				Guid key;
				if( entity.GetType().Name == "Entity" ) {
					Entity de = ( Entity )entity;
					key = ( Guid )de[ entityName + "id" ];
				}
				else {
					// TODO: we guess at id field name - should look for Key field type instead
					key = ( Guid )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
				}
				Guid keyID = key;
				if( keyID == id ) {
					return entity;
				}
			}
			return null;
		}

		public EntityCollection RetrieveMultiple( Microsoft.Xrm.Sdk.Query.QueryBase query ) {
			QueryExpression queryExpression = ( QueryExpression )query;
			EntityCollection retval = new EntityCollection();
			if( data.ContainsKey( queryExpression.EntityName ) ) {
				foreach( Entity entity in data[ queryExpression.EntityName ].Entities ) {
					if( true == EvaluateFilters( queryExpression.Criteria, entity ) && true == EvaluateLinks( queryExpression.LinkEntities, entity ) ) {
						retval.Entities.Add( entity );
					}
				}
			}
			return retval;
		}
        
		public void Update( Entity entity ) {
            // Only support DynamicEntities for now
			Entity de = ( Entity )entity;
			foreach( Entity previousEntity in data[ de.LogicalName ].Entities ) { 
				// TODO: we assume id field is "entitynameid"
				if( previousEntity is Entity
					&& ( ( Guid )( ( Entity )previousEntity )[ de.LogicalName + "id" ] )
					== ( ( Guid )de[ de.LogicalName + "id" ] )
				) {
					foreach( KeyValuePair<string, object> prop in de.Attributes ) {
						( ( Entity )previousEntity ).Attributes.Remove( prop.Key );
						( ( Entity )previousEntity ).Attributes.Add( prop );
					}
					break;
				}
			}
			
			if( m_persist ) {
				PersistToDisk( m_filename );
			}
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		private bool EvaluateLinks( DataCollection<LinkEntity> in_links, Entity in_entity ) {
			// TODO: can we restructure things to avoid this check?
			if( in_links.Count == 0 ) return true;

			foreach( LinkEntity link in in_links ) {
				foreach( Entity entity in data[ link.LinkToEntityName ].Entities ) {

					// TODO: we do this check and value retrieval in both filters and links handing
					// TODO: we assume that both ends of the link are either DynamicEntity or not
					object linkFromFieldValue = null;
					object linkToFieldValue = null;
					try { // another hack, since some entities will have null on the field we are linking
						if( entity.GetType().Name == "Entity" ) {
							// TODO: we only support StringProperty here
							linkFromFieldValue = ( ( Entity )in_entity )[ link.LinkFromAttributeName ];
							linkToFieldValue = ( ( Entity )entity )[ link.LinkToAttributeName ];
						}
						else {
							linkFromFieldValue = in_entity.GetType().GetProperty( link.LinkFromAttributeName ).GetValue( in_entity, null );
							linkToFieldValue = entity.GetType().GetProperty( link.LinkToAttributeName ).GetValue( entity, null );
						}
					}
					catch { }
					// TODO: are we passing the correct entity here? - do we need access to both entities to eval 
					// these criteria? Somehow I don't think so, since otherwise why would we need LinkEntity
					// TODO: we only support inner join. CRM supports left outer and natural joins.
					// TODO: how do we handle other comparisons that aren't Key == Lookup?
					try { // Huge hack since we try to get Value of field that may be null
						if( ( ( Guid )linkFromFieldValue  == ( ( EntityReference )linkToFieldValue ).Id )
							&& EvaluateFilters( link.LinkCriteria, entity ) == true ) {
							// We short circuit - as long as one linked entity meets the criteria, we'll return true
							// TODO: this is a bug - we don't eval all links.
							return true;
						}
					}
					catch { /// HACK try it the other way
						try {
							if( ( ( EntityReference )linkFromFieldValue ).Id == ( Guid )linkToFieldValue
								&& EvaluateFilters( link.LinkCriteria, entity ) == true ) {
								// We short circuit - as long as one linked entity meets the criteria, we'll return true
								// TODO: this is a bug - we don't eval all links.
								return true;
							}
						}
						catch { }
					}
				}
				// TODO: eval nested links
				// EvaluateLinks( link.LinkEntities );
			}
			return false;
		}

		private bool EvaluateFilters( FilterExpression in_filter, Entity in_entity ) {
			List<bool> results = new List<bool>();
			
			foreach( FilterExpression exp in in_filter.Filters ) {
				results.Add( EvaluateFilters( exp, in_entity ) );
			}
			foreach( ConditionExpression exp in in_filter.Conditions ) {
				bool result = true;
				object fieldValue;

				// TODO: extract this fieldvalue retrieval into a method 
				if( in_entity.GetType().Name == "Entity" ) {
					Entity entity = ( Entity )in_entity;
					// TODO: we only support StringProperty here
					fieldValue = entity[ exp.AttributeName ];
				}
				else {
					fieldValue = in_entity.GetType().GetProperty( exp.AttributeName ).GetValue( in_entity, null );
				}

				if( exp.Operator == ConditionOperator.Equal ) {
					foreach( object val in exp.Values ) {
						// TODO: handle other data types
						if( fieldValue is Guid ) {
							if( !val.Equals( ( ( Guid )fieldValue ) ) ) {
								result = false;
								break;
							}
						}
						else {
							if( !val.Equals( fieldValue ) ) {
								result = false;
								break;
							}
						}
					}
				}
				results.Add( result );
			}

			bool retval;
			if( in_filter.FilterOperator == LogicalOperator.And ) {
				retval = true;
			}
			else {
				retval = false;
			}
			foreach( bool result in results ) {
				if( in_filter.FilterOperator == LogicalOperator.And ) {
					retval = retval && result;
				}
				else {
					retval = retval || result;	
				}
			}
			return retval;
		}

	}
}
