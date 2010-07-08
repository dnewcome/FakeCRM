using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.SdkTypeProxy;

namespace Djn.Codegen
{
	class Program
	{
		public static void Main() {
			ICrmService service = new DummyCrmService();
			contact contact = new contact();
			contact.address1_name = "Dan";
			contact.address1_city = "Bethesda";
			Guid id = service.Create( contact );
			

			ConditionExpression cond = new ConditionExpression( "address1_name", ConditionOperator.Equal, new string[]{ "Dan" } );
			ConditionExpression cond2 = new ConditionExpression( "address1_city", ConditionOperator.Equal, new string[] { "Bethesda" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );
			fe.Conditions.Add( cond2 );

			QueryExpression qe = new QueryExpression( "contact" );
			qe.Criteria = fe;

			BusinessEntityCollection bec = service.RetrieveMultiple( qe );
			Console.WriteLine( bec.BusinessEntities.Count );

			
			service.Delete( "contact", id );
			Console.ReadLine();

		}
	}
}
