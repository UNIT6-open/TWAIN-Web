using System;
using System.Xml;

namespace TwainWeb.Standalone.Configurator
{
	public class RuntimeConfigurationManager
	{
		private const string AppSettingsSectionName = "applicationSettings";
		private const string TwainWebSettingsSectionName = "TwainWeb.Standalone.Settings";
		private const string SettingSectionName = "setting";
		private const string NameAttributeName = "name";
		private const string ValueSectionName = "value";

		public static bool UpdateAppSettings(string key, string value)
		{
			var configDoc = new XmlDocument();
			configDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			var configurationNode = configDoc.SelectSingleNode("configuration");
			if (configurationNode == null)
				return false;

			var isUpdated = false;

			for (var i = 0; i < configurationNode.ChildNodes.Count; i++)
			{
				if (configurationNode.ChildNodes[i].Name == AppSettingsSectionName)
				{
					for (int j = 0; j < configurationNode.ChildNodes[i].ChildNodes.Count; j++)
					{
						if (configurationNode.ChildNodes[i].ChildNodes[j].Name == TwainWebSettingsSectionName)
						{
							for (int k = 0; k < configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
							{
								if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name == SettingSectionName)
								{
									if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes != null &&
										configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[NameAttributeName] != null &&
										configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[NameAttributeName].Value == key)
									{
										for (var l = 0; l < configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes.Count; l++)
										{
											if (configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes[l].Name == ValueSectionName)
											{
												configurationNode.ChildNodes[i].ChildNodes[j].ChildNodes[k].ChildNodes[l].InnerText = value;
												isUpdated = true;
												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (isUpdated)
			{
				configDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			}

			return isUpdated;
		}
	}
}
