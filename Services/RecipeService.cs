using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recipeservice.Data;
using recipeservice.Model;
using recipeservice.Services.Interfaces;

namespace recipeservice.Services {
    public class RecipeService : IRecipeService {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IPhaseService _phaseService;

        private readonly IRecipeTypeService _recipeTypeService;
        public RecipeService (ApplicationDbContext context,
            IProductService productService,
            IPhaseService phaseService,
            IRecipeTypeService recipeTypeService) {
            _context = context;
            _productService = productService;
            _phaseService = phaseService;
            _recipeTypeService = recipeTypeService;
        }

        public async Task<(List<Recipe>, int)> getRecipes (int startat, int quantity, List<string> fields,
         RecipeFields orderField, OrderEnum order) {
            var recipesQuery = _context.Recipes.AsQueryable ();
            recipesQuery = ApplyMultifilter(recipesQuery,fields);
            recipesQuery = ApplyOrder (recipesQuery, orderField, order);

            var recipesId = await recipesQuery
                .Skip (startat).Take (quantity)
                .Select (x => x.recipeId)
                .ToListAsync ();

            var queryCount = _context.Recipes.AsQueryable ();
            queryCount = ApplyMultifilter(recipesQuery,fields);
            queryCount = ApplyOrder (queryCount, orderField, order);
            var totalCount = queryCount.Count ();

            List<Recipe> recipes = new List<Recipe> ();
            foreach (var item in recipesId) {
                var recipe = await getRecipe (item);
                if (recipe != null) {
                    if (recipe.recipeTypeId != null) {
                        var recipeType = await _recipeTypeService.getRecipeType (recipe.recipeTypeId.Value);
                        if (recipeType != null)
                            recipe.typeDescription = recipeType.typeDescription;
                    }
                    recipe.phases = new List<Phase> ();
                    foreach (var phaseId in recipe.phasesId) {
                        var phase = await _phaseService.getPhase (phaseId);
                        if (phase != null)
                            recipe.phases.Add (phase);
                    }
                    recipes.Add (recipe);
                }
            }
            return (recipes, totalCount);
        }

        public async Task<Recipe> getRecipe (int recipeId) {
            var recipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Include (x => x.recipeProduct)
                .Where (x => x.recipeId == recipeId)
                .FirstOrDefaultAsync ();
            if (recipe != null) {
                if (recipe.recipeProduct != null) {
                    var product = await _productService.getProduct (recipe.recipeProduct.productId);
                    if (product != null) {
                        recipe.recipeProduct.product = product;
                    }
                }
            }
            if (recipe == null)
                return null;
            if (recipe.recipeTypeId != null) {
                var recipeType = await _recipeTypeService.getRecipeType (recipe.recipeTypeId.Value);
                if (recipeType != null)
                    recipe.typeDescription = recipeType.typeDescription;

            }
            recipe.phases = new List<Phase> ();
            foreach (var phaseId in recipe.phasesId) {
                var phase = await _phaseService.getPhase (phaseId);
                if (phase != null)
                    recipe.phases.Add (phase);
            }
            return recipe;
        }

        public async Task<Recipe> getRecipeCode (string code) {
            var recipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Include (x => x.recipeProduct)
                .Where (x => x.recipeCode == code)
                .FirstOrDefaultAsync ();
            if (recipe != null) {
                if (recipe.recipeProduct != null) {
                    var product = await _productService.getProduct (recipe.recipeProduct.productId);
                    if (product != null) {
                        recipe.recipeProduct.product = product;
                    }
                }
            }
            return recipe;
        }

        public async Task<Recipe> addRecipe (Recipe recipe) {
            recipe.recipeProduct = null;
            recipe.phasesId = new int[0];
            _context.Recipes.Add (recipe);
            await _context.SaveChangesAsync ();
            return recipe;
        }

