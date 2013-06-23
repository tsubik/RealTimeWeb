using System.Web;
using System.Web.Mvc;

namespace RealTimeWeb.PureSSE
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}