//
// Mono.ILAsm.Driver
//    Main Command line interface for Mono ILAsm Compiler
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//  Alex Rønne Petersen <xtzgzorex@gmail.com>
//
// (C) 2003 Jackson Harper, All rights reserved
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Mono.Security;

namespace Mono.ILAsm {
	public sealed class Driver {
		private readonly List<string> il_file_list = new List<string> ();
		private bool show_tokens;
		private bool show_parser;
		private bool scan_only;
		private bool debugging_info;
		private bool key_container;
		private string key_name;
		
		public string OutputFileName { get; set; }
		
		public Target Target { get; set; }
		
		public TextWriter Output { get; set; }
		
		public Driver ()
		{
			Output = Console.Out;
		}

		public ExitCode? Run (string[] args)
		{
			if (!ParseArgs (args))
				return null;
			
			if (il_file_list.Count == 0) {
				Usage (false);
				return ExitCode.Error;
			}

			if (OutputFileName == null)
				OutputFileName = CreateOutputFileName (il_file_list, Target);
			
			var codegen = new CodeGenerator (OutputFileName, Target, debugging_info);
			StrongName sn = null;
			
			try {
				foreach (var file_path in il_file_list) {
					var fileResult = ProcessFile (codegen, file_path);
					if (fileResult != ExitCode.Success)
						return fileResult;
				}
				
				if (scan_only)
					return 0;
				if (Target != Target.Dll && !codegen.HasEntryPoint)
					Report.WriteError (Error.NoEntryPoint, "No entry point found.");
				
				if (key_name != null) {
					sn = LoadKey (key_container, key_name);
					// this overrides any attribute or .publickey directive in the source
					codegen.CurrentModule.Assembly.Name.PublicKey = sn.PublicKey;
				}
				
				codegen.Write (OutputFileName);
				
				if (sn != null)
					try {
						Report.WriteMessage ("Signing assembly with the specified strong name key pair...");
						Sign (sn, OutputFileName);
					} catch (Exception ex) {
						Report.WriteError (Error.SigningFailed, "Could not sign assembly: {0}",
							ex.Message);
						return ExitCode.Error;
					}
			} catch (ILAsmException ie) {
				WriteError (ie.ToString ());
				return ExitCode.Error;
			} catch (Exception ex) {
				// An internal error has occurred...
				WriteError ("{0}{1}{2}", ex.ToString (), Environment.NewLine, ex.StackTrace);
				return ExitCode.Error;
			}

			return ExitCode.Success;
		}

		private ExitCode ProcessFile (CodeGenerator codegen, string filePath)
		{
			if (!File.Exists (filePath)) {
				Report.WriteError (Error.FileNotFound, "File does not exist: {0}", filePath);
				return ExitCode.Abort;
			}
			
			Report.WriteMessage ("Assembling {0} to {1} -> {2}...",
				filePath, Target.ToString ().ToUpper (), OutputFileName);
			Report.WriteMessage (string.Empty);
			
			var reader = File.OpenText (filePath);
			var scanner = new ILTokenizer (reader);
			
			Report.FilePath = filePath;
			Report.Tokenizer = scanner;

			if (show_tokens)
				scanner.NewToken += ShowToken;

			if (scan_only) {
				ILToken tok;
				while ((tok = scanner.GetNextToken ()) != ILToken.EOF)
					Console.WriteLine (tok);
				return ExitCode.Success;
			}

			var parser = new ILParser (codegen, scanner);
			try {
				parser.yyparse (new ScannerAdapter (scanner),
					show_parser ? new yydebug.yyDebugSimple () : null);
			} catch (ILTokenizingException ilte) {
				Report.WriteError (Error.SyntaxError, ilte.Location, "Syntax error at token '" + ilte.Token + "'.");
			} catch (yyParser.yyException ye) {
				Report.WriteError (Error.SyntaxError, scanner.Reader.Location, "Syntax error: " + ye.Message);
			} finally {
				Report.Tokenizer = null;
				Report.FilePath = null;
			}
			
			return ExitCode.Success;
		}

