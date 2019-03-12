using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AutoUnitTestingCore.Compilers;
using log4net;
using log4net.Core;
using NDesk.Options;


namespace AutoUnitTestingCore
{
    class Program
    {
        private static string _inputFile;
        private static bool _isVerboseModeOn;
        private static bool _isForceRewrite;
        private static bool _isDryRun;
        private static string _pathToExecutable;
        private static string _stringInput;
        private static string _pathToOutputData;
        private static string _pathToOutputAnswer;
        private static string _onProcessError;
        private static string _executableArgs;
        private static int _testCount;
        private static int _startIndex;
        private static string _cultureName = "";

        private const string FILE_PATH_INDEX_VAR_NAME = "$INDEX$";

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static readonly OptionSet InputArgs = new OptionSet()
        {
            { "f|input-file=", v =>  _inputFile = v},
            { "s|input-string=", v =>  _stringInput = v},
            { "e|executable=", v => _pathToExecutable = v},
            { "executable-args=", v => _executableArgs = v},
            { "c|test-count=", v => _testCount = Convert.ToInt32(v)},
            { "v|verbose", v => _isVerboseModeOn = true},
            { "o|output-data=", v => _pathToOutputData = v},
            { "a|output-answer=", v => _pathToOutputAnswer = v},
            { "start-index=", v => _startIndex = Convert.ToInt32(v)},
            { "force-rewrite", v => _isForceRewrite = true},
            { "dry-run", v => _isDryRun = true},
            { "on-error=", v => _onProcessError = v},
            { "culture=", v => _cultureName= v},
        };

        public static CultureInfo _cultureInfo;

        static void Main(string[] args)
        {
            List<string> parsedArgs = InputArgs.Parse(args);
            log4net.Config.XmlConfigurator.Configure();
            LogManager.GetRepository().Threshold = (_isVerboseModeOn) ? Level.All : Level.Fatal;

            LogArgs();

            AllChecksBeforeStart();

            Log.Info("App started");

            IInputProvider inputProvider;
            if (_inputFile != null)
                inputProvider = new FileInputProvider(_inputFile);
            else
                inputProvider = new StringInputProvider(_stringInput);

            for (int i = _startIndex; i < _testCount + _startIndex; i++)
            {
                string compiledDataInput = Compiler.Compile(inputProvider, _cultureInfo);

                Log.Info($"Writing to output-data (the start on next line):\n{compiledDataInput}");

                string runnerOutput = "";
                try
                {
                    if (!_isDryRun)
                        runnerOutput = Run(compiledDataInput);
                }
                catch (RunException e)
                {
                    Log.Error($"Process execution error. {e.Message}");
                    string answer = _onProcessError ?? ConsoleAsk.GetAnswer("What to do next: continue or exit? Enter c or e: ", new[] { "e", "c" });
                    if (answer == "e")
                        Environment.Exit(ErrorCodes.UserExitOnProcessError);
                    else if (answer == "c")
                        continue;
                }

                if (!_isDryRun)
                {
                    string realPathToOutputData = _pathToOutputData.Replace(FILE_PATH_INDEX_VAR_NAME, i.ToString());
                    Directory.CreateDirectory(Path.GetDirectoryName(realPathToOutputData));
                    File.WriteAllText(realPathToOutputData, compiledDataInput);
                }

                if (!_isDryRun)
                {
                    string realPathToOutputAnswer = _pathToOutputAnswer.Replace(FILE_PATH_INDEX_VAR_NAME, i.ToString());
                    Directory.CreateDirectory(Path.GetDirectoryName(realPathToOutputAnswer));
                    File.WriteAllText(realPathToOutputAnswer, runnerOutput);
                }

                Log.Info($"Got answer: {runnerOutput}");
            }
        }

        private static void AllChecksBeforeStart()
        {
            if (!CheckRequiredParameters())
            {
                Environment.Exit(ErrorCodes.RequiredParameter);
            }

            if (_testCount > 1 && !CheckOutputPaths())
            {
                Environment.Exit(ErrorCodes.CountGt1NoIndexInOutputPath);
            }

            if (!CheckOutPathExistence())
            {
                Environment.Exit(ErrorCodes.FilesExists);
            }

            if (!CheckParameterValues())
            {
                Environment.Exit(ErrorCodes.ParameterValueError);
            }
        }

