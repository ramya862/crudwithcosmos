using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using ShoppingCartList.Models;

namespace ShoppingCartList.Models
{
    public class ShoppingCartItem : TableEntity
    {
        [JsonProperty("id")]
        [Required(ErrorMessage = "Id is required")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Created { get; set; } = DateTime.Now;
        [Required]
        public string ItemName { get; set; }
        public bool Collected { get; set; }
        [JsonProperty("category")]
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
    }

    public class CreateShoppingCartItem
    {
        public string ItemName { get; set; }
        public string Category { get; set; }
    }

    public class UpdateShoppingCartItem
    {
        public bool Collected { get; set; }
    }


}
public class CreateShoppingCartItemValidator : AbstractValidator<ShoppingCartItem>
{
    public CreateShoppingCartItemValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().WithMessage("Please specify the item name");;
        RuleFor(x => x.Category).NotEmpty().WithMessage("Please specify the category name");;
    }
}
