using UnityEngine;

namespace VersionHistory
{
	
	public class GitCommitParser 
	{
		public static ChangeLogScriptableObject.Item Parse(string text)
		{
			var item = new ChangeLogScriptableObject.Item();

			// Default to Undetermined
			item.Category = ChangeLogScriptableObject.Item.ChangeCategory.Undetermined;
			item.Description = text;

			// Convert the text to lowercase and trim spaces for case-insensitive matching
			var lowerText = text.ToLower().Trim();

			// Determine the category based on the text
			item.Category = DetermineCategory(lowerText);

			return item;
		}
		
		
		private static ChangeLogScriptableObject.Item.ChangeCategory DetermineCategory(string text)
		{
			// Extract the first keyword from the text (up to the first space or colon)
			var keyword = text.Split(' ', ':')[0];

			switch (keyword)
			{
				case "feat":
				case "feature":
				case "features":
				case "added":
				case "add":
					return ChangeLogScriptableObject.Item.ChangeCategory.Added;

				case "change":
				case "changed":
				case "update":
				case "updated":
				case "chore":
				case "chores":
					return ChangeLogScriptableObject.Item.ChangeCategory.Changed;

				case "deprecated":
					return ChangeLogScriptableObject.Item.ChangeCategory.Deprecated;

				case "remove":
				case "removed":
				case "delete":
				case "deleted":
					return ChangeLogScriptableObject.Item.ChangeCategory.Removed;

				case "fix":
				case "fixed":
				case "bugfix":
				case "bugfixed":
					if(text.Contains("security"))
						return ChangeLogScriptableObject.Item.ChangeCategory.Security;
					return ChangeLogScriptableObject.Item.ChangeCategory.Fixed;

				case "security":
				case "sec":
					return ChangeLogScriptableObject.Item.ChangeCategory.Security;

				case "docs":
				case "doc":
				case "documentation":
					if(text.Contains("change"))
						return ChangeLogScriptableObject.Item.ChangeCategory.Changed;
					return ChangeLogScriptableObject.Item.ChangeCategory.Added;

				default:
					return ChangeLogScriptableObject.Item.ChangeCategory.Undetermined;
			}
		}
	}

}