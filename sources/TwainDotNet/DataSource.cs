using System;
using System.Collections.Generic;
using TwainDotNet.TwainNative;
using log4net;
using Duplex = TwainDotNet.TwainNative.Duplex;

namespace TwainDotNet
{
    public class DataSource : IDisposable
    {
	    readonly Identity _applicationId;
	    readonly IWindowsMessageHook _messageHook;
		private readonly ILog _log;

		
		public bool IsDisposed { get; private set; }
	    /// <summary>
		/// A Source never has a state less than 4 if it is open. If it is closed, it has no state.
	    /// </summary>
	    public TwainState? SourceState { get; set; }

        public DataSource(Identity applicationId, Identity sourceId, IWindowsMessageHook messageHook, ILog logger)
        {
            _applicationId = applicationId;
            SourceId = sourceId.Clone();
            _messageHook = messageHook;
			SourceState = null;
	        _log = logger;
        }

        ~DataSource()
        {
            Dispose(false);
        }

        public Identity SourceId { get; private set; }

		public SourceSettings GetAwailableSourceSettings()
	    {
			var pixelTypes = new List<ushort>();
			var pixelTypesCap = Capability.GetCapability(Capabilities.IPixelType, _applicationId, SourceId);
			if (pixelTypesCap != null)
			{
				foreach (var pt in pixelTypesCap)
				{
					pixelTypes.Add((ushort)pt);
				}
			}


			float physicalHeight = 0;
			var physicalHeightCap = Capability.GetCapability(Capabilities.PhysicalHeight, _applicationId, SourceId);

			if (physicalHeightCap != null && physicalHeightCap.Count == 1)
			{
				physicalHeight = ValueConverter.ConvertToFix32(physicalHeightCap[0]);
			}

			float physicalWidth = 0;
			var physicalWidthCap = Capability.GetCapability(Capabilities.PhysicalWidth, _applicationId, SourceId);

			if (physicalWidthCap != null && physicalWidthCap.Count == 1)
			{
				physicalWidth = ValueConverter.ConvertToFix32(physicalWidthCap[0]);
			}

			bool hasADF, hasFlatbed;
			var flatbedResolutions = new List<float>();
			var feederResolutions = new List<float>();

			try
			{
				var documentFeederEnabled = Capability.GetBoolCapability(Capabilities.FeederEnabled, _applicationId, SourceId);
				if (documentFeederEnabled)
				{
					feederResolutions = GetResolutions();
				    Capability.SetCapability(Capabilities.FeederEnabled, false, _applicationId, SourceId);
				    var newDocumentFeederEnabled = Capability.GetBoolCapability(Capabilities.FeederEnabled, _applicationId, SourceId);

				    hasADF = true;
				    hasFlatbed = !newDocumentFeederEnabled;
					if (hasFlatbed) flatbedResolutions = GetResolutions();
				}
			    else
				{
					flatbedResolutions = GetResolutions();
				    Capability.SetCapability(Capabilities.FeederEnabled, true, _applicationId, SourceId);
				    var newDocumentFeederEnabled = Capability.GetBoolCapability(Capabilities.FeederEnabled, _applicationId, SourceId);

				    hasADF = newDocumentFeederEnabled;
				    hasFlatbed = true;

					if (hasADF)
						feederResolutions = GetResolutions();

				}
			}
			catch (Exception)
			{
				hasADF = false;
				hasFlatbed = true;

				flatbedResolutions = GetResolutions();
			}
			
		
			var hasDuplex = false;
			try
			{
				var duplexCap = Capability.GetCapability(Capabilities.Duplex, _applicationId, SourceId);
				if (duplexCap == null) hasDuplex = false;
				else
				{
					foreach (var value in duplexCap)
					{
						if ((Duplex)value == Duplex.None)
						{
							hasDuplex = false;
							break;
						}
						if ((Duplex)value == Duplex.OnePass || (Duplex)value == Duplex.TwoPass)
						{
							hasDuplex = true;
						}
					}
				}
			}
			catch (Exception)
			{
				hasDuplex = false;
			}
			_log.Debug("GetCapabilities, result: Success");

			return new SourceSettings(flatbedResolutions, feederResolutions, pixelTypes, physicalHeight, physicalWidth, hasADF, hasFlatbed, hasDuplex);
	    }

