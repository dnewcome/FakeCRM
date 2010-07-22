using System;
using System.Collections.Generic;
using Djn.Framework;
using Microsoft.Crm;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.SdkTypeProxy;
using Altai.MSCrm;
using Djn.Crm;

namespace Djn.Testing.MockCrmConsole
{
	/**
	 * The Mock crm console is a way to load data into a mock from remote 
	 * CRM instances or to load for inspection.
	 */
	class CrmConsole
	{
		/**
		 * usage: mc <command> <bizorg> <entityname> <filterfield> <filtervalue>
		 *	Note that we support only basic string-field filters currently
		 *	outputs xml serialization to std output
		 */
		static void Main( string[] args ) {
			if( args.Length < 5 ) {
				PrintUsage();
				Environment.Exit( 1 );
			}
			if( args[ 0 ] == "import" ) {
				Import( args[ 1 ], args[ 2 ], args[ 3 ], args[ 4 ] );
			}
			else {
				PrintUsage();
				Environment.Exit( 1 );
			}
		}

		static void PrintUsage() {
			Console.WriteLine( "Usage:" );
			Console.WriteLine( "mc <command> <bizorg> <entityname> <filterfield> { <filtervalue> | % }" );
		}

		static void Import( string in_bizorg, string in_entityname, string in_filterfield, string in_filter ) {
			if( String.IsNullOrEmpty( in_bizorg )
				|| String.IsNullOrEmpty( in_entityname )
				|| String.IsNullOrEmpty( in_filter ) 
			) {
				Console.WriteLine( "Usage:" );
				Console.WriteLine( "import <bizorg> <entityname> <filterfield> { <filtervalue> | % }" );
				Environment.Exit( 1 );
			}

			ConditionOperator oper; ;
			if( in_filter == "%" ) {
				oper = ConditionOperator.Like;
			}
			else {
				oper = ConditionOperator.Equal;
			}

			QueryBase query = CrmQuery
				.Select()
				.From( in_entityname )
				.Where( in_entityname, in_filterfield, oper, new object[] { in_filter } ).Query;

			BusinessEntityCollection bec = DynamicEntityHelper.GetDynamicEntityCollection( in_bizorg, query );
			// Serializer.Serialize( typeof( BusinessEntityCollection ), bec, Console.OpenStandardOutput() );
			Serializer.Serialize( typeof( List<BusinessEntity> ), bec.BusinessEntities, Console.OpenStandardOutput() );
		}
	} // class
} // namespace
