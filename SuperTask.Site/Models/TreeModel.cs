using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TheSite.Models
{

	public class TreeModel
	{

		public System.Guid id { get; set; }

		public string text { get; set; }

		public System.Guid parentId { get; set; }

		public List<TreeModel> children { get; set; }
	}

}
