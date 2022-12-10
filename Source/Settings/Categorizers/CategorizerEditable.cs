using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public abstract class CategorizerEditable : Categorizer {
        protected CategorizerEditable(string name, string description) 
            : base(name, description, true) {}

        public override bool Singleton => false;
    }
}
