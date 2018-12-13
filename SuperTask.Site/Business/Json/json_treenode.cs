using System.Collections.Generic;

namespace Business
{

	public class json_treenode
	{

		public string id { get; set; }
		public string text { get; set; }
		public string type { get; set; }
		public List<json_treenode> children { get; set; }

	}

}