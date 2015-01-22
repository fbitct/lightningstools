using System;
using System.Threading;
using System.Windows.Forms;
using Common.Win32;
using Microsoft.DirectX.DirectInput;
using log4net;
using MFDExtractor.EventSystem.Handlers;

namespace MFDExtractor.EventSystem
{
	internal interface IKeyboardWatcher
	{
		bool KeepRunning { get; set; }
		void Start();
	}

	class KeyboardWatcher : IKeyboardWatcher
	{
		public bool KeepRunning { get; set; }
		private readonly ILog _log;
		private readonly IKeyEventHandler _keyEventHandler;
		public KeyboardWatcher(IInputEvents inputEvents,ILog log = null)
		{
			_keyEventHandler = new KeyEventHandler(inputEvents);
			_log = log ??  LogManager.GetLogger(GetType());
		}

		public void Start()
		{
			KeepRunning = true;
			Device device = null;
			try
			{
				var resetEvent = new AutoResetEvent(false);
				device = new Device(SystemGuid.Keyboard);
				device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
				device.SetEventNotification(resetEvent);
				device.Properties.BufferSize = 255;
				device.Acquire();
				var lastKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
				var currentKeyboardState = new bool[Enum.GetValues(typeof(Key)).Length];
				while (KeepRunning)
				{
					try
					{
						resetEvent.WaitOne(50);
					}
					catch (TimeoutException)
					{
					}
					try
					{
						var curState = device.GetCurrentKeyboardState();
						var possibleKeys = Enum.GetValues(typeof(Key));

						var i = 0;
						foreach (Key thisKey in possibleKeys)
						{
							currentKeyboardState[i] = curState[thisKey];
							i++;
						}

						i = 0;
						foreach (Key thisKey in possibleKeys)
						{
							var isPressedNow = currentKeyboardState[i];
							var wasPressedBefore = lastKeyboardState[i];
							var winFormsKey =(Keys)NativeMethods.MapVirtualKey((uint)thisKey, NativeMethods.MAPVK_VSC_TO_VK_EX);
							if (isPressedNow && !wasPressedBefore)
							{
								_keyEventHandler.ProcessKeyDownEvent(new KeyEventArgs(winFormsKey));
							}
							else if (wasPressedBefore && !isPressedNow)
							{
								_keyEventHandler.ProcessKeyUpEvent(new KeyEventArgs(winFormsKey));
							}
							i++;
						}
						Array.Copy(currentKeyboardState, lastKeyboardState, currentKeyboardState.Length);
					}
					catch (Exception e)
					{
						_log.Debug(e.Message, e);
					}
				}
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception e)
			{
				_log.Error(e.Message, e);
			}
			finally
			{
				if (device != null)
				{
					device.Unacquire();
				}
				Common.Util.DisposeObject(device);
			}
		}
	}
}
