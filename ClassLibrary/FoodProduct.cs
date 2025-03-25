using System;

namespace ClassLibrary
{
    [Serializable]
    public class FoodProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ExpirationDate { get; set; }

        public FoodProduct(int id, string name, DateTime expirationDate)
        {
            Id = id;
            Name = name;
            ExpirationDate = expirationDate;
        }
    }
}