using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace rush
{
    public unsafe struct Pinned<T> where T : struct
    {
        unsafe T* _pValue;
        public unsafe T* Obj { get => _pValue; }

        public Pinned(T* pValue)
        {
            this._pValue = pValue;
        }

        public unsafe void Dispose()
        {
            if (_pValue == null) return;
            Console.WriteLine($"Release item by Pinned");
            Marshal.FreeHGlobal((IntPtr)_pValue);
            _pValue = null;
        }
    }

    public unsafe ref struct Auto<T> where T : struct
    {
        unsafe T* _pValue;
        public unsafe T* Obj { get => _pValue; }

        public unsafe Auto()
        {
            _pValue = (T*)Marshal.AllocHGlobal(sizeof(T));
            Console.WriteLine($"Alloc item by Auto");
        }

        public Pinned<T> Pin()
        {
            Pinned<T> p = new Pinned<T>(_pValue);
            _pValue = null;
            return p;
        }

        public unsafe void Dispose()
        {
            if (_pValue == null) return;
            Marshal.FreeHGlobal((IntPtr)_pValue);
            Console.WriteLine($"Release item by Auto");
            _pValue = null;
        }
    }
}
