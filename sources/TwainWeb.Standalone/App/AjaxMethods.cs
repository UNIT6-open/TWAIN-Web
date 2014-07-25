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
                    for (int i = 0; i < _twain.SourcesCount; i++)
                    {
                        if (!needOfChangeSource && searchSetting == null)
                        {
                            searchSetting = this.ChangeSource(_twain, i, cashSettings);
                        }
                        jsonResult += "{ \"key\": \"" + i + "\", \"value\": \"" + _twain.GetSourceProduct(i).Name + "\"}" + (i != (_twain.SourcesCount - 1) ? "," : "");
                    }

                    jsonResult += "]}";

                    jsonResult += searchSetting != null ? searchSetting.Serialize() : "";
                }
                jsonResult += "}";
                actionResult.Content = Encoding.UTF8.GetBytes(jsonResult);
            }            

            
            return actionResult;
        }        
    }
}

