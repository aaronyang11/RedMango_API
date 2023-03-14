using System.Net;
namespace RedMango_API.Model
{
	public class ApiResponse
	{
		public ApiResponse()
		{
			ErrorMessages = new List<String>();
		}
		public HttpStatusCode StatusCode { get; set; }
		public bool IsSuccess { get; set; } = true;
		public List<String> ErrorMessages { get; set; }
		public object Result { get; set; }
	}
}

