using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rush
{
    public unsafe struct PinnedRc<T> where T: struct
    {
        unsafe T* _pValue;
        unsafe Int64* _pRc;
        public unsafe T* Obj { get => _pValue; }

        public PinnedRc(T* pValue, Int64* pRc)
        {
            this._pRc = pRc;
            this._pValue = pValue;
        }

        public Rc<T> ToRc()
        {
            var rc = new Rc<T>(_pValue);
            _pRc[0]++;
            return rc;
        }

        public unsafe void Dispose()
        {
            if (_pRc == null) return;
            _pRc[0]--;
            Console.WriteLine($"Current Rc: {_pRc[0]}");
            if (_pRc[0] == 0)
            {
                Console.WriteLine($"Release item, Rc: {_pRc[0]}");
                Marshal.FreeHGlobal((IntPtr)_pRc);
            }
            _pRc = null;
            _pValue = null;
        }
    }

    public ref struct Rc<T> where T : struct
    {
        unsafe T* _pValue;
        unsafe Int64* _pRc;

        public unsafe T* Obj { get => _pValue; }

        public unsafe Rc()
        {
            _pRc = (Int64*)Marshal.AllocHGlobal(sizeof(T) + 8);
            _pValue = (T*)(_pRc + 1);
            _pRc[0]++;
            Console.WriteLine($"Alloc item, Rc: {_pRc[0]}");
        }

        public unsafe PinnedRc<T> Pin()
        {
            var prc = new PinnedRc<T>(_pValue, _pRc);
            _pRc[0]++;
            return prc;
        }

        public unsafe Rc(T* pValue)
        {
            if (pValue == null)
            {
                // error
            }

            _pValue = pValue;
            _pRc = (Int64*)((Byte*)pValue - 8);
            _pRc[0]++;
        }

        public unsafe void Dispose()
        {
            if (_pRc == null) return;
            _pRc[0]--;
            Console.WriteLine($"Current Rc: {_pRc[0]}");
            if (_pRc[0] == 0)
            {
                Console.WriteLine($"Release item, Rc: {_pRc[0]}");
                Marshal.FreeHGlobal((IntPtr)_pRc);
            }
            _pRc =  null;
            _pValue = null;
        }
    }

    public ref struct Rc2<T> where T : struct, IDisposable
    {
        unsafe T* _pValue;
        unsafe Int64* _pRc;

        public unsafe T* Obj { get => _pValue; }

        public unsafe Rc2()
        {
            _pRc = (Int64*)Marshal.AllocHGlobal(sizeof(T) + 8);
            _pValue = (T*)(_pRc + 1);
            _pRc[0]++;
            Console.WriteLine($"Alloc item, Rc: {_pRc[0]}");
        }

        public unsafe PinnedRc<T> Pin()
        {
            var prc = new PinnedRc<T>(_pValue, _pRc);
            _pRc[0]++;
            return prc;
        }

        public unsafe Rc2(T* pValue)
        {
            if (pValue == null)
            {
                // error
            }

            _pValue = pValue;
            _pRc = (Int64*)((Byte*)pValue - 8);
            _pRc[0]++;
        }

        public unsafe void Dispose()
        {
            if (_pRc == null) return;
            _pRc[0]--;
            Console.WriteLine($"Current Rc: {_pRc[0]}");
            if (_pRc[0] == 0)
            {
                Console.WriteLine($"Release item, Rc: {_pRc[0]}");
                Marshal.FreeHGlobal((IntPtr)_pRc);
            }
            _pRc = null;
            _pValue = null;
        }
    }
}
