//
// SetEnv.cs: Task for setting environment variables.
// 
// For an overview of the SetEnv options, you can check:
//  http://msdn.microsoft.com/en-us/library/ee862485.aspx
//
// Author:
//   João Matos (triton@vapor3d.org)
// 
// (C) 2011 João Matos
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks.Cpp
{
	public class SetEnv : Task
	{
		public SetEnv()
		{
		}

		public override bool Execute()
		{
			EnvironmentVariableTarget target = EnvironmentVariableTarget.User;

			if ((Target != null) && Target.Length > 0)
				target = (EnvironmentVariableTarget) Enum.Parse(typeof(EnvironmentVariableTarget), Target);

			string env = Environment.GetEnvironmentVariable(Name, target);

			if(Value.Length == 0) {
				if (env != null)
					Environment.SetEnvironmentVariable(Name, null);
				return true;
			}

			if (env == null)
				env = String.Empty;

			string finalValue;

			if (Prefix && (env.Length > 0))
				finalValue = Value + ";" + env;
			else
				finalValue = Value;

			Environment.SetEnvironmentVariable(Name, finalValue, target);

			OutputEnvironmentVariable = finalValue;

			return true;
		}

		[Required]
		public string Name {
			get;
			set;
		}

		[Output]
		public string OutputEnvironmentVariable {
			get;
			set;
		}

		public bool Prefix {
			get;
			set;
		}

		public string Target {
			get;
			set;
		}

		public string Value {
			get;
			set;
		}
	}
}

#endif
