using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";

        public static readonly Texture2D FavIcon   = ContentFinder<Texture2D>.Get(Prefix + "FavIcon");
        public static readonly Texture2D EditIcon  = ContentFinder<Texture2D>.Get(Prefix + "EditIcon");
    }
}
