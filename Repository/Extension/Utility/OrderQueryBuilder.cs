using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Entities.Models;

namespace Repository.Extension.Utility
{
    public static class OrderQueryBuilder
    {
        public static string CreateOrderQuery<T>(string orderByQueryString)
        {
            // Split our string into an array to use all the arguments passed
            var orderParams = orderByQueryString.Trim().Split(',');

            // Gets all our properties of our Type - Name, Age, Etc.
            var propertyInfos = typeof(Employee).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var orderQueryBuilder = new StringBuilder();

            //Goes Through all the parameters we sent through
            foreach (var param in orderParams)
            {
                // Check is param is null
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                // Separates the desc from the param, this creates a string array, then selecting the first index in the array
                var propertyFromQueryName = param.Split(" ")[0];
                //Checks if the string from the property is equal to the name of the property
                var objectProperty = propertyInfos.FirstOrDefault(pi =>
                    pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objectProperty == null)
                    continue;

                // If the param ends with desc we will order our results descending
                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                // Appends the Property name and the direction we are ordering from.
                orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {direction},");
            }

            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

            return orderQuery;
        }
    }
}