		private bool ParseArgs (string[] args)
		{
			string command_arg;
			foreach (var str in args) {
				if ((str [0] != '-') && (str [0] != '/')) {
					il_file_list.Add (str);
					continue;
				}
				
				var cmd = GetCommand (str, out command_arg);
				switch (cmd) {
				case "out":
				case "outp":
				case "outpu":
				case "output":	
					OutputFileName = command_arg;
					break;
				case "exe":
					Target = Target.Exe;
					break;
				case "dll":
					Target = Target.Dll;
					break;
				case "qui":
				case "quie":
				case "quiet":
					Report.Quiet = true;
					break;
				case "deb":
				case "debu":
				case "debug":
					// TODO: support impl and opt
					debugging_info = true;
					break;
				case "nol":
				case "nolo":
				case "nolog":
				case "nologo":
					break; // We don't print a logo...
				// Stubs to stay command line compatible with MS.
				case "noautoinherit":
				case "nocorstub":
				case "stripreloc":
				case "clock":
				case "error":
				case "subsystem":
				case "flags":
				case "alignment":
				case "base":
				case "resource":
				case "pdb":
				case "optimize":
				case "fold":
				case "include":
				case "stack":
				case "enc":
				case "mdv":
				case "msv":
				case "itanium":
				case "x64":
				case "pe64":
					Report.WriteWarning (Warning.InternalWarning,
						"Unimplemented command line option: {0}", cmd);
					break;
				case "key":
					if (command_arg.Length > 0)
						key_container = (command_arg [0] == '@');

					if (key_container)
						key_name = command_arg.Substring (1);
					else
						key_name = command_arg;

					break;
				case "?":
					Usage (false);
					return false;
				case "mono_?":
					Usage (true);
					return false;
				case "mono_scanonly":
					scan_only = true;
					break;
				case "mono_showtokens":
					show_tokens = true;
					break;
				case "mono_showparser":
					show_parser = true;
					break;
				case "-about":
					if (str [0] != '-')
						break;
					
					About ();
					return false;
				case "-version":
					if (str [0] != '-')
						break;

					Version ();
					return false;
				default:
					if (str [0] == '-')
						break;

					il_file_list.Add (str);
					break;
				}
			}
			
			return true;
		}
		
		private static string CreateOutputFileName (IList<string> ilFileList, Target target)
		{
			var file_name = ilFileList [0];
			var ext_index = file_name.LastIndexOf ('.');

			if (ext_index == -1)
				ext_index = file_name.Length;

			return file_name.Substring (0, ext_index) +
				(target == Target.Dll ? ".dll" : ".exe");
		}

		private static void WriteError (string message, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Report.ErrorOutput.WriteLine (string.Format (message, args));
			Report.ErrorOutput.WriteLine ("***** FAILURE *****");
			Console.ResetColor ();
		}

		private static StrongName LoadKey (bool keyContainer, string keyName)
		{
			StrongName sn;
			
			if (keyContainer) {
				var csp = new CspParameters ();
				csp.KeyContainerName = keyName;
				var rsa = new RSACryptoServiceProvider (csp);
				
				sn = new StrongName (rsa);
			} else {
				byte[] data = null;
				using (var fs = File.OpenRead (keyName)) {
					data = new byte [fs.Length];
					fs.Read (data, 0, data.Length);
					fs.Close ();
				}
				
				sn = new StrongName (data);
			}
			
			return sn;
		}

		private static bool Sign (StrongName sn, string fileName)
		{
			// note: if the file cannot be signed (no public key in it) then
			// we do not show an error, or a warning, if the key file doesn't
			// exist
			return sn.Sign (fileName);
		}

		private static void ShowToken (object sender, NewTokenEventArgs args)
		{
			Console.WriteLine ("Token: {0}", args.Token);
		}

		private static string GetCommand (string str, out string command_arg)
		{
			var end_index = str.IndexOfAny (new char[] { ':', '=' }, 1);
			var command = str.Substring (1, (end_index == -1 ? str.Length : end_index) - 1);

			if (end_index != -1)
				command_arg = str.Substring (end_index + 1);
			else
				command_arg = null;

			return command.ToLower ();
		}

		private void Usage (bool dev)
		{
			var n = Environment.NewLine;
			
			Output.WriteLine ("Mono IL Assembler{0}" +
				"ilasm [options] <source files>{0}" +
				"   --about             About the Mono ILAsm compiler.{0}" +
				"   --version           Print the version number of the Mono ILAsm compiler.{0}" +
				"   /output:file_name   Specifies output file.{0}" +
				"   /exe                Compile to executable.{0}" +
				"   /dll                Compile to library.{0}" +
				"   /debug              Include debug information.{0}" +
				"   /key:key_file       Strong name using the specified key file.{0}" +
				"   /key:@key_container Strong name using the specified key container.{0}" +
				"Options can be of the form -option or /option.{0}",
				n);
			
			if (!dev)
				Output.WriteLine ("Pass /mono_? for developer options.", n);
			else
				Output.WriteLine ("Developer options:{0}" +
					"   /mono_scanonly      Only perform tokenization.{0}" +
					"   /mono_showtokens    Show tokens as they're scanned.{0}" +
					"   /mono_showparser    Show parser debug output.{0}",
					n);
		}

		private void About ()
		{
			Output.WriteLine ("For more information on Mono, visit the project Web site{0}" +
				"   http://www.go-mono.com", Environment.NewLine);
		}

		private void Version ()
		{
			var version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			Output.WriteLine ("Mono IL Assembler version {0}", version);
		}
	}
}
