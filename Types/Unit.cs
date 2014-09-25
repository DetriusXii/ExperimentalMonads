using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HodiaInCSharp.Types {
    public sealed class Unit {
        private static Unit instance = new Unit();

        public static Unit Instance {
            get {
                return instance;
            }
        }

        private Unit() {
        }
    }
}
