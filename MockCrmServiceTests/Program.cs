using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.SdkTypeProxy;
using Djn.Crm;

namespace Djn.Testing
{
	class Program
	{
		
 
		public static MockCrmService Setup() {
			MockCrmService m_service = new MockCrmService();
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

			DynamicEntity de = new DynamicEntity();
			de.Name = "mydynamic";
			de.Properties.Add( new StringProperty( "prop1", "foo" ) );
			Guid deID = m_service.Create( de );
			return m_service;
		}

		public static void Teardown() { }

		public static void TestFilters( MockCrmService in_service ) {
			ConditionExpression cond = new ConditionExpression( "address1_name", ConditionOperator.Equal, new string[] { "Dan" } );
			ConditionExpression cond2 = new ConditionExpression( "address1_city", ConditionOperator.Equal, new string[] { "Bethesda" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );
			fe.Conditions.Add( cond2 );

			QueryExpression qe = new QueryExpression( "contact" );
			qe.Criteria = fe;

			BusinessEntityCollection bec = in_service.RetrieveMultiple( qe );
			Console.WriteLine( "TestFilters() found: " + bec.BusinessEntities.Count + " entity. " );
		}

		public static void TestLinks( MockCrmService in_service ) {
			ConditionExpression cond = new ConditionExpression( "title", ConditionOperator.Equal, new string[] { "child" } );
			FilterExpression fe = new FilterExpression();
			fe.FilterOperator = LogicalOperator.And;
			fe.Conditions.Add( cond );

			LinkEntity le = new LinkEntity( "subject", "subject", "subjectid", "parentsubject", JoinOperator.Inner );
			le.LinkCriteria = fe;

			QueryExpression qe = new QueryExpression( "subject" );
			qe.LinkEntities.Add( le );
			

			BusinessEntityCollection bec = in_service.RetrieveMultiple( qe );
			Console.WriteLine( "TestLinks() found: " + bec.BusinessEntities.Count + " entity. " );
		}

		[FestTest]
		public static void TestRetrieve() {
			MockCrmService serviceFromDisk = new MockCrmService( "database.xml" );
			// guid is in the database.xml file
			contact be = ( contact )serviceFromDisk.Retrieve( "contact", new Guid( "0e830282-7bc9-4a71-9745-6cd299632040" ), new AllColumns() );
			Console.WriteLine( "TestRetrieve() found: " + be.address1_name );
		}

		[FestTest]
		public static void TestRetrieveDynamic() {
			MockCrmService serviceFromDisk = new MockCrmService( "database.xml" );
			// guid is in the database.xml file
			DynamicEntity de = ( DynamicEntity )serviceFromDisk.Retrieve( "contact", new Guid( "6d746f8d-b837-4365-afa1-fcab8c0d12c5" ), new AllColumns() );
			Console.WriteLine( "TestRetrieveDynamic() found: " + de.Properties[ "address1_name" ] );
		}

		public static void Main() {
			Fest.Run();
			Console.ReadLine();
		}

		[FestTest]
		public void TestMockData() {
			MockCrmService serviceFromDisk = new MockCrmService( "mockdata.xml" );
			QueryBase query = CrmQuery
				.Select()
				.From( "new_dynamicform" )
				.Join( "new_dynamicform", "new_previousformid", "new_dynamicform", "new_dynamicformid" )
				.Where( "new_dynamicform", "new_name", ConditionOperator.Equal, new object[] { "welcome" } ).Query;
			BusinessEntityCollection bec = serviceFromDisk.RetrieveMultiple( query );
			Console.WriteLine( "TestMockData() found: " + bec.BusinessEntities.Count + " entity. " );
			Fest.AssertTrue( bec.BusinessEntities.Count > 0, "No business entities returned" );
		}

		// [FestTest]
		public void Test1() {
			// TODO: add test for delete back
			// service.Delete( "contact", id );

			MockCrmService service = Setup();
			MockCrmService serviceFromDisk = new MockCrmService( "database.xml" );
			

			TestLinks( service );
			TestLinks( serviceFromDisk );
			TestFilters( service );
			TestFilters( serviceFromDisk );

			// TestRetrieve();
			// m_service.PersistToDisk( "database.xml" );
		}
	}
}
