using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class CategorizerRuleBased : CategorizerEditable {
        private const int Any   = 0;
        private const int All   = 1;
        private const int Label = 0;
        private const int Def   = 1;

        static CategorizerRuleBased() {
            Register(new CategorizerRuleBased());
        }

        public const float IconSize = Settings.IconSize;
        public const float Margin   = Settings.Margin;

        public static readonly CategorizerRuleBased PresetByLimb =
            new CategorizerRuleBased(
                new List<CategoryRule> {
                    new CategoryRule(RuleConditionSurgery.Instance, new RuleActionByLimb()),
                },
                Strings.ByLimbName,
                Strings.ByLimbDesc);

        public static readonly CategorizerRuleBased PresetByType =
            new CategorizerRuleBased(
                new List<CategoryRule> {
                    new CategoryRule(
                        new RuleConditionNot(RuleConditionSurgery.Instance), 
                        RuleActionNoop.Instance),
                    new CategoryRule(
                        new RuleConditionOr(
                            new RuleConditionText(
                                Values.Recipe(
                                    Values.Ingredient(Any, Label)), 
                                Comparison.Equals,
                                "Wood"), 
                            new RuleConditionText(
                                Values.Recipe(Label), 
                                Comparison.Contains,
                                "denture")), 
                        new RuleActionNamed("Primitive")),
                    new CategoryRule(
                        new RuleConditionText(
                            Values.Recipe(
                                Values.Ingredient(Any, Label)),
                            Comparison.Contains,
                            "Bionic"),
                        new RuleActionNamed("Bionic")),
                    new CategoryRule(
                        new RuleConditionText(
                            Values.Recipe(Label),
                            Comparison.Contains,
                            "Administer"),
                        new RuleActionNamed("Drugs")),
                    new CategoryRule(
                        new RuleConditionText(
                            Values.Recipe(
                                Values.Ingredient(Any, Def)),
                            Comparison.Contains,
                            "Prosthetic"),
                        new RuleActionNamed("Prosthetic")),
                    new CategoryRule(
                        new RuleConditionOr(
                            new RuleConditionText(
                                Values.Recipe(
                                    Values.Research(Any, Def)), 
                                Comparison.Equals,
                                "FertilityProcedures"), 
                            new RuleConditionText(
                                Values.Recipe(Def), 
                                Comparison.Equals,
                                "TerminatePregnancy")), 
                        new RuleActionNamed("Fertility")),
                    new CategoryRule(
                        new RuleConditionText(
                            Values.Recipe(Def),
                            Comparison.Equals,
                            "RemoveBodyPart"),
                        new RuleActionNamed("Harvest / Amputate")),
                },
                Strings.ByTypeName,
                Strings.ByTypeDesc);

        protected override IEnumerable<Categorizer> SubOptions(int level) {
            if (level == 0) {
                yield return this;
                yield return PresetByType;
                yield return PresetByLimb;
            }
        }

        private List<CategoryRule> rules;
        private bool allowAfter;

        public CategorizerRuleBased() 
            : this(new List<CategoryRule>()) {}

        public CategorizerRuleBased(List<CategoryRule> rules) 
            : this(rules, Strings.RuleBasedName, Strings.RuleBasedDesc) {}

        private CategorizerRuleBased(List<CategoryRule> rules, string name, string description) 
            : base(name, description) {
            this.rules = rules;
        }

        public override bool AppliesTo(BillMenuEntry entry, bool first) 
            => (first || allowAfter) && rules.Any(r => r.AppliesTo(entry, first));

        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
            var nodes = new List<MenuNode> { parent };
            bool moveLeft = true, moved = false;
            foreach (var rule in rules.Where(r => r.AppliesTo(entry, first))) {
                for (int i = 0, n = nodes.Count; i < n; i++) {
                    if (rule.AppliesOn(!moveLeft || i > 0, moved)) {
                        var node = rule.Apply(entry, parent, root);
                        if (nodes[i] != null) {
                            if (rule.AllowAfter) {
                                if (rule.Copies) {
                                    nodes.Add(node);
                                } else {
                                    nodes[i] = node;
                                }
                            } else { // AllowAfter
                                if (!rule.Copies) {
                                    nodes.RemoveAt(i);
                                    if (i == 0) moveLeft = false;
                                    n--;
                                    i--;
                                }
                                yield return node;
                            }
                        }
                    }
                }
            }
            foreach (var node in nodes) {
                yield return node;
            }
        }

        public override Categorizer Copy() 
            => new CategorizerRuleBased(rules.Select(r => r.Copy()).ToList(),
                                        Strings.ByLimbName,
                                        Strings.ByLimbDesc);

        public override void DoSettings(Rect rect, float _, ref float curY) {
            rect.xMin += IconSize + Margin;
            var icon = new Rect(rect.x, curY, IconSize, IconSize);

            ExtraWidgets.ReorderableList(rules, AddRule, DoRule, rect, ref curY);

            static void DoRule(CategoryRule rule, Rect r, float offset, ref float innerCurY) {
                rule.DoSettings(r, ref innerCurY);
            }
        }

        public void AddRule() => AddRule(new CategoryRule());

        public void AddRule(CategoryRule rule) {
            rules.Add(rule);
            rule.Open();
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);
            Scribe_Values.Look(ref allowAfter, "allowAfter");
        }
    }
}
