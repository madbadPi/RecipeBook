﻿namespace RecipeBook.Data.Models
{
    using System;
    using System.Collections.Generic;

    using RecipeBook.Data.Common.Models;

    public class RecipeType : BaseDeletableModel<string>
    {
        public RecipeType()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Recipes = new HashSet<Recipe>();
        }

        public string Name { get; set; }

        public string ImagePath { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }
    }
}
