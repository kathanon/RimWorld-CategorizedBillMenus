﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class TextValue : Registerable<TextValue> {
        public static string AlwaysMatchMarker = TextOperation.AlwaysMatchMarker;

        public TextValue(string name, string description) 
            : base(name, description, false) {}


        public abstract string Get(BillMenuEntry entry);

        public abstract string Get(BillMenuEntry entry, MenuNode parent);

        public virtual bool Compare(TextOperation comparison, BillMenuEntry entry, string expected) 
            => comparison.Compare(Get(entry), expected);

        public virtual bool Compare(TextOperation comparison, BillMenuEntry entry, MenuNode parent, string expected) 
            => comparison.Compare(Get(entry, parent), expected);

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) {}

        public virtual string SettingsClosedLabel => Name;
    }
}