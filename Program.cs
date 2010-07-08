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
		public static ICrmService m_service;
 
		public static void Setup() {
			m_service = new MockCrmService();
			contact contact = new contact();
			contact.address1_name = "Dan";
			contact.address1_city = "Bethesda";
			Guid id = m_service.Create( contact );

			// data for testing links
			subject subject1 = new subject();
			subject1.title = "parent";
			Guid subject1ID = m_service.Create( subject1 );
			subject subject2 = new subject();
			subject2.title = "child";
			subject2.parentsubject = new Lookup( "subject", subject1ID );
			m_service.Create( subject2 );
		}

		public static void Teardown() { }

		public static void TestFilters() {
			ConditionExpression cond = new ConditionExpression( "address1_name", ConditionOperator.Equal, new string[] { "Dan" } );
			ConditionExpression cond2 = new ConditionExpression( "address1_city", ConditionOperator.Equal, new string[] { "Bethesda" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );
			fe.Conditions.Add( cond2 );

			QueryExpression qe = new QueryExpression( "contact" );
			qe.Criteria = fe;

			BusinessEntityCollection bec = m_service.RetrieveMultiple( qe );
			Console.WriteLine( bec.BusinessEntities.Count );
		}

		public static void TestLinks() {
			ConditionExpression cond = new ConditionExpression( "title", ConditionOperator.Equal, new string[] { "child" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );

			LinkEntity le = new LinkEntity( "subject", "subject", "subjectid", "parentsubject", JoinOperator.Inner );
			le.LinkCriteria = fe;

			QueryExpression qe = new QueryExpression( "subject" );
			qe.LinkEntities.Add( le );
			

			BusinessEntityCollection bec = m_service.RetrieveMultiple( qe );
			Console.WriteLine( bec.BusinessEntities.Count );
		}

		public static void Main() {
		
			// TODO: add test for delete back
			// service.Delete( "contact", id );
			Setup();
			TestLinks();
			Console.ReadLine();

		}

		private static void Test1() { 
			
		}
	}
}
