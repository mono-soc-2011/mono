// 
// DisassemblerBase.cs
//  
// Author:
//       Alex Rønne Petersen <xtzgzorex@gmail.com>
// 
// Copyright (c) 2011 Alex Rønne Petersen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;

namespace Mono.ILDasm {
	internal abstract class DisassemblerBase {
		public CodeWriter Writer { get; private set; }
		
		protected DisassemblerBase (TextWriter output)
		{
			Writer = new CodeWriter (output);
		}
		
		public static string Escape (string identifier)
		{
			// Since keywords in ILAsm don't have any odd symbols, we
			// can just escape them with apostrophes.
			if (KeywordTable.Keywords.ContainsKey (identifier))
				return "'" + identifier + "'";
			
			if (!IsIdentifierStartChar (identifier [0]))
				return "'" + identifier.Replace ("'", "\\'").Replace ("\\", "\\\\") + "'";
			
			foreach (var chr in identifier)
				if (!IsIdentifierChar (chr))
					return "'" + identifier.Replace ("'", "\\'").Replace ("\\", "\\\\") + "'";
			
			return identifier;
		}
		
		// TODO: These methods may not quite be in line with MS.NET...
		
		public static bool IsIdentifierStartChar (char chr)
		{
			return char.IsLetter (chr) || "_$@?`".IndexOf (chr) != -1;
		}
		
		public static bool IsIdentifierChar (char chr)
		{
			return char.IsLetterOrDigit (chr) || "_$@?`.".IndexOf (chr) != -1;
		}
	}
}
