using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public interface ISettingsEntry {
        string Name { get; }

        string Description { get; }

        void DoSettings(WidgetRow row, Rect rect, ref float curY);
    }
}
