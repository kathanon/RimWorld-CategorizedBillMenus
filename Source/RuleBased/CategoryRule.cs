using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public class CategoryRule : IExposable {
        public const float EmptyWidth = 80f;
        public const float Margin     = Settings.Margin;
        public const float RowHeight  = Settings.CheckboxSize + Margin;

        public const UIDirection Dir = UIDirection.RightThenDown;

        private RuleCondition condition;
        private RuleAction action;
        private bool allowAfter;

        public bool AllowAfter { 
            get => allowAfter; 
            protected set => allowAfter = value;
        }
        public bool Copies   => action.Copies;
        public bool OnCopied => condition.OnCopied;
        public bool OnMoved  => condition.OnMoved;

        public CategoryRule() {
            AllowAfter = false;
        }

        public CategoryRule(RuleCondition condition, RuleAction action) : this() {
            this.condition = condition;
            this.action = action;
        }

        public bool AppliesTo(BillMenuEntry entry, bool first) => condition?.Test(entry, first) ?? false;

        public virtual MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) {
            if (condition.Test(entry, parent)) {
                return action.Apply(entry, parent, root);
            } else {
                return parent;
            }
        }

        public bool AppliesOn(bool copy, bool moved) 
            => (!copy || OnCopied) && (!moved || OnMoved);

        public CategoryRule Copy() => new CategoryRule(condition.Copy(), action.Copy());

        public virtual void ExposeData() {
            Scribe_Deep.Look(ref condition, "condition");
            Scribe_Deep.Look(ref action, "action");
            Scribe_Values.Look(ref allowAfter, "allowAfter");
        }

        public void DoSettings(Rect rect, ref float curY) {
            float width = (rect.width - Margin) / 2;
            var row = new WidgetRow();

            var subRect = rect.LeftPartPixels(width);
            row.Init(subRect.x, curY, Dir, subRect.width, Margin / 2);
            var y1 = DoPart(condition, ConditionMenu, "If recipe", row, subRect);

            subRect.x = (y1 > curY) ? rect.x + width + Margin : row.FinalX;
            subRect.xMax = rect.xMax;
            row.Init(subRect.x, curY, Dir, subRect.width, Margin / 2);
            var y2 = DoPart(action, ActionMenu, "then apply", row, subRect);

            curY = Mathf.Max(y1, y2);
        }

        private void ConditionMenu() => RuleCondition.SelectionMenu(c => condition = c, condition);
        private void ActionMenu() => RuleAction.SelectionMenu(a => action = a, action);

        private float DoPart<T>(T cur, Action menu, string label, WidgetRow row, Rect rect) 
                where T : ISettingsEntry {
            row.Label(label);
            row.Gap(Margin - 2f);
            row.MenuButton(cur, menu);
            float curY = row.FinalY + RowHeight;
            cur?.DoSettings(row, rect, ref curY);
            return curY;
        }
    }
}
