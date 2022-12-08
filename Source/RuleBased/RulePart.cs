using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class RulePart<T> : Registerable<T>, ISettingsEntry where T : RulePart<T> {
        public const UIDirection Dir    = UIDirection.RightThenDown;
        public const float Margin       = Settings.Margin;
        public const float SmallMargin  = CategoryRule.SmallMargin;
        public const float RuleIconSize = CategoryRule.RuleIconSize;
        public const float RuleIconYAdj = CategoryRule.RuleIconYAdj;
        public const float RowHeight    = CategoryRule.RowHeight;

        protected RulePart(string name, string description) : base(name, description) {}

        protected virtual void DoSettingsLocal(WidgetRow row, Rect rect, ref float curY) {}

        protected abstract void DoButtons(Rect icon);

        public void DoSettings(WidgetRow row, Rect rect, ref float curY, bool buttons = true) {
            if (buttons) {
                DoButtons(new Rect(rect.x, curY + RuleIconYAdj, RuleIconSize, RuleIconSize));
            }
            DoSettingsLocal(row, rect, ref curY);
            var y = row.FinalY + RowHeight;
            if (y > curY) curY = y;
        }
    }
}
