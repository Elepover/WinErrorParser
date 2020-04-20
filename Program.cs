using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp_NetCore
{
    public partial class Program
    {
        readonly static Regex patternFacilityCode = new Regex(@"_(?<facilityName>[A-Z0-9]+) +(?<facilityCode>\d+)");
        readonly static Regex patternWinErrorCodeOld = new Regex(@"// MessageText:\r?\n//\r?\n(?<message>(?://.*\n)+)//\r?\n#define (?<code>[A-Z_]+) +(?<hResult>\d+)L");
        readonly static Regex patternWinErrorCodeNew = new Regex(@"// MessageText:\r?\n//\r?\n(?<message>(?://.*\n)+)//\r?\n#define (?<code>[A-Z_]+)[ A-Z_]+_\(0x(?<hResult>[\dA-F]+)L\)");

        readonly static StreamReader reader = new StreamReader("winerror.h");
        readonly static Version version = new Version("1.0.0.0");
        readonly static Stopwatch sw = new Stopwatch();
        readonly static Stopwatch overallSw = new Stopwatch();
        readonly static List<FacilityCode> facilityCodes = new List<FacilityCode>();
        readonly static List<WinError> winErrorCodesOldHResult = new List<WinError>();
        readonly static List<WinError> winErrorCodesNewHResult = new List<WinError>();

        static string currentLine;
        public static async Task Main(string[] args)
        {
            // winerror.h parser
            OutLine("parsing...");
            overallSw.Start();
            sw.Start();

            // find the following pattern:
            // // Define the facility codes
            // //
            // #define FACILITY_NULL                    0
            while (await reader.ReadLineAsync() != "// Define the facility codes") { }
            await reader.ReadLineAsync();
            
            while (!string.IsNullOrEmpty(currentLine = await reader.ReadLineAsync()))
            {
                var name = patternFacilityCode.Match(currentLine).Groups["facilityName"].Value;
                var code = int.Parse(patternFacilityCode.Match(currentLine).Groups["facilityCode"].Value);
                Animate("facility", name, code);
                facilityCodes.Add(new FacilityCode(name, code));
            }
            OutLine("");
            OutLine($"{facilityCodes.Count} facility codes parsed, time: {Math.Round(sw.Elapsed.TotalSeconds, 2)}s, overall {Math.Round(overallSw.Elapsed.TotalSeconds, 2)}s");
            sw.Restart();

            // find the following pattern:
            //
            // MessageId: ERROR_SUCCESS
            //
            // MessageText:
            //
            // The operation completed successfully.
            //
            //#define ERROR_SUCCESS                    0L

            // read all the rest stuff, until we encounter this:
            // // Error definitions follow
            var allNonTypedefErrors = new StringBuilder();
            while ((currentLine = await reader.ReadLineAsync()) != "// Error definitions follow")
                allNonTypedefErrors.Append(currentLine + "\r\n");
            // feed them into regex
            foreach (Match match in patternWinErrorCodeOld.Matches(allNonTypedefErrors.ToString()))
            {
                var message = match.Groups["message"].Value.Replace("// ", "").TrimEnd('\n').TrimEnd('\r');
                var code = match.Groups["code"].Value;
                var hResult = int.Parse(match.Groups["hResult"].Value);
                Animate("win32", code, hResult);
                winErrorCodesOldHResult.Add(new WinError(message, code, hResult));
            }
            OutLine("");
            OutLine($"{winErrorCodesOldHResult.Count} old HResults parsed, time: {Math.Round(sw.Elapsed.TotalSeconds, 2)}s, overall {Math.Round(overallSw.Elapsed.TotalSeconds, 2)}s");
            sw.Restart();

            // and this:
            //
            // MessageId: E_UNEXPECTED
            //
            // MessageText:
            //
            // Catastrophic failure
            //
            //#define E_UNEXPECTED                     _HRESULT_TYPEDEF_(0x8000FFFFL)

            // read all the rest stuff
            var allTypedefErrors = new StringBuilder();
            while ((currentLine = await reader.ReadLineAsync()) != "#endif//_WINERROR_")
                allTypedefErrors.Append(currentLine + "\r\n");
            foreach (Match match in patternWinErrorCodeNew.Matches(allTypedefErrors.ToString()))
            {
                var message = match.Groups["message"].Value.Replace("// ", "").TrimEnd('\n').TrimEnd('\r');
                var code = match.Groups["code"].Value;
                var hResult = int.Parse(match.Groups["hResult"].Value, NumberStyles.HexNumber);
                Animate("win32", code, hResult);
                winErrorCodesNewHResult.Add(new WinError(message, code, hResult));
            }
            OutLine("");
            OutLine($"{winErrorCodesNewHResult.Count} new HResults parsed, time: {Math.Round(sw.Elapsed.TotalSeconds, 2)}s, overall {Math.Round(overallSw.Elapsed.TotalSeconds, 2)}s");
            sw.Restart();

            OutLine("parse success, generating markdown document...");
            ParseToMarkdown();
            OutLine($"markdown generated to output.md, time: {Math.Round(sw.Elapsed.TotalSeconds, 2)}s, overall {Math.Round(overallSw.Elapsed.TotalSeconds, 2)}s");
        }
    }
}
