﻿namespace RecipeBook.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using RecipeBook.Data.Models;
    using RecipeBook.Services.Data;
    using RecipeBook.Web.ViewModels.Home;
    using RecipeBook.Web.ViewModels.Recipe;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class RecipeController : Controller
    {
        private readonly IRecipeService recipeService;
        private readonly IRecipeTypeService recipeTypeService;
        private readonly ICookingHistoryService cookingHistoryService;
        private readonly UserManager<ApplicationUser> userManager;

        public RecipeController(IRecipeService recipeService, IRecipeTypeService recipeTypeService, ICookingHistoryService cookingHistoryService, UserManager<ApplicationUser> userManager)
        {
            this.recipeService = recipeService;
            this.recipeTypeService = recipeTypeService;
            this.cookingHistoryService = cookingHistoryService;
            this.userManager = userManager;
        }

        public ActionResult Index(SearchViewModel data)
        {
            var searchViewModel = new SearchViewModel();
            searchViewModel.SearchData = new SearchDataModel();
            searchViewModel.SearchData.Mode = SearchDataModeEnum.Recipe;
            searchViewModel.ResultItems = this.recipeService.GetAll<RecipesSearchResultItemViewModel>();

            return this.View(searchViewModel);
        }

        public IActionResult Details(string id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var recipe = await _context.Recipes
            //    .Include(r => r.RecipeType)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (recipe == null)
            //{
            //    return NotFound();
            //}

            //return View(recipe);
            RecipeViewModel data = this.recipeService.GetById<RecipeViewModel>(id);
            return this.View(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchDataModel searchData)
        {
            var searchViewModel = new SearchViewModel();
            searchViewModel.SearchData = searchData;

            if (!this.ModelState.IsValid)
            {
                return this.View(searchViewModel);
            }

            if (searchData != null && !string.IsNullOrEmpty(searchData.Text))
            {
                var searchRecipesByNameResultItems = this.recipeService.GetByName<RecipesSearchResultItemViewModel>(searchData.Text);
                var searchRecipesByIngredientsResultItems = this.recipeService.GetByIngredients<RecipesSearchResultItemViewModel>(searchData.Text);

                searchViewModel.ResultItems = searchRecipesByNameResultItems.Union(searchRecipesByIngredientsResultItems);
            }
            else if (searchData != null && !string.IsNullOrEmpty(searchData.RecipeTypes))
            {
                var searchRecipesByTypesResultItems = this.recipeService.GetByRecipeTypes<RecipesSearchResultItemViewModel>(searchData.RecipeTypes);
                searchViewModel.ResultItems = searchRecipesByTypesResultItems;
            }
            else
            {
                searchViewModel.ResultItems = this.recipeService.GetAll<RecipesSearchResultItemViewModel>();
            }

            searchViewModel.ResultItems = searchViewModel.ResultItems.OrderByDescending(result => result.RecipeScore);
            return this.View("Index", searchViewModel);
        }

        [Authorize]
        public ActionResult Create()
        {
            RecipeViewModel data = new RecipeViewModel();
            data.AllRecipeTypes = this.recipeTypeService.GetAll<RecipeTypeViewModel>();
            data.IngredientSet = new ViewModels.IngredientsSet.IngredientsSetViewModel();

            return this.View(data);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RecipeViewModel inputData)
        {
            if (this.ModelState.IsValid)
            {
                bool result = await this.recipeService.CreateAsync(new RecipeDataModel()
                {
                    Id = inputData.Id,
                    ImagePath = inputData.ImagePath,
                    Name = inputData.Name,
                    Text = inputData.Text,
                    RecipeTypeId = inputData.RecipeType.Id,
                    //IngredientRecipeTypes = inputData.IngredientRecipeTypes,
                    //IngredientSetId = inputData.IngredientSet.Id,
                    LastCooked = inputData.LastCooked,
                });

                if (result)
                {
                    CookingHistory cookingRecord = new CookingHistory();
                    cookingRecord.RecipeId = inputData.Id;
                    cookingRecord.LastCooked = inputData.LastCooked;
                    cookingRecord.RecipeEasyRate = inputData.EasyRate;
                    cookingRecord.RecipeTasteRate = inputData.TasteRate;
                    cookingRecord.UserId = this.userManager.GetUserId(this.User);
                    await this.cookingHistoryService.CreateAsync(cookingRecord);

                    // redirect to next view
                    return this.RedirectToAction(nameof(this.Edit), "Recipe", new { @id = inputData.Id });
                }
            }

            return this.View(inputData);
        }

        public ActionResult Edit(string Id)
        {
            RecipeViewModel data = this.recipeService.GetById<RecipeViewModel>(Id);
            data.AllRecipeTypes = this.recipeTypeService.GetAll<RecipeTypeViewModel>();

            return this.View(data);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RecipeViewModel inputData)
        {
            if (this.ModelState.IsValid)
            {
                bool result = await this.recipeService.UpdateAsync(new RecipeDataModel()
                {
                    Id = inputData.Id,
                    ImagePath = inputData.ImagePath,
                    Name = inputData.Name,
                    Text = inputData.Text,
                    RecipeTypeId = inputData.RecipeType.Id,
                    PreparationTime = inputData.PreparationTime,
                    //IngredientRecipeTypes = inputData.IngredientRecipeTypes,
                    //IngredientSetId = inputData.IngredientSet.Id,
                    LastCooked = inputData.LastCooked,
                });

                if (result)
                {
                    CookingHistory cookingRecord = new CookingHistory();
                    cookingRecord.RecipeId = inputData.Id;
                    cookingRecord.LastCooked = inputData.LastCooked;
                    cookingRecord.RecipeEasyRate = inputData.EasyRate;
                    cookingRecord.RecipeTasteRate = inputData.TasteRate;
                    cookingRecord.UserId = this.userManager.GetUserId(this.User);
                    await this.cookingHistoryService.CreateAsync(cookingRecord);

                    return this.RedirectToAction(nameof(this.Index), "Recipe", new { @id = inputData.Id });
                }
            }

            return this.View(inputData);
        }

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(string id)
        //{

        //    return this.View();
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    return RedirectToAction(/*nameof(Index)*/);
        //}

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateLastCookedDate(string id)
        {
            DateTime dateTimeNow = DateTime.UtcNow;
            var result = await this.recipeService.UpdateLastCookedDate(id, dateTimeNow);
            if (result)
            {
                var currentRecipe = this.recipeService.GetById<RecipeViewModel>(id);

                CookingHistory cookingRecord = new CookingHistory();
                cookingRecord.RecipeId = currentRecipe.Id;
                cookingRecord.LastCooked = dateTimeNow;
                cookingRecord.RecipeEasyRate = currentRecipe.EasyRate;
                cookingRecord.RecipeTasteRate = currentRecipe.TasteRate;
                cookingRecord.UserId = this.userManager.GetUserId(this.User);

                await this.cookingHistoryService.CreateAsync(cookingRecord);

                return this.Json(new { @result = dateTimeNow.ToString("s") });
            }
            else
            {
                return this.Json(new { @result = string.Empty });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("SideBarSearch")]
        public IActionResult SideBarSearch(SearchDataModel searchData)
        {
            var searchViewModel = new SearchViewModel();

            searchViewModel.SearchData = searchData;

            //TODO change it
            //if (!this.ModelState.IsValid)
            //{
            //    return this.View("Search", searchViewModel);
            //}

            List<RecipesSearchResultItemViewModel> varResultItems = this.recipeService.GetAll<RecipesSearchResultItemViewModel>().ToList();
            bool isPrevFiltered = false;

            if (searchData != null && !string.IsNullOrEmpty(searchData.Text))
            {
                isPrevFiltered = true;
                var searchRecipesByNameResultItems = this.recipeService.GetByNamesList<RecipesSearchResultItemViewModel>(searchData.Text);

                var searchRecipesByIngredientsResultItems = this.recipeService.GetByIngredients<RecipesSearchResultItemViewModel>(searchData.Text);

                varResultItems = searchRecipesByNameResultItems.Union(searchRecipesByIngredientsResultItems).ToList();
            }

            if (searchData != null && !string.IsNullOrEmpty(searchData.RecipeTypes))
            {
                var searchRecipesByTypesResultItems = this.recipeService.GetByRecipeTypes<RecipesSearchResultItemViewModel>(searchData.RecipeTypes);

                if (isPrevFiltered)
                {
                    varResultItems = (from objA in varResultItems
                                      join objB in searchRecipesByTypesResultItems on objA.Id equals objB.Id
                                      select objA).ToList();
                }
                else
                {
                    varResultItems = searchRecipesByTypesResultItems.ToList();
                }

                isPrevFiltered = true;
            }

            if (searchData != null && !string.IsNullOrEmpty(searchData.Ingredients))
            {
                var searchRecipesByIngredientsResultItems = this.recipeService.GetByIngredients<RecipesSearchResultItemViewModel>(searchData.Ingredients);

                if (isPrevFiltered)
                {
                    varResultItems = (from objA in varResultItems
                                      join objB in searchRecipesByIngredientsResultItems on objA.Id equals objB.Id
                                      select objA).ToList();
                }
                else
                {
                    varResultItems = searchRecipesByIngredientsResultItems.ToList();
                }
            }


            searchViewModel.ResultItems = varResultItems.OrderByDescending(result => result.RecipeScore);
            var parView = this.PartialView("ResultList", searchViewModel);
            return parView;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRecipeToMenu(string id)
        {
            bool result = await this.recipeService.AddRecipeToMenu(id);

            return this.Json(new { @id = id, @result = result });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveRecipeFromMenu(string id)
        {
            bool result = await this.recipeService.RemoveRecipeFromMenu(id);

            return this.Json(new { @id = id, @result = result });
        }

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> UpdateLastCookedDate(string id)
        //{
        //    DateTime dateTimeNow = DateTime.UtcNow;
        //    var result = await this.recipeService.UpdateLastCookedDate(id, dateTimeNow);
        //    if (result)
        //    {
        //        var currentRecipe = this.recipeService.GetById<SearchResultItemViewModel>(id);

        //        CookingHistory cookingRecord = new CookingHistory();
        //        cookingRecord.RecipeId = currentRecipe.Id;
        //        cookingRecord.LastCooked = dateTimeNow;
        //        cookingRecord.RecipeEasyRate = currentRecipe.EasyRate;
        //        cookingRecord.RecipeTasteRate = currentRecipe.TasteRate;
        //        cookingRecord.UserId = this.userManager.GetUserId(this.User);

        //        await this.cookingHistoryService.CreateAsync(cookingRecord);
        //    }

        //    return this.Json(new { @result = result });
        //}

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRecipe(string id)
        {
            var result = await this.recipeService.DeleteAsync(id);
            return this.Json(new { @result = result });
        }
    }
}
