using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace VersionHistory
{
	public interface IChangeLogRenderer
	{
		void Render();
	}


	namespace VersionHistory
	{
		public class ChangeLogMarkdownRenderer : IChangeLogRenderer
		{
			private string _fullPath;
			private List<ChangeLogScriptableObject.Version> _versions;
			private ChangeLogSettings _settings;

			public ChangeLogMarkdownRenderer(string fullPath, List<ChangeLogScriptableObject.Version> versions,
				ChangeLogSettings settings)
			{
				_fullPath = fullPath;
				_versions = versions;
				_settings = settings;
			}

			public void Render()
			{
				Debug.Log($"Exporting change log to '{_fullPath}'");

				var b = new StringBuilder();
				b.AppendLine("# Changelog");
				b.AppendLine(_settings.IntroText);
				b.AppendLine("");

				foreach (var version in _versions)
				{
					// Write version header
					if (version.Items.Any(item => item.Included))
					{
						if(version.VersionName.StartsWith("not-versioned-yet"))
							b.AppendLine($"\n## New Version (version not specified yet)\n");
						else
							b.AppendLine($"\n## {version.VersionName}\n");
					}

					// Output version items, sorted by category and only included ones
					foreach (var category in Enum.GetValues(typeof(ChangeLogScriptableObject.Item.ChangeCategory))
						         .Cast<ChangeLogScriptableObject.Item.ChangeCategory>())
					{
						var caption = category;
						var filteredItems = version.Items
							.Where(item => item.Category == category && item.Included)
							.ToList();

						if (category == ChangeLogScriptableObject.Item.ChangeCategory.Undetermined)
						{
							// Treat Undetermined as Added
							filteredItems.ForEach(item =>
								item.Category = ChangeLogScriptableObject.Item.ChangeCategory.Added);

							caption = ChangeLogScriptableObject.Item.ChangeCategory.Added;
							if (filteredItems.Any())
							{
								Debug.LogWarning(
									$"Found undetermined items in version {version.VersionName}. Treating them as 'Added'.");
							}
						}

						if (filteredItems.Any())
						{
							b.AppendLine($"### {caption}\n");
							foreach (var item in filteredItems)
							{
								b.AppendLine($"- {item.Description}");
							}

							b.AppendLine("");
						}
					}
				}

				b.AppendLine(
					"The format is based on [Keep a Changelog](https://keepachangelog.com) and this project adheres to [Semantic Versioning](https://semver.org).");
				b.AppendLine(string.Format(_settings.CopyrightString, _settings.CompanyName));

				File.WriteAllText(_fullPath, b.ToString());
			}
		}
	}
}