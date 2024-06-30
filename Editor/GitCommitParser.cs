using UnityEngine;

namespace VersionHistory
{
	
	public class GitCommitParser 
	{
		public static ChangeLogScriptableObject.Item Parse(string text)
		{
			var item = new ChangeLogScriptableObject.Item();
			item.Category = ChangeLogScriptableObject.Item.ChangeCategory.Undetermined;
			item.Description = text;
			return item;
		}
	}

}