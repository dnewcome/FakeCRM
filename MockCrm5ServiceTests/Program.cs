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
			
			// test data for simple fetch
			Entity de = new Entity();
			de.LogicalName = "mydynamic";
			de["prop1"] = "foo";
			Guid deID = m_service.Create( de );

			// test data for filters
			de = new Entity();
			de.LogicalName = "contact";
			de[ "address1_name" ] = "Dan";
			de[ "address1_city" ] = "Bethesda";
			Guid deID2 = m_service.Create( de );

			// data for testing links
			Guid guid = Guid.NewGuid();
			de = new Entity();
			de.LogicalName = "subject";
			de[ "subjectid" ] = guid;
			Guid deID3 = m_service.Create( de );

			de = new Entity();
			de.LogicalName = "subject";
			de[ "subjectid" ] = guid;
			de[ "title" ] = "child";
			de[ "parentsubject" ] = new EntityReference( "subject", deID3 );
			Guid deID4 = m_service.Create( de );
		}

		[FestTest]
		public void TestSingleFilter() {
			QueryBase query = CrmQuery
				.Select()
				.From( "mydynamic" )
				.Where( "mydynamic", "prop1", ConditionOperator.Equal, new object[] { "foo" } ).Query;

			EntityCollection bec = m_service.RetrieveMultiple( query );
			Fest.AssertTrue( bec.Entities.Count > 0, "found more than zero entities" );
		}

		[FestTest]
		public void TestMultipleFilters() {
			QueryBase query = CrmQuery
				.Select()
				.From( "contact" )
				.Where( "contact", "address1_name", ConditionOperator.Equal, new object[] { "Dan" } )
				.Where( "contact", "address1_city", ConditionOperator.Equal, new object[] { "Bethesda" } ).Query;

			EntityCollection bec = m_service.RetrieveMultiple( query );
			Fest.AssertTrue( bec.Entities.Count > 0, "found more than zero entities" );
		}
		
		[FestTest]
		public void TestLinks() {
			QueryBase query = CrmQuery
				.Select()
				.From( "subject" )
				.Join( "subject", "subjectid", "subject", "parentsubject" )
				.Where( "subject", "title", ConditionOperator.Equal, new object[] { "child" } ).Query;

			EntityCollection bec = m_service.RetrieveMultiple( query );
			Fest.AssertTrue( bec.Entities.Count > 0, "found more than zero entities" );
		}

		public static void Main() {
			Fest.Run();
			Console.ReadLine();
		}
	}
}
