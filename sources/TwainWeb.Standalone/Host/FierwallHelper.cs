using System;
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
		public void AddRuleForPort(int port)
		{
			try
            {
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
		            firewallRule = (INetFwRule) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
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
            catch (Exception ex)
            {
				_logger.Error(ex);
	            throw;
            }
        
		}
	}
}