		private List<float> GetResolutions()
		{
			var resolutions = new List<float>();
		    var resolutionCap = Capability.GetCapability(Capabilities.XResolution, _applicationId, SourceId);
		    if (resolutionCap != null)
		    {
			    foreach (var res in resolutionCap)
			    {
					resolutions.Add(ValueConverter.ConvertToFix32(res));
			    }
		    }

			return resolutions;
		}


	    public void NegotiateTransferCount(ScanSettings scanSettings)
        {
            try
            {
                scanSettings.TransferCount = Capability.SetCapability(
                        Capabilities.XferCount,
                        scanSettings.TransferCount,
                        _applicationId,
                        SourceId);
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public void NegotiateFeeder(ScanSettings scanSettings)
        {

            try
            {
                if (scanSettings.UseDocumentFeeder.HasValue)
                {
                    Capability.SetCapability(Capabilities.FeederEnabled, scanSettings.UseDocumentFeeder.Value, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }

            try
            {
                if (scanSettings.UseAutoFeeder.HasValue)
                {
                    Capability.SetCapability(Capabilities.AutoFeed, scanSettings.UseAutoFeeder == true && scanSettings.UseDocumentFeeder == true, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }

            try
            {
                if (scanSettings.UseAutoScanCache.HasValue)
                {
                    Capability.SetCapability(Capabilities.AutoScan, scanSettings.UseAutoScanCache.Value, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }

        }

        public PixelType GetPixelType(ScanSettings scanSettings)
        {
            switch (scanSettings.Resolution.ColourSetting)
            {
                case ColourSetting.BlackAndWhite:
                    return PixelType.BlackAndWhite;

                case ColourSetting.GreyScale:
                    return PixelType.Grey;

                case ColourSetting.Colour:
                    return PixelType.Rgb;
            }

            throw new NotImplementedException();
        }

        public short GetBitDepth(ScanSettings scanSettings)
        {
            switch (scanSettings.Resolution.ColourSetting)
            {
                case ColourSetting.BlackAndWhite:
                    return 1;

                case ColourSetting.GreyScale:
                    return 8;

                case ColourSetting.Colour:
                    return 16;
            }

            throw new NotImplementedException();
        }

        public bool PaperDetectable
        {
            get
            {
                try
                {
                    return Capability.GetBoolCapability(Capabilities.FeederLoaded, _applicationId, SourceId);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool SupportsDuplex
        {
            get
            {
                try
                {
                    var cap = new Capability(Capabilities.Duplex, TwainType.Int16, _applicationId, SourceId);
                    return ((Duplex)cap.GetBasicValue().Int16Value) != Duplex.None;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void NegotiateColour(ScanSettings scanSettings)
        {
            try
            {
                Capability.SetBasicCapability(Capabilities.IPixelType, (ushort)GetPixelType(scanSettings), TwainType.UInt16, _applicationId, SourceId);
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }

            // TODO: Also set this for colour scanning
            try
            {
                if (scanSettings.Resolution.ColourSetting != ColourSetting.Colour)
                {
                    Capability.SetCapability(Capabilities.BitDepth, GetBitDepth(scanSettings), _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }

        }

        public void NegotiateResolution(ScanSettings scanSettings)
        {
            try
            {
                if (scanSettings.Resolution.Dpi.HasValue)
                {
                    int dpi = scanSettings.Resolution.Dpi.Value;
                    Capability.SetBasicCapability(Capabilities.XResolution, dpi, TwainType.Fix32, _applicationId, SourceId);
                    Capability.SetBasicCapability(Capabilities.YResolution, dpi, TwainType.Fix32, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public void NegotiateDuplex(ScanSettings scanSettings)
        {
            try
            {
                if (scanSettings.UseDuplex.HasValue && SupportsDuplex)
                {
                    Capability.SetCapability(Capabilities.DuplexEnabled, scanSettings.UseDuplex.Value, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public void NegotiateOrientation(ScanSettings scanSettings)
        {
            // Set orientation (default is portrait)
            try
            {
                var cap = new Capability(Capabilities.Orientation, TwainType.Int16, _applicationId, SourceId);
                if ((Orientation)cap.GetBasicValue().Int16Value != Orientation.Default)
                {
                    Capability.SetBasicCapability(Capabilities.Orientation, (ushort)scanSettings.Page.Orientation, TwainType.UInt16, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the size of the page.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiatePageSize(ScanSettings scanSettings)
        {
            try
            {
                var cap = new Capability(Capabilities.Supportedsizes, TwainType.Int16, _applicationId, SourceId);
                if ((PageType)cap.GetBasicValue().Int16Value != PageType.UsLetter)
                {
                    Capability.SetBasicCapability(Capabilities.Supportedsizes, (ushort)scanSettings.Page.Size, TwainType.UInt16, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the automatic rotation capability.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateAutomaticRotate(ScanSettings scanSettings)
        {
            try
            {
                if (scanSettings.Rotation.AutomaticRotate)
                {
                    Capability.SetCapability(Capabilities.Automaticrotate, true, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the automatic border detection capability.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateAutomaticBorderDetection(ScanSettings scanSettings)
        {
            try
            {
                if (scanSettings.Rotation.AutomaticBorderDetection)
                {
                    Capability.SetCapability(Capabilities.Automaticborderdetection, true, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the indicator.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateProgressIndicator(ScanSettings scanSettings)
        {
            try
            {
                if (scanSettings.ShowProgressIndicatorUI.HasValue)
                {
                    Capability.SetCapability(Capabilities.Indicators, scanSettings.ShowProgressIndicatorUI.Value, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public bool Open(ScanSettings settings)
        {
            OpenSource();

            if (settings.AbortWhenNoPaperDetectable && !PaperDetectable)
                throw new FeederEmptyException();

            // Set whether or not to show progress window
            NegotiateProgressIndicator(settings);
            NegotiateTransferCount(settings);
            NegotiateFeeder(settings);
            NegotiateDuplex(settings);

            if (settings.UseDocumentFeeder == true &&
                settings.Page != null)
            {
                NegotiatePageSize(settings);
                NegotiateOrientation(settings);
            }
            if (settings.Area != null)
            {
                NegotiateArea(settings);
            }

            if (settings.Resolution != null)
            {
                NegotiateColour(settings);
                NegotiateResolution(settings);
            }

            // Configure automatic rotation and image border detection
            if (settings.Rotation != null)
            {
                NegotiateAutomaticRotate(settings);
                NegotiateAutomaticBorderDetection(settings);
            }

            return Enable(settings);
        }

        private bool NegotiateArea(ScanSettings scanSettings)
        {
            var area = scanSettings.Area;

            if (area == null)
            {
                return false;
            }

			try
            {
                var cap = new Capability(Capabilities.IUnits, TwainType.Int16, _applicationId, SourceId);
                if ((Units)cap.GetBasicValue().Int16Value != area.Units)
                {
                    Capability.SetCapability(Capabilities.IUnits, (short)area.Units, _applicationId, SourceId);
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
	            return false;
            }

	        float right;
	        float bottom;
			
			float physicalHeight = 0;
			var physicalHeightCap = Capability.GetCapability(Capabilities.PhysicalHeight, _applicationId, SourceId);

			if (physicalHeightCap != null && physicalHeightCap.Count == 1)
			{
				physicalHeight = ValueConverter.ConvertToFix32(physicalHeightCap[0]);
			}

			float physicalWidth = 0;
			var physicalWidthCap = Capability.GetCapability(Capabilities.PhysicalWidth, _applicationId, SourceId);

			if (physicalWidthCap != null && physicalWidthCap.Count == 1)
			{
				physicalWidth = ValueConverter.ConvertToFix32(physicalWidthCap[0]);
			}

			right = physicalWidth < area.Right ? physicalWidth : area.Right;
			bottom = physicalHeight < area.Bottom ? physicalHeight : area.Bottom;

            var imageLayout = new ImageLayout
            {
                Frame = new Frame
                {
                    Left = new Fix32(area.Left),
                    Top = new Fix32(area.Top),
                    Right = new Fix32(right),
                    Bottom = new Fix32(bottom)
                }
            };

	 

            var result = Twain32Native.DsImageLayout(
                _applicationId,
                SourceId,
                DataGroup.Image,
                DataArgumentType.ImageLayout,
                Message.Set,
                imageLayout);

            if (result != TwainResult.Success && result != TwainResult.CheckStatus)
            {
				var condition = DataSourceManager.GetConditionCode(_applicationId, SourceId);
	            return false;
				//throw new TwainException("DsImageLayout.GetDefault error", result, condition);
            }

            return true;
        }

        public void OpenSource()
        {
            var result = Twain32Native.DsmIdentity(
                   _applicationId,
                   IntPtr.Zero,
                   DataGroup.Control,
                   DataArgumentType.Identity,
                   Message.OpenDS,
                   SourceId);

			if (result != TwainResult.Success)
	        {
		        var conditionCode = DataSourceManager.GetConditionCode(_applicationId, SourceId);
				_log.Debug(string.Format("OpenDS, result: {0}, conditionCode: {1}", result, conditionCode));
		        throw new TwainException("Error opening data source", result, conditionCode);
	        }

			_log.Debug("OpenDS, result: " + result);
	        
			SourceState = TwainState.SourceOpen;
        }

        public bool Enable(ScanSettings settings)
        {
            var ui = new UserInterface();
            ui.ShowUI = (short)(settings.ShowTwainUI ? 1 : 0);
            ui.ModalUI = 0;
            ui.ParentHand = _messageHook.WindowHandle;

            var result = Twain32Native.DsUserInterface(
                _applicationId,
                SourceId,
                DataGroup.Control,
                DataArgumentType.UserInterface,
                Message.EnableDS,
                ui);

			_log.Debug(string.Format("EnableDS, result: {0}", result));
            if (result != TwainResult.Success)
            {
                Dispose();
                return false;
            }

			SourceState = TwainState.SourceEnabled;
            return true;
        }




        public void Dispose()
        {
	        if (!IsDisposed)
	        {
		        Dispose(true);
		        GC.SuppressFinalize(this);
	        }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisableAndClose();
            }
	        IsDisposed = true;
        }

	    public void DisableAndClose()
	    {
		    if (Disable())
				Close();
	    }

	    public bool Disable()
	    {
		    if (SourceState != null && SourceState >= TwainState.SourceEnabled)
		    {
			    if (SourceId.Id != 0)
			    {
				    var userInterface = new UserInterface();

				    TwainResult result = Twain32Native.DsUserInterface(
					    _applicationId,
					    SourceId,
					    DataGroup.Control,
					    DataArgumentType.UserInterface,
					    Message.DisableDS,
					    userInterface);


				    if (result != TwainResult.Failure)
				    {
					    _log.Debug(string.Format("DisableDS, result: {0}", result));
					    SourceState = TwainState.SourceOpen;
					    return true;
				    }
				    var condition = DataSourceManager.GetConditionCode(_applicationId, SourceId);
				    _log.Debug(string.Format("DisableDS, result: {0}, conditionCode: {1}", result, condition));
				    return false;
			    }
				return false;
		    }
		    
			return false;		   
	    }

        public void Close()
        {
          
	            var result = Twain32Native.DsmIdentity(
                        _applicationId,
                        IntPtr.Zero,
                        DataGroup.Control,
                        DataArgumentType.Identity,
                        Message.CloseDS,
                        SourceId);

				
	            if (result != TwainResult.Failure)
	            {
					_log.Debug(string.Format("CloseDS, result: {0}", result));
					SourceState = null;
	            }
                else
                {
					var condition = DataSourceManager.GetConditionCode(_applicationId, SourceId);
					_log.Debug(string.Format("CloseDS, result: {0}, conditionCode: {1}", result, condition));
                }
			
            
        }
    }
}
