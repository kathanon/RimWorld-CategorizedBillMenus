using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public abstract class CategorizerSingleton : Categorizer {
        protected CategorizerSingleton(string name, string description) : base(name, description) {}

        public override bool Singleton => true;

        public override Categorizer Create() => this;
    }
}
