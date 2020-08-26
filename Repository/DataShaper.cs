using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Repository
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {


        // An array of properties that we're going to pull out of the input type(class i.e. Employee,etc.)
        public PropertyInfo[] Properties { get; set; }

        // Get all the properties of our input class
        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        // Both methods use the GetRequiredProperties Method to pass the string that contains our field-property
        // we want to fetch
        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);

            return FetchData(entities, requiredProperties);
        }

        public ShapedEntity ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchDataForEntity(entity, requiredProperties);
        }

        // This is the same implementation just for multiple objects - we do this by using the FetchDataForEntity method
        private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<ShapedEntity>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        // This extracts the data from a property - only for a single entity
        private ShapedEntity FetchDataForEntity(T entityType, IEnumerable<PropertyInfo> requiredProperties)
        {
            // This is a IDictionary<string, object>
            var shapedObject = new ShapedEntity();

            // We loop through our requiredProperties
            foreach (var property in requiredProperties)
            {
                // Using reflection we extract the data from our property and add them to the ExpandoObject
                var objectPropertyValue = property.GetValue(entityType);

                // We use tryAdd to add the propertyName as the string and the value extracted ad the object value
                shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
            }

            var objectProperty = entityType.GetType().GetProperty("Id");
            shapedObject.Id = (Guid)objectProperty.GetValue(entityType);

            return shapedObject;
        }

        // This method gets the string and returns the properties we need to return
        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var requiredProperties = new List<PropertyInfo>();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                //if the fieldString is not empty - we split it
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // We check if our string matches the property - if yes we add that property to our list
                foreach (var field in fields)
                {
                    var property = Properties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(),
                        StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                    {
                        continue;
                    }

                    requiredProperties.Add(property);
                }
            }
            else
            {
                // If string is empty we return all properties
                requiredProperties = Properties.ToList();
            }

            return requiredProperties;
        }
    }
}
