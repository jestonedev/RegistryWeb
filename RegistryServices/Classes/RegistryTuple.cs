using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RegistryServices.Classes
{
    public class RegistryTuple<T1, T2>
    {
        [NonSerialized]
        private Tuple<T1, T2> tuple;

        public T1 Item1 {
            get {
                return tuple.Item1;
            }
            set {
                tuple = new Tuple<T1, T2>(value, tuple.Item2);
            }
        }
        public T2 Item2
        {
            get
            {
                return tuple.Item2;
            }
            set {
                tuple = new Tuple<T1, T2>(tuple.Item1, value);
            }
        }

        public RegistryTuple()
        {
            tuple = new Tuple<T1, T2>(default(T1), default(T2));
        }

        public RegistryTuple(T1 item1, T2 item2)
        {
            tuple = new Tuple<T1, T2>(item1, item2);
        }

        public override bool Equals(object obj) {
            return tuple.Equals(obj);
        }

        public override int GetHashCode() {
            return tuple.GetHashCode();
        }

        public override string ToString() {
            return tuple.ToString();
        }
    }
}
