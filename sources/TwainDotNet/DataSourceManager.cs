using System;
using System.Collections.Generic;
using System.Text;
using TwainDotNet.TwainNative;
using System.Runtime.InteropServices;
using TwainDotNet.Win32;
using System.Reflection;
using System.Drawing;
using log4net;

namespace TwainDotNet
{
    public class DataSourceManager : IDisposable
    {
        /// <summary>
        /// The logger for this class.
        /// </summary>
        static ILog _log = LogManager.GetLogger(typeof(DataSourceManager));

        IWindowsMessageHook _messageHook;
        Event _eventMessage;
		private TwainState _twainState = TwainState.PreSession;

        public Identity ApplicationId { get; private set; }
        public DataSource DataSource { get; private set; }

        public DataSourceManager(Identity applicationId, IWindowsMessageHook messageHook)
        {

            // Make a copy of the identity in case it gets modified
            ApplicationId = applicationId.Clone();

            ScanningComplete += delegate { };
            TransferImage += delegate { };

            _messageHook = messageHook;
            _messageHook.FilterMessageCallback = FilterMessage;
            IntPtr windowHandle = _messageHook.WindowHandle;

            _eventMessage.EventPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WindowsMessage)));

            // Initialise the data source manager
            TwainResult result = Twain32Native.DsmParent(
                ApplicationId,
                IntPtr.Zero,
                DataGroup.Control,
                DataArgumentType.Parent,
                Message.OpenDSM,
                ref windowHandle);

			_log.Debug(string.Format("OpenDSM, result: {0}", result));
            if (result == TwainResult.Success)
            {
                //according to the 2.0 spec (2-10) if (applicationId.SupportedGroups
                // | DataGroup.Dsm2) > 0 then we should call DM_Entry(id, 0, DG_Control, DAT_Entrypoint, MSG_Get, wh)
                //right here
				
	            _twainState = TwainState.SourceManagerOpen;
                DataSource = GetDefault(ApplicationId);
            }
            else
            {
                throw new TwainException("Error initialising DSM: " + result, result);
            }
        }

		public DataSource GetDefault(Identity applicationId)
		{
			var defaultSourceId = new Identity();

			// Attempt to get information about the system default source
			var result = Twain32Native.DsmIdentity(
				applicationId,
				IntPtr.Zero,
				DataGroup.Control,
				DataArgumentType.Identity,
				Message.GetDefault,
				defaultSourceId);

			if (result != TwainResult.Success)
			{

				var status = GetConditionCode(applicationId, null);

				_log.Debug(string.Format("GetDefault, result: {0}, conditionCode: {1}", result, status));
				throw new TwainException("Error getting information about the default source: " + result, result, status);
			}
			_log.Debug(string.Format("GetDefault, result: {0}", result));
			return new DataSource(applicationId, defaultSourceId,_messageHook, _log);
		}

		public static DataSource UserSelected(Identity applicationId, IWindowsMessageHook messageHook)
		{
			var defaultSourceId = new Identity();

			// Show the TWAIN interface to allow the user to select a source
			var result = Twain32Native.DsmIdentity(
				applicationId,
				IntPtr.Zero,
				DataGroup.Control,
				DataArgumentType.Identity,
				Message.UserSelect,
				defaultSourceId);

			_log.Debug(string.Format("UserSelect, result: {0}", result));
			return new DataSource(applicationId, defaultSourceId, messageHook, _log);
		}

		public List<DataSource> GetAllSources()
		{
			var sources = new List<DataSource>();
			var id = new Identity();

			// Get the first source
			var result = Twain32Native.DsmIdentity(
				ApplicationId,
				IntPtr.Zero,
				DataGroup.Control,
				DataArgumentType.Identity,
				Message.GetFirst,
				id);

			_log.Debug(string.Format("GetFirst (GetAllSources), result: {0}", result));
			if (result == TwainResult.EndOfList)
			{
				return sources;
			}
			if (result != TwainResult.Success)
			{
				throw new TwainException("Error getting first source.", result);
			}
			
			sources.Add(new DataSource(ApplicationId, id, _messageHook, _log));
			

			while (true)
			{
				// Get the next source
				result = Twain32Native.DsmIdentity(
					ApplicationId,
					IntPtr.Zero,
					DataGroup.Control,
					DataArgumentType.Identity,
					Message.GetNext,
					id);

				_log.Debug(string.Format("GetNext (GetAllSources), result: {0}", result));
				if (result == TwainResult.EndOfList)
				{
					break;
				}
				if (result != TwainResult.Success)
				{
					throw new TwainException("Error enumerating sources.", result);
				}

				sources.Add(new DataSource(ApplicationId, id, _messageHook, _log));
			}

			var sb = new StringBuilder("GetAllSources result: ");
			foreach (var dataSource in sources)
			{
				sb.Append(string.Format("sourceId: {0}; ", dataSource.SourceId.ProductName));
			}
			_log.Debug(sb);
			return sources;
		}

		public DataSource GetSource(string sourceProductName, Identity applicationId, IWindowsMessageHook messageHook)
		{
			// A little slower than it could be, if enumerating unnecessary sources is slow. But less code duplication.
			foreach (var source in GetAllSources())
			{
				if (sourceProductName.Equals(source.SourceId.ProductName, StringComparison.InvariantCultureIgnoreCase))
				{
					return source;
				}
			}

			return null;
		}




        ~DataSourceManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Notification that the scanning has completed.
        /// </summary>
        public event EventHandler<ScanningCompleteEventArgs> ScanningComplete;

        public event EventHandler<TransferImageEventArgs> TransferImage;

        public IWindowsMessageHook MessageHook { get { return _messageHook; } }

        public void StartScan(ScanSettings settings)
        {
            bool scanning = false;

            try
            {
                _messageHook.UseFilter = true;
                scanning = DataSource.Open(settings);
	        }
            catch (TwainException e)
            {
	            CloseDataSource();
                EndingScan();
                throw e;
            }
            finally
            {
                // Remove the message hook if scan setup failed
                if (!scanning)
                {
                    EndingScan();
                }
            }
        }

	    private void CloseDataSource()
	    {
			DataSource.DisableAndClose();
			if (DataSource.SourceState == null)
				_twainState = TwainState.SourceManagerOpen;
	    }
        protected IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (DataSource.SourceId.Id == 0)
            {
                handled = false;
                return IntPtr.Zero;
            }

            int pos = User32Native.GetMessagePos();

            var message = new WindowsMessage();
            message.hwnd = hwnd;
            message.message = msg;
            message.wParam = wParam;
            message.lParam = lParam;
            message.time = User32Native.GetMessageTime();
            message.x = (short)pos;
            message.y = (short)(pos >> 16);

            Marshal.StructureToPtr(message, _eventMessage.EventPtr, false);
            _eventMessage.Message = 0;

            TwainResult result = Twain32Native.DsEvent(
                ApplicationId,
                DataSource.SourceId,
                DataGroup.Control,
                DataArgumentType.Event,
                Message.ProcessEvent,
                ref _eventMessage);

			_log.Debug(string.Format("ProcessEvent, result: {0}", result));
            if (result == TwainResult.NotDSEvent)
            {
                handled = false;
                return IntPtr.Zero;
            }

            switch (_eventMessage.Message)
            {
                case Message.XFerReady:
                    Exception exception = null;
                    try
                    {
	                    DataSource.SourceState = TwainState.TransferReady;
                        TransferPictures();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    CloseDsAndCompleteScanning(exception);
                    break;

                case Message.CloseDS:
                case Message.CloseDSOK:
                case Message.CloseDSReq:
                    CloseDsAndCompleteScanning(null);
                    break;

                case Message.DeviceEvent:
                    break;

                default:
                    break;
            }

            handled = true;
            return IntPtr.Zero;
        }

        protected void TransferPictures()
        {
            if (DataSource.SourceId.Id == 0)
            {
                return;
            }

            var pendingTransfer = new PendingXfers();
            TwainResult result;
	        try
	        {
		        do
		        {
			        pendingTransfer.Count = 0;
			        var hbitmap = IntPtr.Zero;

			        // Get the image info
			        var imageInfo = new ImageInfo();
			        result = Twain32Native.DsImageInfo(
				        ApplicationId,
				        DataSource.SourceId,
				        DataGroup.Image,
				        DataArgumentType.ImageInfo,
				        Message.Get,
				        imageInfo);

					_log.Debug(string.Format("Get(ImageInfo), result: {0}", result));
			        if (result != TwainResult.Success)
			        {
				        CloseDataSource();
				        break;
			        }

			        // Transfer the image from the device
			        result = Twain32Native.DsImageTransfer(
				        ApplicationId,
				        DataSource.SourceId,
				        DataGroup.Image,
				        DataArgumentType.ImageNativeXfer,
				        Message.Get,
				        ref hbitmap);

					_log.Debug(string.Format("Get(ImageNativeXfer), result: {0}", result));

			        if (result != TwainResult.XferDone)
			        {
						_log.ErrorFormat("Transfer the image from the device failed. Result: {0}", result);
				        CloseDataSource();
				        break;
			        }

			        DataSource.SourceState = TwainState.Transfering;

			        // End pending transfers
			        result = Twain32Native.DsPendingTransfer(
				        ApplicationId,
				        DataSource.SourceId,
				        DataGroup.Control,
				        DataArgumentType.PendingXfers,
				        Message.EndXfer,
				        pendingTransfer);

					_log.Debug(string.Format("EndXfer(PendingXfers), result: {0}", result));
			        if (result != TwainResult.Success)
			        {
				        CloseDataSource();
				        break;
			        }

			        DataSource.SourceState = TwainState.TransferReady;

			        if (hbitmap == IntPtr.Zero)
			        {
				        _log.Warn("Transfer complete but bitmap pointer is still null.");
			        }
			        else
			        {
				        using (var renderer = new BitmapRenderer(hbitmap))
				        {
					        TransferImageEventArgs args = new TransferImageEventArgs(renderer.RenderToBitmap(),
						        pendingTransfer.Count != 0);
					        TransferImage(this, args);
					        if (!args.ContinueScanning)
						        break;
				        }
			        }
		        } while (pendingTransfer.Count != 0);
	        }
	        catch (Exception e)
	        {
		        
	        }
            finally
            {
                // Reset any pending transfers
                result = Twain32Native.DsPendingTransfer(
                    ApplicationId,
                    DataSource.SourceId,
                    DataGroup.Control,
                    DataArgumentType.PendingXfers,
                    Message.Reset,
                    pendingTransfer);

				
	            if (result == TwainResult.Success)
	            {
		            _twainState = TwainState.SourceEnabled;
					_log.Debug(string.Format("Reset(PendingXfers), result: {0}", result));
	            }
	            else
	            {
					var conditionCode = GetConditionCode(ApplicationId, DataSource.SourceId);
					_log.ErrorFormat("Reset(PendingXfers), result: {0}, condition code: {1}", result, conditionCode);		
	            }
            }
        }

        protected void CloseDsAndCompleteScanning(Exception exception)
        {
            EndingScan();
			CloseDataSource();
            try
            {
                ScanningComplete(this, new ScanningCompleteEventArgs(exception));
            }
            catch
            {
            }
        }

        protected void EndingScan()
        {
            _messageHook.UseFilter = false;
        }

        public void SelectSource()
        {
            DataSource.Dispose();
            DataSource = UserSelected(ApplicationId, _messageHook);
        }

        public void SelectSource(DataSource dataSource)
        {
            DataSource.Dispose();
            DataSource = dataSource;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Marshal.FreeHGlobal(_eventMessage.EventPtr);

            if (disposing)
            {
                DataSource.Dispose();

                IntPtr windowHandle = _messageHook.WindowHandle;

                if (ApplicationId.Id != 0)
                {
                    // Close down the data source manager
                    var result = Twain32Native.DsmParent(
                        ApplicationId,
                        IntPtr.Zero,
                        DataGroup.Control,
                        DataArgumentType.Parent,
                        Message.CloseDSM,
                        ref windowHandle);

					_log.Debug(string.Format("CloseDSM, result: {0}", result));

	                if (result != TwainResult.Failure)
	                {
		                _twainState = TwainState.SourceManagerLoaded;
	                }
                }

                ApplicationId.Id = 0;
            }
        }

        public static ConditionCode GetConditionCode(Identity applicationId, Identity sourceId)
        {
            Status status = new Status();

            Twain32Native.DsmStatus(
                applicationId,
                sourceId,
                DataGroup.Control,
                DataArgumentType.Status,
                Message.Get,
                status);

            return status.ConditionCode;
        }

        public static readonly Identity DefaultApplicationId = new Identity()
        {
            Id = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0),
            Version = new TwainVersion()
            {
                MajorNum = 1,
                MinorNum = 1,
                Language = Language.USA,
                Country = Country.USA,
                Info = Assembly.GetExecutingAssembly().FullName
            },
            ProtocolMajor = TwainConstants.ProtocolMajor,
            ProtocolMinor = TwainConstants.ProtocolMinor,
            SupportedGroups = (int)(DataGroup.Image | DataGroup.Control),
            Manufacturer = "TwainDotNet",
            ProductFamily = "TwainDotNet",
            ProductName = "TwainDotNet",
        };
    }
}
