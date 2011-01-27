using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.SdkTypeProxy;
using Microsoft.Crm.Sdk.Query;

namespace Djn.Testing
{
	public partial class MockCrmService : ICrmService
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

		public Guid Create( BusinessEntity entity ) {
			Guid id = Guid.NewGuid();
			
			string name = entity.GetType().Name;
			if( data.ContainsKey( name ) == false ) {
				data.Add( name, new BusinessEntityCollection() );
			}

			if( name == "DynamicEntity" ) {
				DynamicEntity de = ( DynamicEntity )entity;
				name = de.Name;
				de.Properties.Add( new KeyProperty( de.Name + "id", new Key( id ) ) );
			}
			else {
				entity.GetType().GetProperty( name + "id" ).SetValue( entity, new Key( id ), null );
			}


			if( !data.ContainsKey( name ) ) {
				data[ name ] = new BusinessEntityCollection();
			}
			data[ name ].BusinessEntities.Add( entity );

			if( m_persist ) {
				PersistToDisk( m_filename );
			}

			return id;
		}

		public void Delete( string entityName, Guid id ) {
			foreach( BusinessEntity entity in data[ entityName ].BusinessEntities ) {
				Key key = ( Key )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
				Guid keyID = key.Value;
				if( keyID == id ) {
					data[ entityName ].BusinessEntities.Remove( entity );
					break;
				}
			}

			if( m_persist ) {
				PersistToDisk( m_filename );
			}
		}

		public object Execute( object request ) {
			if( request.GetType().Name == "RetrieveMultipleRequest" ) {
				RetrieveMultipleResponse response = new RetrieveMultipleResponse();
				response.BusinessEntityCollection = RetrieveMultiple( ( ( RetrieveMultipleRequest )request ).Query );
				return response;
			}
			else if(request.GetType().Name == "RetrieveRequest" ) {
				RetrieveResponse response = new RetrieveResponse();
				RetrieveRequest retrieveRequest = ( RetrieveRequest )request;
				TargetRetrieve target = retrieveRequest.Target;
				if( target.GetType().Name == "TargetRetrieveDynamic" ) {
					TargetRetrieveDynamic trd = ( TargetRetrieveDynamic )target;
					response.BusinessEntity = Retrieve( trd.EntityName, trd.EntityId, retrieveRequest.ColumnSet );
				}
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
		public BusinessEntity Retrieve( string entityName, Guid id, Microsoft.Crm.Sdk.Query.ColumnSetBase columnSet ) {
			foreach( BusinessEntity entity in data[ entityName ].BusinessEntities ) {
				// TODO: factor this check out to method	
				Key key;
				if( entity.GetType().Name == "DynamicEntity" ) {
					DynamicEntity de = ( DynamicEntity )entity;
					key = ( Key )de.Properties[ entityName + "id" ];
				}
				else {
					// TODO: we guess at id field name - should look for Key field type instead
					key = ( Key )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
				}
				Guid keyID = key.Value;
				if( keyID == id ) {
					return entity;
				}
			}
			return null;
		}

		public BusinessEntityCollection RetrieveMultiple( Microsoft.Crm.Sdk.Query.QueryBase query ) {
			QueryExpression queryExpression = ( QueryExpression )query;
			BusinessEntityCollection retval = new BusinessEntityCollection();
			if( data.ContainsKey( query.EntityName ) ) {
				foreach( BusinessEntity entity in data[ query.EntityName ].BusinessEntities ) {
					if( true == EvaluateFilters( queryExpression.Criteria, entity ) && true == EvaluateLinks( queryExpression.LinkEntities, entity ) ) {
						retval.BusinessEntities.Add( entity );
					}
				}
			}
			return retval;
		}

		public void Update( BusinessEntity entity ) {
			// Only support DynamicEntities for now
			DynamicEntity de = ( DynamicEntity )entity;
			foreach( BusinessEntity previousEntity in data[ de.Name ].BusinessEntities ) { 
				// TODO: we assume id field is "entitynameid"
				if( previousEntity is DynamicEntity
					&& ( ( Key )( ( DynamicEntity )previousEntity ).Properties[ de.Name + "id" ] ).Value 
					== ( ( Key )de.Properties[ de.Name + "id" ] ).Value
				) {
					foreach( Property prop in de.Properties ) {
						( ( DynamicEntity )previousEntity ).Properties.Remove( prop.Name );
						( ( DynamicEntity )previousEntity ).Properties.Add( prop );
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

		private bool EvaluateLinks( ArrayList in_links, BusinessEntity in_entity ) {
			// TODO: can we restructure things to avoid this check?
			if( in_links.Count == 0 ) return true;

			foreach( LinkEntity link in in_links ) {
				foreach( BusinessEntity entity in data[ link.LinkToEntityName ].BusinessEntities ) {

					// TODO: we do this check and value retrieval in both filters and links handing
					// TODO: we assume that both ends of the link are either DynamicEntity or not
					object linkFromFieldValue = null;
					object linkToFieldValue = null;
					try { // another hack, since some entities will have null on the field we are linking
						if( entity.GetType().Name == "DynamicEntity" ) {
							// TODO: we only support StringProperty here
							linkFromFieldValue = ( ( DynamicEntity )in_entity ).Properties[ link.LinkFromAttributeName ];
							linkToFieldValue = ( ( DynamicEntity )entity ).Properties[ link.LinkToAttributeName ];
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
						if( ( ( Key )linkFromFieldValue ).Value == ( ( Lookup )linkToFieldValue ).Value
							&& EvaluateFilters( link.LinkCriteria, entity ) == true ) {
							// We short circuit - as long as one linked entity meets the criteria, we'll return true
							// TODO: this is a bug - we don't eval all links.
							return true;
						}
					}
					catch { /// HACK try it the other way
						try {
							if( ( ( Lookup )linkFromFieldValue ).Value == ( ( Key )linkToFieldValue ).Value
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

		private bool EvaluateFilters( FilterExpression in_filter, BusinessEntity in_entity ) {
			List<bool> results = new List<bool>();
			
			foreach( FilterExpression exp in in_filter.Filters ) {
				results.Add( EvaluateFilters( exp, in_entity ) );
			}
			foreach( ConditionExpression exp in in_filter.Conditions ) {
				bool result = true;
				object fieldValue;

				// TODO: extract this fieldvalue retrieval into a method 
				if( in_entity.GetType().Name == "DynamicEntity" ) {
					DynamicEntity entity = ( DynamicEntity )in_entity;
					// TODO: we only support StringProperty here
					fieldValue = entity.Properties[ exp.AttributeName ];
				}
				else {
					fieldValue = in_entity.GetType().GetProperty( exp.AttributeName ).GetValue( in_entity, null );
				}

				if( exp.Operator == ConditionOperator.Equal ) {
					foreach( object val in exp.Values ) {
						// TODO: handle other data types
						if( fieldValue is Key ) {
							if( !val.Equals( ( ( Key )fieldValue).Value ) ) {
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
