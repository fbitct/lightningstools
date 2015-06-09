using System.Linq;
namespace F4Utils.SimSupport
{
	public interface IDoubleResolutionRTTChecker
	{
		bool IsDoubleResolutionRtt { get; }
	}

	public class DoubleResolutionRTTChecker : IDoubleResolutionRTTChecker
	{
		private readonly IBMSConfigFileReader _bmsConfigFileReader;

		public DoubleResolutionRTTChecker(IBMSConfigFileReader bmsConfigFileReader=null)
		{
			_bmsConfigFileReader = bmsConfigFileReader ?? new BMSConfigFileReader();
		}
		public bool IsDoubleResolutionRtt
		{
			get
			{
				return _bmsConfigFileReader
					.ConfigLines
					.Select(Common.Strings.Util.Tokenize)
					.Where(tokens => tokens.Count > 2)
					.Any(tokens => tokens[0].ToLowerInvariant() == "set"
						&& tokens[1].ToLowerInvariant() == "g_bDoubleRTTResolution".ToLowerInvariant()
						&& tokens[2].ToLowerInvariant() == "1".ToLowerInvariant());
			}
		}
	}
}
