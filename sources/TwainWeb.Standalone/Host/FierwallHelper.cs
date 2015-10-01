using System;
using System.Text.RegularExpressions;
using log4net;
using NetFwTypeLib;

namespace TwainWeb.Standalone.Host
{
	public class FierwallHelper
	{
		private readonly ILog _logger;
		public FierwallHelper()
		{
			_logger = LogManager.GetLogger(typeof (FierwallHelper));
		}
		private const string RuleName = "TWAIN@Web";

		private bool IsWinVistaOrHigher
		{
			get
			{
				var os = Environment.OSVersion;
				return (os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6);
			}
		}

		private int? ServicePackVersion
		{
			get
			{
				var servicePackVersionString = Regex.Match(Environment.OSVersion.ServicePack, @"\d+").Value;
				try
				{
					var servicePackVersion = int.Parse(servicePackVersionString);
					return servicePackVersion;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}
		private void AddRuleForPortWinVistaOrHigher(int port)
		{
			var type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
			if (type == null)
			{
				_logger.Error("Can not add rule for port: Type.GetTypeFromProgID returns null");
				return;
			}
			var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
							Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));



			var newRule = false;

			INetFwRule firewallRule = null;
			var existingRules = firewallPolicy.Rules;
			foreach (var existingRule in existingRules)
			{
				var rule = existingRule as INetFwRule;
				if (rule == null) continue;

				if (rule.Name == RuleName)
				{
					firewallRule = rule;
					break;
				}
			}

			if (firewallRule == null)
			{
				firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
				newRule = true;
			}

			firewallRule.Protocol = 6;
			firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
			firewallRule.Description = "Used to allow access to TWAIN@Web.";
			firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
			firewallRule.Enabled = true;
			firewallRule.InterfaceTypes = "All";
			firewallRule.Name = RuleName;
			firewallRule.LocalPorts = port.ToString();

			if (newRule)
				firewallPolicy.Rules.Add(firewallRule);
		}

		private void AddRuleForPortWinXp(int port)
		{
			try
			{
				_logger.Info("Win xp sp > 1");

				var fwMgrType = Type.GetTypeFromProgID(("HNetCfg.FwMgr"));
				var newRule = false;
				if (fwMgrType == null)
				{
					_logger.Error("Can not add rule for port: HNetCfg.FwMgr is null");
					return;
				}

				var fwMgr = (INetFwMgr) Activator.CreateInstance(fwMgrType);

				var profile = fwMgr.LocalPolicy.CurrentProfile;

				INetFwOpenPort openPort = null;

				try
				{
					foreach (var globallyOpenPort in profile.GloballyOpenPorts)
					{
						if (globallyOpenPort is INetFwOpenPort && ((INetFwOpenPort) globallyOpenPort).Name == "TWAIN@Web")
							openPort = globallyOpenPort as INetFwOpenPort; 
					}
				}
				catch (Exception)
				{
					openPort = null;
				}
				if (openPort == null)
				{
					openPort = (INetFwOpenPort) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWOpenPort"));
					newRule = true;
				}

				openPort.Name = "TWAIN@Web";
				openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
				openPort.Port = port;
				openPort.Enabled = true;
				openPort.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;

				if (newRule)
					profile.GloballyOpenPorts.Add(openPort);
			}
			catch (Exception e)
			{
				_logger.Error("Can not add rule for port: " + e);
			}
		}


		public void AddRuleForPort(int port)
		{
			try
			{
				_logger.Info("Operating system: " + Environment.OSVersion.Version + " " + Environment.OSVersion.ServicePack);

				if (IsWinVistaOrHigher)
					AddRuleForPortWinVistaOrHigher(port);
				else
				{					
					if (ServicePackVersion.HasValue && ServicePackVersion.Value > 1)
							AddRuleForPortWinXp(port);
				}
			}
            catch (Exception ex)
            {
				_logger.Error(ex);
	            throw;
            }
        
		}
	}
}
