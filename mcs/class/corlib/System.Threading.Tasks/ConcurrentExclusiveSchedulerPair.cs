// ConcurrentExclusiveSchedulerPair.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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
//
//

#if NET_4_0 || MOBILE

using System;

namespace System.Threading.Tasks
{
	public class ConcurrentExclusiveSchedulerPair : IDisposable
	{
		public ConcurrentExclusiveSchedulerPair ()
		{
			throw new NotImplementedException ();
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler)
		{
			throw new NotImplementedException ();
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler, int maxConcurrencyLevel)
		{
			throw new NotImplementedException ();
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler, int maxConcurrencyLevel, int maxItemsPerStack)
		{
			throw new NotImplementedException ();
		}

		public void Complete ()
		{
			throw new NotImplementedException ();
		}

		public TaskScheduler ConcurrentScheduler {
			get {
				throw new NotImplementedException ();
			}
		}

		public TaskScheduler ExclusiveScheduler {
			get {
				throw new NotImplementedException ();
			}
		}

		public Task Completion {
			get {
				throw new NotImplementedException ();
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif