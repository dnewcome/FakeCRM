using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.SdkTypeProxy;
using System.Xml.Serialization;

namespace Djn.Testing
{
	[XmlInclude( typeof( contact ) )]
	[XmlInclude( typeof( subject ) )]
	public class BusinessEntityList : List<BusinessEntity>
	{
	}
}
