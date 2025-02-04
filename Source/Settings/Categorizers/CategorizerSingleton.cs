﻿namespace CategorizedBillMenus;
public abstract class CategorizerSingleton : Categorizer {
    protected CategorizerSingleton(string name, string description) 
        : base(name, description, false) {}

    public override bool Singleton => true;

    public override Categorizer Copy() => this;
}
