﻿namespace SoderiaLaNueva_Api.Models.DAO
{
    public class GenericComboResponse
    {
        public List<Item> Items { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Description { get; set; } = null!;
        }
    }
}