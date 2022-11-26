using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public struct CheckboxValue {
        public bool Value;
        public readonly bool Old;

        public CheckboxValue(bool val) => Value = Old = val;

        public bool Changed => Value != Old;

        public static implicit operator CheckboxValue(bool val) => new CheckboxValue(val);

        public static implicit operator bool(CheckboxValue val) => val.Value;
    }
}
