using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;

namespace Djn.Codegen
{
	class DummyCrmService : ICrmService
	{
		private Dictionary<string, List<BusinessEntity>> data = 
			new Dictionary<string, List<BusinessEntity>>();

		public Guid Create( BusinessEntity entity ) {
			Guid id = Guid.NewGuid();
			string name = entity.GetType().Name;
			if( name == "DynamicEntity" ) {
				DynamicEntity de = ( DynamicEntity )entity;
				data[ de.Name ].Add( entity );
				de.Properties.Add( new KeyProperty( de.Name + "id", new Key( id ) ) );
			}
			else {
				entity.GetType().GetProperty( name + "id" ).SetValue( entity, new Key( id ), null );
				if( data.ContainsKey( name ) == false ) {
					data.Add( name, new List<BusinessEntity>() );
				}
				data[ name ].Add( entity );
			}
			return id;
		}

		public void Delete( string entityName, Guid id ) {
			foreach( BusinessEntity entity in data[ entityName ] ) {
				Key key = ( Key )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
				Guid keyID = key.Value;
				if( keyID == id ) {
					data[ entityName ].Remove( entity );
					break;
				}
			}
		}

		public object Execute( object request ) {
			throw new NotImplementedException();
		}

		public string Fetch( string fetchXml ) {
			throw new NotImplementedException();
		}

		// Note: we ignore columnset
		public BusinessEntity Retrieve( string entityName, Guid id, Microsoft.Crm.Sdk.Query.ColumnSetBase columnSet ) {
			foreach( BusinessEntity entity in data[ entityName ] ) {
				Key key = ( Key )entity.GetType().GetProperty( entityName + "id" ).GetValue( entity, null );
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
				foreach( BusinessEntity entity in data[ query.EntityName ] ) {
					if( true == EvaluateFilters( queryExpression.Criteria, entity ) ) {
						retval.BusinessEntities.Add( entity );
					}
				}
			}
			return retval;
		}

		public void Update( BusinessEntity entity ) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		private bool EvaluateFilters( FilterExpression in_filter, BusinessEntity in_entity ) {
			List<bool> results = new List<bool>();
			
			foreach( FilterExpression exp in in_filter.Filters ) {
				results.Add( EvaluateFilters( exp, in_entity ) );
			}
			foreach( ConditionExpression exp in in_filter.Conditions ) {
				bool result = true;
				object fieldValue = in_entity.GetType().GetProperty( exp.AttributeName ).GetValue( in_entity, null );
				if( exp.Operator == ConditionOperator.Equal ) {
					foreach( object val in exp.Values ) {
						if( val != fieldValue ) {
							result = false;
							break;
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
