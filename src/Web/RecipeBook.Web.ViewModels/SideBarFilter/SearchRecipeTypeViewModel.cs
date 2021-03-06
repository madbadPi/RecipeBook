﻿namespace RecipeBook.Web.ViewModels.Home
{
    using System.Collections.Generic;

    using RecipeBook.Data.Models;
    using RecipeBook.Services.Mapping;

    public class SearchRecipeTypeViewModel : IMapFrom<RecipeType>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Checked { get; set; }

        public ICollection<SearchRecipeViewModel> Recipes { get; set; }
    }
}
