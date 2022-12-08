﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public abstract class RuleCondition : RulePart<RuleCondition> {
        private static readonly string[] onCopiedTips = {
            "Do not apply on copies of menu options that were created by earlier rules.",
            "Apply on copies of menu options that were created by earlier rules."
        };
        private static readonly string[] onMovedTips = {
            "Do not apply on menu options that were moved by earlier rules.",
            "Apply on menu options that were moved by earlier rules."
        };
        private static readonly Texture2D[] overlayIcons = {
            Textures.NoIcon,
            BaseContent.ClearTex
        };

        private bool onCopied = false;
        private bool onMoved  = true;

        protected RuleCondition(string name, string description)
            : base(name, description) {}

        public bool OnCopied => onCopied;
        public bool OnMoved => onMoved;

        public override void CopyFrom(RuleCondition from) {
            onCopied = from.onCopied;
            onMoved = from.onMoved;
        }

        public abstract bool Test(BillMenuEntry entry, bool first);

        public abstract bool Test(BillMenuEntry entry, MenuNode parent);

        protected override void DoButtons(Rect icon) {
            ExtraWidgets.ToggleButton(icon, ref onCopied, overlayIcons, onCopiedTips, button: TexButton.Copy);
            icon.x += RuleIconSize + SmallMargin;
            ExtraWidgets.ToggleButton(icon, ref onMoved, overlayIcons, onMovedTips, button: TexButton.Paste);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref onCopied, "onCopied");
            Scribe_Values.Look(ref onMoved,  "onMoved", true);
        }
    }
}
