using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static CategorizedBillMenus.ScribeUtils;

namespace CategorizedBillMenus {
    public interface IDefValueGetter<T> : IExposable where T : Def {
        public string Name { get; }
        public string ID { get; }
        public string SettingsClosedLabel { get; }
        public bool Compare(TextOperation comparison, T def, string expected);
        public void DoSettings(WidgetRow row, Rect rect, ref float curY);
        public void CopyToGetter(IDefValueGetter<T> other);
        public void Setup();
    }

    public class DefValueGetter<T> : IDefValueGetter<T> where T : Def {
        private readonly string name;
        private readonly string id;
        private readonly Func<T, string> get;

        public DefValueGetter(string name, string id, Func<T, string> get) {
            this.name = name;
            this.id = id;
            this.get = get;
        }

        public string Name => name;

        public string ID => id;

        public string SettingsClosedLabel => name;

        public bool Compare(TextOperation comparison, T def, string expected) 
            => comparison.Compare(get(def), expected);

        public void DoSettings(WidgetRow row, Rect rect, ref float curY) {}

        public void CopyToGetter(IDefValueGetter<T> other) {}

        public void Setup() {}

        public void ExposeData() {}
    }

    [StaticConstructorOnStartup]
    public abstract class TextValueDef<T> : TextValue where T : Def {
        private static readonly List<Func<IDefValueGetter<T>>> registeredGetters
            = new List<Func<IDefValueGetter<T>>>();

        public static void RegisterGetter(Func<IDefValueGetter<T>> create) 
            => registeredGetters.Add(create);

        private List<IDefValueGetter<T>> getters;
        private int index;
        private bool needSetup = true;

        protected TextValueDef(string name, string id, string description, int getterIndex = 0, 
                params IDefValueGetter<T>[] getters) 
                : base(name, id, description) {
            this.getters = new List<IDefValueGetter<T>> {
                new DefValueGetter<T>(Strings.ValueLabelName,   Strings.ValueLabelID,   d => d?.label),
                new DefValueGetter<T>(Strings.ValueDefNameName, Strings.ValueDefNameID, d => d?.defName),
            };
            this.getters.AddRange(getters);
            index = getterIndex;
        }

        protected TextValueDef(string name, string id, string description, float _) 
            : base(name, id, description) {}

        public override void Setup() {
            if (needSetup) {
                getters.AddRange(registeredGetters.Select(f => f()));
                needSetup = false;
            }
        }

        public void SetGetter(IDefValueGetter<T> getter) {
            var id = getter.ID;
            for (int i = 0; i < getters.Count; i++) {
                if (getters[i].ID == id) {
                    getters[i] = getter;
                    SetupGetter(i);
                    return;
                }
            }
            Log.Error($"[Categorized Bill Menus] Can't use getter with ID '{id}', because it is not registered for type {typeof(T).Name}.");
        }

        protected TextValueDef<T> CopyTo(TextValueDef<T> other) {
            other.getters = getters;
            other.index = index;
            Getter.CopyToGetter(other.Getter);
            return other;
        }

        protected void CopyToGetter<S>(IDefValueGetter<S> other) where S : Def {
            if (other is TextValueDef<T> tvd) {
                CopyTo(tvd);
            }
        }

        protected IDefValueGetter<T> Getter => getters[index];

        protected string GetterClosedLabel => getters[index].SettingsClosedLabel;

        protected abstract T GetDef(BillMenuEntry entry);

        public override string Get(BillMenuEntry entry) => throw new NotSupportedException();

        public override string Get(BillMenuEntry entry, MenuNode parent) => throw new NotSupportedException();

        public override bool Compare(TextOperation comparison, BillMenuEntry entry, string expected)
            => Getter.Compare(comparison, GetDef(entry), expected);

        public override bool Compare(TextOperation comparison, BillMenuEntry entry, MenuNode parent, string expected)
            => true; // If first variant returned true, then this should as well

        public override void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            row.SelectMenuButton(getters[index], getters, SetupGetter, g => g.Name);
            getters[index].DoSettings(row, rect, ref curY);
        }

        private void SetupGetter(int i) {
            index = i;
            Getter.Setup();
        }

        public override string SettingsClosedLabel => $"{Name} {GetterClosedLabel}";

        public override void ExposeData() {
            base.ExposeData();
            string getter = Getter.ID;
            Scribe_Values.Look(ref index,  "getter");
            Scribe_Values.Look(ref getter, "getterID");

            if (LoadingVars) {
                Setup();
                if (index < 0 || index > getters.Count) index = 0;
                if (getter != null && getter != Getter.ID) {
                    for (int i = 0; i < getters.Count; i++) {
                        if (getters[i].ID == getter) {
                            index = i;
                            break;
                        }
                    }
                }
                SetupGetter(index);
            }

            Getter.ExposeData();
        }
    }

    public class TextValueDefConcrete<T> : TextValueDef<T> where T : Def {
        private Func<BillMenuEntry, T> getDef;

        private TextValueDefConcrete(string name, string id, string description) 
            : base(name, id, description, 0f) {}

        public TextValueDefConcrete(string name,
                                    string id,
                                    string description,
                                    Func<BillMenuEntry, T> getDef,
                                    int getterIndex,
                                    params IDefValueGetter<T>[] getters) 
            : base(name, id, description, getterIndex, getters) {
            this.getDef = getDef;
        }

        public override TextValue Copy() 
            => CopyTo(new TextValueDefConcrete<T>(Name, ID, Description));

        protected TextValueDefConcrete<T> CopyTo(TextValueDefConcrete<T> other) {
            other.getDef = getDef;
            base.CopyTo(other);
            return other;
        }

        protected override T GetDef(BillMenuEntry entry) => getDef(entry);
    }
}
