using System;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public class CategoryRule : IExposable {
        public const float EmptyWidth    = 80f;
        public const float SmallMargin   =  4f;
        public const float RuleIconSize  = 18f;
        public const float Margin        = Settings.Margin;
        public const float CheckboxSize  = Settings.CheckboxSize;
        public const float RowHeight     = CheckboxSize + Margin;
        public const float RuleIconYAdj  = (CheckboxSize - RuleIconSize) / 2;
        public const float RuleIconSpace = RuleIconSize + SmallMargin;
        public const UIDirection Dir = UIDirection.RightThenDown;

        private static readonly string[] allowAfterTips 
            = { Strings.AllowAfterNoTips, Strings.AllowAfterYesTips };

        private RuleCondition condition;
        private RuleAction action;
        private bool allowAfter;

        private bool open = false;

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

        public void Open() => open = true;

        public CategoryRule Copy() 
            => new CategoryRule(condition.Copy(), action.Copy());

        public virtual void ExposeData() {
            RuleCondition.Registerable_Look(ref condition, "condition");
            RuleAction   .Registerable_Look(ref action,    "action");
            Scribe_Values.Look(ref allowAfter, "allowAfter");
        }

        public void DoSettings(Rect rect, ref float curY) {
            float width = Mathf.Round((rect.width - Margin) * 0.65f);
            var row = new WidgetRow();
            float y1 = curY, y2 = curY;

            var arrowRect = new Rect(rect.x, curY + RuleIconYAdj, RuleIconSize, RuleIconSize);
            ExtraWidgets.CollapseButton(arrowRect, ref open);

            if (open) {
                // TODO: show icons for new rule - maybe null object?
                var subRect = rect.LeftPartPixels(width);
                DoPart(condition, c => condition = c, Strings.ConditionPrefix, row, subRect, ref y1, 1);

                subRect.x = (y1 > curY) ? rect.x + width + Margin : row.FinalX;
                subRect.xMax = rect.xMax - RuleIconSpace;
                DoPart(action, a => action = a, Strings.ActionPrefix, row, subRect, ref y2);

                var after = new Rect(rect.xMax - RuleIconSize, curY + RuleIconYAdj, RuleIconSize, RuleIconSize);
                ExtraWidgets.ToggleButton(
                    after, ref allowAfter, TexButton.SpeedButtonTextures, allowAfterTips, iconXAdj: RuleIconSize / 3);

                curY = Mathf.Max(y1, y2);
            } else {
                var textRect = new Rect(rect.x + RuleIconSpace, curY, rect.width - RuleIconSpace, CheckboxSize);
                string conditionText = condition?.SettingsClosedLabel(this) ?? "-";
                string actionText = action?.SettingsClosedLabel(this) ?? "-";
                string text = $"{Strings.ConditionPrefix} {conditionText} {Strings.ActionPrefix} {actionText}";
                Widgets.Label(textRect, text);
            }
        }

        private void DoPart<T>(
                T cur, Action<T> set, string label, WidgetRow row, Rect rect, ref float curY, int buttons = 0) 
                where T : RulePart<T> {
            row.Init(rect.x, curY, Dir, rect.width, Margin / 2);
            buttons += cur?.NumToggles ?? 0;
            if (buttons > 0) {
                row.ForceGap(buttons * RuleIconSpace - SmallMargin); // Add space for toggle icons
            }
            row.Label(label);
            row.Gap(Margin - 2f);
            row.SelectMenuButton(cur, set);
            cur?.DoSettings(row, rect, ref curY);
        }
    }
}
