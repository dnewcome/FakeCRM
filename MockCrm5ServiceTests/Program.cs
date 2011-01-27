using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Djn.Crm5;

namespace Djn.Testing
{
	class Program
	{
		MockCrmService m_service;

		public Program() {
            
			m_service = new MockCrmService();
			/*
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
            */

			Entity de = new Entity();
			de.LogicalName = "mydynamic";
			de["prop1"] = "foo";
			Guid deID = m_service.Create( de );
		}

        /*
		[FestTest]
		public void TestFilters() {
			ConditionExpression cond = new ConditionExpression( "address1_name", ConditionOperator.Equal, new string[] { "Dan" } );
			ConditionExpression cond2 = new ConditionExpression( "address1_city", ConditionOperator.Equal, new string[] { "Bethesda" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );
			fe.Conditions.Add( cond2 );

			QueryExpression qe = new QueryExpression( "contact" );
			qe.Criteria = fe;

			BusinessEntityCollection bec = m_service.RetrieveMultiple( qe );
			Console.WriteLine( "TestFilters() found: " + bec.BusinessEntities.Count + " entity. " );
			Fest.AssertTrue( bec.BusinessEntities.Count > 0, "found more than zero entities" );
		}

		[FestTest]
		public void TestLinks() {
			ConditionExpression cond = new ConditionExpression( "title", ConditionOperator.Equal, new string[] { "child" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );

			LinkEntity le = new LinkEntity( "subject", "subject", "subjectid", "parentsubject", JoinOperator.Inner );
			le.LinkCriteria = fe;

			QueryExpression qe = new QueryExpression( "subject" );
			qe.LinkEntities.Add( le );

			BusinessEntityCollection bec = m_service.RetrieveMultiple( qe );
			Console.WriteLine( "TestLinks() found: " + bec.BusinessEntities.Count + " entity. " );
			Fest.AssertTrue( bec.BusinessEntities.Count > 0, "found more than zero entities" );
		}
        */

		[FestTest]
		public void TestDynamic() {
			QueryBase query = CrmQuery
				.Select()
				.From( "mydynamic" )
				.Where( "mydynamic", "prop1", ConditionOperator.Equal, new object[] { "foo" } ).Query;

			EntityCollection bec = m_service.RetrieveMultiple( query );
			Fest.AssertTrue( bec.Entities.Count > 0, "found more than zero entities" );
		}


		public static void Main() {
			Fest.Run();
			Console.ReadLine();
		}
	}
}