        static void LogArgs()
        {
            Log.Debug($"ARG input-file = {_inputFile}");
            Log.Debug($"ARG input-string = {_stringInput}");
            Log.Debug($"ARG executable = {_pathToExecutable}");
            Log.Debug($"ARG executable-args = {_executableArgs}");
            Log.Debug($"ARG test-count = {_testCount}");
            Log.Debug($"ARG verbose = {_isVerboseModeOn}");
            Log.Debug($"ARG output-data = {_pathToOutputData}");
            Log.Debug($"ARG output-answer = {_pathToOutputAnswer}");
            Log.Debug($"ARG force-rewrite = {_isForceRewrite}");
            Log.Debug($"ARG on-error = {_onProcessError}");
            Log.Debug($"ARG dry-run = {_isDryRun}");
            Log.Debug($"ARG start-index = {_startIndex}");
            Log.Debug($"ARG culture = {_cultureName}");
        }

        static bool CheckOutputPaths()
        {
            if (!_pathToOutputData.Contains(FILE_PATH_INDEX_VAR_NAME))
            {
                Log.Fatal($"--output-data parameter does not contain {FILE_PATH_INDEX_VAR_NAME} variable but test-count > 1");
                return false;
            }
            if (!_pathToOutputAnswer.Contains(FILE_PATH_INDEX_VAR_NAME))
            {
                Log.Fatal($"--output-answer parameter does not contain {FILE_PATH_INDEX_VAR_NAME} variable but test-count > 1");
                return false;
            }

            return true;
        }

        static bool CheckOutPathExistence()
        {
            if (_isForceRewrite)
                return true;

            for (int i = _startIndex; i < _testCount + _startIndex; i++)
            {
                string realPathToOutputData = _pathToOutputData.Replace(FILE_PATH_INDEX_VAR_NAME, i.ToString());
                if (File.Exists(realPathToOutputData))
                {
                    Log.Fatal($"File {realPathToOutputData} exists. Use --force-rewrite to rewrite it or change --output-data");
                    return false;
                }

                string realPathToOutputAnswer = _pathToOutputAnswer.Replace(FILE_PATH_INDEX_VAR_NAME, i.ToString());
                if (File.Exists(realPathToOutputAnswer))
                {
                    Log.Fatal($"File {realPathToOutputAnswer} exists. Use --force-rewrite to rewrite it or change --output-answer");
                    return false;
                }
            }

            return true;
        }

        static bool CheckRequiredParameters()
        {
            if (_pathToOutputAnswer == null)
            {
                Log.Fatal("Required --output-answer parameter not specified");
                return false;
            }

            if (_pathToOutputData == null)
            {
                Log.Fatal("Required --output-data parameter not specified");
                return false;
            }

            if (_pathToExecutable == null)
            {
                Log.Fatal("Required --executable parameter not specified");
                return false;
            }

            return true;
        }

        static bool CheckParameterValues()
        {
            if (_onProcessError != null)
            {
                if (!new[] {"e", "c"}.Contains(_onProcessError))
                {
                    Log.Fatal($"Error value for --on-error parameter. Expected e or c, got {_onProcessError}");
                    return false;
                }
            }

            _testCount = (_testCount == 0) ? 1 : _testCount;
            if (_testCount < 0)
            {
                Log.Fatal($"Error value for --test-count parameter. Expected positive integer, got {_testCount}");
                return false;
            }

            try
            {
                _cultureInfo = CultureInfo.GetCultureInfo(_cultureName);
            }
            catch (Exception e)
            {
                Log.Fatal($"Error value for --culture parameter. Exception message: {e.Message}");
                return false;
            }

            return true;
        }

        static string Run(string standardInput)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = _pathToExecutable;
                process.StartInfo.Arguments = _executableArgs;
                process.StartInfo.CreateNoWindow = true; ;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                process.StandardInput.WriteLine(standardInput);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new RunException($"Process exit code is {process.ExitCode}.\nProcess stderr (the start on next line):\n{process.StandardError.ReadToEnd()}");
                }

                return process.StandardOutput.ReadToEnd();
            }
        }
    }

    public abstract class ErrorCodes
    {
        public const int RequiredParameter = 1;
        public const int CountGt1NoIndexInOutputPath = 2;
        public const int FilesExists = 3;
        public const int LuaVariableAlreadyExists = 4;
        public const int UserExitOnProcessError = 5;
        public const int ParameterValueError = 6;
        public const int LuaVariableNotExists = 7;
    }

    public class RunException : Exception
    {
        public RunException()
        {
        }

        public RunException(string message) : base(message)
        {
        }

        public RunException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RunException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class ConsoleAsk
    {
        public static string GetAnswer(string question, string[] expectedAnswers)
        {
            string answer;
            do
            {
                Console.Write(question);
                answer = Console.ReadLine();
            } while (!expectedAnswers.Contains(answer));

            return answer;
        }
    }
}
