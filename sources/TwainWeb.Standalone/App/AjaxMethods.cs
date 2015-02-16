using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TwainWeb.Standalone.App;
using TwainWeb.Standalone.Twain;

namespace TwainWeb.Standalone.App
{
    public class AjaxMethods
    {
        public AjaxMethods(object markerAsync)
        {
            this.markerAsync = markerAsync;
        }
        private int waitTime = 30000;
        private object markerAsync;     

        private ScannerSettings ChangeSource(Twain32 _twain, int sourceIndex, CashSettings cashSettings)
        {            
            var searchSetting = cashSettings.Search(_twain, sourceIndex);
            if (searchSetting == null)
            {
                AsyncMethods.asyncWithWaitTime<int>(sourceIndex, "ChangeSource", _twain.ChangeSource, waitTime, _twain.waitHandle);
                if ( sourceIndex == _twain.SourceIndex && _twain.ChangeSourceResp == null)
                    searchSetting = cashSettings.PushCurrentSource(_twain);                
            }
            return searchSetting;
        }

        public ActionResult Scan(ScanForm command, Twain32 twain)
        {
            var scanResult = new ScanCommand(command, twain).Execute(markerAsync);

            if (scanResult.Validate())
                return new ActionResult { Content = scanResult.FileContent, ContentType = "text/json"};
            else
                throw new Exception(scanResult.Error);
        }

        public ActionResult GetScannerParameters(Twain32 _twain, CashSettings cashSettings, int? sourceIndex)
        {
            var actionResult = new ActionResult{ContentType="text/json"};
            ScannerSettings searchSetting = null;
            lock (markerAsync)
            {
                if (cashSettings.NeedUpdateNow(DateTime.UtcNow))
                {
                    int? currentSourceIndex = _twain.TwainState.IsOpenDataSource ? (int?)_twain.SourceIndex : null;
                    cashSettings.Update(_twain);
                    if (currentSourceIndex.HasValue)
                        searchSetting = ChangeSource(_twain, currentSourceIndex.Value, cashSettings);
                }

                if (!_twain.OpenSM())
                    return actionResult;

                var jsonResult = "{";
                if (_twain.SourcesCount > 0)
                {
                    bool needOfChangeSource = sourceIndex.HasValue && sourceIndex != _twain.SourceIndex;
                    if (needOfChangeSource)
                        searchSetting = this.ChangeSource(_twain, sourceIndex.Value, cashSettings);
                    else if (_twain.TwainState.IsOpenDataSource)
                    {
                        searchSetting = cashSettings.Search(_twain, _twain.SourceIndex);
                        if (searchSetting == null)
                            searchSetting = cashSettings.PushCurrentSource(_twain);
                    }
                    jsonResult += "\"sources\":{ \"selectedSource\": \"" + (needOfChangeSource ? sourceIndex.Value : _twain.SourceIndex) + "\", \"sourcesList\":[";

	                var sources = GetSources(_twain);
	                var sortedSources = SortSources(sources);

	                var i = 0;
	                foreach (var sortedSource in sortedSources)
	                {
						if (!needOfChangeSource && searchSetting == null)
						{
							searchSetting = this.ChangeSource(_twain, sortedSource.Key, cashSettings);
						}
						jsonResult += "{ \"key\": \"" + sortedSource.Key + "\", \"value\": \"" + sortedSource.Value + "\"}" + (i != (sortedSources.Count - 1) ? "," : "");
		                i++;
	                }

                    jsonResult += "]}";

                    jsonResult += searchSetting != null ? searchSetting.Serialize() : "";
                }
                jsonResult += "}";
                actionResult.Content = Encoding.UTF8.GetBytes(jsonResult);
            }            

            
            return actionResult;
        }

	    private Dictionary<int, string> GetSources(Twain32 twain)
	    {
		    var sourses = new Dictionary<int, string>();

			for (var i = 0; i < twain.SourcesCount; i++)
			{
				sourses.Add(i, twain.GetSourceProduct(i).Name);
			}
		    return sourses;
	    }

		private Dictionary<int, string> SortSources(Dictionary<int, string> initialDictionary)
		{
			var sourses = new Dictionary<int, string>();

			var soursesWithoutWia = new Dictionary<int, string>();

			foreach (var sourse in initialDictionary)
			{
				if (sourse.Value.ToLower().Contains("wia"))
				{
					sourses.Add(sourse.Key, sourse.Value);
				}
				else
				{
					soursesWithoutWia.Add(sourse.Key, sourse.Value);
				}
			}

			foreach (var otherSourse in soursesWithoutWia)
			{
				sourses.Add(otherSourse.Key, otherSourse.Value);
			}

			return sourses;
		} 
    }
}

