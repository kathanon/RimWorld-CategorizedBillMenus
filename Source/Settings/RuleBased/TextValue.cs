using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public abstract class TextValue : RegisterableById<TextValue> {
        public static string AlwaysMatchMarker = TextOperation.AlwaysMatchMarker;

        static TextValue() {
            Register(Values.Recipe());
            Register(Values.Limb());
        }

        public TextValue(string name, string id, string description) 
            : base(name, id, description) {}


        public abstract string Get(BillMenuEntry entry);

        public abstract string Get(BillMenuEntry entry, MenuNode parent);

        public virtual bool Compare(TextOperation comparison, BillMenuEntry entry, string expected) 
            => comparison.Compare(Get(entry), expected);

        public virtual bool Compare(TextOperation comparison, BillMenuEntry entry, MenuNode parent, string expected) 
            => comparison.Compare(Get(entry, parent), expected);

        public virtual void Setup() {}

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) {}

        public virtual string SettingsClosedLabel => Name;
    }
}