        public async Task<Recipe> updateRecipe (int recipeId, Recipe recipe) {
            var curRecipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Include (x => x.recipeProduct)
                .Where (x => x.recipeId == recipeId).AsNoTracking ()
                .FirstOrDefaultAsync ();

            if (recipeId != recipe.recipeId && curRecipe == null) {
                return null;
            }

            recipe.phasesId = curRecipe.phasesId;
            recipe.recipeProduct = curRecipe.recipeProduct;

            _context.Recipes.Update (recipe);
            await _context.SaveChangesAsync ();
            return recipe;
        }

        public async Task<Recipe> deleteRecipe (int recipeId) {
            var curRecipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Include (x => x.recipeProduct)
                .Where (x => x.recipeId == recipeId)
                .FirstOrDefaultAsync ();

            if (curRecipe != null) {
                _context.Entry (curRecipe).State = EntityState.Deleted;
                await _context.SaveChangesAsync ();
            }
            return curRecipe;
        }

        public async Task<PhaseProduct> addProductToRecipe (int recipeId, PhaseProduct phaseProduct) {
            var curRecipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Where (x => x.recipeId == recipeId)
                .FirstOrDefaultAsync ();
            if (curRecipe == null) {
                return null;
            }
            var product = await _productService.getProduct (phaseProduct.productId);
            curRecipe.recipeProduct = phaseProduct;
            _context.Recipes.Update (curRecipe);
            await _context.SaveChangesAsync ();
            return phaseProduct;
        }

        public async Task<Recipe> removeProductToRecipe (int recipeId, PhaseProduct phaseProduct) {
            var curRecipe = await _context.Recipes
                .OrderBy (x => x.recipeId)
                .Include (x => x.recipeProduct)
                .Where (x => x.recipeId == recipeId)
                .FirstOrDefaultAsync ();
            if (curRecipe == null) {
                return null;
            }
            if (curRecipe.recipeProduct.productId != phaseProduct.productId) {
                return null;
            }
            curRecipe.recipeProduct = null;
            _context.Recipes.Update (curRecipe);
            await _context.SaveChangesAsync ();
            return await getRecipe (recipeId);
        }

        private IQueryable<Recipe> ApplyMultifilter (IQueryable<Recipe> query, List<string> fields) {
            foreach (var field in fields) {
                string fieldValue = string.Empty;
                var fieldSplit = field.Split (",");
                if (fieldSplit.Count () > 1)
                    fieldValue = fieldSplit[1];

                var fieldFilterEnum = RecipeFields.Default;
                Enum.TryParse (fieldSplit[0], true, out fieldFilterEnum);
                query = ApplyFilter (query, fieldFilterEnum, fieldValue);
            }
            return query;
        }

        private IQueryable<Recipe> ApplyFilter (IQueryable<Recipe> query,
            RecipeFields fieldFilter, string fieldValue) {
            switch (fieldFilter) {
                case RecipeFields.recipeCode:
                    query = query.Where (x => x.recipeCode.Contains (fieldValue));
                    break;
                case RecipeFields.recipeName:
                    query = query.Where (x => x.recipeName.Contains (fieldValue));

                    break;
                case RecipeFields.recipeTypeId:
                    query = query.Where (x => x.recipeTypeId == Convert.ToInt32 (fieldValue));
                    break;
                default:
                    break;
            }
            return query;
        }

        private IQueryable<Recipe> ApplyOrder (IQueryable<Recipe> query,
            RecipeFields orderField, OrderEnum order) {
            switch (orderField) {
                case RecipeFields.recipeCode:
                    if (order == OrderEnum.Ascending)
                        query = query.OrderBy (x => x.recipeCode);
                    else
                        query = query.OrderByDescending (x => x.recipeCode);
                    break;
                case RecipeFields.recipeName:
                    if (order == OrderEnum.Ascending)
                        query = query.OrderBy (x => x.recipeName);
                    else
                        query = query.OrderByDescending (x => x.recipeName);
                    break;
                case RecipeFields.recipeTypeId:
                    if (order == OrderEnum.Ascending)
                        query = query.OrderBy (x => x.recipeTypeId);
                    else
                        query = query.OrderByDescending (x => x.recipeTypeId);
                    break;
                default:
                    query = query.OrderBy (x => x.recipeId);
                    break;
            }
            return query;
        }
    }
}